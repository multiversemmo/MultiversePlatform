/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

#region Using directives

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Axiom.Core;
using Axiom.MathLib;

using Multiverse.Config;
using Multiverse.Network;
using Multiverse.Voice;

#endregion

namespace Multiverse.Base
{
    public delegate void SoundDoneEvent(object sender, EventArgs args);

    public class FMODException : Exception
    {
        FMOD.RESULT errcode;
        public FMODException(string msg, FMOD.RESULT errcode)
            : base(msg)
        {
            this.errcode = errcode;
        }

        public override string Message
        {
            get { return base.Message + FMOD.Error.String(errcode); }
        }
    }
	
	public class SoundSource : Multiverse.Voice.SoundProperties, IDisposable
	{
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SoundSource));

        public event SoundDoneEvent SoundDone;

        private bool alreadyDisposed = false;

        public SoundSource(FMOD.System fmod, FMOD.Sound sound, string name, bool ambient) :
            base(name, ambient)
        {
            FMOD.RESULT result;

            result = fmod.playSound(FMOD.CHANNELINDEX.FREE, sound, true, ref channel);
            SoundManager.CheckResults(result);

            result = channel.setVolume(0);
            SoundManager.CheckResults(result);
        }

        // finalizer
        ~SoundSource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (alreadyDisposed)
            {
                return;
            }
            if (disposing == false)
            {
                //Trace.TraceError("Disposing SoundSource from finalizer");
            }
            //Stop();

            if (channel != null)
                channel.setCallback(FMOD.CHANNEL_CALLBACKTYPE.END, null, 0);
            else
               log.Error("Called Dispose with null channel");

            alreadyDisposed = true;
        }

        protected void OnSoundDone()
        {
            SoundDoneEvent handler = SoundDone;
            if (SoundDone != null)
            {
                SoundDone(this, new EventArgs());
            }
        }

        public void Stop()
        {
            FMOD.RESULT result = channel.stop();
            if (result != FMOD.RESULT.ERR_INVALID_HANDLE)
            {
                SoundManager.LogResults(result);
            }
        }

        protected FMOD.RESULT soundEndCallback(IntPtr channelraw, FMOD.CHANNEL_CALLBACKTYPE type, int command, uint commanddata1, uint commanddata2)
        {
            // Is this really an error?  Seems like info or even debug to me
            log.Debug("sound end callback called");

            done = true;

            OnSoundDone();

            return FMOD.RESULT.OK;
        }

        private FMOD.CHANNEL_CALLBACK endCallback = null;
        
        public void Play()
        {
            if (endCallback == null)
            {
                endCallback = new FMOD.CHANNEL_CALLBACK(soundEndCallback);
            }

            channel.setCallback(FMOD.CHANNEL_CALLBACKTYPE.END, endCallback, 0);

            FMOD.RESULT result = channel.setPaused(false);
            SoundManager.CheckResults(result);
        }

		public void Remove(string name)
		{
			SoundManager.Instance.Remove(name);
		}
    }
        

	public class SoundManager : Voice.ListenerProperties, IDisposable
	{
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SoundManager));

		static SoundManager instance = null;

		const int Voices = 100;
        protected FMOD.System fmod;

        protected Dictionary<string, FMOD.Sound> ambientSounds;
        protected Dictionary<string, FMOD.Sound> positionalSounds;

        public static void CheckResults(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                if (result == FMOD.RESULT.ERR_INVALID_HANDLE)
                {
                    log.WarnFormat("Invalid Sound Handle\n{0}", new StackTrace(true));
                }
                else
                {
                    log.ErrorFormat("FMOD result: {0}\n{1}", FMOD.Error.String(result), new StackTrace(true));
                    //throw new FMODException("Fmod error: ", result);
                }
            }
        }

        public SoundManager()
        {

            FMOD.RESULT result;

            result = FMOD.Factory.System_Create(ref fmod);
            CheckResults(result);

            result = fmod.init(Voices, FMOD.INITFLAG.NORMAL | FMOD.INITFLAG._3D_RIGHTHANDED, System.IntPtr.Zero);
            CheckResults(result);

            result = fmod.set3DSettings(1.0f, 1000.0f, 1.0f);
            CheckResults(result);

            if (instance != null)
            {
                throw new ApplicationException("SoundManager initialized twice");
            }
            Init();
            instance = this;

            ambientSounds = new Dictionary<string, FMOD.Sound>();
            positionalSounds = new Dictionary<string, FMOD.Sound>();
        }

        protected FMOD.Sound GetSound(string name, bool ambient, bool local)
        {
            Dictionary<string, FMOD.Sound> soundDict;
            FMOD.Sound sound;
            FMOD.MODE mode;
            if (ambient)
            {
                soundDict = ambientSounds;
                mode = FMOD.MODE.DEFAULT;
            }
            else
            {
                soundDict = positionalSounds;
                mode = FMOD.MODE._3D;
            }

            if (soundDict.ContainsKey(name))
            {
                sound = soundDict[name];
            }
            else
            {
                if (local)
                {
                    // Ask the resource manager where the file is (check any common asset directory)
                    string filename = ResourceManager.ResolveCommonResourceData(name);

                    FileInfo info = new FileInfo(filename);

                    sound = null;
                    if (info.Length > 200000)
                    {
                        FMOD.RESULT result = fmod.createStream(filename, mode, ref sound);
                        CheckResults(result);
                    }
                    else
                    {
                        FMOD.RESULT result = fmod.createSound(filename, mode, ref sound);
                        CheckResults(result);
                    }
                }
                else
                {
                    sound = null;
                    log.DebugFormat("before playing non-local sound: {0}", name);
                    FMOD.RESULT result = fmod.createStream(name, mode, ref sound);
                    log.DebugFormat("after playing non-local sound: {0}", name);
                    CheckResults(result);
                }
                soundDict.Add(name, sound);
            }

            return sound;
        }

		public void Remove(string name)
		{
		    if (ambientSounds.ContainsKey(name))
			{
				ambientSounds.Remove(name);
			}
			if (positionalSounds.ContainsKey(name))
			{
				positionalSounds.Remove(name);
			}
		}

        public SoundSource GetSoundSource(string name, bool ambient)
        {
            return GetSoundSource(name, ambient, true);
        }

        public SoundSource GetSoundSource(string name, bool ambient, bool local)
        {
            lock (this)
            {
                FMOD.Sound sound = GetSound(name, ambient, local);

                return new SoundSource(fmod, sound, name, ambient);
            }
        }

        public void Update()
        {
            Update(fmod);
        }

		public void Release(SoundSource source) {
            source.Stop();
		}

        public void Dispose()
        {
            FMOD.RESULT result = fmod.release();
            CheckResults(result);
        }

		#region Properties

        public static SoundManager Instance
        {
            get
            {
                return instance;
            }
        }

        public FMOD.System FMODSystemObject
        {
            get
            {
                return fmod;
            }
        }

        public uint StreamBufferSize
        {
            get
            {
                uint size = 0;
                FMOD.TIMEUNIT timeUnit = FMOD.TIMEUNIT.RAWBYTES;
                FMOD.RESULT result = fmod.getStreamBufferSize(ref size, ref timeUnit);
                log.ErrorFormat("Getting fmod stream buffer size: {0}, timeUnit: {1}", size, timeUnit);
                CheckResults(result);

                return size;
            }
            set
            {
                FMOD.RESULT result = fmod.setStreamBufferSize(value, FMOD.TIMEUNIT.RAWBYTES);
                CheckResults(result);
            }
        }


		#endregion
	}
}

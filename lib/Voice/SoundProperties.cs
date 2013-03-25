#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using log4net;
using Multiverse.Lib.LogUtil;
using Axiom.MathLib;

using SpeexWrapper;

#endregion


namespace Multiverse.Voice
{
    public class SoundProperties
    {
        protected FMOD.Channel channel = null;

		protected bool ambient;

        protected bool done = false;

        protected bool looping = false;

        protected string name;

        protected bool linearAttenuation = false;

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SoundProperties));

        public SoundProperties(string name, bool ambient) {
            this.name = name;
            this.ambient = ambient;
        }

        protected void UpdateMode()
        {
            if (channel == null)
                return;
            
            FMOD.MODE mode;

            if (ambient)
            {
                mode = FMOD.MODE.HARDWARE | FMOD.MODE._2D;
            }
            else
            {
                mode = FMOD.MODE.HARDWARE | FMOD.MODE._3D;
            }

            if (looping)
            {
                mode |= FMOD.MODE.LOOP_NORMAL;
            }
            else
            {
                mode |= FMOD.MODE.LOOP_OFF;
            }

            if (linearAttenuation)
            {
                mode |= FMOD.MODE._3D_LINEARROLLOFF;
            }
            else
            {
                mode |= FMOD.MODE._3D_LOGROLLOFF;
            }

            FMOD.RESULT result = channel.setMode(mode);
            CheckRetCode(result);
        }

        public void SetPositionAndVelocity(Vector3 position, Vector3 velocity) {
            if (channel == null)
                return;
            FMOD.VECTOR pos = new FMOD.VECTOR();
            pos.x = position.x;
            pos.y = position.y;
            pos.z = position.z;
            FMOD.VECTOR vel = new FMOD.VECTOR();
            vel.x = velocity.x;
            vel.y = velocity.y;
            vel.z = velocity.z;
            CheckRetCode(channel.set3DAttributes(ref pos, ref vel));
        }
        
        public Vector3 Position
        {
            set
            {
                FMOD.VECTOR pos;
                FMOD.VECTOR vel;

                pos.x = value.x;
                pos.y = value.y;
                pos.z = value.z;

                vel.x = vel.y = vel.z = 0;

                if (channel != null) {
                    FMOD.RESULT result = channel.set3DAttributes(ref pos, ref vel);
                    CheckRetCode(result);
                }

                //Trace.TraceError("setting sound position: " + value.ToString());
            }
            get
            {
                if (channel != null) {
                    FMOD.VECTOR pos = new FMOD.VECTOR();
                    FMOD.VECTOR vel = new FMOD.VECTOR();

                    FMOD.RESULT result = channel.get3DAttributes(ref pos, ref vel);
                    CheckRetCode(result);

                    return new Vector3(pos.x, pos.y, pos.z);
                }
                else
                    return Vector3.Zero;
            }
        }

		public bool Looping {
            get
            {
                return looping;
            }
            set
            {
                if (channel == null)
                    return;
                int count;
                if (value)
                {
                    count = -1;
                }
                else
                {
                    count = 0;
                }
                FMOD.RESULT result = channel.setLoopCount(count);
                CheckRetCode(result);

                looping = true;
                UpdateMode();
            }
		}

        public bool Ambient
        {
            get
            {
                return ambient;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public float Gain
        {
            get
            {
                float vol = 0;
                if (channel != null) {
                    FMOD.RESULT result = channel.getVolume(ref vol);
                    CheckRetCode(result);
                }

                return vol;
            }
            set
            {
                if (channel != null) {
                    FMOD.RESULT result = channel.setVolume(value);
                    CheckRetCode(result);
                }
            }
        }

        public float MinAttenuationDistance
        {
            get
            {
                float min = 0;
                float max = 0;

                if (channel != null) {
                    FMOD.RESULT result = channel.get3DMinMaxDistance(ref min, ref max);
                    CheckRetCode(result);
                }
                return min;
            }
            set
            {
                if (channel == null)
                    return;

                float min = 0;
                float max = 0;

                FMOD.RESULT result = channel.get3DMinMaxDistance(ref min, ref max);
                CheckRetCode(result);

                result = channel.set3DMinMaxDistance(value, max);
                CheckRetCode(result);
            }
        }

        public bool LinearAttenuation
        {
            get
            {
                return linearAttenuation;
            }
            set
            {
                linearAttenuation = value;
                UpdateMode();
            }
        }

        public float MaxAttenuationDistance
        {
            get
            {
                float min = 0;
                float max = 0;

                if (channel != null) {
                    FMOD.RESULT result = channel.get3DMinMaxDistance(ref min, ref max);
                    CheckRetCode(result);
                }
                return max;
            }
            set
            {
                if (channel == null)
                    return;
                
                float min = 0;
                float max = 0;

                FMOD.RESULT result = channel.get3DMinMaxDistance(ref min, ref max);
                CheckRetCode(result);

                result = channel.set3DMinMaxDistance(min, value);
                CheckRetCode(result);
            }
        }

        public void CheckRetCode(FMOD.RESULT result) {
            if (result != FMOD.RESULT.OK)
            {
                if (result == FMOD.RESULT.ERR_INVALID_HANDLE)
                {
                    log.WarnFormat("Invalid Sound Handle\n{0}", new StackTrace(true).ToString());
                }
                else
                {
                    log.ErrorFormat("FMOD result: {0}\n{1}", FMOD.Error.String(result), new StackTrace(true).ToString());
                    //throw new FMODException("Fmod error: ", result);
                }
            }
        }
	}

}

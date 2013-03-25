#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using log4net;
using Multiverse.Lib.LogUtil;
using Axiom.MathLib;

#endregion

namespace Multiverse.Voice {
    
    public class ListenerProperties {



        protected FMOD.VECTOR listenerPosition;
        protected FMOD.VECTOR listenerVelocity;
        protected FMOD.VECTOR listenerForward;
        protected FMOD.VECTOR listenerUp;

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ListenerProperties));

        public void Init()
        {
            ListenerPosition = Vector3.Zero;
            ListenerVelocity = Vector3.Zero;
            ListenerForward = -Vector3.UnitZ;
            ListenerUp = Vector3.UnitY;
        }

        public void Update(FMOD.System fmod) {
            FMOD.RESULT result;

            //Trace.TraceError("setting ListenerPosition: " + listenerPosition.x.ToString() + ", " +
            //    listenerPosition.y.ToString() + ", " + listenerPosition.z.ToString()); 

            result = fmod.set3DListenerAttributes(0, ref listenerPosition, ref listenerVelocity, ref listenerForward, ref listenerUp);
            CheckRetCode(result);

            result = fmod.update();
            LogResults(result);
        }

        ///<summary>
        ///    Return an array of strings which are the driver names for the "microphone" devices.
        ///</summary>
        public string[] GetAllMicrophoneDevices(FMOD.System fmod) {
            FMOD.RESULT result;
            int numSoundSources = 0;
            StringBuilder drivername = new StringBuilder(256);

//             result = system.setDriver(selected);
//             ERRCHECK(result);

            // Get Record drivers 
            result = fmod.getRecordNumDrivers(ref numSoundSources);
            CheckRetCode(result);

            string[] soundSourceNames = new string[numSoundSources];
            
            for (int count=0; count<numSoundSources; count++) {
				FMOD.GUID guid = new FMOD.GUID();
				result = fmod.getRecordDriverInfo(count, drivername, drivername.Capacity, ref guid);
                // result = fmod.getRecordDriverName(count, drivername, drivername.Capacity);
                CheckRetCode(result);
                soundSourceNames[count] = drivername.ToString();
            }
            return soundSourceNames;
        }
        
        ///<summary>
        ///    Return an array of strings which are the driver names for the playback devices.
        ///</summary>
        public string[] GetAllPlaybackDevices(FMOD.System fmod) {
            FMOD.RESULT result;
            int numPlaybackDevices = 0;
            StringBuilder drivername = new StringBuilder(256);

            // Get playback drivers 
            result = fmod.getNumDrivers(ref numPlaybackDevices);
            CheckRetCode(result);

            string[] playbackDeviceNames = new string[numPlaybackDevices];
            
            for (int count=0; count<numPlaybackDevices; count++) {
				FMOD.GUID guid = new FMOD.GUID();
				result = fmod.getDriverInfo(count, drivername, drivername.Capacity, ref guid);
                // result = fmod.getDriverName(count, drivername, drivername.Capacity);
                CheckRetCode(result);
                playbackDeviceNames[count] = drivername.ToString();
            }
            return playbackDeviceNames;
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

        public static void LogResults(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                log.ErrorFormat("FMOD result: {0}\n{1}", FMOD.Error.String(result), new StackTrace(true).ToString());
            }
        }

		#region Properties

        public Vector3 ListenerPosition
        {
            get
            {
                return new Vector3(listenerPosition.x, listenerPosition.y, listenerPosition.z);
            }
            set
            {
                listenerPosition.x = value.x;
                listenerPosition.y = value.y;
                listenerPosition.z = value.z;
            }
        }

        public Vector3 ListenerVelocity
        {
            get
            {
                return new Vector3(listenerVelocity.x, listenerVelocity.y, listenerVelocity.z);
            }
            set
            {
                listenerVelocity.x = value.x;
                listenerVelocity.y = value.y;
                listenerVelocity.z = value.z;
            }
        }

        public Vector3 ListenerForward
        {
            get
            {
                return new Vector3(listenerForward.x, listenerForward.y, listenerForward.z);
            }
            set
            {
                listenerForward.x = value.x;
                listenerForward.y = value.y;
                listenerForward.z = value.z;
            }
        }

        public Vector3 ListenerUp
        {
            get
            {
                return new Vector3(listenerUp.x, listenerUp.y, listenerUp.z);
            }
            set
            {
                listenerUp.x = value.x;
                listenerUp.y = value.y;
                listenerUp.z = value.z;
            }
        }
        
		#endregion Properties

    }
}

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

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

using log4net;

using Axiom.Animating;

using Multiverse.Interface; // for UIScripting
using Multiverse.AssetRepository;
using Multiverse.Utility; // for TimeTool
using Multiverse.Lib.LogUtil;

namespace Multiverse.Base
{
    public delegate void WorldInitializedHandler(object sender, EventArgs e);
    public delegate void FrameStartedHandler(object sender, ScriptingFrameEventArgs e);
    public delegate void FrameEndedHandler(object sender, ScriptingFrameEventArgs e);

    public class ScriptingFrameEventArgs : EventArgs
    {
        protected float time;

        public ScriptingFrameEventArgs(float time)
            : base()
        {
            this.time = time;
        }

        public float TimeSinceLastFrame
        {
            get
            {
                return time;
            }
        }
    }

    public class ClientAPI
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ClientAPI));
        private static readonly log4net.ILog deprecatedLog = log4net.LogManager.GetLogger("ScriptDeprecated");

        protected static IGameWorld betaWorld;
        protected static bool initialized;
        protected static SortedList<long, List<object>> effectWorkQueue;
        protected static YieldEffectHandler yieldHandler;
        protected static Dictionary<AnimationState, AnimationStateInfo> playingAnimations;

        public static event WorldInitializedHandler WorldInitialized;
        public static event FrameStartedHandler FrameStarted;
        public static event FrameEndedHandler FrameEnded;
        public static event EventHandler WorldConnect;
        public static event EventHandler WorldDisconnect;

        static ClientAPI()
        {
            effectWorkQueue = new SortedList<long, List<object>>();
            playingAnimations = new Dictionary<AnimationState, AnimationStateInfo>();
        }

        public static void OnWorldInitialized() {
            WorldInitializedHandler handler = WorldInitialized;
            if (handler != null) {
                handler(null, new EventArgs());
            }
        }

        public static void OnWorldConnect() {
            if (WorldConnect != null)
                WorldConnect(null, new EventArgs());
        }
        public static void OnWorldDisconnect() {
            if (WorldDisconnect != null)
                WorldDisconnect(null, new EventArgs());
        }

        public static void OnFrameStarted(float time)
        {
            FrameStartedHandler handler = FrameStarted;
            if (handler != null)
            {
                handler(null, new ScriptingFrameEventArgs(time));
            }
        }

        public static void OnFrameEnded(float time)
        {
            FrameEndedHandler handler = FrameEnded;
            if (handler != null)
            {
                handler(null, new ScriptingFrameEventArgs(time));
            }
            //if (triggerWorldInitialized &&
                //betaWorld.WorldManager.SceneManager.CurrentViewport != null)
            if (triggerWorldInitialized) {
                OnWorldInitialized();
                triggerWorldInitialized = false;
            }
        }

        public static void InitAPI(IGameWorld gameWorld)
        {
            string scriptPath = null;
            // add to the path so that imports from ClientAPI will be found
            if (System.IO.Directory.Exists("..\\Scripts"))
            {
                scriptPath = "../Scripts/";
            }
            else
            {
                scriptPath = "../../Scripts/";
            }

            UiScripting.AddPath(scriptPath);

            log.InfoFormat("API Script Path: {0}", scriptPath);

            foreach (string dir in RepositoryClass.Instance.RepositoryDirectoryList) {
                string scriptRepository;
                scriptRepository = string.Format("{0}/Scripts/", dir);
                if (Directory.Exists(scriptRepository))
                {
                    UiScripting.AddPath(scriptRepository);
                    log.InfoFormat("World Script Path: {0}", scriptRepository);
                }
                scriptRepository = string.Format("{0}/IPCE/", dir);
                if (Directory.Exists(scriptRepository))
                {
                    UiScripting.AddPath(scriptRepository);
                    log.InfoFormat("World Script Path: {0}", scriptRepository);
                }
            }

            // Create a dictionary to initialize globals for the ClientAPI module
            Dictionary<string, object> globals = new Dictionary<string, object>();

            // Load the ClientAPI python code
            UiScripting.RunModule(scriptPath, "ClientAPI.py", "ClientAPI", true, globals);

            // Run world startup script
            if (!UiScripting.RunFile("Startup.py"))
                throw new PrettyClientException("bad_script.htm", "Unable to run startup scripts");

            yieldHandler = UiScripting.SetupDelegate<YieldEffectHandler>("return generator.next()", null);
            betaWorld = gameWorld;
            initialized = true;
        }

        public delegate int YieldEffectHandler(object generator);

        public static void QueueYieldEffect(object generator, int milliseconds)
        {
            long time = milliseconds + TimeTool.CurrentTime;
            List<object> timeList;

            if (effectWorkQueue.ContainsKey(time))
            {
                timeList = effectWorkQueue[time];
            }
            else
            {
                timeList = new List<object>();
                effectWorkQueue[time] = timeList;
            }
            timeList.Add(generator);
        }

        public static void ProcessYieldEffectQueue()
        {
            long currentTime = TimeTool.CurrentTime;

            // process any pending queue items
            while ((effectWorkQueue.Count > 0) && (effectWorkQueue.Keys[0] < currentTime))
            {
                // fetch the generator object
                List<object> generatorList = effectWorkQueue.Values[0];

                // remove it from the queue
                effectWorkQueue.RemoveAt(0);

                foreach (object generator in generatorList)
                {
                    int nextWait = 0;
                    bool done = false;

                    try
                    {
                        // call the python yield handler code with the generator object, which does
                        // the next slice of work on the effect.
                        nextWait = yieldHandler(generator);
                    }
                    catch (IronPython.Runtime.Exceptions.StopIterationException)
                    {
                        done = true;
                    }
                    catch (Exception ex)
                    {
                        string pystack = UiScripting.FormatException(ex);
                        LogUtil.ExceptionLog.ErrorFormat("Exception in yield effect handler.  Python Stack Trace: {0}\n.Full Stack Trace: {1}", pystack, ex);
                        log.Error("Cancelling effect execution.");
                        done = true;
                    }

                    if (!done)
                    {
                        // add it back to the queue at the next scheduled time offset
                        QueueYieldEffect(generator, nextWait);
                    }
                }
            }
        }

        public static AnimationStateInfo PlaySceneAnimation(AnimationState state, float speed, bool looping)
        {
            if (playingAnimations.ContainsKey(state))
            {
                log.ErrorFormat("Attempted to play an already playing animation: {0}", state.Name);
                return null;
            }

            AnimationStateInfo stateInfo = new AnimationStateInfo(state, speed, looping);

            playingAnimations[state] = stateInfo;
            state.Time = 0;

            return stateInfo;
        }

        public static void StopSceneAnimation(AnimationState state)
        {
            if (!playingAnimations.ContainsKey(state))
            {
                log.InfoFormat("Attempted to stop an animation that is not playing: {0}", state.Name);
                return;
            }

            playingAnimations.Remove(state);

            return;
        }

        public static void ProcessSceneAnimations(float timeSinceLastFrame)
        {
            if (playingAnimations.Count > 0)
            {
                List<AnimationState> removals = new List<AnimationState>();

                foreach (AnimationStateInfo info in playingAnimations.Values)
                {
                    float overflow = 0f;
                    if (info.AddTime(timeSinceLastFrame, out overflow))
                        removals.Add(info.State);
                }

                // remove any animations that have finished
                foreach (AnimationState state in removals)
                {
                    playingAnimations.Remove(state);
                }
            }
        }

        protected static bool triggerWorldInitialized;
        public static bool TriggerWorldInitialized
        {
            get
            {
                return triggerWorldInitialized;
            }
            set
            {
                triggerWorldInitialized = true;
            }
        }
        public static log4net.ILog Log {
            get { return log; }
        }

        protected static Dictionary<string, object> deprecatedCalls = new Dictionary<string, object>();

        public static void ScriptDeprecated(string version, string oldMethod, string newMethod)
        {
            StackTrace t = new StackTrace(true);
            StackFrame f = null;
            int i = 0;
            bool foundPython = false;

            // look for the first stack frame that appears to be in a python script
            for (; i < t.FrameCount; i++)
            {
                f = t.GetFrame(i);
                string filename = f.GetFileName();
                if ((filename != null) && filename.ToLowerInvariant().EndsWith(".py"))
                {
                    foundPython = true;
                    break;
                }
            }

            if (foundPython)
            {
                // generate a string that uniquely identifies a call to a deprecated interface
                //   from a particular line of script code.  We will use this string to avoid
                //   printing the same deprecated message over and over for the same line of code.
                string instanceID = oldMethod + f.GetFileName() + f.GetFileLineNumber().ToString();

                if (!deprecatedCalls.ContainsKey(instanceID))
                {
                    // mark the call instance ID in the 
                    deprecatedCalls[instanceID] = null;

                    deprecatedLog.WarnFormat("DEPRECATED:{0}: {1} in the Client API should no longer be used.  You should replace it with {2}", version, oldMethod, newMethod);
                    deprecatedLog.WarnFormat("  {0} is called:", oldMethod);

                    // continue from the index where the previous loop found a python file
                    for (; i < t.FrameCount; i++)
                    {
                        f = t.GetFrame(i);
                        string filename = f.GetFileName();
                        if ((filename != null) && filename.ToLowerInvariant().EndsWith(".py"))
                        {
                            deprecatedLog.WarnFormat("    at {0} in {1}: line {2}", f.GetMethod().Name, filename, f.GetFileLineNumber());
                        }
                    }
                }
            }
        }
    }
}

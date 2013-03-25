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

using Multiverse.Config;

namespace Multiverse.Test
{
	public class BehaviorParms
	{
		public int MaxMessagesPerFrame = 10;    // The number of messages handled per frame
		public long DirUpdateInterval = 100;    // time in ticks between direction updates to the server
		public long OrientUpdateInterval = 100; // time in ticks between orientation updates to the server
		public int moveInterval = 100;          // Milliseconds between moves or rotates
		public int moveMaximum = 500;           // The maximum distance we'll move from the starting point, in meters
		public float forwardFraction = .5f;     // Move forward 50% of the time
		public float backFraction = .2f;        // Move back 20% of the time
		public float rotateFraction = .05f;     // Rotate 5% of the time 
		public float sideFraction = .25f;       // Move to one side or another 25% of the time
		public float playerSpeed = 7.0f * Client.OneMeter; // The speed at which the player moves: 7 m/s
		public float rotateSpeed = 90.0f;       // Rotate speed in degree when user hits a rotate key
        public int playerUpdateInterval = 5000; // Send player data every 5 seconds even if it hasn't changed
		public int maxActiveTime = 5000;        // The player is active for a random time between 0 and 5 seconds
		public int maxIdleTime = 15000;         // The player is idle for a random time between 0 and 15 seconds
	}
	
    class Program
    {
        public static string WorldSettingsFile = "world_settings.xml";

        static void Main(string[] args)
        {
			bool verifyServer = false;
			BehaviorParms behaviorParms = new BehaviorParms();
			for (int i = 0; i < args.Length; ++i) {
				switch (args[i]) {
				case "--verify_server":
					verifyServer = true;
					break;
				case "--log_level":
					Debug.Assert(i + 1 < args.Length);
					// Logger.LogLevel = int.Parse(args[++i]);
					break;
				case "--max_messages_per_frame":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.MaxMessagesPerFrame = int.Parse(args[++i]);
					break;
				case "--dir_update_interval":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.DirUpdateInterval = int.Parse(args[++i]);
					break;
				case "--orient_update_interval":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.OrientUpdateInterval = int.Parse(args[++i]);
					break;
				case "--move_interval":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.moveInterval = int.Parse(args[++i]);
					break;
				case "--move_maximum":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.moveMaximum = int.Parse(args[++i]);
					break;
				case "--forward_fraction":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.forwardFraction = float.Parse(args[++i]);
					break;
				case "--back_fraction":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.backFraction = float.Parse(args[++i]);
					break;
				case "--rotate_fraction":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.rotateFraction = float.Parse(args[++i]);
					break;
				case "--side_fraction":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.sideFraction = float.Parse(args[++i]);
					break;
				case "--player_speed":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.playerSpeed = float.Parse(args[++i]);
					break;
				case "--rotate_speed":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.rotateSpeed = float.Parse(args[++i]);
					break;
				case "--player_update_interval":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.playerUpdateInterval = int.Parse(args[++i]);
					break;
				case "--max_active_time":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.maxActiveTime = int.Parse(args[++i]);
					break;
				case "--max_idle_time":
					Debug.Assert(i + 1 < args.Length);
					behaviorParms.maxIdleTime = int.Parse(args[++i]);
					break;
				default:
					break;
				}
			}
			Client client = new Client(verifyServer, behaviorParms);
			client.SourceConfig("../" + WorldSettingsFile);
			Trace.TraceInformation("Starting test client");
			try {
				client.Start();
			}
			catch (Exception e) {
				// We get an error, so return an error code of -1
				Trace.TraceInformation(string.Format("TestClient got error '{0}'",
													 e.Message));
				Trace.Flush();
				System.Environment.Exit(-1);
			}
			Trace.TraceInformation("Exiting test client");
        }
    }
}

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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using log4net;

#endregion

namespace Multiverse.Utility
{
    public class DebugMonitor
    {
        static Dictionary<object, StackTrace> lockHolders =
            new Dictionary<object, StackTrace>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DebugMonitor));

		public DebugMonitor() {
		}

		public static void Enter(object lockable) {
			if (!System.Threading.Monitor.TryEnter(lockable, 3000)) {
				try {
					System.Threading.Monitor.Enter(lockHolders);
                    log.WarnFormat("Failed to acquire lock on {0} from {1}", lockable.GetHashCode(), new StackTrace(true));
                    foreach (object key in lockHolders.Keys)
                        log.InfoFormat("Lock on {0} held by: {1}", key, lockHolders[key]);
                    throw new Exception("ACK!");
                } finally {
					System.Threading.Monitor.Exit(lockHolders);
				}
                
            }
            try {
				System.Threading.Monitor.Enter(lockHolders);
				lockHolders[lockable.GetHashCode()] = new StackTrace(true);
            } finally {
				System.Threading.Monitor.Exit(lockHolders);
			}
        }
        public static void Exit(object lockable) {
            try {
				System.Threading.Monitor.Enter(lockHolders);
				lockHolders.Remove(lockable);
            } finally {
				System.Threading.Monitor.Exit(lockHolders);
			}
			System.Threading.Monitor.Exit(lockable);
		}
        public static void Wait(object lockable) {
            try {
				System.Threading.Monitor.Enter(lockHolders);
				lockHolders.Remove(lockable);
            } finally {
				System.Threading.Monitor.Exit(lockHolders);
			}
			System.Threading.Monitor.Wait(lockable);
			try {
				System.Threading.Monitor.Enter(lockHolders);
				lockHolders[lockable] = new StackTrace(true);
            } finally {
				System.Threading.Monitor.Exit(lockHolders);
			}
        }
        public static void PulseAll(object lockable) {
			System.Threading.Monitor.PulseAll(lockable);
		}
        public static void Pulse(object lockable) {
			System.Threading.Monitor.Pulse(lockable);
		}
    }
}

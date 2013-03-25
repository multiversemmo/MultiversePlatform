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
using System.Threading;
using System.Diagnostics;

#if DEBUG_THREADS
using Monitor = Multiverse.Utility.DebugMonitor;
#endif

namespace Multiverse.Utility
{
/// <summary>
///   ReadWriteLock that supports locking on reads and writes and does not
///   starve writers (though it will starve readers)
/// </summary>
	public class ReadWriteLock
	{
		int readerCount = 0;
		int writerCount = 0;
		int writersWaiting = 0;
#if DEBUG_THREADS
		StackTrace writeLockHolder = null;
#endif

		public ReadWriteLock() {
		}

		public void BeginRead() {
            Monitor.Enter(this);
			while (writerCount != 0 || writersWaiting != 0)
                Monitor.Wait(this);
			readerCount++;
            Monitor.Exit(this);
		}
		public void EndRead() {
            Monitor.Enter(this);
			readerCount--;
            Monitor.PulseAll(this);
            Monitor.Exit(this);
		}
		public void BeginWrite() {
            Monitor.Enter(this);
			writersWaiting++;
			while (readerCount != 0 || writerCount != 0)
                Monitor.Wait(this);
			writersWaiting--;
			writerCount++;
#if DEBUG_THREADS
			writeLockHolder = new StackTrace(true);
#endif
            Monitor.Exit(this);
		}
		public void EndWrite() {
            Monitor.Enter(this);
			writerCount--;
#if DEBUG_THREADS
			writeLockHolder = null;
#endif
            Monitor.PulseAll(this);
            Monitor.Exit(this);
		}
	}
}

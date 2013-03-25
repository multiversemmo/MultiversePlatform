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

namespace Multiverse.Utility {
	public class TimeTool {
        /// <summary>
        ///   Used to override the client's idea of time, for use in
        ///   some automation tasks.
        /// </summary>
        protected static long timeOverride = 0;
        /// <summary>
        ///   Keep track of the last timestamp, so that we can have
        ///   monotonically non-decreasing timestamps
        /// </summary>
        protected static long lastTimestamp = 0;
        /// <summary>
        ///   Stash this number here, so we don't have to recalculate it
        /// </summary>
        protected const long intRange = (long)int.MaxValue - (long)int.MinValue + 1;

        public static long TimeOverride
        {
            get
            {
                return timeOverride;
            }
            set
            {
                timeOverride = value;
            }
        }

		public static long CurrentTime {
			get {
                long timestamp;
                if (timeOverride == 0)
                {
                    timestamp = System.Environment.TickCount;
                    while (timestamp < lastTimestamp)
                    {
                        timestamp += intRange;
                    }
                    lastTimestamp = timestamp;
                }
                else
                {
                    timestamp = timeOverride;
                }
				return timestamp;
			}
		}
	}
}

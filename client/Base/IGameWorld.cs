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

namespace Multiverse.Base {
    /// <summary>
    /// TargetChanged event for scripting API
    /// </summary>
    /// <param name="target">the new target</param>
    public delegate void TargetChangedHandler(ObjectNode target);

	public interface IGameWorld {
        void Initialize();

		void SetupMessageHandlers();

		// void TargetUnit(string selection);
		// void TargetMobHelper(ObjectNode last, bool reverse, bool onlyAttackable);

        /// <summary>
        ///   This is used so that the client can send messages to the user
        ///   in whatever system the given game world uses.
        ///
        ///   This is the equivalent of the ClientAPI.Write, but is more 
        ///   easily available to the C# code.
        /// </summary>
        /// <param name="msg"></param>
        void Write(string msg);

        /// <summary>
        ///   Get or set the WorldManager object that keeps track of objects 
        ///   in the world.
        /// </summary>
		WorldManager WorldManager {
			get;
			set;
		}
    }
}

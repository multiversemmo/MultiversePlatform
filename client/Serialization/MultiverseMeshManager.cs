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

using Axiom.Core;
using Axiom.Animating;

namespace Multiverse.Serialization {
	public class MultiverseMeshManager : MeshManager {
		protected MultiverseMeshManager() { }

		// Our version of Init that will replace the static instance reference
		// to an Axiom MeshManager with our own.
		public static void Init() {
			instance = new MultiverseMeshManager();
			instance.Initialize();
			// GarbageManager.Instance.Add(instance);
		}

		public override Resource Create(string name, bool isManual) {
			return new MultiverseMesh(name);
		}
	}

	public class MultiverseSkeletonManager : SkeletonManager {
		protected MultiverseSkeletonManager() { }

		// Our version of Init that will replace the static instance reference
		// to an Axiom MeshManager with our own.
		public static void Init() {
			instance = new MultiverseSkeletonManager();
			// GarbageManager.Instance.Add(instance);
		}

		public override Resource Create(string name, bool isManual) {
			return new MultiverseSkeleton(name);
		}
	}
}

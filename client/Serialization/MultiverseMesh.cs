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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Axiom.Core;
using Axiom.Animating;
using Axiom.Collections;
using Axiom.Configuration;
using Axiom.MathLib;
using Axiom.Serialization;
using Axiom.Graphics;

namespace Multiverse.Serialization {
	public class MultiverseMesh : Mesh {

		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public MultiverseMesh(string name)
			: base(name) {
		}

		#endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MultiverseMesh));

		#region Implementation of Resource

		/// <summary>
		///		Loads the mesh data.
		/// </summary>
		protected override void LoadImpl() {
            // meshLoadMeter.Enter();

			// load this bad boy if it is not to be manually defined
			if (!isManual) {
				// get the resource data from MeshManager
				Stream data = MeshManager.Instance.FindResourceData(name);
				string extension = Path.GetExtension(name);

				// mesh loading stats
				int before, after;

				// get the tick count before loading the mesh
				before = Environment.TickCount;

				if (extension == ".mesh") {
					// instantiate a mesh reader and pass in the stream data
					MeshSerializer meshReader = new MeshSerializer();
					// import the .mesh file
					meshReader.ImportMesh(data, this);
				} else if (extension == ".xml") {
					OgreXmlMeshReader meshReader = new OgreXmlMeshReader(data);
					// import the .xml file
					meshReader.Import(this);
				} else if (extension == ".dae") {
					ColladaMeshReader meshReader = new ColladaMeshReader(data, "tmp");
					// import the .dae file
					meshReader.Import(this);
				} else {
					data.Close();
					throw new AxiomException("Unsupported mesh format '{0}'", extension);
				}

				// get the tick count after loading the mesh
				after = Environment.TickCount;

				// record the time elapsed while loading the mesh
				log.InfoFormat("Mesh: Loaded '{0}', took {1}ms", this.name, (after - before));

				// close the stream (we don't need to leave it open here)
				data.Close();
			}

			// prepare the mesh for a shadow volume?
			if (MeshManager.Instance.PrepareAllMeshesForShadowVolumes) {
                if (edgeListsBuilt || autoBuildEdgeLists) {
                    PrepareForShadowVolume();
                }
                if (!edgeListsBuilt && autoBuildEdgeLists) {
                    BuildEdgeList();
                }
            }
            // meshLoadMeter.Exit();
		}
			
		#endregion
	}

	public class MultiverseSkeleton : Skeleton {
		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public MultiverseSkeleton(string name)
			: base(name) {
		}

		#endregion


		#region Implementation of Resource

		/// <summary>
		///		Loads the mesh data.
		/// </summary>
		/// <summary>
		///    Generic load, called by SkeletonManager.
		/// </summary>
		public override void Load() {
			if (isLoaded) {
				Unload();
				isLoaded = false;
			}

            skeletonLoadMeter.Enter();

			LogManager.Instance.Write("Skeleton: Loading '{0}'...", name);

			// load the skeleton file
			Stream data = SkeletonManager.Instance.FindResourceData(name);


			string extension = Path.GetExtension(name);

			if (extension == ".skeleton") {
				// instantiate a new skeleton reader
                OgreSkeletonSerializer reader = new OgreSkeletonSerializer();
				reader.ImportSkeleton(data, this);
			} else if (extension == ".xml") {
				// instantiate a new skeleton reader
				OgreXmlSkeletonReader reader = new OgreXmlSkeletonReader(data);
				reader.Import(this);
			} else {
				data.Close();
				throw new Exception("Unsupported skeleton file format '" + extension + "'");
			}
			data.Close();

			isLoaded = true;
            
            skeletonLoadMeter.Exit();
		}

		#endregion
	}
}

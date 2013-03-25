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
using System.Xml;
using System.Diagnostics;
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{

    public delegate void SubMeshChangeEventHandler(object sender, EventArgs args);

	public class SubMeshInfo
	{
		private string name;
		private string materialName;
		private bool show;
        private SubMeshCollection parent;

        public SubMeshInfo(SubMeshCollection parent, string name, string materialName, bool show)
        {
            this.parent = parent;
            this.name = name;
            this.materialName = materialName;
            this.show = show;
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                parent.OnChange();
            }
        }

        public string MaterialName
        {
            get
            {
                return materialName;
            }
            set
            {
                materialName = value;
                parent.OnChange();
            }
        }

        public bool Show
        {
            get
            {
                return show;
            }
            set
            {
                show = value;
                parent.OnChange();
            }
        }
	}

	public class SubMeshCollection : IEnumerable<SubMeshInfo>
	{
		private List<SubMeshInfo> subMeshList;

        public event SubMeshChangeEventHandler Changed;

        /// <summary>
        /// Standard Constructor
        /// </summary>
        /// <param name="meshName"></param>
		public SubMeshCollection(string meshName)
		{
			subMeshList = buildSubMeshList(meshName);
		}

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="srcCollection"></param>
        public SubMeshCollection(SubMeshCollection srcCollection)
        {
            this.subMeshList = new List<SubMeshInfo>();
            foreach ( SubMeshInfo srcInfo in srcCollection )
            {
                SubMeshInfo subMeshInfo = new SubMeshInfo(this, srcInfo.Name, srcInfo.MaterialName, srcInfo.Show);
                this.subMeshList.Add(subMeshInfo);
            }
        }

		public SubMeshCollection(XmlReader r)
		{
            this.subMeshList = new List<SubMeshInfo>();
			while (r.Read())
			{
				// look for the start of an element
				if (r.NodeType == XmlNodeType.Whitespace)
				{
					continue;
				}
				if (r.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
				if (r.NodeType == XmlNodeType.Element)
				{
					if (String.Equals(r.Name, "SubMeshInfo"))
					{
				        subMeshList.Add(parseSubMeshInfo(r));
					}
				}
			}
		}

		private SubMeshInfo parseSubMeshInfo(XmlReader r)
		{
			string namein = "";
			string materialNamein= "";
			bool showin = false;
			for (int i = 0; i < r.AttributeCount; i++)
			{
				r.MoveToAttribute(i);
				switch (r.Name)
				{
					case "Name":
						namein = r.Value;
						break;
					case "MaterialName":
						materialNamein = r.Value;
						break;
					case "Show":
						if (String.Equals(r.Value, "True"))
						{
							showin = true;
						}
						else
						{
							showin = false;
						}
						break;
				}
			}
			SubMeshInfo info = new SubMeshInfo(this, namein, materialNamein, showin);
			r.MoveToElement();
			return info;
		}


		/// <summary>
		/// This method builds a list of submeshes, which is used by the application to
		/// control which submeshes are displayed.
		/// </summary>
		private List<SubMeshInfo> buildSubMeshList(string meshName)
		{
		    List<SubMeshInfo> list = new List<SubMeshInfo>();
			Mesh mesh = MeshManager.Instance.Load(meshName);
			for (int i = 0; i < mesh.SubMeshCount; i++)
			{
                SubMesh subMesh = mesh.GetSubMesh(i);
				SubMeshInfo subMeshInfo = new SubMeshInfo(this, subMesh.Name, subMesh.MaterialName, true);
				list.Add(subMeshInfo);
			}

            return list;
		}

        public bool CheckValid(WorldEditor app, string meshName)
        {
            // get the list from the mesh
            List<SubMeshInfo> listFromMesh;
            if ( app.CheckAssetFileExists(meshName) ) {
                listFromMesh = buildSubMeshList(meshName);
            }
            else
            {
                // if we can't find the mesh, then log it and say its ok, since we have no idea if the submeshes are right.
                app.AddMissingAsset(meshName);
                return true;
            }

            // counts differ, check fails
            if (subMeshList.Count != listFromMesh.Count)
            {
                return false;
            }

            bool foundSubMesh = false;

            foreach (SubMeshInfo info in subMeshList)
            {

                foundSubMesh = false;
                for (int i = 0; i < listFromMesh.Count; i++)
                {
                    SubMeshInfo infoFromMesh = listFromMesh[i];

                    if (infoFromMesh.Name == info.Name)
                    {
                        // name is the same, so we found it and its ok
                        foundSubMesh = true;

                        // remove the submesh info from consideration in future iterations
                        listFromMesh.Remove(infoFromMesh);
                        break;
                    }
                }

                // we didn't find the submesh, so the check fails
                if (!foundSubMesh)
                {
                    return false;
                }
            }

            // the temporary list should be empty at this point
            Debug.Assert(listFromMesh.Count == 0);

            // if we get this far, all the checks passed
            return true;
        }


        public void ToManifest(System.IO.StreamWriter w, string meshName)
        {
            // get the list from the mesh
            List<SubMeshInfo> listFromMesh = buildSubMeshList(meshName);

            // bool foundSubMesh = false;

            foreach (SubMeshInfo info in subMeshList)
            {

                // foundSubMesh = false;
                for (int i = 0; i < listFromMesh.Count; i++)
                {
                    SubMeshInfo infoFromMesh = listFromMesh[i];

                    if (infoFromMesh.Name == info.Name)
                    {
                        // name is the same, so we found it and its ok
                        // foundSubMesh = true;

                        if (infoFromMesh.MaterialName != info.MaterialName)
                        {
                            // material names differ.  output the material
                            w.WriteLine("Material:{0}", info.MaterialName);
                        }

                        // remove the submesh info from consideration in future iterations
                        listFromMesh.Remove(infoFromMesh);
                        break;
                    }
                }
            }

            // the temporary list should be empty at this point
            Debug.Assert(listFromMesh.Count == 0);
        }

        /// <summary>
        /// Raise an event when the submesh changes
        /// </summary>
        public void OnChange()
        {
            SubMeshChangeEventHandler e = Changed;
            if (e != null)
            {
                e(null, new EventArgs());
            }
        }

		/// <summary>
		/// Look up subMeshInfo by name in the SubMeshList
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public SubMeshInfo FindSubMeshInfo(string name)
		{
			foreach (SubMeshInfo subMeshInfo in subMeshList)
			{
				if (subMeshInfo.Name == name)
				{
					return subMeshInfo;
				}
			}
			return null;
		}

		/// <summary>
		/// Set the material for a subMesh
		/// </summary>
		/// <param name="name"></param>
		/// <param name="materialName"></param>
		public void SetSubMeshMaterial(string name, string materialName)
		{
			SubMeshInfo subMeshInfo = FindSubMeshInfo(name);
			if (subMeshInfo != null)
			{
				subMeshInfo.MaterialName = materialName;
			}
		}

		public void ShowSubMesh(string name, bool show)
		{
			SubMeshInfo subMeshInfo = FindSubMeshInfo(name);
			if (subMeshInfo != null)
			{
				subMeshInfo.Show = show;
			}
		}

        public SubMeshInfo this[int i]
        {
            get
            {
                return subMeshList[i];
            }
        }

		public void ToXml(XmlWriter w)
		{

			w.WriteStartElement("SubMeshes");
			foreach (SubMeshInfo info in subMeshList)
			{
				w.WriteStartElement("SubMeshInfo");
				w.WriteAttributeString("Name", info.Name);
				w.WriteAttributeString("MaterialName", info.MaterialName);
				w.WriteAttributeString("Show", info.Show.ToString());
				w.WriteEndElement();
			}
			w.WriteEndElement();
		}

		
        #region IEnumerable<SubMeshInfo> Members

        public IEnumerator<SubMeshInfo> GetEnumerator()
        {
            return subMeshList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return subMeshList.GetEnumerator();
        }

        #endregion
    }
}


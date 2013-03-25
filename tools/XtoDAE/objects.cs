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
using System.IO;
using System.Diagnostics;

namespace xtodae {
    public class ParseResult {
        static ParseResult instance = null;

        public List<object> objects;
        public Dictionary<string, object> objectsByName;
        public Dictionary<string, object> objectsByUuid;
        Geometry geometry = new Geometry();

        private void ProcessFrame(Frame f) {
            foreach (object subObj in f.options) {
                if (subObj is FrameTransformMatrix) {
                    FrameTransformMatrix tmp = (FrameTransformMatrix)subObj;
                    Debug.Assert(tmp.frameMatrix.IsIdentity);
                } else if (subObj is Mesh) {
                    geometry.AddMesh((Mesh)subObj);
                } else if (subObj is Frame) {
                    ProcessFrame((Frame)subObj);
                } else {
                    Console.WriteLine("Unexpected type: " + subObj);
                }
            }
        }

        public void Process()
        {
            foreach (object obj in objects)
            {
                if (obj is Frame) {
                    ProcessFrame((Frame)obj);
                } else if (obj is Material) {
                    // continue;
                } else if (obj is Header) {
                    // continue;
                } else {
                    Console.WriteLine("Unexpected type: " + obj);
                }
            }
        }

        public void Export(string outFile, string imagePrefix, string name, string units)
        {
            if (imagePrefix == null)
                imagePrefix = "";
            float unitFactor = 0.0254f;
            if (units != null)
                unitFactor = float.Parse(units);
            if (name == null) {
                name = outFile;
                if (name.Contains("/")) {
                    string[] parts = name.Split('/');
                    name = parts[parts.Length - 1];
                }
                if (name.Contains("\\")) {
                    string[] parts = name.Split('\\');
                    name = parts[parts.Length - 1];
                }
                if (name.Contains("."))
                    name = name.Split('.')[0];
            }

            FileStream stream = new FileStream(outFile, FileMode.Create);
            TextWriter writer = new StreamWriter(stream);

            // Build a mesh index for each material
            Dictionary<Material, int> meshIndexDict = new Dictionary<Material, int>();
            foreach (Material material in geometry.indexSets.Keys) {
                int meshIndex = meshIndexDict.Count;
                meshIndexDict[material] = meshIndex;
            }
            
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            writer.WriteLine("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\">");
            writer.WriteLine("  <asset>");
            writer.WriteLine("    <unit meter=\"{0}\"/>", unitFactor);
            writer.WriteLine("    <up_axis>Y_UP</up_axis>");
            writer.WriteLine("  </asset>");

            writer.WriteLine("  <library_images>");
            foreach (Material material in geometry.indexSets.Keys) {
                TextureFilename texture = material.GetTextureFilename();
                string textureName = texture.filename.Trim('"');
                string[] parts = textureName.Split('.');
                string ext = "jpg";
                if (parts.Length == 2 && parts[1].Length != 0)
                    ext = parts[1];
                textureName = parts[0];
                writer.WriteLine("    <image id=\"{0}\" name=\"{0}\">", textureName);
                writer.WriteLine("      <init_from>{0}{1}.{2}</init_from>", imagePrefix, textureName, ext);
                writer.WriteLine("    </image>");
            }
            writer.WriteLine("  </library_images>");

            writer.WriteLine("  <library_effects>");
            foreach (Material material in geometry.indexSets.Keys) {
                TextureFilename texture = material.GetTextureFilename();
                string textureName = texture.filename.Trim('"');
                textureName = textureName.Split('.')[0];
                int meshIndex = meshIndexDict[material];
                writer.WriteLine("    <effect id=\"{0}-fx\" name=\"{0}\">", material.name);
                writer.WriteLine("      <profile_COMMON>");
                writer.WriteLine("        <technique sid=\"standard\">");
                writer.WriteLine("          <phong>");
                writer.WriteLine("            <emission>");
                writer.WriteLine("              <color sid=\"emission\">{0} {1} {2} {3}</color>", 1, 1, 1, 1);
                writer.WriteLine("            </emission>");
                writer.WriteLine("            <ambient>");
                writer.WriteLine("              <color sid=\"ambient\">{0} {1} {2} {3}</color>", 1, 1, 1, 1);
                writer.WriteLine("            </ambient>");
                writer.WriteLine("            <diffuse>");
                writer.WriteLine("              <texture texture=\"{0}\" texcoord=\"CHANNEL0\">", textureName);
                writer.WriteLine("                <extra>");
                writer.WriteLine("                  <technique profile=\"MAYA\">");
                writer.WriteLine("                    <wrapU sid=\"wrapU0\">TRUE</wrapU>");
                writer.WriteLine("                    <wrapV sid=\"wrapV0\">TRUE</wrapV>");
                writer.WriteLine("                    <blend_mode>ADD</blend_mode>");
                writer.WriteLine("                  </technique>");
                writer.WriteLine("                </extra>");
                writer.WriteLine("              </texture>");
                writer.WriteLine("            </diffuse>");
                writer.WriteLine("            <specular>");
                writer.WriteLine("              <color sid=\"specular\">{0} {1} {2} {3}</color>", 0, 0, 0, 1);
                // writer.WriteLine("              <color sid=\"specular\">{0} {1} {2} {3}</color>", material.specularColor.red, material.specularColor.green, material.specularColor.blue, 1);
                writer.WriteLine("            </specular>");
                writer.WriteLine("            <shininess>");
                writer.WriteLine("              <float sid=\"shininess\">2</float>");
                writer.WriteLine("            </shininess>");
                writer.WriteLine("            <reflective>");
                writer.WriteLine("              <color sid=\"reflective\">{0} {1} {2} {3}</color>", 1, 1, 1, 1);
                writer.WriteLine("            </reflective>");
                writer.WriteLine("            <reflectivity>");
                writer.WriteLine("              <float sid=\"reflectivity\">0</float>");
                writer.WriteLine("            </reflectivity>");
                writer.WriteLine("            <transparent>");
                writer.WriteLine("              <color sid=\"transparent\">{0} {1} {2} {3}</color>", 1, 1, 1, 1);
                writer.WriteLine("            </transparent>");
                writer.WriteLine("            <transparency>");
                writer.WriteLine("              <float sid=\"transparency\">0</float>");
                writer.WriteLine("            </transparency>");
                writer.WriteLine("          </phong>");
                writer.WriteLine("        </technique>");
                writer.WriteLine("      </profile_COMMON>");
                writer.WriteLine("    </effect>");
            }
            writer.WriteLine("  </library_effects>");

            writer.WriteLine("  <library_materials>");
            foreach (Material material in geometry.indexSets.Keys) {
                writer.WriteLine("    <material id=\"{0}\" name=\"{0}\">", material.name);
                writer.WriteLine("      <instance_effect url=\"#{0}-fx\"/>", material.name);
                writer.WriteLine("    </material>");
            }
            writer.WriteLine("  </library_materials>");

            writer.WriteLine("  <library_geometries>");
            foreach (Material material in geometry.indexSets.Keys) {
                List<List<int>> indexes = geometry.indexSets[material];
                List<VertexEntry> vertices = geometry.vertexSets[material];
                int meshIndex = meshIndexDict[material]; 
                string meshName = string.Format("{0}-lib.{1}", name, meshIndex);
                writer.WriteLine("    <geometry id=\"{0}\" name=\"{0}\">", meshName, meshIndex);
                writer.WriteLine("      <mesh>");
                writer.WriteLine("        <source id=\"{0}-positions\" name=\"position\">", meshName);
                writer.Write("          <float_array id=\"{0}-positions-array\" count=\"{1}\">", meshName, vertices.Count * 3);
                foreach (VertexEntry entry in vertices)
                    writer.Write("{0} {1} {2} ", entry.position.x, entry.position.y, -1 * entry.position.z);
                writer.WriteLine("</float_array>");
                writer.WriteLine("          <technique_common>");
                writer.WriteLine("            <accessor source=\"#{0}-positions-array\" count=\"{1}\" stride=\"3\">", meshName, vertices.Count);
                writer.WriteLine("              <param name=\"X\" type=\"float\"/>");
                writer.WriteLine("              <param name=\"Y\" type=\"float\"/>");
                writer.WriteLine("              <param name=\"Z\" type=\"float\"/>");
                writer.WriteLine("            </accessor>");
                writer.WriteLine("          </technique_common>");
                writer.WriteLine("        </source>");
                writer.WriteLine("        <source id=\"{0}-normals\" name=\"normal\">", meshName);
                writer.Write("          <float_array id=\"{0}-normals-array\" count=\"{1}\">", meshName, vertices.Count * 3);
                foreach (VertexEntry entry in vertices)
                    writer.Write("{0} {1} {2} ", entry.normal.x, entry.normal.y, -1 * entry.normal.z);
                writer.WriteLine("</float_array>");
                writer.WriteLine("          <technique_common>");
                writer.WriteLine("            <accessor source=\"#{0}-normals-array\" count=\"{1}\" stride=\"3\">", meshName, vertices.Count);
                writer.WriteLine("              <param name=\"X\" type=\"float\"/>");
                writer.WriteLine("              <param name=\"Y\" type=\"float\"/>");
                writer.WriteLine("              <param name=\"Z\" type=\"float\"/>");
                writer.WriteLine("            </accessor>");
                writer.WriteLine("          </technique_common>");
                writer.WriteLine("        </source>");
                writer.WriteLine("        <source id=\"{0}-map1\" name=\"map1\">", meshName);
                writer.Write("          <float_array id=\"{0}-map1-array\" count=\"{1}\">", meshName, vertices.Count * 2);
                foreach (VertexEntry entry in vertices)
                    writer.Write("{0} {1} ", entry.texCoords.u, -1 * entry.texCoords.v);
                writer.WriteLine("</float_array>");
                writer.WriteLine("          <technique_common>");
                writer.WriteLine("            <accessor source=\"#{0}-map1-array\" count=\"{1}\" stride=\"2\">", meshName, vertices.Count);
                writer.WriteLine("              <param name=\"S\" type=\"float\"/>");
                writer.WriteLine("              <param name=\"T\" type=\"float\"/>");
                writer.WriteLine("            </accessor>");
                writer.WriteLine("          </technique_common>");
                writer.WriteLine("        </source>");
                writer.WriteLine("        <vertices id=\"{0}-vertices\">", meshName);
                writer.WriteLine("          <input semantic=\"POSITION\" source=\"#{0}-positions\"/>", meshName);
                writer.WriteLine("        </vertices>");
                writer.WriteLine("        <polygons material=\"{0}\" count=\"{1}\">", material.name, indexes.Count);
                writer.WriteLine("          <input semantic=\"VERTEX\" source=\"#{0}-vertices\" offset=\"0\"/>", meshName);
                writer.WriteLine("          <input semantic=\"NORMAL\" source=\"#{0}-normals\" offset=\"1\"/>", meshName);
                writer.WriteLine("          <input semantic=\"TEXCOORD\" source=\"#{0}-map1\" offset=\"2\" set=\"0\"/>", meshName);
                foreach (List<int> poly in indexes) {
                    writer.Write("          <p>");
                    // Since the z is inverted for directx, the winding order of 
                    // polygons needs to be inverted as well
                    List<int> tmp = new List<int>(poly);
                    tmp.Reverse();
                    foreach (int i in tmp)
                        writer.Write("{0} {0} {0} ", i);
                    writer.WriteLine("</p>");
                }
                writer.WriteLine("        </polygons>");
                writer.WriteLine("      </mesh>");
                writer.WriteLine("    </geometry>");
            }
            writer.WriteLine("  </library_geometries>");

            writer.WriteLine("  <library_visual_scenes>");
            writer.WriteLine("    <visual_scene id=\"DefaultScene\">");
            foreach (Material material in geometry.indexSets.Keys) {
                int meshIndex = meshIndexDict[material];
                string nodeName = string.Format("{0}.{1}", name, meshIndex);
                string meshName = string.Format("{0}-lib.{1}", name, meshIndex);
                TextureFilename texture = material.GetTextureFilename();
                string textureName = texture.filename.Trim('"');
                textureName = textureName.Split('.')[0];
                writer.WriteLine("      <node id=\"{0}\">", nodeName);
                writer.WriteLine("        <instance_geometry url=\"#{0}\">", meshName);
                writer.WriteLine("          <bind_material>");
                writer.WriteLine("            <technique_common>");
                writer.WriteLine("              <instance_material symbol=\"{0}\" target=\"#{0}\"/>", textureName);
                writer.WriteLine("            </technique_common>");
                writer.WriteLine("          </bind_material>");
                writer.WriteLine("        </instance_geometry>");
                writer.WriteLine("      </node>");
            }
            writer.WriteLine("    </visual_scene>");
            writer.WriteLine("  </library_visual_scenes>");
            writer.WriteLine("  <scene>");
            writer.WriteLine("    <instance_visual_scene url=\"#DefaultScene\"/>");
            writer.WriteLine("  </scene>");
            writer.WriteLine("</COLLADA>");
            writer.Close();
            stream.Close();
        }
        public static ParseResult Instance {
            get {
                if (instance == null)
                    instance = new ParseResult();
                return instance;
            }
        }
    }

    public class VertexEntry {
        public Vector position;
        public Vector normal;
        public Coords2d texCoords;

        public VertexEntry(Vector pos, Vector norm, Coords2d coords) {
            position = pos;
            normal = norm;
            texCoords = coords;
        }

        public bool Matches(VertexEntry other) {
            return position.Matches(other.position) && normal.Matches(other.normal) && texCoords.Matches(other.texCoords);
        }
    }

    public class Geometry
    {
        public Dictionary<Material, List<VertexEntry>> vertexSets = new Dictionary<Material, List<VertexEntry>>();
        public Dictionary<Material, List<List<int>>> indexSets = new Dictionary<Material, List<List<int>>>();
 
         
        public int AddVertex(Material mat, VertexEntry newEntry) {
            if (!vertexSets.ContainsKey(mat))
                vertexSets[mat] = new List<VertexEntry>();
            List<VertexEntry> vertices = vertexSets[mat];
            for (int i = 0; i < vertices.Count; ++i) {
                VertexEntry v = vertices[i];
                if (newEntry.Matches(v))
                    return i;
            }
            vertices.Add(newEntry);
            return vertices.Count - 1;            
        }

        public void AddMesh(Mesh m)
        {
            MeshNormals meshNormals = m.GetMeshNormals();
            MeshTextureCoords meshTextureCoords = m.GetMeshTextureCoords();
            MeshMaterialList meshMaterialList = m.GetMeshMaterialList();
            Debug.Assert(meshNormals != null);
            Debug.Assert(meshTextureCoords != null);
            Debug.Assert(meshMaterialList != null);
            Debug.Assert(meshNormals.faceNormals.Length == m.faces.Length);
            for (int faceIndex = 0; faceIndex < m.faces.Length; ++faceIndex) {
                MeshFace face = m.faces[faceIndex];
                MeshFace normalFace = meshNormals.faceNormals[faceIndex];
                Material material = meshMaterialList.GetMaterialByFace(faceIndex);
                Debug.Assert(face.faceVertexIndices.Length == normalFace.faceVertexIndices.Length);
                List<int> polygonPoints = new List<int>();
                for (int pointIndex = 0; pointIndex < face.faceVertexIndices.Length; ++pointIndex) {
                    int vertexIndex = face.faceVertexIndices[pointIndex];
                    int normalIndex = normalFace.faceVertexIndices[pointIndex];
                    Vector position = m.vertices[vertexIndex];
                    Vector normal = meshNormals.normals[normalIndex];
                    Coords2d texCoords = meshTextureCoords.textureCoords[vertexIndex];
                    VertexEntry vertexEntry = new VertexEntry(position, normal, texCoords);
                    int newVertexIndex = AddVertex(material, vertexEntry);
                    polygonPoints.Add(newVertexIndex);
                    // Either add the point to poly, or add a new poly for the point.
                }
                if (!indexSets.ContainsKey(material))
                    indexSets[material] = new List<List<int>>();
                indexSets[material].Add(polygonPoints);
            }
        }
    }

    public class Node {
        public Node parent;
        public string type;
        public string name;
        public string uuid;
    }

    public class Header : Node {
        public Header() {
            type = "Header";
        }
        public int major;
        public int minor;
        public int flags;
    }

    public class Vector : Node {
        public Vector() {
            type = "Vector";
        }
        public float x;
        public float y;
        public float z;

        public bool Matches(Vector other) {
            return other.x == x && other.y == y && other.z == z;
        }
    }

    public class ColorRGB : Node {
        public ColorRGB() {
            type = "ColorRGB";
        }
        public float red;
        public float green;
        public float blue;
    }

    public class ColorRGBA : Node {
        public ColorRGBA() {
            type = "ColorRGBA";
        }
        public float red;
        public float green;
        public float blue;
        public float alpha;
    }

    public class Coords2d : Node {
        public Coords2d() {
            type = "Coords2d";
        }
        public float u;
        public float v;
        
        public bool Matches(Coords2d other) {
            return other.u == u && other.v == v;
        }
    }

    public class Matrix4x4 : Node {
        public Matrix4x4() {
            type = "Matrix4x4";
        }
        public float[] matrix = new float[16];

        public bool IsIdentity {
            get {
                for (int i = 0; i < matrix.Length; ++i) {
                    if ((i % 5) == 0) {
                        if (matrix[i] != 1.0f)
                            return false;
                    } else {
                        if (matrix[i] != 0.0f)
                            return false;
                    }
                }
                return true;
            }
        }
    }

    public class FrameTransformMatrix : Node {
        public FrameTransformMatrix() {
            type = "FrameTransformMatrix";
        }
        public Matrix4x4 frameMatrix;
    }

    public class Frame : Node {
        public Frame() {
            type = "Frame";
        }
        public List<object> options = new List<object>();
    }

    public class MeshFace : Node {
        public MeshFace() {
            type = "MeshFace";
        }
        public int nFaceVertexIndices;
        public int[] faceVertexIndices;
    }

    public class Mesh : Node {
        public Mesh() {
            type = "Mesh";
        }
        public int nVertices;
        public Vector[] vertices;
        public int nFaces;
        public MeshFace[] faces;
        public List<object> options = new List<object>();

        public MeshNormals GetMeshNormals() {
            foreach (object obj in options)
                if (obj is MeshNormals)
                    return (MeshNormals)obj;
            return null;
        }
        public MeshTextureCoords GetMeshTextureCoords() {
            foreach (object obj in options)
                if (obj is MeshTextureCoords)
                    return (MeshTextureCoords)obj;
            return null;
        }
        public MeshMaterialList GetMeshMaterialList() {
            foreach (object obj in options)
                if (obj is MeshMaterialList)
                    return (MeshMaterialList)obj;
            return null;
        }
    }

    public class TextureFilename : Node {
        public TextureFilename() {
            type = "TextureFilename";
        }
        public string filename;
    }
    
    public class Material : Node {
        public Material() {
            type = "Material";
        }
        public ColorRGBA faceColor;
        public float power;
        public ColorRGB specularColor;
        public ColorRGB emissiveColor;
        public List<object> options = new List<object>();

        public TextureFilename GetTextureFilename() {
            foreach (object obj in options)
                if (obj is TextureFilename)
                    return (TextureFilename)obj;
            return null;
        }
    }
    
    public class MeshTextureCoords : Node {
        public MeshTextureCoords() {
            type = "MeshTextureCoords";
        }
        public int nTextureCoords;
        public Coords2d[] textureCoords;
    }

    public class MeshNormals : Node {
        public MeshNormals() {
            type = "MeshNormals";
        }
        public int nNormals;
        public Vector[] normals;
        public int nFaceNormals;
        public MeshFace[] faceNormals;
    }

    public class MeshMaterialList : Node {
        public MeshMaterialList() {
            type = "MeshMaterialList";
        }
        public int nMaterials;
        public int nFaceIndexes;
        public int[] FaceIndexes;
        public List<Material> options = new List<Material>();

        public Material GetMaterialByFace(int faceIndex) {
            Debug.Assert(FaceIndexes.Length > faceIndex);
            Debug.Assert(options.Count > FaceIndexes[faceIndex]);
            return options[FaceIndexes[faceIndex]];
        }
    }
}

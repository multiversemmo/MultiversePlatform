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
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Animating;
using System.ComponentModel;
using System.Xml;
using Multiverse.CollisionLib;

namespace Multiverse.CollisionLib
{
    public enum PolygonKind {
        Illegal = 0,
        CV,
        Terrain,
        Bounding
    }
    
    public class PathData
    {
        protected static int codeVersion = 1;
        protected int version;
        protected const float oneMeter = 1000.0f;
        protected List<PathObject> pathObjects;

        public PathData() {
            version = codeVersion;
            pathObjects = new List<PathObject>();
        }
        
        public PathData(XmlReader r) {
            pathObjects = new List<PathObject>();
            FromXml(r);
        }
        
        public void AddModelPathObject(bool logPathGeneration, PathObjectType type, string modelName,
                                       Vector3 modelPosition, Matrix4 modelTransform, 
                                       List<CollisionShape> shapes, float terrainHeight) {
            PathGenerator pathGenerator = new PathGenerator(logPathGeneration, modelName, type, terrainHeight,
                                                            modelTransform, shapes);
            // Perform traversal and creation of polygons, arcs
            // between polygons, and portals to the terrain
            pathGenerator.GeneratePolygonsArcsAndPortals();
            List<PathPolygon> polygons = new List<PathPolygon>();
            List<PathArc> portals = new List<PathArc>();
            List<PathArc> arcs = new List<PathArc>();
            foreach(GridPolygon r in pathGenerator.CVPolygons)
                polygons.Add(new PathPolygon(r.Index, PolygonKind.CV, r.CornerLocs));
            foreach(GridPolygon r in pathGenerator.TerrainPolygons)
                polygons.Add(new PathPolygon(r.Index, PolygonKind.Terrain, r.CornerLocs));
            foreach(PolygonArc portal in pathGenerator.TerrainPortals)
                portals.Add(new PathArc(portal.Kind, portal.Poly1Index, portal.Poly2Index, MakePathEdge(portal.Edge)));
            foreach(PolygonArc arc in pathGenerator.PolygonArcs)
                arcs.Add(new PathArc(arc.Kind, arc.Poly1Index, arc.Poly2Index, MakePathEdge(arc.Edge)));
            pathObjects.Add(new PathObject(pathGenerator.ModelName, type.name, pathGenerator.FirstTerrainIndex, 
                                           new PathPolygon(0, PolygonKind.Bounding, pathGenerator.ModelCorners),
                    polygons, portals, arcs));
        }

        public int CodeVersion {
            get { return codeVersion; }
        }
        
        public int Version {
            get { return version; }
        }
        
        public List<PathObject> PathObjects {
            get { return pathObjects; }
        }
        
        protected void FromXml(XmlReader r)
        {
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                if (r.Name == "version")
                    version = int.Parse(r.Value);
            }
            r.MoveToElement(); //Moves the reader back to the element node.
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                    continue;
                else if (r.NodeType == XmlNodeType.EndElement)
                    break;
                else if (r.NodeType == XmlNodeType.Element)
                {
                    if (r.Name == "PathObject")
                        pathObjects.Add(new PathObject(r));
                }
            }
        }

        public void ToXml(XmlWriter w)
        {
            if (pathObjects.Count > 0)
            {
                w.WriteStartElement("PathData");
                w.WriteAttributeString("version", version.ToString());
                foreach (PathObject obj in pathObjects)
                    obj.ToXml(w);
                w.WriteEndElement();
            }
        }

        protected PathEdge MakePathEdge (GridEdge edge) {
            return new PathEdge(edge.StartLoc, edge.EndLoc);
        }
        
    }
    
    public class PathObjectType {
        public string name;
        public float height;
        public float width;
        public float maxClimbSlope;
        public float gridResolution;
        public float maxDisjointDistance;
        public int minimumFeatureSize;

        public PathObjectType(string name, float height, float width, 
                              float maxClimbSlope, float gridResolution, 
                              float maxDisjointDistance, int minimumFeatureSize) {
            this.AcceptValues(name, height, width, maxClimbSlope, 
                gridResolution, maxDisjointDistance, minimumFeatureSize);
        }
        
        public PathObjectType(XmlReader r) {
            FromXml(r);
        }
        
        public PathObjectType() {
        }
        
        public PathObjectType(PathObjectType other) {
            name = other.name;
            height = other.height;
            width = other.width;
            maxClimbSlope = other.maxClimbSlope;
            gridResolution = other.gridResolution;
            maxDisjointDistance = other.maxDisjointDistance;
            minimumFeatureSize = other.minimumFeatureSize;
        }
        
        public void AcceptValues(string name, float height, float width, 
                                 float maxClimbSlope, float gridResolution, 
                                 float maxDisjointDistance, int minimumFeatureSize) {
            this.name = name;
            this.height = height;
            this.width = width;
            this.maxClimbSlope = maxClimbSlope;
            this.gridResolution = gridResolution;
            this.maxDisjointDistance = maxDisjointDistance;
            this.minimumFeatureSize = minimumFeatureSize;
        }
        
		public void ToXml(XmlWriter w)
		{
			w.WriteStartElement("PathObjectType");
			w.WriteAttributeString("name", name);
			w.WriteAttributeString("height", height.ToString());
			w.WriteAttributeString("width", width.ToString());
			w.WriteAttributeString("maxClimbSlope", maxClimbSlope.ToString());
			w.WriteAttributeString("gridResolution", gridResolution.ToString());
			w.WriteAttributeString("maxDisjointDistance", maxDisjointDistance.ToString());
			w.WriteAttributeString("minimumFeatureSize", minimumFeatureSize.ToString());
            w.WriteEndElement();
		}

        public void FromXml(XmlReader r)
        {
			// first parse the attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                case "name":
                    name = r.Value;
                    break;
                case "height":
                    height = float.Parse(r.Value);
                    break;
                case "width":
                    width = float.Parse(r.Value);
                    break;
                case "maxClimbSlope":
                    maxClimbSlope = float.Parse(r.Value);
                    break;
                case "gridResolution":
                    gridResolution = float.Parse(r.Value);
                    break;
                case "maxDisjointDistance":
                    maxDisjointDistance = float.Parse(r.Value);
                    break;
                case "minimumFeatureSize":
                    minimumFeatureSize = int.Parse(r.Value);
                    break;
                }
            }
        }

    }

    public class PathObject
    {
        protected string modelName;
        protected string type;
        protected int firstTerrainIndex;
        protected PathPolygon boundingPolygon;
        protected List<PathPolygon> polygons;
        protected List<PathArc> portals;
        protected List<PathArc> arcs;

        public PathObject(string modelName, string type, int firstTerrainIndex, PathPolygon boundingPolygon, 
                          List<PathPolygon> polygons, List<PathArc> portals, List<PathArc> arcs)
        {
            this.modelName = modelName;
            this.type = type;
            this.firstTerrainIndex = firstTerrainIndex;
            this.boundingPolygon = boundingPolygon;
            this.polygons = polygons;
            this.portals = portals;
            this.arcs = arcs;
        }
        
        public PathObject(XmlReader r)
        {
            FromXml(r);
        }
        
        public void FromXml(XmlReader r)
        {
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                if (r.Name == "modelName")
                    modelName = r.Value;
                if (r.Name == "type")
                    type = r.Value;
                if (r.Name == "firstTerrainIndex")
                    firstTerrainIndex = int.Parse(r.Value);
            }
            r.MoveToElement(); //Moves the reader back to the element node.
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    // save the name of the element
                    string elementName = r.Name;
                    switch (elementName)
                    {
                    case "BoundingPolygon":
                        List<PathPolygon> polys = ReadPolygons(r);
                        Debug.Assert(polys.Count == 1, "In PathObject.FromXml, must have exactly one polygon in BoundingPolygon list!");
                        boundingPolygon = polys[0];
                        break;
                    case "PathPolygons":
                        polygons = ReadPolygons(r);
                        break;
                    case "PathPortals":
                        portals = ReadArcs(r);
                        break;
                    case "PathArcs":
                        arcs = ReadArcs(r);
                        break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }

        private List<PathPolygon> ReadPolygons(XmlReader r)
        {
            List<PathPolygon> polygons = new List<PathPolygon>();
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                r.MoveToElement(); //Moves the reader back to the element node.
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    // save the name of the element
                    if (r.Name == "PathPolygon")
                        polygons.Add(new PathPolygon(r));
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
            return polygons;
        }
        
        private List<PathArc> ReadArcs(XmlReader r)
        {
            List<PathArc> theArcs = new List<PathArc>();
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                r.MoveToElement(); //Moves the reader back to the element node.
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    // save the name of the element
                    if (r.Name == "PathArc")
                        theArcs.Add(new PathArc(r));
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
            return theArcs;
        }
        
        public void ToXml(XmlWriter w) 
        {
            w.WriteStartElement("PathObject");
            w.WriteAttributeString("modelName", modelName);
            w.WriteAttributeString("type", type);
            w.WriteAttributeString("firstTerrainIndex", firstTerrainIndex.ToString());
            w.WriteStartElement("BoundingPolygon");
            boundingPolygon.ToXml(w);
            w.WriteEndElement();
            if (polygons.Count > 0)
            {
                w.WriteStartElement("PathPolygons");
                foreach(PathPolygon rect in polygons)
                    rect.ToXml(w);
                w.WriteEndElement();
            }
            if (portals.Count > 0)
            {
                w.WriteStartElement("PathPortals");
                foreach(PathArc portal in portals)
                    portal.ToXml(w);
                w.WriteEndElement();
            }
            if (arcs.Count > 0)
            {
                w.WriteStartElement("PathArcs");
                foreach(PathArc arc in arcs)
                    arc.ToXml(w);
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }
        
    }

    public class PathPolygon
    {
        protected int index;
        protected PolygonKind kind;
        protected Vector3 [] corners;

        public PathPolygon(int index, PolygonKind kind, Vector3 [] corners)
        {
            this.kind = kind;
            this.index = index;
            this.corners = corners;
        }
        
        public PathPolygon(XmlReader r)
        {
            FromXml(r);
        }
        
        public static String PolygonKindToString(PolygonKind kind) {
            switch (kind) {
            case PolygonKind.Illegal:
                return "Illegal";
            case PolygonKind.CV:
                return "CV";
            case PolygonKind.Terrain:
                return "Terrain";
            case PolygonKind.Bounding:
                return "Bounding";
            default:
                return "Unknown PolygonKind " + (int)kind;
            }
        }

        public static PolygonKind PolygonKindFromString(String s) {
            switch (s) {
            case "Illegal":
                return PolygonKind.Illegal;
            case "CV":
                return PolygonKind.CV;
            case "Terrain":
                return PolygonKind.Terrain;
            case "Bounding":
                return PolygonKind.Bounding;
            default:
                return PolygonKind.Illegal;
            }
        }

        public int Index {
            get { return index; }
        }

        public PolygonKind Kind {
            get { return kind; }
        }

        public Vector3 [] Corners  {
            get { return corners; }
        }


        public void FromXml(XmlReader r)
        {
            corners = new Vector3[4];
            int cornerNumber = 0;
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                for (int i = 0; i < r.AttributeCount; i++)
                {
                    r.MoveToAttribute(i);

                    // set the field in this object based on the element we just read
                    if (r.Name == "kind")
                    {
                        kind = PolygonKindFromString(r.Value);
                        break;
                    }
                    if (r.Name == "index")
                    {
                        index = int.Parse(r.Value);
                        break;
                    }
                }
                r.MoveToElement(); //Moves the reader back to the element node.
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    // save the name of the element
                    string elementName = r.Name;
                    switch (elementName)
                    {
                    case "Corner":
                        corners[cornerNumber++] = XmlHelperClass.ParseVectorAttributes(r);
                        break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }

        public void ToXml(XmlWriter w) 
        {
            w.WriteStartElement("PathPolygon");
            w.WriteAttributeString("kind", PathPolygon.PolygonKindToString(kind));
            w.WriteAttributeString("index", index.ToString());
            foreach(Vector3 corner in corners) 
                XmlHelperClass.WriteVectorElement(w, "Corner", corner);
            w.WriteEndElement();
        }
        
    }
    
    public class PathEdge
    {
        protected Vector3 start, end;

        public PathEdge(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
        }
        
        public PathEdge(XmlReader r)
        {
            FromXml(r);
        }
        
        public void FromXml(XmlReader r)
        {
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                r.MoveToElement(); //Moves the reader back to the element node.
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    // save the name of the element
                    string elementName = r.Name;
                    switch (elementName)
                    {
                    case "Start":
                        start = XmlHelperClass.ParseVectorAttributes(r);
                        break;
                    case "End":
                        end = XmlHelperClass.ParseVectorAttributes(r);
                        break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }

        public void ToXml(XmlWriter w) 
        {
            w.WriteStartElement("PathEdge");
            XmlHelperClass.WriteVectorElement(w, "Start", start);
            XmlHelperClass.WriteVectorElement(w, "End", end);
            w.WriteEndElement();
        }

    }
    
    public class PathArc
    {
        protected ArcKind kind;
        protected int poly1Index;
        protected int poly2Index;
        protected PathEdge edge;

        public PathArc(ArcKind kind, int poly1Index, int poly2Index, PathEdge edge)
        {
            this.kind = kind;
            this.poly1Index = poly1Index;
            this.poly2Index = poly2Index;
            this.edge = edge;
        }

        public PathArc(XmlReader r)
        {
            FromXml(r);
        }
        
        public static String ArcKindToString(ArcKind kind) {
            switch (kind) {
            case ArcKind.Illegal:
                return "Illegal";
            case ArcKind.CVToCV:
                return "CVToCV";
            case ArcKind.TerrainToTerrain:
                return "TerrainToTerrain";
            case ArcKind.CVToTerrain:
                return "CVToTerrain";
            default:
                return "Unknown ArcKind " + (int)kind;
            }
        }

        public static ArcKind ArcKindFromString(String s) {
            switch (s) {
            case "Illegal":
                return ArcKind.Illegal;
            case "CVToCV":
                return ArcKind.CVToCV;
            case "TerrainToTerrain":
                return ArcKind.TerrainToTerrain;
            case "CVToTerrain":
                return ArcKind.CVToTerrain;
            default:
                return ArcKind.Illegal;
            }
        }

        public ArcKind Kind {
            get { return kind; }
        }

        public int Poly1Index {
            get { return poly1Index; }
        }

        public int Poly2Index {
            get { return poly2Index; }
        }

        public PathEdge Edge {
            get { return edge; }
        }

        public void FromXml(XmlReader r)
        {
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                for (int i = 0; i < r.AttributeCount; i++)
                {
                    r.MoveToAttribute(i);

                    // set the field in this object based on the element we just read
                    switch (r.Name)
                    {
                    case "kind":
                        kind = ArcKindFromString(r.Value);
                        break;
                    case "poly1Index":
                        poly1Index = int.Parse(r.Value);
                        break;
                    case "poly2Index":
                        poly2Index = int.Parse(r.Value);
                        break;
                    }
                }
                r.MoveToElement(); //Moves the reader back to the element node.
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    // save the name of the element
                    string elementName = r.Name;
                    switch (elementName)
                    {
                    case "PathEdge":
                        edge = new PathEdge(r);
                        break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }

        public void ToXml(XmlWriter w) 
        {
            w.WriteStartElement("PathArc");
            w.WriteAttributeString("kind", ArcKindToString(kind));
            w.WriteAttributeString("poly1Index", poly1Index.ToString());
            w.WriteAttributeString("poly2Index", poly2Index.ToString());
            edge.ToXml(w);
            w.WriteEndElement();
        }

    }
    
    class XmlHelperClass {
		public static Vector3 ParseVectorAttributes(XmlReader r)
		{
			float x = 0;
			float y = 0;
			float z = 0;

			for (int i = 0; i < r.AttributeCount; i++)
			{
				r.MoveToAttribute(i);

				// set the field in this object based on the element we just read
				switch (r.Name)
				{
					case "x":
						x = float.Parse(r.Value);
						break;
					case "y":
						y = float.Parse(r.Value);
						break;
					case "z":
						z = float.Parse(r.Value);
						break;
				}
			}
			r.MoveToElement(); //Moves the reader back to the element node.

			return new Vector3(x, y, z);
		}

        public static void WriteVectorElement(XmlWriter w, string elementName, Vector3 v)
        {
            w.WriteStartElement(elementName);
            w.WriteAttributeString("x", v.x.ToString());
            w.WriteAttributeString("y", v.y.ToString());
            w.WriteAttributeString("z", v.z.ToString());
            w.WriteEndElement();
        }
    }

}


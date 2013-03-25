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
using System.Text.RegularExpressions;

using Axiom.MathLib;


namespace Multiverse.Serialization.Collada
{
    public class Controller
    {
        protected GeometrySet target;
        protected Matrix4 bindShapeMatrix = Matrix4.Identity;
        protected string name;

        public Controller( string name )
        {
            this.name = name;
            this.bindShapeMatrix = Matrix4.Identity;
        }

        public GeometrySet Target
        {
            get { return target; }
            set { target = value; }
        }
        public string Name
        {
            get { return name; }
        }
        public Matrix4 BindShapeMatrix
        {
            get { return bindShapeMatrix; }
            set { bindShapeMatrix = value; }
        }
    }

    public class RigidController : Controller
    {
        public string parentBone;

        public RigidController( string name, string parentBone )
            : base( name )
        {
            this.parentBone = parentBone;
        }

        public string ParentBone
        {
            get { return parentBone; }
        }
    }

    /// <summary>
    /// A SkinController supports mesh deformation by binding mesh vertices 
    /// to a hierarchy of transform nodes.  A vertex can be associated with
    /// more than one transform; it has a weight for each affecting transform.
    /// </summary>
    public class SkinController : Controller
    {
        // The input sources that will override those in the mesh geometry object.
        // (e.g. bind_shape_position and bind_shape_normal vector data)
        // This is a mapping from input index to list of input sources 
        // (which may in turn be compound sources like vertex)
        public Dictionary<int, List<InputSourceCollection>> InputSources
        {
            get { return m_InputSources; }
        }
        Dictionary<int, List<InputSourceCollection>> m_InputSources;

        // The inverse bind matrices for the nodes that influence this skin
        // These bind matrices are interpreted in the unitless context of 
        // the collada file.  Unit conversion will happen later.
        public Dictionary<string, Matrix4> InverseBindMatrices
        {
            get { return m_InverseBindMatrices; }
        }
        Dictionary<string, Matrix4> m_InverseBindMatrices;

        /// <summary>
        /// A skinned mesh may also be deformed by a set of morphs. This is
        /// the set of morphs applied to the skin; null if there are no morphs.
        /// </summary>
        public MorphController Morph
        {
            get { return m_Morph; }
            set { m_Morph = value; }
        }
        MorphController m_Morph;

        public SkinController( string name )
            : base( name )
        {
            m_InputSources = new Dictionary<int, List<InputSourceCollection>>();
            m_InverseBindMatrices = new Dictionary<string, Matrix4>();
        }

        // Get a list containing all the entries in the various input source lists
        public List<InputSource> GetAllInputSources()
        {
            List<InputSource> sources = new List<InputSource>();

            foreach( List<InputSourceCollection> tmpList in m_InputSources.Values )
            {
                foreach( InputSourceCollection tmp in tmpList )
                {
                    sources.AddRange( tmp.GetSources() );
                }
            }
            return sources;
        }
    }

    /// <summary>
    /// Provides for a linear combination of different poses, each of
    /// which has the same number of vertices.  I think that this
    /// covers both the Axiom "morph animation" and "pose animation".
    /// If method is "NORMALIZE", it's more like a morph animation.
    /// <summary>
    public class MorphController : Controller
    {
        // One of "NORMALIZED" or "RELATIVE"
        public string Method
        {
            get { return m_Method; }
        }
        protected string m_Method;

        // Correspondence between a semantic ("MORPH_TARGET" or "MORPH_WEIGHT") and
        // the name of a source.
        public Dictionary<int, List<InputSourceCollection>> InputSources
        {
            get { return m_InputSources; }
        }
        Dictionary<int, List<InputSourceCollection>> m_InputSources;
        

        public MorphController( string name, string method )
            : base( name )
        {
            this.m_Method = method;
            m_InputSources = new Dictionary<int, List<InputSourceCollection>>();
        }

        public static int GetTargetAttributeIndex( string targetAttribute )
        {
            Regex rx = new Regex( "\\((\\d)\\)" );
            Match m = rx.Match( targetAttribute );
            Debug.Assert( m.Groups.Count == 2 );
            return int.Parse( m.Groups[ 1 ].Value );
        }

        public InputSource GetInputSource( string targetComponent )
        {
            foreach( List<InputSourceCollection> inputs in InputSources.Values )
            {
                foreach( InputSourceCollection input in inputs )
                {
                    foreach( InputSource source in input.GetSources() )
                    {
                        if( source.Source == targetComponent )
                        {
                            return source;
                        }
                    }
                }
            }
            return null;
        }

        public InputSource GetInputSourceBySemantic( string semantic )
        {
            foreach( List<InputSourceCollection> inputs in InputSources.Values )
            {
                foreach( InputSourceCollection input in inputs )
                {
                    foreach( InputSource source in input.GetSources() )
                    {
                        if( source.Semantic == semantic )
                        {
                            return source;
                        }
                    }
                }
            }
            return null;
        }

    }
}

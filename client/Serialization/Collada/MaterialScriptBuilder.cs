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

using Axiom.Core;


namespace Multiverse.Serialization.Collada
{
    internal class MaterialScriptBuilder
    {
        #region Public Properties

        internal static string MaterialNamespace
        {
            get { return s_MaterialNamespace; }
            set { s_MaterialNamespace = value; }
        }
        private static string s_MaterialNamespace = null;

        #endregion Public Properties

        //readonly log4net.ILog s_Log;
        static readonly log4net.ILog s_Log = log4net.LogManager.GetLogger( typeof( MaterialScriptBuilder ) );

        internal MaterialScriptBuilder( string materialNamespace )
        {
            s_MaterialNamespace = materialNamespace;

            m_Images = new Dictionary<string, string>();
            m_Textures = new Dictionary<string, List<TextureTechnique>>();
            m_Materials = new Dictionary<string, List<MaterialTechnique_13>>();
            m_Techniques = new Dictionary<string, List<MaterialTechnique_14>>();
            m_Effects = new Dictionary<string, string>();
            m_SurfaceSamplers = new Dictionary<string, SurfaceSampler>();
            m_Surfaces = new Dictionary<string, Surface>();
        }

        internal string GetMaterialScript()
        {
            StringBuilder sb = new StringBuilder();
            GetMaterialScript_13( sb );
            GetMaterialScript_14( sb );
            return sb.ToString();
        }

        #region Fields for accumulating material components
        /// <summary>
        ///  Mapping from material id to effect id
        /// </summary>
        private Dictionary<string, string> m_Effects;

        // Mapping from texture id to texture techniques
        private Dictionary<string, List<TextureTechnique>> m_Textures;

        // Mapping from effect id to a list of techniques
        private Dictionary<string, List<MaterialTechnique_14>> m_Techniques;

        // Mapping from material id to material techniques
        private Dictionary<string, List<MaterialTechnique_13>> m_Materials;

        /// <summary>
        ///  Mapping from image id to file name
        /// </summary>
        private Dictionary<string, string> m_Images;

        private Dictionary<string, SurfaceSampler> m_SurfaceSamplers;

        private Dictionary<string, Surface> m_Surfaces;

        #endregion Fields for accumulating material components


        #region Methods for adding material components 
        // TODO: These are a semi-necessary evil. Currently the XML is parsed in
        // the ColladaMeshReader_* classes; if we were to parse the XML here, then
        // all of this could be hidden.
        
        internal void AddTechniques( string effectId, List<MaterialTechnique_13> techniques )
        {
            s_Log.Info( "AddTechniques: " + effectId );

            m_Materials[ effectId ] = techniques;
        }

        internal void AddTextureTechniques( string id, List<TextureTechnique> techniques )
        {
            s_Log.Info( "AddTextureTechniques: " + id );

            m_Textures[ id ] = techniques;
        }

        internal void AddTechnique( string effectId, MaterialTechnique_14 technique )
        {
            s_Log.Info( "AddTechnique: " + effectId );

            if( !m_Techniques.ContainsKey( effectId ) )
            {
                m_Techniques[ effectId ] = new List<MaterialTechnique_14>();
            }

            m_Techniques[ effectId ].Add( technique );
        }

        internal void AddImage( string imageId, string imageSourcePath )
        {
            s_Log.Info( "AddImage: " + imageId );

            m_Images[ imageId ] = imageSourcePath;
        }

        internal void AddEffect( string effectId, string url )
        {
            s_Log.Info( "AddEffect: " + effectId );

            m_Effects[ effectId ] = url;
        }

        internal void AddSurface( string sid, Surface surface )
        {
            s_Log.Info( "AddSurface: " + sid );

            if( null != surface.image )
            {
                m_Surfaces[ sid ] = surface;
            }
        }

        internal void AddSurfaceSampler( string sid, SurfaceSampler sampler )
        {
            s_Log.Info( "AddSurfaceSampler: " + sid );

            if( null != sampler.surfaceName )
            {
                m_SurfaceSamplers[ sid ] = sampler;
            }
        }

        #endregion Methods for adding material components 

        private void GetMaterialScript_13( StringBuilder sb )
        {
            s_Log.Info( String.Format(
                "Building namespace '{0}' from Materials list",
                MaterialNamespace ) );

            foreach( string materialId in m_Materials.Keys )
            {
                s_Log.Info( "    building: " + materialId );

                List<MaterialTechnique_13> materialTechniques = m_Materials[ materialId ];
                sb.Append( "material " );
                if( MaterialNamespace != null )
                {
                    sb.Append( MaterialNamespace );
                    sb.Append( "." );
                }
                sb.Append( materialId );
                sb.Append( "\n" );
                sb.Append( "{\n" );
                foreach( MaterialTechnique_13 mt in materialTechniques )
                {
                    sb.Append( "    technique\n" );
                    sb.Append( "    {\n" );
                    foreach( TechniquePass pass in mt.passes )
                    {
                        sb.Append( "        pass\n" );
                        sb.Append( "        {\n" );
                        sb.Append( "            shading " );
                        sb.Append( pass.shader );
                        sb.Append( "\n" );
                        sb.Append( "\n" );
                        sb.Append( "            ambient  1.00000 1.00000 1.00000 1.00000\n" );
                        sb.Append( "            diffuse  1.00000 1.00000 1.00000 1.00000\n" );
                        sb.Append( "            specular 0.00000 0.00000 0.00000 1.00000 0.00000\n" );
                        sb.Append( "            emissive 0.00000 0.00000 0.00000 1.00000\n" );
                        sb.Append( "\n" );
                        List<TextureTechnique> textureTechniques;
                        if( pass.textureId != null )
                            textureTechniques = m_Textures[ pass.textureId ];
                        else
                            textureTechniques = new List<TextureTechnique>();
                        foreach( TextureTechnique tt in textureTechniques )
                        {
                            sb.Append( "            texture_unit\n" );
                            sb.Append( "            {\n" );
                            sb.Append( "                texture " );
                            sb.Append( m_Images[ tt.imageId ] );
                            sb.Append( "\n" );
                            sb.Append( "                tex_coord_set 0\n" );
                            sb.Append( "            }\n" );
                        }
                        sb.Append( "        }\n" );
                    }
                    sb.Append( "    }\n" );

                }
                sb.Append( "}\n" );
            }
        }

        private void GetMaterialScript_14( StringBuilder sb )
        {
            s_Log.Info( String.Format(
                "Building namespace '{0}' from Effects list",
                MaterialNamespace ) );

            char[] pathDelims = { '/', '\\' };
            foreach( string materialId in m_Effects.Keys )
            {
                s_Log.Info( "    building: " + materialId );

                string effectId = m_Effects[ materialId ];
                if( !m_Techniques.ContainsKey( effectId ) )
                {
                    // TODO: This is a work-around for a case where an effect had
                    // an empty definition.  No technique is added for the effect,
                    // but the effectId is still in the effects dictionary. A better
                    // fix is don't put an effectId for an empty effect in the 
                    // dictionary in the first place.
                    continue;
                }
                List<MaterialTechnique_14> materialTechniques = m_Techniques[ effectId ];
                sb.Append( "material " );
                if( MaterialNamespace != null )
                {
                    sb.Append( MaterialNamespace );
                    sb.Append( "." );
                }
                sb.Append( materialId );
                sb.Append( "\n" );
                sb.Append( "{\n" );
                foreach( MaterialTechnique_14 mt in materialTechniques )
                {
                    sb.Append( "    technique\n" );
                    sb.Append( "    {\n" );
                    sb.Append( "        pass\n" );
                    sb.Append( "        {\n" );
                    sb.Append( "            shading phong\n" );
                    sb.Append( "\n" );
                    sb.Append( "            ambient  " + AsRGBAString( mt.Ambient ) + "\n" );
                    sb.Append( "            diffuse  " + AsRGBAString( mt.Diffuse ) + "\n" );
                    sb.Append( "            specular " + AsRGBAString( mt.Specular ) + string.Format( " {0:f5}", mt.Shininess ) + "\n" );
                    sb.Append( "            emissive " + AsRGBAString( mt.Emissive ) + "\n" );
                    sb.Append( "\n" );
                    if( mt.DiffuseTexture != null )
                    {
                        string diffuseTexture = string.Empty;
                        string imageId = mt.DiffuseTexture.imageId;
                        // If we have a surface sampler, use that.
                        // If we don't, go ahead and just use the image
                        if( m_SurfaceSamplers.ContainsKey( imageId ) )
                        {
                            SurfaceSampler sampler = m_SurfaceSamplers[ imageId ];
                            if( m_Surfaces.ContainsKey( sampler.surfaceName ) )
                            {
                                Surface surface = m_Surfaces[ sampler.surfaceName ];
                                imageId = surface.image;
                            }
                        }
                        if( m_Images.ContainsKey( imageId ) )
                            diffuseTexture = m_Images[ imageId ];
                        int idx = diffuseTexture.LastIndexOfAny( pathDelims );
                        if( idx >= 0 )
                            diffuseTexture = diffuseTexture.Substring( idx + 1 );
                        sb.Append( "            texture_unit\n" );
                        sb.Append( "            {\n" );
                        sb.Append( "                texture " );
                        sb.Append( diffuseTexture );
                        sb.Append( "\n" );
                        sb.Append( "                tex_coord_set 0\n" );
                        sb.Append( "            }\n" );
                    }
                    sb.Append( "        }\n" );
                    sb.Append( "    }\n" );
                }
                sb.Append( "}\n" );
            }
        }

        private string AsRGBAString( ColorEx color )
        {
            return string.Format( 
                "{0:f5} {1:f5} {2:f5} {3:f5}", 
                color.r, color.g, color.b, color.a );
        }
    }
}

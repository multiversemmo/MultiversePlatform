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
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
    public delegate void AutoSplatConfigChangeHandler( object container, object changedElement );

    public class AutoSplatRules
    {
        public event AutoSplatConfigChangeHandler HeightRangeAdded;
        public event AutoSplatConfigChangeHandler HeightRangeRemoved;
        public event AutoSplatConfigChangeHandler HeightRangeMoved;

        private readonly List<AutoSplatHeightAngleRange> m_RangeList = new List<AutoSplatHeightAngleRange>();
        private readonly Dictionary<Guid, AutoSplatHeightAngleRange> m_IdToRange = new Dictionary<Guid, AutoSplatHeightAngleRange>();

        public string[] layerTextureNames = new string[ AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES ];

        public long MinHeightMM { get; private set; }
        public long MaxHeightMM { get; private set; }


        #region Constructors

        public AutoSplatRules( long minHeightMM, long maxHeightMM )
        {
            MinHeightMM = minHeightMM;
            MaxHeightMM = maxHeightMM;

            AddHeight( MinHeightMM, 0 );
            AddHeight( MaxHeightMM, 0 );
        }

        public AutoSplatRules( long minHeightMM, long maxHeightMM, AutoSplatConfig autoSplatConfig )
        {
            MinHeightMM = minHeightMM;
            MaxHeightMM = maxHeightMM;

            ConvertConfigToRules( autoSplatConfig );
        }

        public AutoSplatRules( XmlReader reader )
        {
            FromXml( reader );
        }

        #endregion Constructors

        #region Xml serialization

        public void FromXml( XmlReader reader )
        {
            reader.ReadToFollowing( "AutoSplatRules" );

            reader.ReadToFollowing( "long" );
            Debug.Assert( reader.GetAttribute( "name" ).Equals( "minHeight" ) );
            MinHeightMM = long.Parse( reader.GetAttribute( "value" ) );

            reader.ReadToFollowing( "long" );
            Debug.Assert( reader.GetAttribute( "name" ).Equals( "maxHeight" ) );
            MaxHeightMM = long.Parse( reader.GetAttribute( "value" ) );

            TexturesFromXml( reader );

            HeightRangesFromXml( reader );
        }

        public void TexturesFromXml( XmlReader reader )
        {
            reader.ReadToFollowing( "textures" );

            reader.ReadToFollowing( "texture" );

            do
            {
                TextureElementFromXml( reader );

            } while( reader.ReadToNextSibling( "texture" ) );
        }

        void TextureElementFromXml( XmlReader reader )
        {
            XmlReader subReader = reader.ReadSubtree();

            subReader.ReadToFollowing( "string" );
            Debug.Assert( subReader.GetAttribute( "name" ).Equals( "name" ) );
            string name = subReader.GetAttribute( "value" );

            subReader.ReadToFollowing( "int" );
            Debug.Assert( subReader.GetAttribute( "name" ).Equals( "textureIndex" ) );
            int index = int.Parse( subReader.GetAttribute( "value" ) );

            Debug.Assert( index < layerTextureNames.Length );

            layerTextureNames[ index ] = name;

            subReader.Close();
        }

        public void HeightRangesFromXml( XmlReader reader )
        {
            reader.ReadToFollowing( "heightRanges" );

            reader.ReadToFollowing( "heightRange" );

            do
            {
                InternalAddHeightAngleRange( new AutoSplatHeightAngleRange( reader ) );

            } while( reader.ReadToNextSibling( "heightRange" ) );
        }

        public void ToXml( XmlWriter writer )
        {
            writer.WriteStartElement( "AutoSplatRules" );

            writer.WriteStartElement( "long" );
            writer.WriteAttributeString( "name", "minHeight" );
            writer.WriteAttributeString( "value", MinHeightMM.ToString() );
            writer.WriteEndElement();

            writer.WriteStartElement( "long" );
            writer.WriteAttributeString( "name", "maxHeight" );
            writer.WriteAttributeString( "value", MaxHeightMM.ToString() );
            writer.WriteEndElement();

            TexturesToXml( writer );
            HeightRangesToXml( writer );

            writer.WriteEndElement();
        }

        void TexturesToXml( XmlWriter writer )
        {
            writer.WriteStartElement( "textures" );

            for( int i = 0; i < layerTextureNames.Length; i++ )
            {
                if( !string.IsNullOrEmpty( layerTextureNames[ i ] ) )
                {
                    writer.WriteStartElement( "texture" );

                    writer.WriteStartElement( "string" );
                    writer.WriteAttributeString( "name", "name" );
                    writer.WriteAttributeString( "value", layerTextureNames[ i ] );
                    writer.WriteEndElement();

                    writer.WriteStartElement( "int" );
                    writer.WriteAttributeString( "name", "textureIndex" );
                    writer.WriteAttributeString( "value", i.ToString() );
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        void HeightRangesToXml( XmlWriter writer )
        {
            writer.WriteStartElement( "heightRanges" );

            foreach( AutoSplatHeightAngleRange range in m_RangeList )
            {
                range.ToXml( writer );
            }
            writer.WriteEndElement();
        }

        #endregion Xml serialization

        public AutoSplatHeightAngleRange AddHeight( long heightMM, int textureIndex )
        {
            AutoSplatHeightAngleRange range = new AutoSplatHeightAngleRange( heightMM, textureIndex, textureIndex );
            InternalAddHeightAngleRange( range );
            return range;
        }

        public AutoSplatHeightAngleRange AddHeight( long heightMM, AutoSplatHeightAngleRange slopesTemplate )
        {
            AutoSplatHeightAngleRange range = new AutoSplatHeightAngleRange( heightMM, slopesTemplate );
            InternalAddHeightAngleRange( range );
            return range;
        }

        public void UndoRemoveHeightRange( AutoSplatHeightAngleRange range )
        {
            InternalAddHeightAngleRange( range );
        }

        public void RemoveHeight( Guid rangeId )
        {
            if( m_IdToRange.ContainsKey( rangeId ) )
            {
                InternalRemoveHeightAngleRange( rangeId );
            }
        }

        public long GetHeightMM( Guid rangeId )
        {
            if( m_IdToRange.ContainsKey( rangeId ) )
            {
                return m_IdToRange[ rangeId ].HeightMM;
            }
            return 0;
        }

        public AutoSplatHeightAngleRange GetRange( Guid rangeId )
        {
            if( m_IdToRange.ContainsKey( rangeId ) )
            {
                return m_IdToRange[ rangeId ];
            }
            return null;
        }

        private void InternalAddHeightAngleRange( AutoSplatHeightAngleRange range )
        {
            if( range == null )
            {
                throw new NullReferenceException();
            }

            // Make sure the height angle range to be added is not duplicated
            foreach( AutoSplatHeightAngleRange angleRange in m_RangeList )
            {
                if( angleRange.HeightMM == range.HeightMM )
                {
                    throw new ArgumentException( "Duplicate heights are not allowed." + range );
                }
            }

#if REJECT_OUT_OF_BOUNDS_RANGES
            // A properly configured world will never have a range outside of the
            // height limits, but it is possible that a legacy world will not have
            // saved the AutoSplatConfig, so it uses the default--and the default
            // config might have ranges outside the world's limits.  Rather than
            // try to guess how to fit it, we'll just discard it, and leave it to
            // the user to set up a configuration that they want.
            if( range.HeightMM <= MaxHeightMM &&
                range.HeightMM >= MinHeightMM )
            {
                m_RangeList.Add( range );
                m_RangeList.Sort(); // Keep sorted by height

                m_IdToRange.Add( range.Id, range );

                FireHeightRangeAdded( range );
            }
#else
            m_RangeList.Add( range );
            m_RangeList.Sort(); // Keep sorted by height

            m_IdToRange.Add( range.Id, range );

            // TODO: This is not right yet, but useful in the short term
            // For legacy worlds, we may have ranges outside the min/max
            // that occurs in the world. We'll stretch the bounds for 
            // auto-splatting, but now they are out of sync with the terrain.
            AdjustMinMax();

            FireHeightRangeAdded( range );
#endif
        }

        // This assumes that the range list is sorted. It expands the min/max to 
        // contain all the heights in the list; it will never shrink the min/max.
        void AdjustMinMax()
        {
            MinHeightMM = Math.Min( MinHeightMM, m_RangeList[ 0 ].HeightMM );
            MaxHeightMM = Math.Max( MaxHeightMM, m_RangeList[ m_RangeList.Count - 1 ].HeightMM );
        }

        private void InternalRemoveHeightAngleRange( Guid rangeId )
        {
            int index = m_RangeList.IndexOf( m_IdToRange[ rangeId ] );

            // Design constraints require that you always have a min and max
            // range, so we are not allowed to remove the extremes. Otherwise,
            // go crazy!
            if( 0 != index && (m_RangeList.Count - 1) != index )
            {
                AutoSplatHeightAngleRange range = m_IdToRange[ rangeId ];

                m_RangeList.Remove( range );
                m_IdToRange.Remove( rangeId );

                FireHeightRangeRemoved( range );
            }
        }



        public void MoveHeight( Guid id, long newHeightMM )
        {
            if( m_IdToRange.ContainsKey( id ) )
            {
                AutoSplatHeightAngleRange targetRange = m_IdToRange[ id ];

                if( newHeightMM.Equals( targetRange.HeightMM ) )
                {
                    // This move isn't going to change anything, so get out early.
                    return;
                }

                int index = m_RangeList.IndexOf( targetRange );

                AutoSplatHeightAngleRange splitRange = null;

                if( 0 == index || (m_RangeList.Count - 1) == index )
                {
                    // This move alters a range at an extreme, which implicitly splits the
                    // range as we must always keep a range at each extreme. 
                    splitRange = new AutoSplatHeightAngleRange( targetRange.HeightMM, targetRange );
                }
                else
                {
                    // Guaranteed to be neither the first nor last
                    AutoSplatHeightAngleRange prev = m_RangeList[ index - 1 ];
                    AutoSplatHeightAngleRange next = m_RangeList[ index + 1 ];

                    if( newHeightMM < prev.HeightMM || next.HeightMM < newHeightMM )
                    {
                        throw new ConstraintException(
                            "MoveHeight would change ordering of HeightAngleRange list;" +
                            " only Add and Remove should change the sort ordering." );
                    }

                    // If the move would make the height coincide with one of its 
                    // neighbors, back it off slightly to make things sort nicely.
                    if( newHeightMM.Equals( prev.HeightMM ) )
                    {
                        ++newHeightMM;
                    }
                    else if( newHeightMM.Equals( next.HeightMM ) )
                    {
                        --newHeightMM;
                    }
                }

                targetRange.HeightMM = newHeightMM;

                FireHeightRangeMoved( targetRange );

                if( null != splitRange )
                {
                    // Add the result of the split 
                    InternalAddHeightAngleRange( splitRange );
                }
            }
        }

        protected void FireHeightRangeAdded( AutoSplatHeightAngleRange range )
        {
            if( null != HeightRangeAdded )
            {
                HeightRangeAdded( this, range );
            }
        }

        protected void FireHeightRangeRemoved( AutoSplatHeightAngleRange range )
        {
            if( null != HeightRangeRemoved )
            {
                HeightRangeRemoved( this, range );
            }
        }

        protected void FireHeightRangeMoved( AutoSplatHeightAngleRange range )
        {
            if( null != HeightRangeMoved )
            {
                HeightRangeMoved( this, range );
            }
        }

        public IEnumerable<AutoSplatHeightAngleRange> GetHeightAngleRanges()
        {
            return m_RangeList;
        }

        const int SAND_INDEX = 0;
        const int GRASS_INDEX = 1;
        const int ROCK_INDEX = 2;
        const int SNOW_INDEX = 3;

        private void ConvertConfigToRules( AutoSplatConfig autoSplatConfig )
        {
            // Initialize textures

            layerTextureNames[ SAND_INDEX ]  = autoSplatConfig.SandTextureName;
            layerTextureNames[ GRASS_INDEX ] = autoSplatConfig.GrassTextureName;
            layerTextureNames[ ROCK_INDEX ]  = autoSplatConfig.RockTextureName;
            layerTextureNames[ SNOW_INDEX ]  = autoSplatConfig.SnowTextureName;

            // Create height angle ranges bottom-up; in all cases we use rock for 
            // the vertical -- the steeper the rockier. Actually, more accurately, 
            // we don't use full vertical; in the auto splat normal rules, we only 
            // go up to one radian.  So each of these is actually a triplet, where 
            // we use the same texture for up to 57.2 degrees, then start adding rock.

            AutoSplatHeightAngleRange range;
            float angleStart = 0f;
            float angleMid   = 90f - 57.2957795f;
            float angleEnd   = 90f;

            // Min height - Sand
            range = new AutoSplatHeightAngleRange( Math.Min( MinHeightMM, 0 ) );
            range.AddAngleTexturePair( angleStart, SAND_INDEX );
            range.AddAngleTexturePair( angleMid, SAND_INDEX );
            range.AddAngleTexturePair( angleEnd, ROCK_INDEX );
            InternalAddHeightAngleRange( range );

            // Grass - blend inflection point for grass
            range = new AutoSplatHeightAngleRange( (long) autoSplatConfig.SandToGrassHeight );
            range.AddAngleTexturePair( angleStart, GRASS_INDEX );
            range.AddAngleTexturePair( angleMid, GRASS_INDEX );
            range.AddAngleTexturePair( angleEnd, ROCK_INDEX );
            InternalAddHeightAngleRange( range );

            // Rock - blend inflection point for rock
            range = new AutoSplatHeightAngleRange( (long) autoSplatConfig.GrassToRockHeight );
            range.AddAngleTexturePair( angleStart, ROCK_INDEX );
            range.AddAngleTexturePair( angleEnd, ROCK_INDEX );
            InternalAddHeightAngleRange( range );

            // Snow - blend inflection point for snow
            range = new AutoSplatHeightAngleRange( (long) autoSplatConfig.RockToSnowHeight );
            range.AddAngleTexturePair( angleStart, SNOW_INDEX );
            range.AddAngleTexturePair( angleMid, SNOW_INDEX );
            range.AddAngleTexturePair( angleEnd, ROCK_INDEX );
            InternalAddHeightAngleRange( range );

            // Max height - Snow!
            if( MaxHeightMM > autoSplatConfig.RockToSnowHeight )
            {
                // Max height - Snow!
                range = new AutoSplatHeightAngleRange( MaxHeightMM );
                range.AddAngleTexturePair( angleStart, SNOW_INDEX );
                range.AddAngleTexturePair( angleEnd, SNOW_INDEX );
                InternalAddHeightAngleRange( range );
            }
        }

        #region Legacy CG simulation

        private double clamp( double input )
        {
            if( input < 0.0 )
            {
                return 0.0;
            }
            else if( input > 1.0 )
            {
                return 1.0;
            }
            return input;
        }

        // This function is likely to stop being used, but please keep it in the tree
        // for a bit.  It's a direct translation of the CG code found in the tree
        // under Media/Common/GpuPrograms/Terrain.cg to do AutoSplatting.  It's 
        // helpful to debug when an autosplat conversion comes out wrong.
        public float[] GetLegacyAutoSplatSampleNormalized( long inputHeightMM, Vector3 normal )
        {
            // ripped from terrain.cg
            double[] norms = new double[ AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES ];

            for( int i = 0; i < norms.Length; i++ )
            {
                norms[ i ] = 0;
            }

            double no_rock_factor = clamp( normal.y );
            double rock_factor = 1 - no_rock_factor;
            double heightMM = inputHeightMM;

            double SAND_GRASS_HEIGHT = m_RangeList[ 1 ].HeightMM;
            double GRASS_ROCK_HEIGHT = m_RangeList[ 2 ].HeightMM;
            double ROCK_SNOW_HEIGHT = m_RangeList[ 3 ].HeightMM;

            // sand factor
            if( heightMM < SAND_GRASS_HEIGHT )
            {
                norms[ SAND_INDEX ] =
                    clamp( (1.0 - (heightMM / SAND_GRASS_HEIGHT)) * no_rock_factor );
            }
            else
            {
                norms[ SAND_INDEX ] = 0;
            }
            // grass factor
            if( heightMM < SAND_GRASS_HEIGHT )
            {
                norms[ GRASS_INDEX ] =
                    clamp( (heightMM / SAND_GRASS_HEIGHT) * no_rock_factor );
            }
            else if( heightMM < GRASS_ROCK_HEIGHT )
            {
                norms[ GRASS_INDEX ] =
                    clamp( (1.0 - (heightMM - SAND_GRASS_HEIGHT) /
                    (GRASS_ROCK_HEIGHT - SAND_GRASS_HEIGHT)) * no_rock_factor );
            }
            else
            {
                norms[ GRASS_INDEX ] = 0;
            }
            // rock factor
            if( heightMM > SAND_GRASS_HEIGHT )
            {
                if( heightMM < GRASS_ROCK_HEIGHT )
                {
                    norms[ ROCK_INDEX ] =
                        clamp( ((heightMM - SAND_GRASS_HEIGHT) /
                        (GRASS_ROCK_HEIGHT - SAND_GRASS_HEIGHT)) * no_rock_factor );
                }
                else
                {
                    norms[ ROCK_INDEX ] =
                        clamp( (1.0 - (heightMM - GRASS_ROCK_HEIGHT) /
                        (ROCK_SNOW_HEIGHT - GRASS_ROCK_HEIGHT)) * no_rock_factor );
                }
            }
            else
            {
                norms[ ROCK_INDEX ] = 0;
            }
            // snow factor
            if( heightMM > GRASS_ROCK_HEIGHT )
            {
                norms[ SNOW_INDEX ] =
                    clamp( ((heightMM - GRASS_ROCK_HEIGHT) /
                    (ROCK_SNOW_HEIGHT - GRASS_ROCK_HEIGHT)) * no_rock_factor );
            }
            else
            {
                norms[ SNOW_INDEX ] = 0;
            }
            // needs more cowbell
            norms[ ROCK_INDEX ] = clamp( norms[ ROCK_INDEX ] + rock_factor );
            float[] ans = new float[ AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES ];
            for( int i = 0; i < ans.Length; i++ )
            {
                ans[ i ] = (float) clamp( norms[ i ] );
            }
            return ans;
        }

        #endregion Legacy CG simulation


        public float[] GetAutoSplatSampleNormalized( long heightMM, Vector3 normal )
        {
            AutoSplatHeightAngleRange lowerRange = null;
            AutoSplatHeightAngleRange higherRange = null;

            // We assume the ranges are sorted in increasing order by height
            foreach( AutoSplatHeightAngleRange range in m_RangeList )
            {
                if( range.HeightMM == heightMM )
                {
                    lowerRange = range;
                    higherRange = range;
                    break;
                }

                if( range.HeightMM < heightMM )
                {
                    lowerRange = range;
                    continue;
                }

                if( range.HeightMM > heightMM )
                {
                    higherRange = range;
                    // We should have both the lower & upper bounds now, so break;
                    break;
                }
            }

            if( lowerRange == null )
            {
                lowerRange = m_RangeList[ 0 ]; // allows us to continue
            }
            if( higherRange == null )
            {
                higherRange = m_RangeList[ m_RangeList.Count - 1 ]; // allows us to continue
            }

            // We want the angle of the normal relative to the XZ plane, so
            // we first use the dot product to get the angle to the Y-axis
            // which is perpendicular to the XZ plane, convert it to degress,
            // and then subtract it from 90.
            float angleRadians = normal.Dot( Vector3.UnitY );
            float angleDegrees = Convert.ToSingle( 90 - RadiansToDegrees( angleRadians ) );

            if( lowerRange == higherRange )
            {
                // No need to do any weighting since we at the exact height
                return lowerRange.GetAutoSplatSampleNormalized( angleDegrees );
            }

            // Compute the gradiant weighting for the lower & higher angled textures
            float lowerWeight;
            float higherWeight;

            long heightDiff = higherRange.HeightMM - lowerRange.HeightMM;

            if( heightDiff == 0 )
            {
                // Give equal weighting to both samples.
                // This covers the case when we have two ranges at the
                // same height....this really shouldn't happen due to the
                // way we choose the lower/higher ranges.
                lowerWeight = 0.5f;
                higherWeight = 0.5f;
            }
            else
            {
                // How close is the angle to the higher/lower angle?  Normalize
                // that distance from 0..1 and use that as the gradient weights
                higherWeight = ((float) (heightMM - lowerRange.HeightMM)) / heightDiff;
                lowerWeight = 1f - higherWeight;
            }


            float[] lowerNormalizedSample = lowerRange.GetAutoSplatSampleNormalized( angleDegrees );
            float[] higherNormalizedSample = higherRange.GetAutoSplatSampleNormalized( angleDegrees );

            float[] normalizedSample = new float[ AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES ];
            for( int i = 0; i < AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES; i++ )
            {
                normalizedSample[ i ] = lowerNormalizedSample[ i ] * lowerWeight +
                                      higherNormalizedSample[ i ] * higherWeight;
            }
            return normalizedSample;
        }

        public byte[] GetAutoSplatSampleBytes( long heightMM, Vector3 normal )
        {
            float[] normalizedSample = GetAutoSplatSampleNormalized( heightMM, normal );
            byte[] sampleBytes = ConvertNormalizedSamplesToBytes( normalizedSample );
            return sampleBytes;
        }

        public byte[] ConvertNormalizedSamplesToBytes( float[] normalizedSamples )
        {
            byte[] result = new byte[ AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES ];

            if( normalizedSamples == null )
            {
                return result;
            }

            if( normalizedSamples.Length != AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES )
            {
                throw new ArgumentException( 
                    "Internal Error: normalized samples should be an array of " + 
                    AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES + " floats" );
            }

            for( int i = 0; i < normalizedSamples.Length; i++ )
            {
                float sampleNormalized = normalizedSamples[ i ];
                result[ i ] = (byte) (sampleNormalized * 255f);
            }

            return result;
        }

        private double RadiansToDegrees( double angleRadians )
        {
            return angleRadians * (180.0 / Math.PI);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( "Rules [" );
            bool first = true;
            foreach( AutoSplatHeightAngleRange range in m_RangeList )
            {
                if( first )
                {
                    first = false;
                }
                else
                {
                    builder.AppendLine( ", " );
                }
                builder.Append( "    " );
                builder.Append( range.ToString() );
            }
            builder.AppendLine( "]" );

            return builder.ToString();
        }
    }
}

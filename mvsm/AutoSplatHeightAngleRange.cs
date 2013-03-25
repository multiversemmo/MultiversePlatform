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


namespace Axiom.SceneManagers.Multiverse
{
    public delegate void AngleMovedHandler( object sender, Guid pairId, float oldAngle, float newAngle );

    public delegate void TextureIndexChangedHandler( object sender, Guid pairId, int oldIndex, int newIndex );

    public delegate void AngleTexturePairHandler( object sender, Guid pairId, float angle, int textureIndex );

    public class AutoSplatHeightAngleRange : IComparable<AutoSplatHeightAngleRange>
    {
        #region Public interface

        #region Generated events

        public event AngleTexturePairHandler    AngleTexturePairAdded;
        public event AngleTexturePairHandler    AngleTexturePairRemoved;
        public event AngleMovedHandler          AngleMoved;
        public event TextureIndexChangedHandler TextureIndexChanged;

        #endregion Generated events

        public long HeightMM
        {
            get { return m_heightMM; }
            set { m_heightMM = value; }
        }
        long m_heightMM;

        public Guid Id { get; private set; }

        /// <summary>
        /// Pairs (and therfore Ids) for Horizontal and Vertical are guaranteed to always exist.
        /// </summary>
        public Guid HorizontalId { get { return m_AngleSortedPairs[ 0 ].Id; } }
        public Guid VerticalId { get { return m_AngleSortedPairs[ m_AngleSortedPairs.Count - 1 ].Id; } }

        public IEnumerable<Guid> AllIdsInAscendingAngleOrder
        {
            get
            {
                for( int i = 0; i < m_AngleSortedPairs.Count; i++ )
                {
                    yield return m_AngleSortedPairs[ i ].Id;
                }
            }
        }

        #region Constructors

        public AutoSplatHeightAngleRange( long heightMM )
        {
            Id = Guid.NewGuid();

            m_heightMM = heightMM;
        }

        public AutoSplatHeightAngleRange( long heightMM, int horizontalTextureIndex, int verticalTextureIndex ) :
            this(heightMM)
        {
            InternalAddAngleTexturePair( new AutoSplatAngleTexturePair( AutoSplatAngleTexturePair.Horizontal, horizontalTextureIndex ) );
            InternalAddAngleTexturePair( new AutoSplatAngleTexturePair( AutoSplatAngleTexturePair.Vertical, verticalTextureIndex ) );
        }

        public AutoSplatHeightAngleRange( long heightMM, AutoSplatHeightAngleRange other ) :
            this(heightMM)
        {
            foreach( AutoSplatAngleTexturePair otherPair in other.m_AngleSortedPairs )
            {
                InternalAddAngleTexturePair( new AutoSplatAngleTexturePair( otherPair ) );
            }
        }

        public AutoSplatHeightAngleRange( XmlReader reader )
        {
            Id = Guid.NewGuid();

            FromXml( reader );
        }

        #endregion Constructors

        #region Xml Serialization 

        public void FromXml( XmlReader reader )
        {
            Debug.Assert( reader.Name.Equals( "heightRange" ) );

            XmlReader subReader = reader.ReadSubtree();

            subReader.ReadToFollowing( "long" );
            Debug.Assert( subReader.GetAttribute( "name" ).Equals( "height" ) );
            m_heightMM = long.Parse( subReader.GetAttribute( "value" ) );

            subReader.ReadToFollowing( "slopes" );

            subReader.ReadToFollowing( "slope" );

            do
            {
                InternalAddAngleTexturePair( new AutoSplatAngleTexturePair( subReader ) );

            } while( subReader.ReadToNextSibling( "slope" ) );

            subReader.Close();
        }

        public void ToXml( XmlWriter writer )
        {
            writer.WriteStartElement( "heightRange" );

                writer.WriteStartElement( "long" );
                
                writer.WriteAttributeString( "name", "height" );
                writer.WriteAttributeString( "value", m_heightMM.ToString() );
                
                writer.WriteEndElement();
                
                writer.WriteStartElement( "slopes" );

                    foreach( AutoSplatAngleTexturePair pair in m_AngleSortedPairs )
                    {
                        pair.ToXml( writer );
                    }

                writer.WriteEndElement();

            writer.WriteEndElement();
        }

        #endregion Xml Serialization

        /// <summary>
        /// This is like an 'add' operation, but you need to restore the id of the
        /// pair literally because further operations in the undo stack will refer
        /// to the pair by id.
        /// </summary>
        /// <param name="angleDegrees">angle of the slope</param>
        /// <param name="textureIndex">texture associated with the slope</param>
        /// <param name="id">unique id by which the slope is accessed</param>
        /// <returns></returns>
        public AutoSplatAngleTexturePair UndoRemoveAngleTexturePair( float angleDegrees, int textureIndex, Guid id )
        {
            AutoSplatAngleTexturePair pair = new AutoSplatAngleTexturePair( angleDegrees, textureIndex, id );

            InternalAddAngleTexturePair( pair );

            return pair;
        }

        public AutoSplatAngleTexturePair AddAngleTexturePair( float angleDegrees, int textureIndex )
        {
            AutoSplatAngleTexturePair pair = new AutoSplatAngleTexturePair( angleDegrees, textureIndex );

            InternalAddAngleTexturePair( pair );

            return pair;
        }

        public void RemoveAngleTexturePair( Guid pairId )
        {
            InternalRemoveAngleTexturePair( GetPair( pairId ) );
        }

        public float GetAngleDegrees( Guid pairId )
        {
            Debug.Assert( m_IdToPairMap.ContainsKey( pairId ) );

            return m_IdToPairMap[ pairId ].AngleDegrees;
        }

        public int GetTextureIndex( Guid pairId )
        {
            Debug.Assert( m_IdToPairMap.ContainsKey( pairId ) );

            return m_IdToPairMap[ pairId ].TextureIndex;
        }

        public void MoveAngle( Guid pairId, float angleDegrees )
        {
            AutoSplatAngleTexturePair pair = GetPair( pairId );

            if( ! pair.AngleDegrees.Equals( angleDegrees ) )
            {
                AutoSplatAngleTexturePair clone = null;

                if( pair.IsCriticalAngle )
                {
                    clone = new AutoSplatAngleTexturePair( pair );
                    // We won't need to 'infinitesimally bump' the new angle because
                    // we already tested to establish that the values are not equal.
                }
                else
                {
                    // Guarenteed to be neither first nor last because it's not critical
                    int pairIndex = GetIndex( pairId );

                    float prevAngle = m_AngleSortedPairs[ pairIndex - 1 ].AngleDegrees;
                    float nextAngle = m_AngleSortedPairs[ pairIndex + 1 ].AngleDegrees;

                    if( angleDegrees < prevAngle || angleDegrees > nextAngle )
                    {
                        // Only Add and Remove should change the sorting order
                        throw new ConstraintException( 
                            "MoveAngle would change ordering of pairs;" + 
                            "only Add and Remove should change the sorting order." );
                    }

                    // If the move would make the angle coincide exactly with one of
                    // its neighbors, back it off a teensie-weensie bit.
                    const float infinitesimal = 0.0001f;
                    if( angleDegrees.Equals( prevAngle ) )
                    {
                        angleDegrees += infinitesimal;
                    }
                    else if( angleDegrees.Equals( nextAngle ) )
                    {
                        angleDegrees -= infinitesimal;
                    }
                }

                float oldAngle = pair.AngleDegrees;
                
                pair.AngleDegrees = angleDegrees;

                FireAngleMoved( pairId, oldAngle, angleDegrees );

                if( null != clone )
                {
                    // So we are splitting one of the critical pairs. By waiting until
                    // after we've moved the original pair to add the clone, we assure
                    // the sorting order stays the same for pre-existing pairs. The
                    // Outside World cares about this order.
                    InternalAddAngleTexturePair( clone );
                }
            }
        }

        public void SetPairTextureIndex( Guid pairId, int textureIndex )
        {
            int oldIndex = GetPair( pairId ).TextureIndex;

            if( ! textureIndex.Equals( oldIndex ) )
            {
                GetPair( pairId ).TextureIndex = textureIndex;

                FireTextureIndexChanged( pairId, oldIndex, textureIndex );
            }
        }

        public float[] GetAutoSplatSampleNormalized( float angleDegrees )
        {
            AutoSplatAngleTexturePair lowerPair = null;
            AutoSplatAngleTexturePair higherPair = null;

            // We assume the pairs are sorted in increasing order by angle
            foreach( AutoSplatAngleTexturePair pair in m_AngleSortedPairs )
            {
                if( pair.AngleDegrees == angleDegrees )
                {
                    lowerPair = pair;
                    higherPair = pair;
                    break;
                }

                if( pair.AngleDegrees < angleDegrees )
                {
                    lowerPair = pair;
                    continue;
                }

                if( pair.AngleDegrees > angleDegrees )
                {
                    higherPair = pair;
                    // We should have both the lower & upper bounds now, so break;
                    break;
                }
            }

            if (lowerPair == null)
            {
                Debug.Assert(lowerPair != null,
                              "Unable to find lower angle when getting autosplat sample.  Angle=" + angleDegrees + " Range=" +
                              this);
            }
            if (higherPair == null)
            {
                Debug.Assert(higherPair != null,
                              "Unable to find higher angle when getting autosplat sample.  Angle=" + angleDegrees +
                              " Range=" + this);
            }

            // Compute the gradiant weighting for the lower & higher angled textures
            float lowerWeight;
            float higherWeight;

            float angleDiff = higherPair.AngleDegrees - lowerPair.AngleDegrees;

            if( angleDiff == 0 || lowerPair.TextureIndex == higherPair.TextureIndex)
            {
                lowerWeight = 0f;
                higherWeight = 1f;
            }
            else
            {
                // How close is the angle to the higher/lower angle?  Normalize
                // that distance from 0..1 and use that as the gradient weights
                higherWeight = (angleDegrees - lowerPair.AngleDegrees) / angleDiff;
                lowerWeight = 1f - higherWeight;
            }

            float[] normalizedSample = new float[AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES];

            // It's important that we set higher second because
            // if lower & higher angle are equal the same, we
            // set the higherWeight to 1f above and want to make 
            // sure it's set that way here.
            normalizedSample[ lowerPair.TextureIndex ] = lowerWeight;
            normalizedSample[ higherPair.TextureIndex ] = higherWeight;

            return normalizedSample;
        }

        public IEnumerable<AutoSplatAngleTexturePair> GetAngleTexturePairs()
        {
            return m_AngleSortedPairs;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( m_heightMM );
            builder.Append( "mm [" );
            bool first = true;
            foreach( AutoSplatAngleTexturePair pair in m_AngleSortedPairs )
            {
                if( first )
                {
                    first = false;
                }
                else
                {
                    builder.Append( ", " );
                }
                builder.Append( pair );
            }
            builder.Append( "]" );

            return builder.ToString();
        }

        #region Implementation of IComparable<AutoSplatHeightAngleRange>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo( AutoSplatHeightAngleRange other )
        {
            long diff = m_heightMM - other.m_heightMM;
            return (int) diff;
        }

        #endregion

        #endregion Public interface

        #region Private implementation

        readonly List<AutoSplatAngleTexturePair> m_AngleSortedPairs = new List<AutoSplatAngleTexturePair>();

        readonly Dictionary<Guid,AutoSplatAngleTexturePair> m_IdToPairMap = new Dictionary<Guid, AutoSplatAngleTexturePair>();

        void InternalAddAngleTexturePair( AutoSplatAngleTexturePair pair )
        {
            if( pair == null )
            {
                throw new NullReferenceException();
            }

            if( m_IdToPairMap.ContainsKey( pair.Id ) )
            {
                throw new ArgumentException( "Key already in dictionary" );
            }

            // Make sure the pair *angle* to be added is not a duplicated.
            foreach( AutoSplatAngleTexturePair atPair in m_AngleSortedPairs )
            {
                if( atPair.AngleDegrees.Equals( pair.AngleDegrees ) )
                {
                    throw new ArgumentException( "Duplicate angles are not allowed." );
                }
            }

            m_AngleSortedPairs.Add( pair );
            m_AngleSortedPairs.Sort(); // Keep sorted by angle

            m_IdToPairMap.Add( pair.Id, pair );

            FireAngleTexturePairAdded( pair );
        }

        void InternalRemoveAngleTexturePair( AutoSplatAngleTexturePair pair )
        {
            // You are not allowed to remove critical pairs!  We have a design
            // constraint that there are always a horizontal and a vertical pair.
            //
            // This is not as constraining as it might sound. Assume the 'remove'
            // request is the outcome of a GUI action--like dragging to the trash.
            // Even if you attempt to remove a critical pair, the act of moving it
            // to the trash hotspot will move it away from the critical position,
            // which in turn causes a new pair to be inserted to replace it in the 
            // critical position.  You could do it all day if that's how you like
            // to spend your time.

            if( ! pair.IsCriticalAngle )
            {
                m_IdToPairMap.Remove( pair.Id );

                m_AngleSortedPairs.Remove( pair );
                
                FireAngleTexturePairRemoved( pair );
            }
        }

        AutoSplatAngleTexturePair GetPair( Guid pairId )
        {
            if( m_IdToPairMap.ContainsKey( pairId ) )
            {
                return m_IdToPairMap[ pairId ];
            }

            throw new ArgumentException( "Attempting to get an unregistered AngleTexturePair" );
        }

        int GetIndex( Guid pairId )
        {
            int index = m_AngleSortedPairs.IndexOf( GetPair( pairId ) );

            if( index >= 0 )
            {
                return index;
            }

            throw new IndexOutOfRangeException();
        }

        #region Event firing

        void FireAngleMoved( Guid pairId, float oldAngle, float newAngle )
        {
            if( null != AngleMoved )
            {
                AngleMoved( this, pairId, oldAngle, newAngle );
            }
        }

        void FireTextureIndexChanged( Guid pairId, int oldIndex, int newIndex )
        {
            if( null != TextureIndexChanged )
            {
                TextureIndexChanged( this, pairId, oldIndex, newIndex );
            }
        }

        void FireAngleTexturePairAdded( AutoSplatAngleTexturePair pair )
        {
            if( null != AngleTexturePairAdded )
            {
                AngleTexturePairAdded( this, pair.Id, pair.AngleDegrees, pair.TextureIndex );
            }
        }

        void FireAngleTexturePairRemoved( AutoSplatAngleTexturePair pair )
        {
            if( null != AngleTexturePairRemoved )
            {
                AngleTexturePairRemoved( this, pair.Id, pair.AngleDegrees, pair.TextureIndex );
            }
        }

        #endregion Event firing

        #endregion Private implementation
    }
}

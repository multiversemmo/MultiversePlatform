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
using System.Diagnostics;
using System.Xml;


namespace Axiom.SceneManagers.Multiverse
{
    public delegate void AutoSplatTextureIndexChanged( AutoSplatAngleTexturePair sender, int oldTextureIndex, int newTextureIndex );

    public delegate void AutoSplatAngleChanged( AutoSplatAngleTexturePair sender, float oldAngle, float newAngle );

    public class AutoSplatAngleTexturePair : IComparable<AutoSplatAngleTexturePair>
    {
        public event AutoSplatTextureIndexChanged TextureIndexChanged;
        public event AutoSplatAngleChanged        AngleChanged;

        public const float Vertical   = 90;
        public const float Horizontal = 0;

        #region Constructors

        public AutoSplatAngleTexturePair( float angleDegrees, int textureIndex )
            : this( angleDegrees, textureIndex, Guid.NewGuid() )
        {}

        // This c'tor overload is intended specifically for undo operations where you
        // have to restore an id literally as well as the semantics of the pair.  If
        // not performing an undo, use the overload that just takes angle/index.
        public AutoSplatAngleTexturePair( float angleDegrees, int textureIndex, Guid id )
        {
            Id = id;

            m_angleDegrees = angleDegrees;
            m_textureIndex = textureIndex;

            Validate( m_angleDegrees, m_textureIndex );
        }

        public AutoSplatAngleTexturePair( AutoSplatAngleTexturePair other ) 
            : this( other.AngleDegrees, other.TextureIndex )            
        {}

        public AutoSplatAngleTexturePair( XmlReader reader )
        {
            Id = Guid.NewGuid();

            FromXml( reader );
        }

        #endregion Constructors
        
        #region Xml Serialization 

        public void FromXml( XmlReader reader )
        {
            Debug.Assert( reader.Name.Equals( "slope" ) );

            XmlReader subReader = reader.ReadSubtree();

            subReader.ReadToFollowing( "float" );
            Debug.Assert( subReader.GetAttribute( "name" ).Equals( "angleDegrees" ) );
            m_angleDegrees = float.Parse( subReader.GetAttribute( "value" ) );

            subReader.ReadToFollowing( "int" );
            Debug.Assert( subReader.GetAttribute( "name" ).Equals( "textureIndex" ) );
            m_textureIndex = int.Parse( subReader.GetAttribute( "value" ) );

            subReader.Close();
        }

        public void ToXml( XmlWriter writer )
        {
            writer.WriteStartElement( "slope" );

            writer.WriteStartElement( "float" );
            writer.WriteAttributeString( "name", "angleDegrees" );
            writer.WriteAttributeString( "value", AngleDegrees.ToString() );
            writer.WriteEndElement();

            writer.WriteStartElement( "int" );
            writer.WriteAttributeString( "name", "textureIndex" );
            writer.WriteAttributeString( "value", TextureIndex.ToString() );
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        #endregion Xml Serialization 


        public Guid Id { get; private set; }

        /// <summary>
        /// An angle is critical because each range must contain at least a horizontal and a vertical pair.
        /// </summary>
        public bool IsCriticalAngle { get { return IsHorizontal || IsVertical; } }
        public bool IsHorizontal    { get { return AngleDegrees.Equals( Horizontal ); } }
        public bool IsVertical      { get { return AngleDegrees.Equals( Vertical ); } }

        public float AngleDegrees
        {
            get { return m_angleDegrees; }
            internal set
            {
                if( m_angleDegrees == value )
                {
                    return;
                }

                Validate( value, m_textureIndex );

                float oldAngle = m_angleDegrees;

                m_angleDegrees = value;
                
                FireAngleChanged( oldAngle, m_angleDegrees );
            }
        }
        float m_angleDegrees;

        public int TextureIndex
        {
            get { return m_textureIndex; }

            set
            {
                if( m_textureIndex == value )
                {
                    return;
                }

                Validate( m_angleDegrees, value );

                int oldIndex = m_textureIndex;

                m_textureIndex = value;

                FireTextureIndexChanged( oldIndex, m_textureIndex );
            }
        }
        int m_textureIndex;

        protected void FireTextureIndexChanged( int oldIndex, int newIndex )
        {
            if( null != TextureIndexChanged )
            {
                TextureIndexChanged( this, oldIndex, newIndex );
            }
        }

        protected void FireAngleChanged( float oldAngle, float newAngle )
        {
            if( null != AngleChanged )
            {
                AngleChanged( this, oldAngle, newAngle );
            }
        }


        void Validate( float angleDegress, int textureIndex )
        {
            if( angleDegress < Horizontal ||
                angleDegress > Vertical )
            {
                throw new ArgumentOutOfRangeException( "angleDegress",
                                                       "Angle must be between 0 and 90 degrees inclusive." );
            }

            if( textureIndex >= AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES )
            {
                throw new ArgumentOutOfRangeException( "textureIndex",
                                                       "Layer texture index greater than or equal to MAX_LAYER_TEXTURES.  Index: " +
                                                       textureIndex + " Max: " +
                                                       (AlphaSplatTerrainConfig.MAX_LAYER_TEXTURES) );
            }
        }

        #region Implementation of IComparable<AutoSplatAngleTexturePair>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects 
        /// being compared. The return value has the following meanings: 
        /// Value Meaning 
        ///     Less than zero This object is less than the  parameter.
        ///     Zero This object is equal to the other. 
        ///     Greater than zero This object is greater than the other.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo( AutoSplatAngleTexturePair other )
        {
            if( m_angleDegrees.Equals( other.m_angleDegrees ) )
            {
                return m_textureIndex - other.m_textureIndex;
            }

            return (m_angleDegrees > other.m_angleDegrees)
                       ? 1
                       : -1;
        }

        #endregion

        public override bool Equals( object obj )
        {
            AutoSplatAngleTexturePair pair = obj as AutoSplatAngleTexturePair;
            if( pair == null )
            {
                return false;
            }
            return CompareTo( pair ) == 0;
        }

        public override int GetHashCode()
        {
            return (int) m_angleDegrees << 8 + m_textureIndex;
        }

        public override string ToString()
        {
            return string.Format( "[{0}�,#{1}]", m_angleDegrees, m_textureIndex );
        }

        int IComparable<AutoSplatAngleTexturePair>.CompareTo( AutoSplatAngleTexturePair other )
        {
            return CompareTo( other );
        }
    }
}

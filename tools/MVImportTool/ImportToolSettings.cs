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

using Microsoft.Win32;

namespace MVImportTool
{
    interface IToolSettings
    {
        void Save();
        void Restore();
    }

    /// <summary>
    /// This is a helper class to abstract settings of the application that
    /// can be saved/restored.
    /// </summary>
    internal class ImportToolSettings : IToolSettings
    {
        internal class SaveableAttribute : Attribute
        {
            internal string Name;

            /// <summary>
            /// Decorates values that can be saved in the registry
            /// </summary>
            /// <param name="name">name by which the value is accessed</param>
            internal SaveableAttribute( string name )
            {
                Name = name;
            }
        }

        #region Public properties
        [Saveable( "SourceFolder" )]
        public string SourceFolder
        {
            get { return m_SourceFolder; }
            set { m_SourceFolder = value; }
        }
        string m_SourceFolder;

        [Saveable( "SourceFilter" )]
        public string SourceFilter
        {
            get { return m_SourceFilter; }
            set { m_SourceFilter = value; }
        }
        string m_SourceFilter;

        [Saveable( "CheckedRepositories" )]
        public List<string> CheckedRepositories
        {
            get { return m_CheckedRepositories; }
            set { m_CheckedRepositories = value; }
        }
        List<string> m_CheckedRepositories;

        [Saveable( "ConversionToolExeFile" )]
        public string ConversionToolExeFile
        {
            get { return m_ConversionToolExeFile; }
            set { m_ConversionToolExeFile = value; }
        }
        string m_ConversionToolExeFile;

        [Saveable( "WorkingFolder" )]
        public string WorkingFolder
        {
            get { return m_WorkingFolder; }
            set { m_WorkingFolder = value; }
        }
        string m_WorkingFolder;

        [Saveable( "ConvertSource" )]
        public bool ConvertSource
        {
            get { return m_ConvertSource; }
            set { m_ConvertSource = value; }
        }
        bool m_ConvertSource;

        [Saveable( "InstallResult" )]
        public bool InstallResult
        {
            get { return m_InstallResult; }
            set { m_InstallResult = value; }
        }
        bool m_InstallResult;


        [Saveable( "ConfirmInstall" )]
        public bool ConfirmInstall
        {
            get { return m_ConfirmInstall; }
            set { m_ConfirmInstall = value; }
        }
        bool m_ConfirmInstall;


        [Saveable( "OverwritePolicy" )]
        public uint OverwritePolicy
        {
            get { return m_OverwritePolicy; }
            set { m_OverwritePolicy = value; }
        }
        uint m_OverwritePolicy;

        [Saveable( "ConversionCommandFlags" )]
        public string ConversionCommandFlags
        {
            get { return m_ConversionCommandFlags; }
            set { m_ConversionCommandFlags = value; }
        }
        string m_ConversionCommandFlags;


        public uint InstallFilter
        {
            get { return m_InstallFilter; }
            set { m_InstallFilter = value; }
        }
        uint m_InstallFilter;

        #endregion Public properties

        internal ImportToolSettings()
        {
        }

        #region IToolSettings Members

        public void Save()
        {
#if DO_IT_THE_RIGHT_WAY
                // Use reflection to find saveable properties.
                foreach( PropertyInfo info in this.GetType().GetProperties() )
                {
                    object[] attr = info.GetCustomAttributes( SaveableAttribute, true );

                    if( 0 < attr.Length )
                    {
                        SaveableAttribute sAttr = attr as SaveableAttribute;

                        object obj = info.GetValue( this, null );

                        if( obj is string[] )
                        {
                        }
                        else
                        {
                        }
                    }
                }
#else
            SetString( "SourceFolder", SourceFolder );
            SetString( "SourceFilter", SourceFilter );
            SetStrings( "CheckedRepositories", CheckedRepositories );
            SetString( "ConversionToolExeFile", ConversionToolExeFile );
            SetString( "WorkingFolder", WorkingFolder );
            SetBool( "ConvertSource", ConvertSource );
            SetBool( "InstallResult", InstallResult );
            SetUint( "InstallFilter", InstallFilter );
            SetBool( "ConfirmInstall", ConfirmInstall );
            SetUint( "OverwritePolicy", OverwritePolicy );
            SetString( "ConversionCommandFlags", ConversionCommandFlags );
#endif
        }


        public void Restore()
        {
            SourceFolder = GetString( "SourceFolder" );
            SourceFilter = GetString( "SourceFilter" );
            CheckedRepositories = GetStrings( "CheckedRepositories" );
            ConversionToolExeFile = GetString( "ConversionToolExeFile" );
            WorkingFolder = GetString( "WorkingFolder" );
            ConvertSource = GetBool( "ConvertSource" );
            InstallResult = GetBool( "InstallResult" );
            InstallFilter = GetUint( "InstallFilter" );
            ConfirmInstall = GetBool( "ConfirmInstall" );
            OverwritePolicy = GetUint( "OverwritePolicy" );
            ConversionCommandFlags = GetString( "ConversionCommandFlags" );
        }

        #endregion

        #region Registry key twiddling

        RegistryKey m_ImportToolKey;

        string GetString( string setting )
        {
            if( OpenImportToolKey() )
            {
                object keyValue = m_ImportToolKey.GetValue( setting );

                if( null != keyValue )
                {
                    return keyValue.ToString();
                }
            }
            return String.Empty;
        }

        bool GetBool( string setting )
        {
            try { return Convert.ToBoolean( GetString( setting ) ); }
            catch { return false; }
        }

        uint GetUint( string setting )
        {
            try { return Convert.ToUInt32( GetString( setting ) ); }
            catch { return 0; }
        }

        List<string> GetStrings( string setting )
        {
            List<string> valueList = new List<string>();

            object value = m_ImportToolKey.GetValue( setting );

            if( null != value )
            {
                string[] values = value as string[];

                if( null != values )
                {
                    valueList.AddRange( values );
                }
            }

            return valueList;
        }

        void SetString( string setting, string value )
        {
            if( OpenImportToolKey() )
            {
                m_ImportToolKey.SetValue( setting, value );
            }
        }

        void SetBool( string setting, bool value )
        {
            SetString( setting, value.ToString() );
        }

        void SetUint( string setting, uint value )
        {
            SetString( setting, value.ToString() );
        }

        void SetStrings( string setting, List<string> value )
        {
            if( OpenImportToolKey() )
            {
                m_ImportToolKey.SetValue( setting, value.ToArray(), RegistryValueKind.MultiString );
            }
        }

        // If the ImportTool key is not already open, open it read/write.
        // This creates the key if it does not already exist.
        // Returns true if the key is open.
        bool OpenImportToolKey()
        {
            if( null == m_ImportToolKey )
            {
                using( RegistryKey mvRoot = GetMultiverseRegistryRoot() )
                {
                    if( null != mvRoot )
                    {
                        m_ImportToolKey = mvRoot.OpenSubKey( "ImportTool", true );

                        // If the key does not exist, create it
                        if( null == m_ImportToolKey )
                        {
                            m_ImportToolKey = mvRoot.CreateSubKey( "ImportTool" );
                        }
                    }
                }
            }

            return (null != m_ImportToolKey);
        }

        RegistryKey GetMultiverseRegistryRoot()
        {
            RegistryKey multiverseRootKey = null;

            using( RegistryKey usrRoot = Registry.CurrentUser )
            {
                RegistryKey softwareKey = usrRoot.OpenSubKey( "Software" );
                multiverseRootKey = softwareKey.OpenSubKey( "Multiverse", true );
            }

            return multiverseRootKey;
        }

        #endregion Registry key twiddling
    }
}

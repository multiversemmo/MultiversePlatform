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
using System.Windows.Forms;

namespace MVImportTool
{
    /// <summary>
    /// This class is used to install converted components into a repository.
    /// The components installed include the direct results of the conversion
    /// (e.g. '.mesh' files) as well as files the components depend on.  In
    /// particular, '.material' files depend on textures, but the paths to
    /// image files are not mentioned in the '.material' file; hence, the 
    /// the installer will crawl the '.dae' file to find the texture sources.
    /// 
    /// TODO: The converted components are assumed to be in the same directory
    /// as the '.dae' file.  Currently the ConversionTool always builds into 
    /// the same directory as the source file, but conceivably that might 
    /// change in the future.  Maybe the filename and working directory should
    /// be specified separately....?
    /// </summary>
    internal class RepositoryInstaller
    {
        #region Properties

        /// <summary>
        /// Global settings for filtering files by component type. These
        /// allow yo uto select the types of files to install.
        /// </summary>
        [Flags]
        internal enum InstallFilterFlags
        {
            Mesh      = 0x01,
            Materials = 0x02,
            Skeleton  = 0x04,
            Physics   = 0x08,
            Textures  = 0x10,
            All = (Mesh | Materials | Skeleton | Physics | Textures)
        }

        internal InstallFilterFlags FilterFlags
        {
            get { return m_FilterFlags; }
            set { m_FilterFlags = value; }
        }
        InstallFilterFlags m_FilterFlags;

        /// <summary>
        /// The choices presented to the user regarding overwriting files.
        /// </summary>
        [Flags]
        internal enum OverwritePolicyFlags
        {
            Always,
            Never,
            Ask
        }

        internal OverwritePolicyFlags OverwritePolicy
        {
            get { return m_OverwritePolicy; }
            set 
            { 
                m_OverwritePolicy = value; 
                InstallationCandidate.OverwritePolicy = value;
            }
        }
        OverwritePolicyFlags m_OverwritePolicy;

        /// <summary>
        /// If true, present the user with a dialog before actually copying
        /// files to the repository.  The dialog allows you to select which
        /// files get installed on an individual basis.
        /// </summary>
        public bool ConfirmInstallation
        {
            get { return m_ConfirmInstallation; }
            set { m_ConfirmInstallation = value; }
        }
        bool m_ConfirmInstallation;

        #endregion Properties

        /// <summary>
        /// Construct an instance of this installer. The installer operates
        /// on a single DAE file.  If you want to install from another DAE
        /// file, create another instance.
        /// 
        /// This only installs files with a creation date after the earliest
        /// time specified by the argument.
        /// </summary>
        /// <param name="daeFile">the source DAE file</param>
        /// <param name="earliestTime">time the </param>
        /// <param name="log">log for reporting status; can be null</param>
        public RepositoryInstaller( string daeFile, DateTime earliestTime, ILog log )
        {
            m_DAEFile = daeFile;

            m_EarliestTime = earliestTime;

            m_LogWriter = new LogWriter( log );

            InstallationCandidate.Log = m_LogWriter;

            // These are the file types that can be produced; a particular
            // conversion run does not necessarilly produce all types.
            // TODO: It would be nice if these came from a config file.
            // TODO: Maybe make this a property that is set up by the client.
            m_ProductFiles.Add( ".mesh", null );
            m_ProductFiles.Add( ".material", null );
            m_ProductFiles.Add( ".skeleton", null );
            m_ProductFiles.Add( ".physics", null );

            // This associates the file type with a repository subdir 
            // TODO: Same as above...
            m_RepositorySubdirectories.Add( ".mesh", "Meshes" );
            m_RepositorySubdirectories.Add( ".material", "Materials" );
            m_RepositorySubdirectories.Add( ".skeleton", "Skeletons" );
            m_RepositorySubdirectories.Add( ".physics", "Physics" );

            m_Candidates.Add( "Meshes", new List<InstallationCandidate>() );
            m_Candidates.Add( "Materials", new List<InstallationCandidate>() );
            m_Candidates.Add( "Skeletons", new List<InstallationCandidate>() );
            m_Candidates.Add( "Physics", new List<InstallationCandidate>() );
            m_Candidates.Add( "Textures", new List<InstallationCandidate>() );

            m_TypeToFilterFlagMap.Add( "Meshes", InstallFilterFlags.Mesh );
            m_TypeToFilterFlagMap.Add( "Materials", InstallFilterFlags.Materials );
            m_TypeToFilterFlagMap.Add( "Skeletons", InstallFilterFlags.Skeleton );
            m_TypeToFilterFlagMap.Add( "Physics", InstallFilterFlags.Physics );
            m_TypeToFilterFlagMap.Add( "Textures", InstallFilterFlags.Textures );

            FindConversionProductFiles();

            FilterFlags = InstallFilterFlags.All;

            m_Finder = new TextureFinder( daeFile );

            //if( FindConversionProductFiles() )
            //{
            //    FilterFlags = InstallFilterFlags.All;

            //    m_Finder = new TextureFinder( daeFile );
            //}
        }

        /// <summary>
        /// Perform the installation to a single repository. This installs 
        /// all conversion products, and texture files referenced by the 
        /// source DAE file.
        /// </summary>
        /// <param name="targetRepository">repository path</param>
        public void InstallTo( string targetRepository )
        {
            ClearState();

            if( Directory.Exists( targetRepository ) )
            {
                ScheduleConversionFiles( targetRepository );

                ScheduleTextureFiles( targetRepository );

                WarnAboutMissingTextures();

                if( GetApprovalForInstallation( targetRepository ) )
                {
                    InstallApprovedFiles();
                }
            }
            else
            {
                string msg = String.Format(
                    "Cannot find repository '{0}'",
                    targetRepository );

                MessageBox.Show(
                    msg,
                    "Repository Installer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning );
            }
        }

        // Warn about texture files referenced in the DAE file that could
        // not be found; this does not prevent the rest of the installation
        // from proceding.
        // TODO: Some people may not like the pop-up nag--especially if they
        // are running this in a batch mode! :)  Maybe add a control that
        // switches the warning to the log stream.
        void WarnAboutMissingTextures()
        {
            if( IsTypeEnabled( "Textures" ) )
            {
                if( 0 < m_Finder.MissingTextures.Count )
                {
                    FileInfo daeInfo = new FileInfo( m_DAEFile );

                    StringBuilder msgText = new StringBuilder( String.Format(
                        "File <{0}> referenced image files that could \n" +
                        " not be found; they cannot be installed in the repository\n\n",
                        daeInfo.Name ) );

                    foreach( string imageId in m_Finder.MissingTextures.Keys )
                    {
                        msgText.Append( m_Finder.MissingTextures[ imageId ].FullName );
                        msgText.Append( "\n" );
                    }

                    MessageBox.Show(
                        msgText.ToString(),
                        "Repository Installer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning );
                }
            }
        }

        #region Internal state

        // Map the file extension to the repository subdirectory it gets installed to.
        readonly Dictionary<string, string> m_RepositorySubdirectories = new Dictionary<string, string>();

        readonly Dictionary<string, InstallFilterFlags> m_TypeToFilterFlagMap = new Dictionary<string, InstallFilterFlags>();

        // Map the file extension to the actual file of that type produced by the conversion.
        Dictionary<string, FileInfo> m_ProductFiles = new Dictionary<string, FileInfo>();

        // Candidates categorized by their types
        Dictionary<string, List<InstallationCandidate>> m_Candidates = new Dictionary<string, List<InstallationCandidate>>();

        string          m_DAEFile;
        DateTime        m_EarliestTime;
        TextureFinder   m_Finder;
        LogWriter       m_LogWriter;

        // Since an instance of this class can install into more than one 
        // repository, we need to reset the state prior to each install.
        void ClearState()
        {
            // We need a new copy of the keys because we are going to
            // modify the collection as we iterate.
            string[] types = new string[ m_Candidates.Keys.Count ];

            m_Candidates.Keys.CopyTo( types, 0 );

            foreach( string type in types )
            {
                if( m_ProductFiles.ContainsKey( type ) )
                {
                    m_ProductFiles[ type ] = null;
                }
                if( m_Candidates.ContainsKey( type ) )
                {
                    m_Candidates[ type ].Clear();
                }
            }
        }
        #endregion

        // Checks the type against the type-filter flags
        bool IsTypeEnabled( string type )
        {
            return Convert.ToBoolean( FilterFlags & m_TypeToFilterFlagMap[ type ] );
        }

        // Discover all the files produced from the source DAE file, e.g. .mesh file.
        // Note that we do a time-check to make sure that the files we found are
        // actually produced from the dae file, and not a relic from something earlier
        // that just happened to have the same name.
        //
        // Return false if no files were found.
        bool FindConversionProductFiles()
        {
            FileInfo daeInfo = new FileInfo( m_DAEFile );

            bool productFound = false;

            if( daeInfo.Exists )
            {
                List<string> types = new List<string>();
                types.AddRange( m_ProductFiles.Keys );

                foreach( string type in types )
                {
                    string prodFile = Path.ChangeExtension( m_DAEFile, type );

                    FileInfo prodInfo = new FileInfo( prodFile );

                    if( prodInfo.Exists )
                    {
                        if( m_EarliestTime < prodInfo.LastWriteTime )
                        {
                            m_ProductFiles[ type ] = prodInfo;
                            
                            productFound = true;
                        }
                    }
                }
            }

            if( ! productFound )
            {
                MessageBox.Show(
                    "No installable files were found.\n" +
                    "Did you forget to convert the DAE file?",
                    "Repository Installer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning );
            }

            return productFound;
        }

        #region Installation scheduling

        // Schedule converted files for installation. They actually get installed
        // after passing user filters that enable installation.
        void ScheduleConversionFiles( string target )
        {
            DirectoryInfo dirInfo = new DirectoryInfo( target );

            if( dirInfo.Exists )
            {
                foreach( string type in m_ProductFiles.Keys )
                {
                    if( null != m_ProductFiles[ type ] )
                    {
                        string typeSubdirectory = target + "\\" + m_RepositorySubdirectories[ type ];

                        string newFile = typeSubdirectory + "\\" + m_ProductFiles[ type ].Name;

                        ScheduleForInstallation( m_RepositorySubdirectories[ type ], m_ProductFiles[ type ], newFile );
                    }
                }
            }
        }

        // Schedule texture files for installation. They actually get installed
        // after passing user filters that enable installation.
        void ScheduleTextureFiles( string target )
        {
            string textureDir = target + "\\Textures\\";

            foreach( string imageId in m_Finder.Textures.Keys )
            {
                FileInfo textureFile = m_Finder.Textures[ imageId ];

                string newFile = textureDir + textureFile.Name;

                ScheduleForInstallation( "Textures", textureFile, newFile );
            }
        }

        void ScheduleForInstallation( string type, FileInfo source, string destination )
        {
            if( m_Candidates.ContainsKey( type ) )
            {
                InstallationCandidate candidate = new InstallationCandidate( source, destination );

                candidate.IsEnabled = IsTypeEnabled( type );

                m_Candidates[ type ].Add( candidate );
            }
        }
        #endregion Installation scheduling

        #region Installation--confirmation and copying

        // This is for fast lookup of the components approved by the user.
        // The value for each key will always be 'null'.
        Dictionary<string, object> m_ApprovedForInstallation;

        // Present the user with a detailed list of installable components.
        // The components marked if they are schedule for installation.
        // The use can select/unselect individual files for installation.
        bool GetApprovalForInstallation( string targetRepository )
        {
            InstallationPreviewDialog preview = new InstallationPreviewDialog();

            preview.TargetRepository = targetRepository;

            SortedList<string,object> candidateSources = new SortedList<string, object>();

            // Build the set of available components, marked if they are
            // scheduled for installation.
            foreach( string type in m_Candidates.Keys )
            {
                List<InstallationCandidate> candidateList = m_Candidates[ type ];

                foreach( InstallationCandidate candidate in candidateList )
                {
                    if( ! candidateSources.ContainsKey( candidate.Source.Name ) )
                    {
                        candidateSources.Add( candidate.Source.Name, null );

                        preview.Add( candidate.Destination, IsTypeEnabled( type ) );
                    }
                }
            }

            return BuildApprovedList( preview );
        }

        private bool BuildApprovedList( InstallationPreviewDialog preview )
        {
            m_ApprovedForInstallation = new Dictionary<string, object>();

            if( ConfirmInstallation )
            {
                if( DialogResult.OK != preview.ShowDialog() )
                {
                    return false;
                }
            }

            foreach( string approvedItem in preview.CheckedItems )
            {
                m_ApprovedForInstallation.Add( approvedItem, null );
            }

            return true;
        }

        class UserCancelException : Exception
        {
        }

        // Finally--Install the files the user approved.
        void InstallApprovedFiles()
        {
            try
            {
                foreach( string type in m_Candidates.Keys )
                {
                    List<InstallationCandidate> candidates = m_Candidates[ type ];

                    foreach( InstallationCandidate candidate in candidates )
                    {
                        if( m_ApprovedForInstallation.ContainsKey( candidate.Destination ) )
                        {
                            // User's selection overrides the global settings
                            candidate.IsEnabled = true;

                            candidate.Install();
                        }
                    }
                }
            }
            catch( UserCancelException )
            {
                m_LogWriter.WriteLine( "User canceled installation" );
            }
        }
        #endregion Installation--confirmation and copying

        #region Internal class: Installation Candidate
        // This represents a component that can be installed, but it must
        // be enabled for the installation to proceed.  The intent is to
        // have an item that can be scheduled for installation, but then 
        // present it to the user for approval before carrying out the 
        // installation.
        class InstallationCandidate
        {
            internal readonly FileInfo Source;
            internal readonly string Destination;

            internal static LogWriter Log;

            internal static OverwritePolicyFlags OverwritePolicy;

            internal bool IsEnabled
            {
                get { return m_IsEnabled; }
                set { m_IsEnabled = value; }
            }
            bool m_IsEnabled;

            internal InstallationCandidate( FileInfo source, string destination )
            {
                Source = source;
                Destination = destination;
            }

            internal void Install()
            {
                if( IsEnabled )
                {
                    FileInfo target = new FileInfo( Destination );

                    if( ! target.Directory.Exists )
                    {
                        target.Directory.Create();
                    }

                    if( IsOkayToOverwrite( target ) )
                    {
                        Log.WriteLine( "   Installing " + Destination );

                        Source.CopyTo( Destination, true );
                    }
                }
            }

            bool IsOkayToOverwrite( FileInfo fileInfo )
            {
                bool isOkay = true;

                switch( OverwritePolicy )
                {
                    case OverwritePolicyFlags.Never:
                    {
                        isOkay = ! fileInfo.Exists;
                        break;
                    }
                    case OverwritePolicyFlags.Ask:
                    {
                        isOkay = AskIsOkayToOverwrite( fileInfo );
                        break;
                    }
                }

                return isOkay;
            }

            bool AskIsOkayToOverwrite( FileInfo fileInfo )
            {
                string msg = String.Format(
                    "File already exists; do you want to overwrite it?\n" +
                    "(Click 'Cancel' to abort the installation.)\n" +
                    "\nFilename:\n"  +
                    "    " + fileInfo.FullName );

                DialogResult result = 
                    MessageBox.Show(
                            msg,
                            "Repository Installer",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question );

                if( DialogResult.Cancel == result )
                {
                    throw new UserCancelException();
                }

                return (DialogResult.Yes == result);
            }
        }
        #endregion Internal class

    }
}

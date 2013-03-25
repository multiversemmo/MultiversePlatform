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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace MVImportTool
{
    /// <summary>
    /// This is the main form for the stand-alone MV-Import Tool.
    /// </summary>
    public partial class MVImportToolForm : Form
    {
        public MVImportToolForm()
        {
            InitializeComponent();

            m_Settings.Restore();

            m_ConfigurationDialog.Settings = m_Settings;
            commandControl1.Settings = m_Settings;
        }

        private void MVImportToolForm_Load( object sender, EventArgs e )
        {
            m_Log = new LogWriter( logPanel1.Log );

            RestoreFromSavedSettings();

            TidyWorkspace();

            m_Worker.StandardErrorHandler = logPanel1.Log.DataReceivedHandler;
            m_Worker.StandardOutHandler = logPanel1.Log.DataReceivedHandler;

            SetStartButtonEnable( true );
        }


        private void StartButton_Click( object sender, EventArgs e )
        {
            SetStartButtonEnable( false );

            ShowLogTab();

            RunCommands();

            SetStartButtonEnable( true );
        }

        private void StopButton_Click( object sender, EventArgs e )
        {
            m_Worker.RequestAbort();
            
            SetStartButtonEnable( true );
        }

        private void ExitButton_Click( object sender, EventArgs e )
        {
            this.Close();
        }

        private void SetStartButtonEnable( bool isEnabled )
        {
            StartButton.Enabled =   isEnabled;
            StopButton.Enabled  = ! isEnabled;
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            UpdateSavedSettings();
        }

        protected void ShowCommandLineHelp()
        {
            ShowLogTab();

            m_Log.Clear();

            SetCommandExe();

            m_Worker.CommandArgs = "--help";
            m_Worker.Run();

            m_Log.Write( m_Worker.OutData );
        }

        protected void ShowLogTab()
        {
            ExecutionTabControl.SelectedTab = LogTabPage;
        }

        ImportToolSettings m_Settings = new ImportToolSettings();

        CommandWorker m_Worker = new CommandWorker();

        LogWriter m_Log;

        ImporterConfigurationDialog m_ConfigurationDialog = new ImporterConfigurationDialog();


        // This is the action that takes place when you click on the "Run" 
        // button.
        protected void RunCommands()
        {
            try
            {
                UpdateSavedSettings();

                SetCommandExe();

                ConvertFiles();
            }
            catch( WarningException ex )
            {
                MessageBox.Show(
                    ex.Message, "Import Tool", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning );
            }
            catch( Exception ex )
            {
                string msg =
                    "Unknown exception; contact technical support.\n" +
                    "  Message text from exception:\n" +
                    ex.Message;

                MessageBox.Show(
                    msg, "Import Tool", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning );
            }
        }

        private void SetCommandExe()
        {
            m_Worker.CommandExe = Path.GetFileName( m_Settings.ConversionToolExeFile );
            m_Worker.CommandDirectory = Path.GetDirectoryName( m_Settings.ConversionToolExeFile );
        }

        // This applies the conversion command to each of the files checked
        // in the 'Source Files' control, and installs the results into the
        // checked repositories.
        private void ConvertFiles()
        {
            m_Log.Clear();

            if( 0 == sourceFileSelector1.CheckedFiles.Count )
            {
                throw new WarningException(
                    "No COLLADA files are selected; you must check at\n" +
                    "least one file in the 'COLLADA Files' tab." );
            }

            foreach( FileInfo info in sourceFileSelector1.CheckedFiles )
            {
                try
                {
                    string workingFilename = CopyToWorkingFolder( info );

                    DateTime startTime = ConvertOneFile( workingFilename );

                    InstallResultFiles( workingFilename, startTime );
                }
                catch( WarningException ex )
                {
                    throw ex;
                }
                catch( Exception ex )
                {
                    m_Log.WriteLine( String.Format(
                        "\nCommand failed on file <{0}>\n" +
                        "Details:\n  " +
                        ex.Message,
                        info.Name ) );
                }
            }
        }

        // Return the start-time of the conversion process.
        private DateTime ConvertOneFile( string workingFilename )
        {
            DateTime startTime = DateTime.MaxValue;

            if( m_Settings.ConvertSource )
            {
                try
                {
                    m_Worker.CommandArgs = BuildArgs( workingFilename );

                    // Issue some status chatter to the log to inform the user
                    // of action taking place.
                    m_Log.WriteLine( String.Format(
                         "===========[ Converting '{0}' ]=============",
                         Path.GetFileName( workingFilename ) ) );


                    m_Log.WriteLine( "Command: " + m_Worker.CommandArgs );

                    m_Log.WriteLine(
                         "===========================================================" );

#if THREADING_WORKS
                    // TODO: This should be running in a thread, but I have not yet
                    // been able to get the async stdout/stderr reading from the
                    // subprocess to work right.

                        System.Threading.Thread runThread =
                            new System.Threading.Thread( worker.RunInThread );
                        runThread.Start();
#else
                    startTime = DateTime.Now;   // get a little more precision...

                    int exitCode = m_Worker.Run();

                    if( 0 != exitCode )
                    {
                        throw new Exception( String.Format(
                            "ConversionTool exit code: '{0}'", exitCode ) );
                    }
#endif
                }
                finally
                {
                    m_Log.Write( m_Worker.OutData );
                }
            }

            return startTime;
        }

        private string CopyToWorkingFolder( FileInfo fileInfo )
        {
            m_Log.WriteLine( String.Format( 
                "\n>>> Copy <{0}> to work folder", fileInfo.Name ) );

            if( ! Directory.Exists( m_Settings.WorkingFolder ) )
            {
                Directory.CreateDirectory( m_Settings.WorkingFolder );
            }

            string newFile = Path.Combine( m_Settings.WorkingFolder, fileInfo.Name );

            fileInfo.CopyTo( newFile, true );

            return newFile;
        }

        private string BuildArgs( string sourceFilename )
        {
            string name = sourceFilename.Replace( '\\', '/' );

            // ConversionTool command parser needs the path to the source in quotes
            // if there are spaces in the path.  To get the quotes into the args
            // list, you need to escape them in the DOS shell. Hence, this
            // hyper-escaped quoting around the name.
            string args = m_Settings.ConversionCommandFlags + @" \""" + name + @"\"" ";

            return args;
        }

        private void InstallResultFiles( string sourceFilename, DateTime startTime )
        {
            if( m_Settings.InstallResult )
            {
                if( 0 == repositoryControl1.CheckedRespositoryPaths.Count )
                {
                    throw new WarningException(
                        "You have enabled 'Copy files to Asset Repositories', but no \n" +
                        "repositories are selected. You must check at least one repository\n" +
                        "on the 'Repositories' tab to install conversion results." );
                }

                FileInfo sourceInfo = new FileInfo( sourceFilename );

                m_Log.WriteLine( " " );
                m_Log.WriteLine(
                    "===[ Copying ]=============================================" );
                m_Log.WriteLine(
                    String.Format( 
                    ">>> From\t<{0}>", sourceInfo.Directory.FullName ) );

                // Implicitly, this asks if we did a conversion pass, or are we just
                // installing from a recent conversion.
                if( startTime.Equals( DateTime.MaxValue ) )
                {
                    startTime = GetEarliestReasonableTimeForInstallableFiles( sourceFilename );
                }

                RepositoryInstaller installer = new RepositoryInstaller( sourceFilename, startTime, logPanel1.Log );

                installer.FilterFlags = (RepositoryInstaller.InstallFilterFlags) m_Settings.InstallFilter;

                installer.ConfirmInstallation = m_Settings.ConfirmInstall;

                installer.OverwritePolicy = (RepositoryInstaller.OverwritePolicyFlags) m_Settings.OverwritePolicy;

                foreach( string repository in repositoryControl1.CheckedRespositoryPaths )
                {
                    m_Log.WriteLine(
                        "===[ Copying ]=============================================" );
                    m_Log.WriteLine(
                        String.Format( ">>> To\t<{0}>", repository ) );
                    m_Log.WriteLine(
                        "===========================================================" );

                    installer.InstallTo( repository );
                }
            }
        }

        // This becomes relevant when you run a copy-files command with the
        // convert-files disabled. 
        DateTime GetEarliestReasonableTimeForInstallableFiles( string filename )
        {
            FileInfo info = new FileInfo( filename );

            if( info.Exists )
            {
                return info.LastWriteTime;
            }
            else
            {
                m_Log.WriteLine( "Warning: Attempting to install Axiom files, but the source DAE file" );
                m_Log.WriteLine( "         is missing from the working folder. " );
                m_Log.WriteLine( "         You might want to re-convert the DAE file." );

                return DateTime.Today;
            }
        }

        private void UpdateSavedSettings()
        {
            m_Settings.SourceFolder = sourceFileSelector1.Folder;
            m_Settings.SourceFilter = sourceFileSelector1.FileFilter;
            
            m_Settings.CheckedRepositories = repositoryControl1.CheckedRespositoryPaths;

            m_Settings.Save();
        }

        private void RestoreFromSavedSettings()
        {
            try
            {
                sourceFileSelector1.Folder = m_Settings.SourceFolder;
                sourceFileSelector1.FileFilter = m_Settings.SourceFilter;
            }
            catch( ArgumentException )
            {
                string msg = 
                    "Unable to restore the saved setting for the source folder.\n" +
                    "Saved source location was: \n\n    " +
                    m_Settings.SourceFolder;

                MessageBox.Show(
                    msg, "COLLADA Import Tool",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning );
            }
            
            repositoryControl1.CheckedRespositoryPaths = m_Settings.CheckedRepositories;
        }

        // This method deletes any file in the workspace folder that is more than a day old.
        // 
        // The workspace is just a scratch area that this application uses to, well, to
        // carry out its work.  The contents are owned by the app, and have no long-term
        // significance.  The thing is, every time we do something, we copy or build files
        // in this workspace, and the user has no particular reason to look in the folder.
        // Over time, if we don't do something about it, we'll accrue a lot of useless
        // files. 
        private void TidyWorkspace()
        {
            // Do we even have a setting for the folder?
            if( string.IsNullOrEmpty( m_Settings.WorkingFolder ) )
            {
                // No setting; this is likely the first time this application has run.
                // Regardless, create the folder in a safe location and use that for the
                // setting.  
                string importPath = Path.Combine( "Multiverse", "Import" );

                string fullPath = Path.Combine( 
                    Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), importPath );

                m_Settings.WorkingFolder = Path.Combine( fullPath, "Workspace" );
            }

            DirectoryInfo workspace = new DirectoryInfo( m_Settings.WorkingFolder );

            if( ! workspace.Exists )
            {
                workspace.Create();
            }
            else
            {
                FileInfo[] files = workspace.GetFiles();

                DateTime yesterday = DateTime.Now.AddDays( -1.0 );

                foreach( FileInfo file in files )
                {
                    if( yesterday > file.LastWriteTime )
                    {
                        file.Delete();
                    }
                }
            }
        }


        private void configurationToolStripMenuItem_Click( object sender, EventArgs e )
        {
            m_ConfigurationDialog.ShowDialog();
        }

        #region Help Menu support

        private void launchOnlineHelpToolStripMenuItem_Click( object sender, EventArgs e )
        {
            // The argument here is for an "anchor" on the page; I don't know why
            // you'd need this, sinche the base page is dedicated to this app.
            LaunchDoc( String.Empty );
        }

        private void releaseNotesToolStripMenuItem_Click( object sender, EventArgs e )
        {
            LaunchReleaseNotes();
        }

        private void submitFeedbackOrABugToolStripMenuItem_Click( object sender, EventArgs e )
        {
            LaunchFeedback();
        }

        private void commandFlagsToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ShowCommandLineHelp();
        }

        private void aboutCOLLADAImportToolToolStripMenuItem_Click( object sender, EventArgs e )
        {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            string msg = string.Format( 
                "Multiverse COLLADA Import Tool\n\n" + 
                "Version: {0}\n\n" + 
                "Copyright 2007 The Multiverse Network, Inc.\n\n" +
                "Portions of this software are covered by additional copyrights and license agreements which can be found in the Licenses folder in this program's install folder.", 
                assemblyVersion );

            DialogResult result = MessageBox.Show( 
                this, 
                msg, 
                "About Multiverse COLLADA Import Tool", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information );
        }

        public static void LaunchDoc( string anchor )
        {
            string target = String.Format( "{0}#{1}", helpBaseURL, anchor );
            System.Diagnostics.Process.Start( target );
        }

        public static void LaunchFeedback()
        {
            System.Diagnostics.Process.Start( feedbackBaseURL );
        }

        public static void LaunchReleaseNotes()
        {
            System.Diagnostics.Process.Start( releaseNotesURL );
        }

        const string helpBaseURL = "http://update.multiverse.net/wiki/index.php/Using_the_Multiverse_COLLADA_Import_Tool";
        const string feedbackBaseURL = "http://update.multiverse.net/custportal/login.php";
        const string releaseNotesURL = "http://update.multiverse.net/wiki/index.php/Tools_Version_1.5_Release_Notes"; 
    }
    #endregion Help Menu support
}

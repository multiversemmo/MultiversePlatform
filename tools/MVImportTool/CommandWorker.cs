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

namespace MVImportTool
{
    /// <summary>
    /// This class runs a command in a subprocess. You configure it 
    /// </summary>
    internal class CommandWorker
    {
        #region Command properties
#if ABLE_TO_EXECUTE_ANYWHERE
        // location where the command executes
        internal string WorkingDirectory
        {
            get { return m_WorkingDirectory; }
            set { m_WorkingDirectory = value; }
        }
        string m_WorkingDirectory;
#endif

        // executable command name, sans path
        public string CommandExe
        {
            get { return m_CommandExe; }
            set { m_CommandExe = value; }
        }
        string m_CommandExe;
        
        // path to executable command
        public string CommandDirectory
        {
            get { return m_CommandDirectory; }
            set { m_CommandDirectory = value; }
        }
        string m_CommandDirectory;
        
        // command-line args presented to the command
        public string CommandArgs
        {
            get { return m_CommandArgs; }
            set { m_CommandArgs = value; }
        }
        string m_CommandArgs;


        // Event handler for asyncronous reading from StandardOut
        public DataReceivedEventHandler StandardOutHandler
        {
            get { return m_StandardOutHandler; }
            set { m_StandardOutHandler = value; }
        }
        DataReceivedEventHandler m_StandardOutHandler;


        // Event handler for asyncronous reading from StandardError
        public DataReceivedEventHandler StandardErrorHandler
        {
            get { return m_StandardErrorHandler; }
            set { m_StandardErrorHandler = value; }
        }
        DataReceivedEventHandler m_StandardErrorHandler;

        // Accumulated data from syncronous reads from StandardOut
        public string OutData
        {
            get { return m_OutData.ToString(); }
        }
        StringBuilder m_OutData = new StringBuilder();

        #endregion Command properties


        internal CommandWorker()
        {
        }

        // Request the subprocess to abort
        internal void RequestAbort()
        {
            m_IsAbortRequested = true;
        }
        bool m_IsAbortRequested = false;

#if CAN_READ_STDOUT_ASYNC
        // return exit code
        internal int Run()
        {
            Process p = new Process();
            int exitCode = -1;
            m_IsAbortRequested = false;

            try
            {
                p.StartInfo.FileName = CommandDirectory + "\\" + CommandExe;
                p.StartInfo.Arguments = CommandArgs;
                p.StartInfo.CreateNoWindow = true;
//              p.StartInfo.WorkingDirectory = WorkingDirectory;
                p.StartInfo.WorkingDirectory = CommandDirectory;

                SetupOutputRedirection( p );

                p.Start();

                OutData = p.StandardOutput.ReadToEnd();
//                string stdOutData = p.StandardOutput.ReadToEnd();

                #region wait loop
                bool isRunning = true;

                while( isRunning )
                {
                    p.WaitForExit( 100 );

                    isRunning = !p.HasExited;

                    // If you want to provide a hook to cancel the process,
                    // this is where you'd test for it.
                    if( m_IsAbortRequested )
                    {
                        m_IsAbortRequested = false;

                        p.Kill();

                        int limit = 300;
                        while( !p.HasExited )
                        {
                            System.Threading.Thread.Sleep( 10 );

                            --limit;

                            if( 0 >= limit )
                            {
                                p.CloseMainWindow();
                                break;
                            }
                        }
                    }
                #endregion wait loop
                }

                exitCode = p.ExitCode;
            }
            finally
            {
                p.Close();
            }

            return exitCode;
        }
#else
        // Use this to start the subprocess in a thread
        internal void RunInThread()
        {
            ExitCode = Run();
        }

        // This is the exit code returned from the subprocess
        public int ExitCode;

        // return exit code
        internal int Run()
        {
            Process p = new Process();
            int exitCode = -1;
            m_IsAbortRequested = false;

            try
            {
                p.StartInfo.FileName = CommandDirectory + "\\" + CommandExe;
                p.StartInfo.Arguments = CommandArgs;
                p.StartInfo.CreateNoWindow = true;
//              p.StartInfo.WorkingDirectory = WorkingDirectory;
                p.StartInfo.WorkingDirectory = CommandDirectory;

                SetupOutputRedirection( p );

                p.Start();

                EnableOutputRedirection( p );

                #region wait loop
                bool isRunning = true;

                while( isRunning )
                {
                    p.WaitForExit( 100 );

					m_OutData.Append( p.StandardOutput.ReadToEnd() );

                    isRunning = !p.HasExited;

                    if( m_IsAbortRequested )
                    {
                        m_IsAbortRequested = false;

                        p.Kill();

                        int limit = 300;
                        while( !p.HasExited )
                        {
                            System.Threading.Thread.Sleep( 10 );

                            --limit;

                            if( 0 >= limit )
                            {
                                p.CloseMainWindow();
                                break;
                            }
                        }
                        m_OutData.Append( p.StandardOutput.ReadToEnd() );
                        m_OutData.Append( "===[ *** RECEIVIED ABORT *** ]===" );
                    }
                #endregion wait loop
                }

                exitCode = p.ExitCode;
            }
            finally
            {
                m_OutData.Append( p.StandardOutput.ReadToEnd() );
                p.Close();
            }

            return exitCode;
        }
#endif

        // These are the parts of output redirection that take place
        // *before* the process starts.
        private void SetupOutputRedirection( Process p )
        {
            if( null != StandardOutHandler )
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
#if CAN_READ_STDOUT_ASYNC
                p.OutputDataReceived += StandardOutHandler;
#else
                m_OutData = new StringBuilder();
#endif
            }

            if( null != StandardErrorHandler )
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardError = true;
                p.ErrorDataReceived += StandardErrorHandler;
            }
        }

        // These are the parts of output redirection that take place
        // *after* the process begins.
        private static void EnableOutputRedirection( Process p )
        {
            if( p.StartInfo.RedirectStandardOutput )
            {
#if CAN_READ_STDOUT_ASYNC
    //            p.BeginOutputReadLine();
#endif
            }

            if( p.StartInfo.RedirectStandardError )
            {
                p.BeginErrorReadLine();
            }
        }
    }
}

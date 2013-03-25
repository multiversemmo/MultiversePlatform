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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace MVImportTool
{
    public partial class LogPanel : UserControl
    {
        public ILog Log
        {
            get { return m_Log; }
        }
        RedirectedLog m_Log;

        public LogPanel()
        {
            InitializeComponent();

            m_Log = new RedirectedLog( WriteToLogPanel );
        }

        private void WriteToLogPanel( object sender, LogEventArgs e )
        {
            SetLogText( (sender as RedirectedLog).Text, e.IsHighlighted );
        }

        public class LogEventArgs : EventArgs
        {
            public bool IsHighlighted;

            public LogEventArgs( bool isHighlighted )
            {
                IsHighlighted = isHighlighted;
            }
        }

        public delegate void LogEventHandler( object sender, LogEventArgs e );

        #region Cross-thread technique
        // This is a thread-save technique for manipulating a Forms control
        // [Ref. MS doc "Make Thread-Safe Calls to Windows Forms Controls"]

        delegate void SetTextCallback( string text, bool isHighlighted );

        // If the calling thread is different from the thread that
        // created the TextBox control, this method creates a
        // SetTextCallback and calls itself asynchronously using the
        // Invoke method.
        //
        // If the calling thread is the same as the thread that created
        // the TextBox control, the Text property is set directly. 
        private void SetLogText( string text, bool isHighlighted )
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if( this.LogTextBox.InvokeRequired )
            {
                SetTextCallback d = new SetTextCallback( SetLogText );
                this.Invoke( d, new object[] { text, isHighlighted } );
            }
            else
            {
                if( ! isHighlighted )
                {
                    LogTextBox.Text = text;
                }
                else
                {
                    // TODO: Hmm, this doesn't actually change the text color...
                    Color foreColor = LogTextBox.ForeColor;
                    Color backColor = LogTextBox.BackColor;

                    LogTextBox.ForeColor = Color.DarkGreen;
                    LogTextBox.BackColor = Color.AntiqueWhite;

                    LogTextBox.Text = text;

                    LogTextBox.ForeColor = foreColor;
                    LogTextBox.BackColor = backColor;
                }
            }
        }
        #endregion Cross-thread technique



        /// <summary>
        /// This class buffers text; it provides notification when the buffer
        /// gets updated.  The handler that receives the notification reads
        /// the buffer from the Text property on this class.
        /// 
        /// The class provides an event handler suitable for capturing 
        /// asynchronous output streams from the System.Diagnostics.Process 
        /// class.
        /// </summary>
        internal class RedirectedLog : ILog
        {
            internal string Text
            {
                get { return m_LogStream.ToString(); }
            }

            internal RedirectedLog( LogEventHandler updateHandler )
            {
                m_LogStream = new StringWriter();
                UpdateEvent += updateHandler;
            }


            #region ILog Members

            public void SetHighlight( bool isHighlighted )
            {
                m_IsHighlighted = isHighlighted;
            }

            public void Clear()
            {
                m_LogStream.Close();

                m_LogStream = new StringWriter();

                Update();
            }

            public void Append( string text )
            {
                m_LogStream.Write( text );
                m_LogStream.Flush();

                Update();
            }

            public DataReceivedEventHandler DataReceivedHandler
            {
                get { return RedirectedDataHandler; }
            }

            #endregion

            bool m_IsHighlighted;

            StringWriter m_LogStream;

            event LogEventHandler UpdateEvent;

            private void Update()
            {
                if( null != UpdateEvent )
                {
                    UpdateEvent.Invoke( this, new LogEventArgs( m_IsHighlighted ) );
                }
            }

            private void RedirectedDataHandler( object sender, DataReceivedEventArgs args )
            {
                if( ! String.IsNullOrEmpty( args.Data ) )
                {
                    Append( args.Data );
                }
            }
        }
    }
}

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
    /// Public interface to the log kept by the application
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Control whether text sent to the log is hightlighted.
        /// </summary>
        /// <param name="isHighlighted">true enables highlighting</param>
        void SetHighlight( bool isHighlighted );

        /// <summary>
        /// Clear the log.  When the log is attached to a text-panel, this
        /// clears the contents of the panel.
        /// </summary>
        void Clear();

        /// <summary>
        /// Append new text to the log.  
        /// </summary>
        /// <param name="text">text to apend</param>
        void Append( string text );

        /// <summary>
        /// Get a handler that can service asynchronous process output.
        /// </summary>
        DataReceivedEventHandler DataReceivedHandler
        {
            get;
        }
    }

    /// <summary>
    /// Helper for writing to an ILog
    /// </summary>
    public class LogWriter
    {
        public ILog Log
        {
            get { return m_Log; }
            set { m_Log = value; }
        }
        ILog m_Log;

        public LogWriter( ILog log )
        {
            m_Log = log;
        }

        public void Clear()
        {
            if( null != m_Log )
            {
                m_Log.Clear();
            }
        }

        public void Write( string text )
        {
            if( null != m_Log )
            {
                m_Log.SetHighlight( true );

                m_Log.Append( text );

                m_Log.SetHighlight( false );
            }
        }

        public void WriteLine( string text )
        {
            Write( text + System.Environment.NewLine );
        }
    }

}

#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Diagnostics;
using System.IO;

namespace Axiom.Core {
    /// <summary>
    ///     Log class for writing debug/log data to files.
    /// </summary>
    public sealed class Log : IDisposable {
        #region Fields

        /// <summary>
        ///     File stream used for kepping the log file open.
        /// </summary>
        private FileStream log;
        /// <summary>
        ///     Writer used for writing to the log file.
        /// </summary>
        private StreamWriter writer;
        /// <summary>
        ///     Level of detail for this log.
        /// </summary>
        private LoggingLevel logLevel;
        /// <summary>
        ///     Debug output enabled?
        /// </summary>
        private bool debugOutput;

        /// <summary>
        ///     LogMessageLevel + LoggingLevel <= LOG_THRESHOLD = message logged.
        /// </summary>
        const int LogThreshold = 4;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Constructor.  Creates a log file that also logs debug output.
        /// </summary>
        /// <param name="fileName">Name of the log file to open.</param>
        public Log(string fileName) : this(fileName, true) {}

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="fileName">Name of the log file to open.</param>
        /// <param name="debugOutput">Write log messages to the debug output?</param>
        public Log(string fileName, bool debugOutput) {
            this.debugOutput = debugOutput;
            logLevel = LoggingLevel.Normal;

			if(fileName != null) 
			{
				// create the log file, or ope
				log = File.Open(fileName, FileMode.Create);

				// get a stream writer using the file stream
				writer = new StreamWriter(log);
				writer.AutoFlush = true;	//always flush after write
			}

        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///     Gets/Sets the level of the detail for this log.
        /// </summary>
        /// <value></value>
        public LoggingLevel LogDetail {
            get {
                return logLevel;
            }
            set {
                logLevel = value;
            }
        }

        #endregion Properties

        #region Methods 

        /// <summary>
        ///     Write a message to the log.
        /// </summary>
        /// <remarks>
        ///     Message is written with a LogMessageLevel of Normal, and debug output is not written.
        /// </remarks>
        /// <param name="message">Message to write, which can include string formatting tokens.</param>
        /// <param name="substitutions">
        ///     When message includes string formatting tokens, these are the values to 
        ///     inject into the formatted string.
        /// </param>
        public void Write(string message, params object[] substitutions) {
            Write(LogMessageLevel.Normal, false, message, substitutions);
        }

        /// <summary>
        ///     Write a message to the log.
        /// </summary>
        /// <remarks>
        ///     Message is written with a LogMessageLevel of Normal, and debug output is not written.
        /// </remarks>
        /// <param name="maskDebug">If true, debug output will not be written.</param>
        /// <param name="message">Message to write, which can include string formatting tokens.</param>
        /// <param name="substitutions">
        ///     When message includes string formatting tokens, these are the values to 
        ///     inject into the formatted string.
        /// </param>
        public void Write(bool maskDebug, string message, params object[] substitutions) {
            Write(LogMessageLevel.Normal, maskDebug, message, substitutions);
        }

        /// <summary>
        ///     Write a message to the log.
        /// </summary>
        /// <param name="level">Importance of this logged message.</param>
        /// <param name="maskDebug">If true, debug output will not be written.</param>
        /// <param name="message">Message to write, which can include string formatting tokens.</param>
        /// <param name="substitutions">
        ///     When message includes string formatting tokens, these are the values to 
        ///     inject into the formatted string.
        /// </param>
        public void Write(LogMessageLevel level, bool maskDebug, string message, params object[] substitutions) {
			if(message == null)
				throw new ArgumentNullException("The log message cannot be null");
			if( ((int)logLevel + (int)level) > LogThreshold)
				return;	//too verbose a message to write

			// construct the log message
			if (substitutions != null && substitutions.Length > 0) 
			{
				message = string.Format(message, substitutions);
			}
			// write the the debug output if requested
			//XEONX: This is moved to first to allow diagnostics output without using a log file
			if (debugOutput && !maskDebug) 
			{
				System.Diagnostics.Debug.WriteLine(message);
			}
			if(writer.BaseStream != null) {
                
                // prepend the current time to the message
                message = string.Format("[{0}] {1}", DateTime.Now.ToString("hh:mm:ss"), message);

                // write the message and flush the buffer
                writer.WriteLine(message);
				//writer auto-flushes
			}
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        ///     Called to dispose of this objects resources.
        /// </summary>
        /// <remarks>
        ///     For the log, closes any open file streams and file writers.
        /// </remarks>
        public void Dispose() {
			if(writer != null)
            writer.Close();
			if(log != null)
            log.Close();
        }

        #endregion
    }
}

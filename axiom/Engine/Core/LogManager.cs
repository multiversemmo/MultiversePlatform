using System;
using System.Collections;
using log4net;
using log4net.Config;
using log4net.Layout;
using log4net.Appender;
using Multiverse.Lib.LogUtil;

namespace Axiom.Core {
	/// <summary>
	/// Summary description for LogManager.
	/// </summary>
	public sealed class LogManager : IDisposable {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static LogManager instance = new LogManager();

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal LogManager() {
            if (instance == null) {
                instance = this;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static LogManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

        #region Fields
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Axiom");
#if NOT
        /// <summary>
        ///     List of logs created by the log manager.
        /// </summary>
        private Hashtable logList = new Hashtable();
        /// <summary>
        ///     The default log to which output is done.
        /// </summary>
        private Log defaultLog;
#endif
        #endregion Fields

        #region Properties
#if NOT
        /// <summary>
        ///     Gets/Sets the default log to use for writing.
        /// </summary>
        /// <value></value>
        public Log DefaultLog {
            get {
                if (defaultLog == null) {
                    throw new AxiomException("No logs have been created yet.");
                }

                return defaultLog;
            }
            set {
                defaultLog = value;
            }
        }

        /// <summary>
        ///     Sets the level of detail of the default log.
        /// </summary>
        public LoggingLevel LogDetail {
            get {
                return DefaultLog.LogDetail;
            }
            set {
                DefaultLog.LogDetail = value;
            }
        }
#endif
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

        public void WriteException(string message, params object[] substitutions)
        {
            LogUtil.ExceptionLog.ErrorFormat(message, substitutions);
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
        private void Write(LogMessageLevel level, bool maskDebug, string message, params object[] substitutions) {
            switch (level) {
                case LogMessageLevel.Trivial:
                    if (substitutions.Length > 0)
                        log.DebugFormat(message, substitutions);
                    else
                        log.Debug(message);
                    break;
                case LogMessageLevel.Normal:
                    if (substitutions.Length > 0)
                        log.InfoFormat(message, substitutions);
                    else
                        log.Info(message);
                    break;
                case LogMessageLevel.Critical:
                    if (substitutions.Length > 0)
                        log.ErrorFormat(message, substitutions);
                    else
                        log.Error(message);
                    break;
            }
        }

        public void Error(string message, params object[] substitutions) {
            Write(LogMessageLevel.Critical, false, message, substitutions);
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        ///     Destroys all the logs owned by the log manager.
        /// </summary>
        public void Dispose() {
#if NOT
            // dispose of all the logs
            foreach (IDisposable o in logList.Values) {
                o.Dispose();
            }

            logList.Clear();
#endif
        }

#endregion
    }
}
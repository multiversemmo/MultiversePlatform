using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using log4net;
using log4net.Config;
using log4net.Layout;
using log4net.Appender;

namespace Multiverse.Lib.LogUtil
{
    public class LogUtil
    {
        private static readonly log4net.ILog exceptionLog = log4net.LogManager.GetLogger("Exception");
        private static readonly log4net.ILog logUtilLog = log4net.LogManager.GetLogger("LogUtil");

        static LogUtil()
        {
        }

        public static log4net.ILog ExceptionLog
        {
            get
            {
                return exceptionLog;
            }
        }

        public static void InitializeLogging(string configFilename, string defaultConfigFilename, string defaultLogfileName)
        {
            InitializeLogging(configFilename, defaultConfigFilename, defaultLogfileName, true);
        }

        /// <summary>
        ///    Utility function to initialize logging.  This will check for a log configuration file
        ///    and use that to configure the logging.  If the log configuration file is not available,
        ///    this method will check the default log configuration file.  If that file is not 
        ///    available either, this method will create a primitive configuration that writes logs to
        ///    the filename specified as the defaultLogfileName
        /// </summary>
        /// <param name="configFilename">the path (can be relative) to the log config file</param>
        /// <param name="defaultConfigFilename">the path (can be relative) to the default log config file</param>
        /// <param name="defaultLogfileName">the filename to use for logging if the config files are not found</param>
        public static void InitializeLogging(string configFilename, string defaultConfigFilename, string defaultLogfileName, bool interactive)
        {
            if (File.Exists(configFilename))
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(configFilename));
            }
            else if (File.Exists(defaultConfigFilename))
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(defaultConfigFilename));
            }
            else
            {
                // should only get here if someone has a messed up install

                if (interactive)
                {
                    RollingFileAppender rfa = new RollingFileAppender();
                    rfa.Layout = new PatternLayout("%-5p [%d{ISO8601}] %-20.20c{1} %m%n");
                    rfa.File = defaultLogfileName;
                    rfa.AppendToFile = false;
                    rfa.MaximumFileSize = "5MB";
                    rfa.MaxSizeRollBackups = 2;
                    rfa.RollingStyle = RollingFileAppender.RollingMode.Once;
                    rfa.ActivateOptions();
                    BasicConfigurator.Configure(rfa);
                    SetLogLevel(log4net.Core.Level.Info, true);
                }
                else
                {
                    // use the trace appender if we are not running interactive
                    TraceAppender ta = new TraceAppender();
                    ta.Layout = new PatternLayout("%-5p [%d{ISO8601}] %-20.20c{1} %m%n");
                    ta.ActivateOptions();
                    BasicConfigurator.Configure(ta);
                    SetLogLevel(log4net.Core.Level.Info, true);
                }

                logUtilLog.Info("Unable to find logging config files.  Using fallback simple logging configuration.");
                logUtilLog.InfoFormat("Logging config file: {0}", configFilename);
            }
        }

        public static void SetLogLevel(log4net.Core.Level logLevel, bool force)
        {

            log4net.Repository.ILoggerRepository[] repositories = LogManager.GetAllRepositories();
            foreach (log4net.Repository.ILoggerRepository repository in repositories)
            {
                log4net.Repository.Hierarchy.Hierarchy hierarchy = repository as log4net.Repository.Hierarchy.Hierarchy;
                if (hierarchy.Root.Level > logLevel || force)
                    hierarchy.Root.Level = logLevel;
            }
        }
    }
}

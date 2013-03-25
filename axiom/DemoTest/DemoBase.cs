#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

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
using System.Globalization;
using System.Threading;
using System.Xml;
using System.IO;
using Axiom.Utility;
using Axiom.Core;
using log4net;
using log4net.Config;
using log4net.Layout;
using log4net.Appender;

namespace Demos {

    /// <summary>
    ///     Base class for demo objects
    /// </summary>
    public class DemoBase : TechDemo {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DemoBase));
        private static string MyDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string ClientAppDataFolder = Path.Combine(MyDocumentsFolder, "AxiomDemos");
        private static string ConfigFolder = Path.Combine(ClientAppDataFolder, "Config");
        private static string LogFolder = Path.Combine(ClientAppDataFolder, "Logs");
        private static string FallbackLogfile = Path.Combine(LogFolder, "AxiomDemos.log");
        private string rootDir = "c:/Junk/OgreMedia/";
        private string[] directories = new string[] { "fonts", 
                                                      "gui",
                                                      "materials/programs",
                                                      "materials/scripts",
                                                      "materials/textures",
                                                      "models",
                                                      "overlays",
                                                      "packs",
                                                      "particle" 
        };

        public DemoBase() {
            // Set up log configuration folders
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);
                // Note that the DisplaySettings.xml should also show up in this folder.

            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);

            LogUtil.InitializeLogging(Path.Combine(ConfigFolder, "LogConfig.xml"), "DefaultLogConfig.xml", FallbackLogfile, true);
        }
        

        protected override void SetupResources() {
            foreach (string s in directories)
                ResourceManager.AddCommonArchive(rootDir + s, "Folder");
        }

        protected override void CreateScene()
        { }
    }

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
            SetLogLevel(log4net.Core.Level.Debug, true);
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

            log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();
            foreach (log4net.Repository.ILoggerRepository repository in repositories)
            {
                log4net.Repository.Hierarchy.Hierarchy hierarchy = repository as log4net.Repository.Hierarchy.Hierarchy;
                if (hierarchy.Root.Level > logLevel || force)
                    hierarchy.Root.Level = logLevel;
            }
        }
    }

}


        

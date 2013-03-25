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

package multiverse.server.util;

import java.util.*;

// checked for locks

public class Log {

    public static void init() {
	if (logger == null)
	    initLogging();
    }

    public static void init(Properties properties) {
	if (logger != null) {
	    logger.removeAppender("MVDefaultConsoleAppender");
	}
	org.apache.log4j.PropertyConfigurator.configure(properties);
	if (logger == null)
	    initLogging();
	else {
	    syncWithOtherLevel();
	    makeFailsafeAppender(true);
	}
	boolean rotate = Boolean.parseBoolean(properties.getProperty("multiverse.rotate_logs_on_startup", "false"));
	if (rotate) {
	    rotateLogs();
	}
    }

    private Log() {
	init();
    }
    //private static Log singleton = new Log();

    public static void net(String s) {
	logger.trace(s);
    }

    public static void trace(String s) {
	logger.trace(s);
    }
    
    public static void debug(String s) {
	logger.debug(s);
    }

    public static void info(String s) {
	logger.info(s);
    }

    public static void warn(String s) {
	logger.warn(s);
    }

    public static void error(String s) {
	logger.error(s);
    }

    public static void logAtLevel(int level, String s) {
        if (logLevel <= level) {
            switch (level) {
            case 0:
                Log.trace(s);
                break;
            case 1:
                Log.debug(s);
                break;
            case 2:
                Log.info(s);
                break;
            case 3:
                Log.warn(s);
                break;
            case 4:
                Log.error(s);
                break;
            }
        }
    }
    
    public static void dumpStack() {
        dumpStack("");
    }
    
    public static void dumpStack(String context) {
        logger.error(buildStackDump(context, Thread.currentThread(), 5).toString());
    }
    
    public static void dumpStack(String context, Thread thread) {
        logger.error(buildStackDump(context, thread, 5).toString());
    }
    
    public static void warnAndDumpStack(String context) {
        logger.warn(buildStackDump(context, Thread.currentThread(), 5).toString());
    }

    public static void warnAndDumpStack(String context, Thread thread) {
        logger.warn(buildStackDump(context, thread, 5).toString());
    }
    
    protected static StringBuilder buildStackDump(String context, Thread thread, int framesToSkip) {
        StringBuilder traceStr = new StringBuilder(1000);
        traceStr.append((context == null || context.length() == 0 ? "Dumping stack for thread " : context + ", dumping stack for thread ") + thread.getName());
        int cnt = 0;
        for (StackTraceElement elem : thread.getStackTrace()) {
            cnt++;
            if (cnt < framesToSkip)
                continue;
            traceStr.append("\n       at ");
            traceStr.append(elem.toString());
        }
        return traceStr;
    }
    
    public static String exceptionToString(Exception e) {
        Throwable throwable = e;
        StringBuilder traceStr = new StringBuilder(1000);
        do {
            traceStr.append(throwable.toString());
            for (StackTraceElement elem : throwable.getStackTrace()) {
                traceStr.append("\n       at ");
                traceStr.append(elem.toString());
            }
            throwable = throwable.getCause();
            if (throwable != null) {
                traceStr.append("\nCaused by: ");
            }
        } while (throwable != null);
        return traceStr.toString();
    }

    public static void exception(String context, Exception e) {
        logger.error((context == null || context.length() == 0 ? "Exception: " : context + " " ) + exceptionToString(e));
    }
    
    public static void exception(Exception e) {
        logger.error("Exception: " + e + exceptionToString(e));
    }
    
    private static void initLogging() {
        String disableLogs = System.getProperty("multiverse.disable_logs",null);
        if (disableLogs != null) {
            logger = org.apache.log4j.Logger.getRootLogger();
            logger.addAppender(new NullAppender());
        }
        else {
            String loggerName = System.getProperty("multiverse.loggername", "MV");
            logger = org.apache.log4j.Logger.getLogger(loggerName);
            syncWithOtherLevel();
            makeFailsafeAppender(false);
        }
    }

    private static void syncWithOtherLevel() {
	org.apache.log4j.Level log4jLevel = logger.getEffectiveLevel();
	if (log4jLevel == org.apache.log4j.Level.TRACE)
	    logLevel= 0;
	else if (log4jLevel == org.apache.log4j.Level.DEBUG)
	    logLevel= 1;
	else if (log4jLevel == org.apache.log4j.Level.INFO)
	    logLevel= 2;
	else if (log4jLevel == org.apache.log4j.Level.WARN)
	    logLevel= 3;
	else if (log4jLevel == org.apache.log4j.Level.ERROR)
	    logLevel= 4;
	setLogLevel(logLevel);

    }

    private static void makeFailsafeAppender(boolean complain) {
	// Log to console if no logging config.  (if there's no appender
	// for the root or process logger, then assume logging config
	// is missing)
	org.apache.log4j.Logger rootLogger;
	rootLogger = org.apache.log4j.Logger.getRootLogger();
	Enumeration rootAppenders = rootLogger.getAllAppenders();
	Enumeration ourAppenders = logger.getAllAppenders();
	boolean rootEmpty = ! rootAppenders.hasMoreElements();
	boolean ourEmpty = ! ourAppenders.hasMoreElements();
	if (rootEmpty && ourEmpty)  {
	    if (complain)
		System.out.println("Missing log config file, logging to console");
	    org.apache.log4j.ConsoleAppender appender=
		new org.apache.log4j.ConsoleAppender(
		    new org.apache.log4j.PatternLayout("%-5p [%d{ISO8601}] %-10t %m%n"), "System.err");
	    appender.setName("MVDefaultConsoleAppender");
	    logger.addAppender(appender);
	}
/*
	while (rootAppenders.hasMoreElements()) {
	    org.apache.log4j.Appender appender =
		(org.apache.log4j.Appender)rootAppenders.nextElement();
	    if (appender.getName().equals("ERRORS")) {
System.out.println("setting filter on ERRORS");
		org.apache.log4j.varia.LevelMatchFilter filter =
			new org.apache.log4j.varia.LevelMatchFilter();
		filter.setLevelToMatch("ERROR");
		filter.setAcceptOnMatch(true);
		appender.addFilter(filter);
	    }
	}
*/
    }

    private static void rotateLogs() {
	Enumeration appenders = org.apache.log4j.Logger.getRootLogger().getAllAppenders();
	while (appenders.hasMoreElements()) {
	    org.apache.log4j.Appender a = (org.apache.log4j.Appender)appenders.nextElement();
	    if (a instanceof org.apache.log4j.RollingFileAppender &&
			! a.getName().equals("ErrorLog")) {
		((org.apache.log4j.RollingFileAppender)a).rollOver();
	    }
	}
    }

    // 0=trace 1=debug 2=info 3=warn 4=error
    public static void setLogLevel(int level) {
	if (level != logLevel) {
	    if (level == 0) logger.setLevel(org.apache.log4j.Level.TRACE);
	    else if (level == 1) logger.setLevel(org.apache.log4j.Level.DEBUG);
	    else if (level == 2) logger.setLevel(org.apache.log4j.Level.INFO);
	    else if (level == 3) logger.setLevel(org.apache.log4j.Level.WARN);
	    else if (level == 4) logger.setLevel(org.apache.log4j.Level.ERROR);
            else
                return;
	}
	logLevel = level;
        loggingWarn = logLevel <= 3;
        loggingInfo = logLevel <= 2;
        loggingDebug = logLevel <= 1;
        loggingNet = logLevel <= 0;
        loggingTrace = loggingNet;
    }

    public static int getLogLevel() {
        return logLevel;
    }
    
    private static int logLevel = 1;

    static org.apache.log4j.Logger logger ;

    static public boolean loggingWarn = false;
    static public boolean loggingInfo = false;
    static public boolean loggingDebug = false;
    static public boolean loggingNet = false;
    static public boolean loggingTrace = false;

    private static class NullAppender extends org.apache.log4j.AppenderSkeleton
    {
        protected void append(org.apache.log4j.spi.LoggingEvent event)
        {
        }
        public void close()
        {
        }
        public boolean requiresLayout()
        {
            return false;
        }
    }

}

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

import multiverse.server.engine.PropertyFileReader;
import multiverse.server.marshalling.MarshallingRuntime;

import java.util.*;
import java.io.*;
import java.lang.management.*; 

/**
 * This class provide the common log initialization and pid
 * registration functionality used by both Engine processes,
 * DomainCommand processes and DomainServer processes.  It consists
 * exclusively of static methods.
 */
public class InitLogAndPid {
    
    public static Properties initLogAndPid(String args[]) {
        return initLogAndPid(args, null, null);
    }

    public static Properties initLogAndPid(String args[], String worldName,
        String hostName)
    {
        // Initialize the log system, so we are certain to see
        // the property file and properties displayed.
        boolean disableLogs =
            System.getProperty("multiverse.disable_logs","false").equals("true");
        Log.init();

        String pid="?";
        RuntimeMXBean runtimeBean =
            java.lang.management.ManagementFactory.getRuntimeMXBean();
        pid = runtimeBean.getName();

    	Properties properties = readAndParseProperties(args, worldName,
            hostName);

        // Set the log level to the value in the properties file, or
        // 1, meaning debug, if the property is not found.  Plugins
        // are free to subsequently set the log level to a different
        // value.
        String logLevelString;
        logLevelString = properties.getProperty("multiverse.log_level");

        Integer logLevel = null;
        if (logLevelString != null) {
            try {
                logLevel = Integer.parseInt(logLevelString.trim());
            }
            catch (Exception e) { /* ignore */ }
        }

        if (! disableLogs)
            Log.init(properties);
        else if (logLevel != null)
            Log.setLogLevel(logLevel);

        Log.info("pid "+pid);
        writePidFile(args, pid);

        Log.debug("Using property file " + PropertyFileReader.propFile);
        Log.debug("Properties are:");
        String sKey;
        Enumeration en = properties.propertyNames();
        while (en.hasMoreElements()) {
            sKey = (String)en.nextElement();
            Log.debug("    " + sKey + " = " + properties.getProperty(sKey));
        }

        if (logLevel != null)
            Log.setLogLevel(logLevel);

        Log.info("The log level is " + Log.getLogLevel());

        String build = properties.getProperty("multiverse.build");
        if (build != null) {
            Log.info("Multiverse Server Build " + build);
        }
    	
        Log.info("Multiverse server version " +
            ServerVersion.getVersionString());

        String typeNumFileName = getTypeNumbersArg(args);
        if (typeNumFileName != "")
            MarshallingRuntime.initializeBatch(typeNumFileName);

        return properties;
    }
        
    public static String getTypeNumbersArg(String args[]) {
        for (int i=0; i<args.length - 1; i++) {
            if (args[i].equals("-t"))
                return args[i + 1];
        }
        return "";
    }

    public static Properties readAndParseProperties(String args[],
        String worldName, String hostName)
    {
        // Read properties file 
    	PropertyFileReader pfr = new PropertyFileReader();
    	Properties properties = null;
    	if (PropertyFileReader.usePropFile) {
            properties = pfr.readPropFile();
        }

        if (worldName != null)  {
            String home = System.getenv("MV_HOME");
            properties = readPropertyFile(home+"/config/"+worldName+
                "/world.properties", properties);
        }

        if (hostName != null) {
            String home = System.getenv("MV_HOME");
            properties = readPropertyFile(home+"/config/"+worldName+
                "/"+hostName+".properties", properties);
        }

        properties = parsePropertyArgs(args, properties);
        return properties;
    }

    private static Properties readPropertyFile(String fileName,
        Properties properties)
    {
        File propertyFile = new File(fileName);
        if (propertyFile.exists()) {
            Properties overrideProperties = new Properties(properties);
            try {
                overrideProperties.load(new FileInputStream(propertyFile));
                properties = overrideProperties;
                // Can't log debug here because log not initialized
                //Log.debug("Using properties file "+fileName);
            } catch (IOException e) {
                Log.exception("Loading properties file "+fileName, e);
            }
        }
        return properties;
    }    

    private static Properties parsePropertyArgs(String[] args,
        Properties defaults)
    {
        Properties properties = new Properties(defaults);
        for (int ii=0; ii < args.length; ii++) {
            if (args[ii].startsWith("-P") && args[ii].indexOf('=') != -1) {
                int equal = args[ii].indexOf('=');
                String key = args[ii].substring(2,equal);
                String value = args[ii].substring(equal+1);
                properties.put(key,value);
            }
            else if (args[ii].equals("-p")) {
                ii++;
                if (ii >= args.length) {
                    Log.error("Missing file name for -p");
                    break;
                }
                properties = readPropertyFile(args[ii], properties);
            }
        }
        return properties;
    }

    private static void writePidFile(String[] argv, String pid)
    {
        String pidFileName = null;
        for (int ii = 0; ii < argv.length; ii++) {
            if (argv[ii].equals("--pid")) {
                pidFileName = argv[ii+1];
                break;
            }
        }
        if (pidFileName == null)
            return;

        int nDigits = 0;
        for (int ii = 0; ii < pid.length(); ii++) {
            if (Character.isDigit(pid.charAt(ii)))
                nDigits++;
            else
                break;
        }
        if (nDigits == 0)
            return;

        try {
            FileOutputStream pidFile = new FileOutputStream(pidFileName);
            pidFile.write((pid.substring(0,nDigits)+"\n").getBytes());
            pidFile.close();
        }
        catch (java.io.IOException e) {
            Log.exception(pidFileName, e);
        }
    }

}

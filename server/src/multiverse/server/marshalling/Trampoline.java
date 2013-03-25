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

package multiverse.server.marshalling;

import java.lang.reflect.Method;
import java.lang.reflect.Modifier;
import java.util.*;
import java.io.FileOutputStream;
import multiverse.server.util.Log;
import multiverse.server.engine.PropertyFileReader;
import java.lang.management.RuntimeMXBean;
import java.lang.reflect.InvocationTargetException;

/** 
 * The initial class of all server processes is this class Trampoline,
 * whose virtue is that it _doesn't_ reference any of the classes that
 * must be injected to support byte-code marshalling of server messages.
 */
public class Trampoline {

    protected static Class getClassForClassName(String className) {
        try {
            Class c = Class.forName(className);
            return c;
        }
        catch (Exception e) {
            return null;
        }
    }
    
    /**
     * The main program reads the property file, and uses the property
     * file to initialize the log.  It then logs all the properties, 
     * makes a copy of the args, and calls MarshallingRuntime.initialize(),
     * which marshals all the server classes that are declared as 
     * requiring marshalling.  If MarshallingRuntime.initialize() returns
     * false, that means that the set of classes requiring marshalling
     * was inconsistent.  Extensive information about the nature of the
     * inconsistency will have been logged by MarshallingRuntime.initialize(),
     * and Trampoline exits the process.
     * <p>
     * The first arg is always the class containing the main program
     * for server process; for most server processes, this is the 
     * class multiverse.server.engine.Engine.  This main program 
     * is invoked using the Java reflection mechanism, because that
     * way Trampoline doesn't directly reference the main class of
     * the process.
     */
    public static void main(String[] argv) throws Throwable {
        if (argv.length < 1) {
            System.out.println("Usage: java multiverse.server.marshalling.Trampoline <main_class> [args]");
            return;
        }
        String mainClassName = argv[0];
        String[] new_argv = new String[argv.length - 1];
        System.arraycopy(argv, 1, new_argv, 0, new_argv.length);

        // Initialize the log system, so we are certain to see
        // the property file and properties displayed.
        boolean disableLogs =
            System.getProperty("multiverse.disable_logs","false").equals("true");
        Log.init();

        String pid="?";
        RuntimeMXBean runtimeBean =
            java.lang.management.ManagementFactory.getRuntimeMXBean();
        pid = runtimeBean.getName();

	// Read properties file 
    	PropertyFileReader pfr = new PropertyFileReader();
    	if (PropertyFileReader.usePropFile) {
            properties = pfr.readPropFile();
        }

        properties = parsePropertyArgs(argv, properties);

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
        writePidFile(argv, pid);

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

        // Make a copy of new_argv for MarshallingRuntime, because getOpt messes up the vector
        String[] mr_argv = new String[new_argv.length];
        System.arraycopy(new_argv, 0, mr_argv, 0, mr_argv.length);

        // Set up the marshalling runtime
        if (MarshallingRuntime.initialize(mr_argv)) {
            System.out.println("Exiting because MarshallingRuntime.initialize() found missing or incorrect classes");
            System.exit(1);
        }

        Class cl = getClassForClassName(mainClassName);
        if (cl == null) {
            System.out.println("Loading of class '" + mainClassName + "' returned null!");
            return;
        }
        Method method = null;
        try {
            method = cl.getMethod("main", new Class[] {
                argv.getClass()
            });
            /* Method main is sane ?
             */
            int m = method.getModifiers();
            Class r = method.getReturnType();
            if (!(Modifier.isPublic(m) && Modifier.isStatic(m)) || Modifier.isAbstract(m)
                    || (r != Void.TYPE)) {
                throw new NoSuchMethodException();
            }
        } catch (NoSuchMethodException no) {
            System.out.println("In class " + mainClassName
                    + ": public static void main(String[] argv) is not defined");
            return;
        }
        try {
            method.invoke(null, new Object[] {
                    new_argv
            });
        } catch (InvocationTargetException ex) {
            //ex.getCause().printStackTrace();
            throw ex.getCause();
        } catch (Exception ex) {
            //ex.printStackTrace();
            throw ex;
        }
    }
    
    /**
     * Get the Properties instance loaded by the main program
     */
    public static Properties getProperties() {
        return properties;
    }
    
    private static Properties parsePropertyArgs(String[] args,
        Properties defaults)
    {
        Properties props = new Properties(defaults);
        for (int ii=0; ii < args.length; ii++) {
            if (args[ii].startsWith("-P") && args[ii].indexOf('=') != -1) {
                int equal = args[ii].indexOf('=');
                String key = args[ii].substring(2,equal);
                String value = args[ii].substring(equal+1);
                props.put(key,value);
            }
        }
        return props;
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

    private static Properties properties = null;
}


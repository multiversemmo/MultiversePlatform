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

package multiverse.server.engine;

import java.util.*;
import java.io.*;

/**
 * Reads values from property file whose location is specified by
 * the command-line (System) property multiverse.propertyfile.
 * If multiverse.propertyfile is NOT specified, then the Engine will 
 * default to system properties, set with -D on Java command line.
 */

import multiverse.server.util.Log;

public class PropertyFileReader {

    public PropertyFileReader() {
        try {
            if (propFile == null)
                propFile = System.getProperty("multiverse.propertyfile"); 
            if (propFile==null) {
                Log.debug("No property file specified.  Will use command-line properties.");
                usePropFile = false;
            } else {
                File f = new File(propFile);
                if (f.exists()) {
                    //if (Log.loggingDebug)
                        //Log.debug("Using property file " + propFile);
                    usePropFile = true;
                } else {
                    if (Log.loggingDebug)
                        Log.debug("Specified property file " + propFile + " does not exist! Defaulting to command-line properties.");
                    usePropFile = false;
                }
            }
        } catch (Exception e) {
            Log.exception("PropertyFileReader caught exception finding Properties file", e);
        }
    } //PropertyFileReader
	
    /**
     * Read properties file into a Properties object.
     */	 
    public Properties readPropFile() {
        File f = new File(propFile);
        Properties properties = new Properties(System.getProperties());
		
        //if (Log.loggingDebug)
            //Log.debug("Reading prop file " +  propFile);
        if (f.exists()) {
            try {
                properties.load(new FileInputStream(propFile));	 
		        
            } catch (IOException e) {
                Log.exception("PropertyFileReader.readPropFile caught exception finding Properties file", e);
            }
			
        } else { 
            Log.error("Properties file " + propFile + " does not exist.");
        }

        return properties;
    } //readPropFile
    
    public static String propFile = null;
    public static boolean usePropFile = false;
	
}

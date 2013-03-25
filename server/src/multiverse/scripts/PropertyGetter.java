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

package multiverse.scripts;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.Properties;

/**
 * 
 * Reads the property file specified as first argument and returns value of property
 * specified as second argument.
 *
 */
public class PropertyGetter {
	static String propFile = System.getProperty("multiverse.propertyfile");
	static Properties properties = new Properties();
	static String propName;
	static String win_env_var = System.getProperty("win_env_var");
	
    public static void main(String args[]) {
    	if (propFile == null) {
            System.err.println("ERROR: Property file must be specified with -D.");
            System.exit(1);
        }

        if (args.length < 1) {
            System.err.println("ERROR: Specify property name!");
            System.exit(1);
        }

        propName = args[0];
        String defaultValue = null;
        if (args.length > 1)
            defaultValue = args[1];

        File f = new File(propFile);

        if (f.exists()) {
            try {
                properties.load(new FileInputStream(propFile));
            } catch (IOException e) {
                System.out.println("Error finding Properties file - " + f.getAbsoluteFile());
            }

            String propValue = properties.getProperty(propName, defaultValue);

            // On Linux, just output property value
            if (win_env_var == null) {
                System.out.print(propValue);
                // Output set ENV_VAR=value for windows batch file
            } else {
                System.out.println("set " + win_env_var + "=" + propValue);
            }
        } else {
            System.out.println("Properties file " + propFile + " does not exist.");
        }
    }
	
    public String getWorldName() {
    	return properties.getProperty("multiverse.worldname"); 
    }
    
    public String  getWorldFileName() {
    	return properties.getProperty("multiverse.mvwfile"); 
    }    
}

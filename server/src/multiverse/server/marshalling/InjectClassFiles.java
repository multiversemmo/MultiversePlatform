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

import java.util.*;
import multiverse.server.util.Log;
import java.io.File;

/** 
 * Read the set of classes to be marshalled, and then create injected
 * versions in the specified output directory.
 */
public class InjectClassFiles {

    /**
     * The main program parses command line args giving the input
     * directory containing the class file hierarchy, a set of scripts
     * that specify the classes into which marshalling should be
     * injected, and the output directory into which injected class
     * files and the table of class names vs. type numbers should be
     * written.
     */
    public static void main(String[] argv) throws Throwable {
        Properties props = new Properties();
        props.put("log4j.appender.FILE", "org.apache.log4j.RollingFileAppender");
        props.put("log4j.appender.FILE.File", "${multiverse.logs}/inject.out");
        props.put("log4j.appender.FILE.MaxFileSize", "50MB");
        props.put("log4j.appender.FILE.layout", "org.apache.log4j.PatternLayout");
        props.put("log4j.appender.FILE.layout.ConversionPattern", "%-5p %m%n");
        props.put("multiverse.log_level", "0");
        props.put("log4j.rootLogger", "DEBUG, FILE");
        Log.init(props);

        if (argv.length < 6 || (argv.length & 1) == 1) {
            usage();
            System.exit(1);
        }
        List<String> marshallersFiles = new LinkedList<String>();
        String inputDir = "";
        String outputDir = "";
        String typeNumFileName = "";
        for (int i=0; i<argv.length; i+=2) {
            String arg = argv[i];
            String value = argv[i+1];
            if (arg.equals("-m")) {
                File marshallersFile = new File(value);
                if (marshallersFile.isFile())
                    marshallersFiles.add(value);
//                 else
//                     System.out.println("Marshallers file '" + value + "' was not found.");
            }
            else if (arg.equals("-i")) {
                File in = new File(value);
                if (in.isDirectory())
                    inputDir = value;
                else {
                    System.err.println("Class file input directory '" + value + "' does not exist!");
                    System.exit(1);
                }
            }
            else if (arg.equals("-o")) {
                File out = new File(value);
                if (out.isDirectory())
                    out.mkdir();
                outputDir = value;
            }
            else if (arg.equals("-t"))
                typeNumFileName = value;
        }
        if (marshallersFiles.size() == 0) {
            System.err.println("No marshaller files were supplied!");
            System.exit(1);
        }
        if (inputDir == "") {
            System.err.println("The class file input directory was not supplied!");
            System.exit(1);
        }
        if (outputDir == "") {
            System.err.println("The class file output directory was not supplied!");
            System.exit(1);
        }
        if (typeNumFileName == "") {
            System.err.println("The typenumbers.txt file name to which type numbers are written was not supplied!");
            System.exit(1);
        }
        // Create a new version of argv that adds the -r option,
        // indicating that class files should be saved.
        String[] mr_argv = new String[argv.length + 1];
        System.arraycopy(argv, 0, mr_argv, 0, argv.length);
        mr_argv[argv.length] = "-r";

        // Set up the marshalling runtime
        if (MarshallingRuntime.initialize(mr_argv)) {
            System.out.println("Exiting because MarshallingRuntime.initialize() found missing or incorrect classes");
            System.exit(1);
        }
    }

    protected static void usage() {
        System.out.println("Usage: java multiverse.server.marshalling.InjectClassFiles [ -m marshallersfile.txt | -i input_directory | -o output_directory ]");
    }
            
}


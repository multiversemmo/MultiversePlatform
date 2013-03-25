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

import org.apache.bcel.util.ClassPath;
import java.util.*;

/**
 * Provide a class loader that invokes InjectionGenerator to add
 * marshalling methods to the environment.  The class loader is
 * careful to only refer the MarshallingRuntime using reflection
 * methods, to avoid having loading the class loader itself causing
 * loading of server classes before the MarshallingRuntime is
 * initialized.
 */
public class MarshallingClassLoader extends java.lang.ClassLoader {

    public MarshallingClassLoader(java.lang.ClassLoader parent) {
        super(parent);
        classPath = ClassPath.SYSTEM_CLASS_PATH;
    }
    
    protected synchronized Class<?> loadClass(String className, boolean resolve)
                    throws ClassNotFoundException {
//         int lastDot = className.lastIndexOf(".");
//         String simpleName = className.substring(lastDot + 1);
        Class cl = (Class)loadedClasses.get(className);
        if (cl != null)
            return cl;

        for (int i = 0; i < ignoredPackages.length; i++) {
            if (className.startsWith(ignoredPackages[i])) {
                cl = getParent().loadClass(className);
                return cl;
            }
        }
        if (!injecting) {
            if (marshallingRuntimeClass == null) {
                // Try to initialize the method object we'll use to call
                // MarshallingRuntime.maybeInjectMarshalling
                marshallingRuntimeClass = (Class)loadedClasses.get("multiverse.server.marshalling.MarshallingRuntime");
                if (marshallingRuntimeClass != null) {
                    // We have the class, so we should be able to get the method
                    try {
                        maybeInjectMarshallingMethod = marshallingRuntimeClass.getMethod("maybeInjectMarshalling", new Class[] { className.getClass() } );
                        addMarshallingClassMethod = marshallingRuntimeClass.getMethod("addMarshallingClass", new Class[] { className.getClass(), this.getClass().getClass() } );
                        injecting = true;
                    }
                    catch (Exception e) {
                        throw new RuntimeException("MarshallingClassLoader.loadClass: Could not find MarshallingRuntime.maybeInjectMarshalling method");
                    }
                }
            }
        }
        boolean classInjected = false;
        if (injecting) {
            // The method object we'll use to call MarshallingRuntime.maybeInjectMarshalling 
            // is initialized so invoke it.
            try {
                byte [] bytes = (byte [])maybeInjectMarshallingMethod.invoke(null, new Object[] { className });
                if (bytes != null) {
                    cl = defineClass(className, bytes, 0, bytes.length);
                    classInjected = true;
                }
            } catch (Exception ex) {
                ex.printStackTrace();
            }
        }
        // If we're not injecting, or if this was not an injectable
        // class, just call findClass to get it, avoiding the overhead
        // of the JavaClass mechanism
        Class existingClass = loadedClasses.get(className);
        if (existingClass != null)
            return existingClass;
        if (cl == null)
            cl = loadClassWithoutInjection(className);
        if (resolve)
            resolveClass(cl);
        loadedClasses.put(className, cl);
        if (classInjected) {
            try {
                addMarshallingClassMethod.invoke(null, new Object[] { className, cl });
            }
            catch (Exception ex) {
                System.out.println("Exception while loading class " + className + ": " + ex);
                ex.printStackTrace();
                return null;
            }
        }
        return cl;
    }
    
    protected Class loadClassWithoutInjection(String className) 
                  throws ClassNotFoundException {
        try {
            byte [] bytes = classPath.getBytes(className);
            Class cl = defineClass(className, bytes, 0, bytes.length);
            return cl;
        } catch (Exception e) {
            throw new ClassNotFoundException("loadClassWithoutInjection: exception loading class '" + 
                className + "': " + e.toString());
        }
    }
    
//     public static void dumpStack(String context, Thread thread) {
//         StringBuilder traceStr = new StringBuilder(1000);
//         traceStr.append((context == null || context.length() == 0 ? "Dumping stack for thread " : context + ", dumping stack for thread ") + thread.getName());
//         for (StackTraceElement elem : thread.getStackTrace()) {
//             traceStr.append("\n       at ");
//             traceStr.append(elem.toString());
//         }
//         System.out.println(traceStr.toString());
//     }
    
    public static String[] ignoredPackages = {
        "java.", "javax.", "sun.", "apache.", "org.", "com.sun."
//         "java.awt.", "java.io.", "java.math.", "java.nio.", "java.security.",
//         "java.text.", "java.applet.", "java.lang.", "java.net.", "java.rmi.", 
//         "java.sql.", "java.util.", "sun.", "apache.", "org."
    };

    private HashMap<String, Class> loadedClasses = new HashMap<String, Class>();

    private Class marshallingRuntimeClass = null;
    
    private ClassPath classPath = null;
    
    private boolean injecting = false;
    
    private java.lang.reflect.Method maybeInjectMarshallingMethod = null;

    private java.lang.reflect.Method addMarshallingClassMethod = null;
    
}

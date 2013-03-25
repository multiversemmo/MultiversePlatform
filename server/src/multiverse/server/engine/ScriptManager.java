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

import multiverse.server.util.*;
import org.mozilla.javascript.*;

import java.io.*;
 
import org.python.core.PySystemState;
import org.python.core.PyObject;
import org.python.core.PyModule;
import org.python.core.PyFile;
import org.python.core.PyString;
import org.python.core.PyStringMap;
import org.python.core.Py;
import org.python.core.imp;
import org.python.core.__builtin__;

public class ScriptManager {
    public ScriptManager() {
    }

    /** Initialize the script manager, must be called prior to running
        any scripts.  Only one of init() or initLocal() should be called.
        Script managers calling init() share a single global namespace.
    */
    public void init()
    {
        if (cx != null)
            return;
        initPythonState();
        pyLocals = null;
        // Set up JavaScript
        try {
	    cx = Context.enter();
	    scope = new ImporterTopLevel(cx, true);
	}
	catch(Exception e) {
	    throw new MVRuntimeException(e.toString());
	}
    }

    /** Initialize the script manager, must be called prior to running
        any scripts.  Only one of initLocal() or init() should be called.
        Script managers calling initLocal() have a private local
        namespace, but share the global namespace with all script managers.
    */
    public void initLocal()
    {
        if (cx != null)
            return;
        initPythonState();
        pyLocals = (new PyModule("main", new PyStringMap())).__dict__;
    }

    private void initPythonState()
    {
        synchronized (this.getClass()) {
            if (pySystemState == null) {
                PySystemState.initialize();
                //pySystemState = new PySystemState();
                pySystemState = Py.defaultSystemState;
                pySystemState.setClassLoader(this.getClass().getClassLoader());
                mvmodule = imp.addModule("mvmodule");
                // runPYFile("/tmp/init.py");
            }
        }
    }

    /**
     * executes the buffer, using JS interpreter
     * resturns result object
     */
    public synchronized Object runJSBuffer(String buf) 
	throws JavaScriptException {
	Object result = cx.evaluateString(scope, // scope
					  buf,  // source
					  "ScriptManager", // sourceName
					  1,  // line number
					  null); // security domain
	return result;
    }

    /**
     * chooses JS or PY based on extension (.py or .js)
     */
    public synchronized void runFile(String filename) 
	throws JavaScriptException, 
	       FileNotFoundException, 
	       IOException,
	       MVRuntimeException
    {
        try {
            if (filename.endsWith(".py")) {
                runPYFile(filename);
            }
            else if (filename.endsWith(".js")) {
                runJSFile(filename);
            }
            else {
                throw new MVRuntimeException("Unknown script file type");
            }
        }
        catch (FileNotFoundException e) {
            // ignore
        }
        catch (MVRuntimeException e) {
            throw e;
        }
        catch (Exception e) {
            // ignore
        }
    }

    public synchronized void runFileWithThrow(String filename) 
	throws JavaScriptException, 
	       FileNotFoundException, 
	       IOException,
	       MVRuntimeException
    {
        if (filename.endsWith(".py")) {
            runPYFile(filename);
        }
        else if (filename.endsWith(".js")) {
            runJSFile(filename);
        }
        else {
            throw new MVRuntimeException("Unknown script file type");
        }
    }

    /**
     * JS file
     */
    public synchronized Object runJSFile(String filename) 
	throws JavaScriptException,
                FileNotFoundException,
                IOException
    {
	Reader in = null;
        try {
            in = new FileReader(filename);
        } catch (FileNotFoundException e) {
            Log.warn("ScriptManager.runJSFile: file not found: " + filename);
            throw e;
        }
	Object result = null;
        try {
            result = cx.evaluateReader(scope, in, filename, 1, null);
        } catch (IOException e) {
            Log.exception("ScriptManager.runJSFile file="+filename, e);
            throw e;
        } catch (RuntimeException e) {
            Log.exception("ScriptManager.runJSFile file="+filename, e);
            throw e;
        }
	
        return result;
    }

    /**
     * python file
     * returns false if the file cant be found
     */
    public synchronized boolean runPYFile(String filename) 
        throws FileNotFoundException
    {
        if (Log.loggingDebug)
            Log.debug("runPYFile: file=" + filename);
        FileInputStream in = null;
        try {
            in = new FileInputStream(filename);
        }
        catch(FileNotFoundException e) {
            Log.warn("ScriptManager.runPYFile: file not found: " + filename);
            throw e;
        }

        try {
            Py.setSystemState(pySystemState);
            // runCode(code, locals, globals)
            Py.runCode(Py.compile_flags(in, filename, "exec",null),
                pyLocals, mvmodule.__dict__);
        } catch (RuntimeException e) {
            Log.exception("ScriptManager.runPYFile: file="+filename, e);
            throw e;
        }
	return true;
    }

    public synchronized String getResultString(Object resultObj) {
	return Context.toString(resultObj);
    }

    public static class ScriptOutput {
        public ScriptOutput(String out, String err) {
            stdout = out;
            stderr = err;
        }
        public String stdout;
        public String stderr;
    }

    public synchronized ScriptOutput runPYScript(String script)
    {
        ByteArrayOutputStream stdout = new ByteArrayOutputStream();
        ByteArrayOutputStream stderr = new ByteArrayOutputStream();
        PyObject saveStdout = pySystemState.stdout;
        PyObject saveStderr = pySystemState.stderr;
        pySystemState.stdout = new PyFile(stdout);
        pySystemState.stderr = new PyFile(stderr);
        Py.setSystemState(pySystemState);

        // exec(object, globals, locals)
        Py.exec(Py.compile_flags(script, "<string>", "exec",null),
            mvmodule.__dict__, pyLocals);

        pySystemState.stdout = saveStdout;
        pySystemState.stderr = saveStderr;
        return new ScriptOutput(stdout.toString(), stderr.toString());
    }

    public synchronized PyObject evalPYScript(String script)
    {
        Py.setSystemState(pySystemState);
        // eval(object, globals, locals)
        return __builtin__.eval(new PyString(script), mvmodule.__dict__, pyLocals);
    }

    public synchronized String evalPYScriptAsString(String script)
    {
        Py.setSystemState(pySystemState);
        // eval(object, globals, locals)
        PyObject result = __builtin__.eval(new PyString(script),
            mvmodule.__dict__, pyLocals);
        if (result == null)
            return null;
        else
            return result.toString();
    }

    private Context cx = null;
    private ScriptableObject scope = null;
    private static PyModule mvmodule = null;
    private static PySystemState pySystemState = null;
    private PyObject pyLocals;
}

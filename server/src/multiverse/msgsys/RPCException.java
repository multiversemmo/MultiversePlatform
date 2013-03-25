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

package multiverse.msgsys;


import java.util.List;

/** RPC handler threw an exception when handling a request.  This
exception is thrown locally when the remote handler throws an
exception.  This exception carries the full stack frames from
the remote context.
*/
public class RPCException extends RuntimeException
{
    /** Create RPCException from remote exception data.
    */
    public RPCException(ExceptionData exceptionData)
    {
        super(exceptionData.getMessage());
        agentName = exceptionData.getAgentName();
        exceptionClass = exceptionData.getExceptionClassName();
        List<StackFrame> stackTrace = exceptionData.getStackTrace();
        StackTraceElement[] exFrames = new StackTraceElement[stackTrace.size()];
        int ii = 0;
        for (StackFrame stackFrame : stackTrace) {
            exFrames[ii] = new StackTraceElement(stackFrame.declaringClass,
                stackFrame.methodName, stackFrame.fileName,
                stackFrame.lineNumber);
            ii++;
        }
        setStackTrace(exFrames);

        if (exceptionData.getCause() != null)
            initCause(new RPCException(exceptionData.getCause()));
    }

    /** Get remote exception class name.
    */
    public String getExceptionClassName()
    {
        return exceptionClass;
    }

    /** Get remote agent name.
    */
    public String getAgentName()
    {
        return agentName;
    }

    public String toString()
    {
        String myName = getClass().getName()+"("+exceptionClass+" in "+agentName+")";
        String message = getLocalizedMessage();
        return (message != null) ? (myName + ": " + message) : myName;
    }

    private String exceptionClass;
    private String agentName;

    static String myAgentName;
    
    private static final long serialVersionUID = 1L;
}


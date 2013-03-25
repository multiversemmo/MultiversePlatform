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

import java.util.ArrayList;
import java.util.List;

/** Extract data from an exception.  This is used to carry exception data
between agent.
*/
public class ExceptionData
{
    public ExceptionData()
    {
    }

    public ExceptionData(Throwable ex)
    {
        agentName = RPCException.myAgentName;
        exceptionClass = ex.getClass().getName();
        detailMessage = ex.getMessage();
        StackTraceElement[] exFrames = ex.getStackTrace();
        stackTrace = new ArrayList<StackFrame>(exFrames.length);
        for (StackTraceElement frame : exFrames) {
            stackTrace.add(new StackFrame(frame));
        }
        if (ex.getCause() != null)
            cause = new ExceptionData(ex.getCause());
    }

    public String getExceptionClassName()
    {
        return exceptionClass;
    }

    public String getMessage()
    {
        return detailMessage;
    }

    public ExceptionData getCause()
    {
        return cause;
    }

    List<StackFrame> getStackTrace()
    {
        return stackTrace;
    }
 
    public String getAgentName()
    {
        return agentName;
    }

    private String agentName;
    private String exceptionClass;
    private String detailMessage;
    private ExceptionData cause;
    private ArrayList<StackFrame> stackTrace;
}


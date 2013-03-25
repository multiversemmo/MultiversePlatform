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

package multiverse.management;

import java.util.List;
import java.util.LinkedList;
import java.util.Set;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;

import gnu.getopt.Getopt;

import sun.management.ConnectorAddressLink;
import sun.jvmstat.monitor.MonitoredHost;
import sun.jvmstat.monitor.HostIdentifier;
import javax.management.remote.JMXServiceURL;
import javax.management.remote.JMXConnector;
import javax.management.remote.JMXConnectorFactory;
import javax.management.MBeanServerConnection;
import javax.management.ObjectName;
import javax.management.JMException;

public class CommandMain
{

    // Usage: mvm [-q] -p <pid|agent> -f <script-file> -s <script>
    public static void main(String[] args)
    {
        Getopt g = new Getopt("CommandMain", args, "p:f:s:q");
        List<String> scripts = new LinkedList<String>();
        List<String> scriptNames = new LinkedList<String>();
        List<String> processes = new LinkedList<String>();
        boolean quiet = false;

        int c;
        while ((c = g.getopt()) != -1) {
            String script;
            switch(c) {
            // process
            case 'p':
                processes.add(g.getOptarg());
                break;
            // script-file
            case 'f':
                String fileName = g.getOptarg();
                scriptNames.add(fileName);
                script = readFile(fileName);
                if (script == null)
                    System.exit(1);
                scripts.add(script);
                break;
            // script
            case 's':
                script = g.getOptarg();
                scripts.add(script);
                scriptNames.add(script);
                break;
            // quiet
            case 'q':
                quiet = true;
                break;
            }
        }

        if (processes.size() == 0 || scripts.size() == 0) {
            System.err.println("Usage: mvm -p <pid|agent-name> -s <script> -f <script-file>");
            System.exit(1);
        }

        String argvString = "argv = [\"<string>\"";
        int arg = 0;
        for (int ii = g.getOptind(); ii < args.length ; ii++) {
            argvString += ",\""+args[ii]+"\"";
            arg++;
        }
        argvString += "]\n";

        boolean ok = true;
        for (String process : processes) {
            int ss = 0;
            for (String script : scripts) {
                if (! quiet)
                    System.out.println("Process "+process+": "+scriptNames.get(ss));
                ss++;
                ok = ok && execScript(process, argvString + script);
            }
        }
        if (ok)
            System.exit(0);
        else
            System.exit(1);
    }

    static String readFile(String fileName)
    {
        File scriptFile = new File(fileName);
        if (! scriptFile.exists()) {
            System.err.println(fileName+": file does not exist");
            return null;
        }

        char[] data = new char[(int)scriptFile.length()];
        try {
            FileReader reader = new FileReader(scriptFile);
            reader.read(data,0,(int)scriptFile.length());
            reader.close();
        }
        catch (java.io.IOException e) {
            System.err.println(fileName+": "+e);
            return null;
        }
        return new String(data);
    }

    static boolean execScript(String process, String script)
    {
        int vmid = -1;
        try {
            vmid = Integer.parseInt(process);
        }
        catch (NumberFormatException e) {
            vmid = findVmid(process);
        }
        if (vmid == -1) {
            System.err.println(process+": Could not find process");
            return false;
        }
        
        try {
            JMXConnector jmxc;
            MBeanServerConnection server;

            String address = ConnectorAddressLink.importFrom(vmid);
            JMXServiceURL jmxUrl = new JMXServiceURL(address);
            jmxc = JMXConnectorFactory.connect(jmxUrl);
            server = jmxc.getMBeanServerConnection();
            Object[] parameters = {script};
            String[] signature = {"java.lang.String"};
            Object result;
            result = server.invoke(new ObjectName("net.multiverse:type=Engine"),
                "runPythonScript", parameters, signature);
            System.out.println(result.toString());
            jmxc.close();
        }
        catch (IOException e) {
            System.err.println("Unable to attach to " + vmid +
                ": " + e.getMessage());
            return false;
        }
        catch (javax.management.MalformedObjectNameException e) {
            System.err.println("Internal error: " + e.getMessage());
            return false;
        }
        catch (javax.management.InstanceNotFoundException e) {
            System.err.println("Process "+vmid+" is not a Multiverse engine");
            return false;
        }
        catch (javax.management.MBeanException e) {
            System.err.println("Error: "+e);
            return false;
        }
        catch (javax.management.ReflectionException e) {
            System.err.println("Error: "+e);
            return false;
        }
        return true;
    }
    
    static int findVmid(String agentName)
    {
        MonitoredHost host;
        if (activeVms == null) {
            try {
                host = MonitoredHost.getMonitoredHost(new HostIdentifier((String)null));
                activeVms = host.activeVms();
            } catch (java.net.URISyntaxException e) {
                throw new InternalError(e.getMessage());
            } catch (sun.jvmstat.monitor.MonitorException e) {
                throw new InternalError(e.getMessage());
            }
        }

        for (Object vm : activeVms) {
            try {
                String address = ConnectorAddressLink.importFrom((Integer)vm);
                if (address == null)
                    continue;
                JMXConnector jmxc;
                MBeanServerConnection server;
                JMXServiceURL jmxUrl = new JMXServiceURL(address);
                jmxc = JMXConnectorFactory.connect(jmxUrl);
                server = jmxc.getMBeanServerConnection();
                Object result;
                result = server.getAttribute(
                    new ObjectName("net.multiverse:type=Engine"), "AgentName");
                jmxc.close();
                if (result != null && result.toString().equals(agentName))
                    return (Integer)vm;
            }
            catch (IOException e) {
                System.err.println("Unable to attach to " + (Integer)vm +
                    ": " + e.getMessage());
            }
            catch (javax.management.InstanceNotFoundException e) {
                // ignore
            }
            catch (JMException e) {
                System.err.println("Unable to attach to " + (Integer)vm + ": " + e);
            }
        }

        return -1;
    }

    static Set activeVms = null;
    
}


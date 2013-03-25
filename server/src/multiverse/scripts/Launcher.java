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

/**
 * This is the server process launcher that essentially replaces 
 * multiverse.sh and start-multiverse.bat. 
 * 
 * It requires lines in the properties file like this:
 * 
 * TODO: 
 * - Create run directory if needed.
 * - Create run/worldname directory if needed
 * - Add code to Engine to read in logFile name and set in Log.java, assuming that actually works
 * - Add real exit() method to Engine that Launcher can call via JMX. 

#
# Server startup
#
multiverse.servers=messageServer,loginManager,animationServer,combatServer,objectManager,worldManager,proxyServer,worldReader,mobServer

animationServer.scripts=-i bin\\wmgr_local1.js,config\\common\\global_props.py,config\\sampleworld\\global_props.py,config\\common\\anim.py,config\\sampleworld\\extensions_anim.py
animationServer.logFile=anim.out

loginManager.scripts=bin\\login_manager.py,config\\common\\character_factory.py,config\\sampleworld\\character_factory.py,config\\sampleworld\\extensions_login.py
loginManager.logFile=login_manager.out

combatServer.scripts=-i bin\\wmgr_local1.js,config\\common\\global_props.py,config\\sampleworld\\global_props.py,config\\common\\ability_db.py,config\\common\\combat.py,config\\sampleworld\\extensions_combat.py
combatServer.logFile=combat.out

objectManager.scripts=-i bin\\wmgr_local1.js,config\\common\\global_props.py,config\\sampleworld\\global_props.py,config\\common\\obj_manager.py,config\\sampleworld\\items_db.py,config\\sampleworld\\mobs_db.py,config\\sampleworld\\extensions_objmgr.py
objectManager.logFile=objmgr.out

worldManager.scripts=-i bin\\wmgr_local1.js,config\\common\\global_props.py,config\\sampleworld\\global_props.py,config\\common\\world_mgr.py,config\\sampleworld\\extensions_wmgr.py
worldManager.logFile=wmgr1.out

proxyServer.scripts=-i bin\\proxy.py,-i config\\common\\events.py,config\\common\\proxy.py,config\\common\\global_props.py,config\\sampleworld\\global_props.py,config\\sampleworld\\extensions_proxy.py
proxyServer.logFile=proxy.out

worldReader.scripts=-i bin\\mobserver_local.js,config\\common\\global_props.py,config\\sampleworld\\global_props.py,config\\common\\worldreader.py,config\\sampleworld\\extensions_worldreader.py
worldReader.logFile=worldreader.out

mobServer.scripts=-i bin\\mobserver_local.js,config\\common\\mobserver_init.py,config\\sampleworld\\mobserver_init.py,config\\common\\mobserver.py,config\\sampleworld\\mobserver.py,config\\common\\questplugin.py,config\\common\\extensions_mobserver.py
mobServer.logFile=mobserver.out

NOTE: the above are outdated, and probably don't work.  Also has sampleworld hardcoded,
and of course that should be based on the worldname.

 * 
 */

package multiverse.scripts;

import java.lang.management.ManagementFactory;
import java.util.*;
import javax.management.ObjectName;
import multiverse.server.engine.PropertyFileReader;

public class Launcher {

    Launcher() {
        propFile = System.getProperty("multiverse.propertyfile");
        System.out.println((new StringBuilder("Using property file ")).append(propFile).toString());
        PropertyFileReader pfr = new PropertyFileReader();
        properties = pfr.readPropFile();
    }

    public int exit() {
        System.runFinalization();
        /* MBeanServer mbs = */ ManagementFactory.getPlatformMBeanServer();
        try {
            ObjectName name = new ObjectName("multiverse.server.engine.Launcher:type=Launcher");
            ManagementFactory.getPlatformMBeanServer().unregisterMBean(name);
            System.out.println("Unregistered Launcher with JMX mgmt agent");
            System.exit(0);
            
        } catch(Exception ex) {
            System.out.println((new StringBuilder("Message Server: caught exception: ")).append(ex).toString());
            ex.printStackTrace();
        }
        System.exit(0);
        return 0;
    }

    public void startAllServers() {
    	//Create run directory if needed.
    	
    	//Create run/worldname directory if needed
    	
        String servers = properties.getProperty("multiverse.servers");
        try {
            if(servers != null) {
                String serverArray[] = servers.split(",");
                for(int i = 0; i < serverArray.length; i++)
                    if(serverArray[i] != null)
                    {
                        System.out.println((new StringBuilder(">>>Starting server #")).append(i).toString());
                        startServer(serverArray[i]);
                        Thread.sleep(5000L);
                    } else {
                        System.out.println((new StringBuilder("ERROR - server ")).append(i).append(" is null").toString());
                    }

            } else {
                System.out.println("server list is null!");
            }
        } catch(Exception ex) {
            System.out.println((new StringBuilder("Error starting all servers: caught exception: ")).append(ex).toString());
            ex.printStackTrace();
        }
    }

    public void printElements(Vector<String> v) {
        System.out.println("ELEMENTS OF COMMAND VECTOR");
        for(Iterator it = v.iterator(); it.hasNext(); System.out.println(it.next()));
    }

    public Process startServer(String svrName) {
        Vector<String> cmds = new Vector<String>();
        Process p = null;
        System.out.println((new StringBuilder("Starting ")).append(svrName).toString());
        cmds.addElement("java");
        cmds.addElement((new StringBuilder("-Dmultiverse.propertyfile=")).append(propFile).toString());
        cmds.addElement("-Dcom.sun.management.jmxremote");
        cmds.addElement((new StringBuilder("-Dmultiverse.servername=")).append(svrName).toString());
        // TODO: Steve will decide what to do
        if(svrName == "messageServer")
            cmds.addElement("multiverse.msgsvr.MessageServer");
        else
            cmds.addElement("multiverse.server.engine.Engine");
        try {
            String scriptlist = properties.getProperty((new StringBuilder(String.valueOf(svrName))).append(".scripts").toString());
            if(scriptlist != null)
            {
                String scripts[] = scriptlist.split(",");
                System.out.print("scripts: ");
                for(int i = 0; i < scripts.length; i++) {
                    System.out.print((new StringBuilder(String.valueOf(scripts[i]))).append(",  ").toString());
                    cmds.addElement(scripts[i]);
                }

                System.out.println("\n---------");
            } else {
                System.out.println((new StringBuilder("No scripts specified for ")).append(svrName).toString());
            }
            List<String> lCmds = cmds;
            ProcessBuilder pb = new ProcessBuilder(lCmds);
            if(pb != null) {
                String cp = System.getProperty("java.class.path");
                Map<String, String> env = pb.environment();
                env.put("CLASSPATH", cp);
                p = pb.start();
            } else {
                System.out.println("pb is null!");
            }
        } catch(Exception e) {
            System.out.println("Exception in Launcher ");
            e.printStackTrace();
        }
        return p;
    }

    public static void main(String args[]) {
        Launcher launcher = new Launcher();
        String command = "all";
        command = args[0];
        if(command == null)
            command = "all";
        try {
            launcher.startAllServers();
        } catch(Exception e) {
            System.out.println("Exception in Launcher ");
            e.printStackTrace();
        }
    }

    public static Properties properties = new Properties();
    public String propFile;

}

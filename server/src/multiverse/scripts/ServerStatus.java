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

import java.io.*;

public class ServerStatus {

   public static void main(String args[]) {
       boolean running = false;
       String serverName, worldName, sFile;

       if (args.length < 2) {
           System.err.println("Error - specify server name and world name on cmd line.");
           System.exit(1);
       } else {
           worldName = args[0];
           serverName = getServerName(args[1]);

           sFile = PATH + File.separator + worldName + File.separator + STATUSFILE;
           File f = new File(sFile);
           if (!f.exists()) {
               System.err.println("Error - Staus file " + STATUSFILE + " does not exist");
               System.exit(1);
           }

           try {

               BufferedReader inputStream = new BufferedReader(new FileReader(sFile));

               String line;
               while ((line = inputStream.readLine()) != null) {
                   if (line.length()>0 && line.contains("java.exe")) {
                       running = true;
                   }
               }

           }  catch (Exception e) {
               System.err.println("File input error");
           }

           if (running==true)
               System.out.println(serverName + ": running");
           else
               System.out.println(serverName + ": not running");

       }
   }

   public static String getServerName(String sName) {
       String s = "default";
       if (sName.equals("anim"))
           s="Animation server";
       else if (sName.equals("combat"))
           s="Combat server";
       else if (sName.equals("domain"))
           s="Message domain server";
       else if (sName.equals("objmgr"))
           s="Object server";
       else if (sName.equals("wmgr_1"))
           s="World manager";
       else if (sName.equals("login_manager"))
           s="Login server";
       else if (sName.equals("mobserver"))
           s="Mob server";
       else if (sName.equals("proxy_1"))
           s="Proxy server";
       else if (sName.equals("startup"))
           s="Startup monitor";
       else if (sName.equals("instance"))
           s="Instance server";
       else if (sName.equals("voiceserver"))
           s="Voice server";

       return s;
   }

   public static String RUNNING = "java.exe";
   public static String NOT_RUNNING = "No tasks";
   public static String PATH = "run";
   public static String STATUSFILE = "status.txt";

}

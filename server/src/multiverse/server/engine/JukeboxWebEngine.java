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

import java.io.IOException;
import java.io.PrintWriter;

import javax.servlet.ServletException;


import java.util.*;

import javax.servlet.*;
import javax.servlet.http.*;

import multiverse.server.plugins.JukeboxWebPlugin;

public class JukeboxWebEngine extends HttpServlet {
    private ArrayList<String> nameList;
    private ArrayList<String> typeList;
    private ArrayList<String> urlList;
    private ArrayList<String> costList;
    private ArrayList<String> descriptionList;

    private static final long serialVersionUID = 1L;

    public void init(ServletConfig config) throws ServletException {
	nameList = new ArrayList<String>();
	typeList = new ArrayList<String>();
	urlList = new ArrayList<String>();
	costList = new ArrayList<String>();
	descriptionList = new ArrayList<String>();
	JukeboxWebEngineThread jukeboxWebEngineThread = new JukeboxWebEngineThread();
	new Thread(jukeboxWebEngineThread).start();
    }

    protected void doGet(HttpServletRequest req, HttpServletResponse res)
	throws ServletException, IOException
    {
	res.setContentType("text/html");
	res.setHeader("pragma", "no-cache");
	String playerOid = req.getParameter("poid");
	PrintWriter out = res.getWriter();
	if ((playerOid == null) || (playerOid.length()==0)) {
	    out.print("<HTML><HEAD><TITLE>Jukebox Media Manager</TITLE></HEAD>");
	    out.print("<BODY><H3>Jukebox Media Control:</H3><TABLE border=\"1\">");
	    out.print("<TR><TH>NAME</TH><TH>TYPE</TH><TH>URL</TH><TH>COST</TH><TH>DESCRIPTION</TH></TR>");
	    for (int i = 0; i < nameList.size(); i++) {
		out.print("<TR>");
		out.print("<TD>" + nameList.get(i) + "</TD>");
		out.print("<TD>" + typeList.get(i) + "</TD>");
		out.print("<TD>" + urlList.get(i) + "</TD>");
		out.print("<TD>" + costList.get(i) + "</TD>");
		out.print("<TD>" + descriptionList.get(i) + "</TD>");
		out.print("</TR>");
	    }
	    out.print("</TABLE><HR><FORM METHOD=POST>");
	    out.print("<TABLE>");
	    out.print("<TR><TD>name:</TD><TD><INPUT TYPE=TEXT NAME=name></TD></TR>");
	    out.print("<TR><TD>type:</TD><TD><INPUT TYPE=TEXT NAME=type></TD></TR>");
	    out.print("<TR><TD>url:</TD><TD><INPUT TYPE=TEXT NAME=url></TD></TR>");
	    out.print("<TR><TD>cost:</TD><TD><INPUT TYPE=TEXT NAME=cost></TD></TR>");
	    out.print("<TR><TD>description:</TD><TD><INPUT TYPE=TEXT NAME=description></TD></TR>");
	    out.print("<TR><TD></TD></TR>");
	    out.print("<TR><TD align=\"center\" colspan=\"2\">");
	    out.print("<INPUT TYPE=SUBMIT NAME=action VALUE=add>");
	    out.print("<INPUT TYPE=SUBMIT NAME=action VALUE=delete>");
	    out.print("<INPUT TYPE=SUBMIT NAME=action VALUE=get>");
	    out.print("</TD></TR>");
	    out.print("</TABLE>");
	    out.print("</FORM></BODY></HTML>");
	    out.close();
	} else {
	    int funds = getMoney(playerOid);
	    out.print("<HTML><HEAD><TITLE>Jukebox Funds Manager</TITLE></HEAD>");
	    out.print("<BODY><H3>Jukebox Funds Control:</H3>");
	    out.print("Player OID: " + playerOid + "<BR>");
	    out.print("Current Funds: $" + (funds/100));
	    if ((funds%100) < 10) {
		out.print(".0" + (funds%100) + "<BR>");
	    } else {
		out.print("." + (funds%100) + "<BR>");
	    }
	    out.print("<FORM METHOD=POST>");
	    out.print("<INPUT TYPE=TEXT NAME=money><BR>");
	    out.print("<INPUT TYPE=SUBMIT NAME=action VALUE=deposit><BR>");
	    out.print("<INPUT TYPE=HIDDEN NAME=poid VALUE=" + playerOid + "><BR>");
	    out.print("</FORM>");
	    out.print("</BODY></HTML>");
	    out.close();
	}
    }

    protected void doPost(HttpServletRequest req, HttpServletResponse res)
	throws ServletException, IOException
    {
	String name = req.getParameter("name");
	String type = req.getParameter("type");
	String url = req.getParameter("url");
	String cost = req.getParameter("cost");
	String description = req.getParameter("description");
	String poid = req.getParameter("poid");
	String money = req.getParameter("money");
	String msg = "";
	if ((poid == null) || (poid.length()==0)) {
	    if ((name.length()==0) && (!req.getParameter("action").equals("get"))) {
		res.sendError(HttpServletResponse.SC_BAD_REQUEST, "No name specified.");
		return;
	    }
	    if (req.getParameter("action").equals("add")) {
		if (type.length()==0) {
		    res.sendError(HttpServletResponse.SC_BAD_REQUEST, "Type must be stream or audio.");
		    return;
		}
		if (url.length()==0) {
		    res.sendError(HttpServletResponse.SC_BAD_REQUEST, "No url specified.");
		    return;
		}
		if (cost.length()==0) {
		    res.sendError(HttpServletResponse.SC_BAD_REQUEST, "No cost specified.");
		    return;
		}
		if (description.length()==0) {
		    description = "n/a";
		}
		if (addTrack(name, type, url, cost, description)) {
		    msg = "" + name + " has been added.";
		} else {
		    res.sendError(HttpServletResponse.SC_BAD_REQUEST, "" + name + " is already in the list.");
		    return;
		}
	    } else if (req.getParameter("action").equals("delete")) {
		if (deleteTrack(name)) {
		    msg = "" + name + " has been deleted.";
		} else {
		    res.sendError(HttpServletResponse.SC_BAD_REQUEST, "" + name + " is not in the list.");
		    return;
		}
	    } else if (req.getParameter("action").equals("get")) {
		if (getTracks()) {
		    msg = "Got tracks.";
		} else {
		    res.sendError(HttpServletResponse.SC_BAD_REQUEST, "Cannot get tracks.");
		    return;
		}
	    }
	} else {
	    if (req.getParameter("action").equals("deposit")) {
		if (addMoney(poid, money)) {
		    msg = "Added money.";
		} else {
		    res.sendError(HttpServletResponse.SC_BAD_REQUEST, "Cannot add money.");
		    return;
		}
	    }
	}

	res.setContentType("text/html");
	res.setHeader("pragma", "no-cache");
	PrintWriter out = res.getWriter();
	out.print("<HTML><HEAD><TITLE>Jukebox Manager</TITLE></HEAD><BODY>");
	out.print(msg);
	out.print("<HR><A HREF=\"");
	out.print(req.getRequestURL());
	if (!((poid == null)||(poid.length()==0))) {
	    out.print("?poid="+poid);
	}
	out.print("\">Return</A></BODY></HTML>");
	out.close();
    }

    public String getServletInfo() {
	return "JukeboxWebEngine";
    }

    private synchronized boolean addTrack(String name, String type, String url, String cost, String description)
	throws IOException
    {
	JukeboxWebPlugin jukeboxWebPlugin = (JukeboxWebPlugin)Engine.getPlugin("JukeboxWebPlugin");
	if (jukeboxWebPlugin == null) return false;
	if (nameList.contains(name)) return false;
	nameList.add(name);
	typeList.add(type);
	urlList.add(url);
	costList.add(cost);
	descriptionList.add(description);
	jukeboxWebPlugin.addTrack(name, type, url, cost, description);
	return true;
    }

    private synchronized boolean deleteTrack(String name) throws IOException {
	JukeboxWebPlugin jukeboxWebPlugin = (JukeboxWebPlugin)Engine.getPlugin("JukeboxWebPlugin");
	if (jukeboxWebPlugin == null) return false;
	int index = nameList.indexOf(name);
	if (index == -1) return false;
	nameList.remove(index);
	typeList.remove(index);
	urlList.remove(index);
	costList.remove(index);
	descriptionList.remove(index);
	jukeboxWebPlugin.deleteTrack(name);
	return true;
    }

    private synchronized boolean getTracks() {
	JukeboxWebPlugin jukeboxWebPlugin = (JukeboxWebPlugin)Engine.getPlugin("JukeboxWebPlugin");
	if (jukeboxWebPlugin == null) return false;
	ArrayList trackData = jukeboxWebPlugin.getTracks();
	if (trackData == null) return false;
	nameList.clear();
	typeList.clear();
	urlList.clear();
	costList.clear();
	descriptionList.clear();
	for (int i = trackData.size(); i-- > 0;) {
	    HashMap trackInfo = (HashMap)trackData.get(i);
	    nameList.add((String)trackInfo.get("name"));
	    typeList.add((String)trackInfo.get("type"));
	    urlList.add((String)trackInfo.get("url"));
	    costList.add((String)trackInfo.get("cost"));
	    descriptionList.add((String)trackInfo.get("description"));
	}
	return true;
    }

    private synchronized int getMoney(String poid) {
	JukeboxWebPlugin jukeboxWebPlugin = (JukeboxWebPlugin)Engine.getPlugin("JukeboxWebPlugin");
	if (jukeboxWebPlugin == null) return 0;
	int money = jukeboxWebPlugin.getMoney(poid);
	return money;
    }

    private synchronized boolean addMoney(String poid, String money) {
	JukeboxWebPlugin jukeboxWebPlugin = (JukeboxWebPlugin)Engine.getPlugin("JukeboxWebPlugin");
	Double dDollars = new Double(money);
	dDollars = dDollars * 100.0;
	Integer iDollars = new Integer(dDollars.intValue());
	if (jukeboxWebPlugin == null) return false;
	jukeboxWebPlugin.addMoney(poid, iDollars.toString());
	return true;
    }
}


class JukeboxWebEngineThread implements Runnable {
    public void run() {
	// System.setProperty("multiverse.propertyfile", "c:/cygwin/usr/local/apache-tomcat-6.0.14/webapps/LES_world/WEB-INF/lib/multiverse.properties");
	// System.setProperty("multiverse.worldname", "LES_world");
	// System.setProperty("multiverse.logs", "c:/cygwin/usr/local/apache-tomcat-6.0.14/logs");
	// System.setProperty("multiverse.loggername", "jukeboxweb");
	String args[] = new String[3];
	args[0] = "-i";
	// args[1] = "c:/cygwin/usr/local/apache-tomcat-6.0.14/webapps/LES_world/WEB-INF/lib/mvmessages.py";
	// args[2] = "c:/cygwin/usr/local/apache-tomcat-6.0.14/webapps/LES_world/WEB-INF/lib/jukeboxweb.py";
	args[1] = System.getProperty("multiverse.jukebox.arg1");
	args[2] = System.getProperty("multiverse.jukebox.arg2");
	Engine.main(args);
    }
}

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">

<%@ page import="java.util.*, java.net.*, java.io.*" %>

<%
    String masterHostname = "www.multiverse.net";
    int masterPort = 9005;

    String processing = request.getParameter("loginRefer");
    String usernameString = request.getParameter("username");
    String passwordString = request.getParameter("password");

    int success = 0;
    int token = 0;

    if (processing != null) {



        Socket socket = null;
        try {
            socket = new Socket(masterHostname, masterPort);
            InputStream sin = socket.getInputStream();
            OutputStream sout = socket.getOutputStream();

            DataInputStream din = new DataInputStream(sin);
            DataOutputStream dout = new DataOutputStream(sout);
            
            byte[] username = usernameString.getBytes();
            dout.writeInt(username.length);
            dout.write(username);

            byte[] password = passwordString.getBytes();
            dout.writeInt(password.length);
            dout.write(password);

            // read success (1) failure (0)
            success = din.readInt();

            din.readInt();

            // read user id
            token = din.readInt();
        }
        catch(SocketException se) {
            %> ERROR: caught SocketException <%= se %> <%
        }
        catch(Exception e) {
            %> ERROR: caught Exception <%= e %> <%
        }
        finally {
            if (socket != null) {
                try {
                    socket.close();
                }
                catch(Exception e) {
                    socket = null;
                }
            }
        }

        if (success == 1) {
            session.setAttribute("token", new Integer(token));
            if(session.getAttribute("loginTargetPage") == null) {
                response.sendRedirect("charlist.jsp");
            } else {
                response.sendRedirect((String)session.getAttribute("loginTargetPage"));
            }
            return;
        }
    }
%>

<html>
  <head>
    <title>Login</title>
  </head>

  <body>
    <h1>Login</h1>

    <form action="login.jsp" method="post">
      <input type="hidden" name="loginRefer" value="yes">
      <table>
        <tr><td><table>
          <tr><td>Username</td><td><input type="text" name="username"></td></tr>
          <tr><td>Password</td><td><input type="password" name="password"></td></tr>
        </table></td></tr>
        <tr><td><br></td></tr>
        <tr><td><input type="submit" value="Login"></td></tr>
      </table>
    </form>
  </body>
</html>

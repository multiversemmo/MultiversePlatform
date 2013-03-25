<%@ page import="java.sql.*, java.util.*" %>
<html>
<head><title> Add Results </title>
<%@ include file="style.html" %>
</head>
<body background="images/bkgd_sub.gif" bottommargin="0" leftmargin="0" marginheight="0" marginwidth="0" rightmargin="0" topmargin="0">

<%@ include file="logo.html" %>
Add Results
<%@ include file="stripes.html" %>

<%
    String connectString = getServletContext().getInitParameter("connect-string");
    String dbuser = getServletContext().getInitParameter("username");
    String dbpasswd = getServletContext().getInitParameter("password");


    // The newInstance() call is a work around for some 
    // broken Java implementations
    Class.forName("com.mysql.jdbc.Driver").newInstance();
    Connection con = DriverManager.getConnection(connectString, dbuser, dbpasswd);

    String worldname = request.getParameter("worldname");
    String servername = request.getParameter("servername");
    int serverport = Integer.parseInt(request.getParameter("serverport"));

    String update = "INSERT INTO worlds (worldname, servername, serverport) VALUES (\"" +
                    worldname + "\", \"" + 
                    servername + "\", \"" +
                    serverport + "\")";
    Statement stmt = con.createStatement();
    int rows = stmt.executeUpdate(update);
%>

update: <%= update %><br>

<%
    if(rows >= 1) {
%>
World <%= worldname %> successfully added!
<%
    } //if

    // clean up
    if(stmt != null) stmt.close();
    if(con != null) con.close();
%>

<%@ include file="bottom.html" %>
</body>
</html>

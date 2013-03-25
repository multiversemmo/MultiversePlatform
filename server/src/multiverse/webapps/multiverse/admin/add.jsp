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

    String username = request.getParameter("username");
    String password = request.getParameter("password");
    String email = request.getParameter("email");

    String update = "INSERT INTO user (username, password, email) VALUES (\"" +
                    username + "\", \"" + 
                    password + "\", \"" +
                    email + "\")";
    Statement stmt = con.createStatement();
    int rows = stmt.executeUpdate(update);
%>

update: <%= update %><br>

<%
    if(rows >= 1) {
%>
User <%= username %> successfully added!
<%
    } //if

    // clean up
    if(stmt != null) stmt.close();
    if(con != null) con.close();
%>

<%@ include file="bottom.html" %>
</body>
</html>

<%@ page import="java.sql.*, java.util.*" %>
<html>
<head><title> Edit User </title>
<%@ include file="style.html" %>
</head>
<body background="images/bkgd_sub.gif" bottommargin="0" leftmargin="0" marginheight="0" marginwidth="0" rightmargin="0" topmargin="0">

<%@ include file="logo.html" %>
Edit Existing User
</font></td>
<td align="left" valign="top"><img src="images/right_crnr.gif" width="19" height="41" border="0"></td>	
</tr>

<!-- Third row contains body text -->
<tr align="left" valign="top">
<td align="left" valign="top" background="images/stripes.gif">

<%
    String connectString = getServletContext().getInitParameter("connect-string");
    String dbuser = getServletContext().getInitParameter("username");
    String dbpasswd = getServletContext().getInitParameter("password");


    // The newInstance() call is a work around for some 
    // broken Java implementations
    Class.forName("com.mysql.jdbc.Driver").newInstance();
    Connection con = DriverManager.getConnection(connectString, dbuser, dbpasswd);

    String query;
    String username = request.getParameter("username");
    String password = null;
    String email = null;
    if(username != null) {
        query = "SELECT password, email FROM user WHERE username = '" +
                username +
                "'";

        Statement stmt = con.createStatement();
        ResultSet rs = stmt.executeQuery(query);

	if(rs.next()){
            password = rs.getString(1);
	    email = rs.getString(2);
	}

        if(rs != null) rs.close();
        if(stmt != null) stmt.close();
    }

    // clean up
    if(con != null) con.close();

    if(password == null) {
	password = "";
    }

    if(email == null) {
	email = "";
    }
%>

<!-- Table with labels and entry fields -->
<table>
<tr height="8"></tr>

<form action="update.jsp" method="post">
<tr>
<td align="right"><font size="+1" color="FFFFFF">User Name:</font></td>
<td><input type="text" name="username" maxlength="15" size="15"
value="<%= username %>"></td>
</tr>
<tr height="8"></tr>

<tr>
<td align="right"><font size="+1" color="FFFFFF">Password:</font></td>
<td><input type="text" name="password" maxlength="15" size="15"
value="<%= password %>"></td>
</tr>
<tr height="8"></tr>

<tr>
<td align="right"><font size="+1" color="FFFFFF">eMail Addr:</font></td>
<td><input type="text" name="email" maxlength="30" size="20"
value="<%= email %>"></td>
</tr>
<tr height="8"></tr>

<tr>
<td></td><td><input type="submit" value="Update User"></td>
</tr>
<tr height="8"></tr>
</form>
</table>

</td>
<td align="left" valign="top"><img src="images/clear.gif" width="9" border="0"></td>			
<td align="left" valign="top" bgcolor="#EFEFEF"><img src="images/grey.gif" width="19" border="0"></td>
<td align="left" valign="top" bgcolor="#EFEFEF">
<!-- Body text here -->

Change password and email address for existing user.

<%@ include file="bottom.html" %>
</body>
</html>

<%@ page import="java.sql.*, java.util.*" %>
<html>
<head><title> Edit World </title>
<%@ include file="style.html" %>
</head>
<body background="images/bkgd_sub.gif" bottommargin="0" leftmargin="0" marginheight="0" marginwidth="0" rightmargin="0" topmargin="0">

<%@ include file="logo.html" %>
Edit Existing World
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
    String worldname = request.getParameter("worldname");
    String servername = null;
    int serverport = 0;
    if(worldname != null) {
        query = "SELECT servername, serverport FROM worlds WHERE worldname = '" +
                worldname +
                "'";

        Statement stmt = con.createStatement();
        ResultSet rs = stmt.executeQuery(query);

	if(rs.next()){
            servername = rs.getString(1);
	    serverport = rs.getInt(2);
	}

        if(rs != null) rs.close();
        if(stmt != null) stmt.close();
    }

    // clean up
    if(con != null) con.close();

    if(servername == null) {
	servername = "";
    }
%>

<!-- Table with labels and entry fields -->
<table>
<tr height="8"></tr>

<form action="update.jsp" method="post">
<tr>
<td align="right"><font size="+1" color="FFFFFF">World Name:</font></td>
<td><input type="text" name="username" maxlength="15" size="15"
value="<%= worldname %>"></td>
</tr>
<tr height="8"></tr>

<tr>
<td align="right"><font size="+1" color="FFFFFF">Server Name:</font></td>
<td><input type="text" name="password" maxlength="15" size="15"
value="<%= servername %>"></td>
</tr>
<tr height="8"></tr>

<tr>
<td align="right"><font size="+1" color="FFFFFF">Port Number:</font></td>
<td><input type="text" name="email" maxlength="30" size="20"
value="<%= serverport %>"></td>
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

Change server name and port for existing world.

<%@ include file="bottom.html" %>
</body>
</html>

<%@ page import="java.sql.*, java.util.*" %>
<html>
<head><title> World Administration </title>
<%@ include file="style.html" %>
</head>
<body background="images/bkgd_sub.gif" bottommargin="0" leftmargin="0" marginheight="0" marginwidth="0" rightmargin="0" topmargin="0">

<%@ include file="logo.html" %>
World Administration
</font></td>
<td align="left" valign="top"><img src="images/right_crnr.gif" width="19" height="41" border="0"></td>	
</tr>

<!-- Third row contains body text -->
<tr align="left" valign="top">
<td align="left" valign="top" background="images/stripes.gif">

<!-- Table with labels and entry fields -->
<table>
<tr height="8"></tr>

<form action="results.jsp" method="post" name="inputform">
<tr>
<td align="right"><font size="+1" color="FFFFFF">World Name:</font></td>
<td><input type="text" name="worldname" maxlength="32" size="15"></td>
</tr>

<tr height="8"></tr>
<tr>
<td></td><td><input type="submit" value="Search"></td>
</tr>
<tr height="8"></tr>

<tr>
<td></td><td><input type="button" value="Edit World"
onclick="document.inputform.action='edit.jsp';document.inputform.submit()">
</td>
</tr>
<tr height="8"></tr>

<tr>
<td></td><td><input type="button" value="Add World"
onclick="document.inputform.action='new.jsp';document.inputform.submit()">
</td>
</tr>
<tr height="8"></tr>
</form>
</table>

</td>
<td align="left" valign="top"><img src="images/clear.gif" width="9" border="0"></td>			
<td align="left" valign="top" bgcolor="#EFEFEF"><img src="images/grey.gif" width="19" border="0"></td>
<td align="left" valign="top" bgcolor="#EFEFEF">
<!-- Body text here -->

<%
    String connectString = getServletContext().getInitParameter("connect-string");
    String dbuser = getServletContext().getInitParameter("username");
    String dbpasswd = getServletContext().getInitParameter("password");


    // The newInstance() call is a work around for some 
    // broken Java implementations
    Class.forName("com.mysql.jdbc.Driver").newInstance();
    Connection con = DriverManager.getConnection(connectString, dbuser, dbpasswd);

    String query = "SELECT COUNT(*) FROM worlds";       
    Statement stmt = con.createStatement();
    ResultSet rs = stmt.executeQuery(query);

    int count = 0;
    if(rs.next()){
	count = rs.getInt(1);
    }
%>

<%= count %> Worlds in Database

<%
    // clean up
    if(rs != null) rs.close();
    if(stmt != null) stmt.close();
    if(con != null) con.close();
%>

<%@ include file="bottom.html" %>
</body>
</html>

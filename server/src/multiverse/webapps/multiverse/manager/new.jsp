<%@ page import="java.sql.*, java.util.*" %>
<html>
<head><title> Add User </title>
<%@ include file="style.html" %>
</head>
<body background="images/bkgd_sub.gif" bottommargin="0" leftmargin="0" marginheight="0" marginwidth="0" rightmargin="0" topmargin="0">

<%@ include file="logo.html" %>
Add New World
</font></td>
<td align="left" valign="top"><img src="images/right_crnr.gif" width="19" height="41" border="0"></td>	
</tr>

<!-- Third row contains body text -->
<tr align="left" valign="top">
<td align="left" valign="top" background="images/stripes.gif">

<%
    String worldname = request.getParameter("worldname");
%>

<!-- Table with labels and entry fields -->
<table>
<tr height="8"></tr>

<form action="add.jsp" method="post">
<tr>
<td align="right"><font size="+1" color="FFFFFF">World Name:</font></td>
<td><input type="text" name="worldname" maxlength="32" size="15"
value="<%= worldname %>"></td>
</tr>
<tr height="8"></tr>

<tr>
<td align="right"><font size="+1" color="FFFFFF">Server Name:</font></td>
<td><input type="text" name="servername" maxlength="15" size="15"></td>
</tr>
<tr height="8"></tr>

<tr>
<td align="right"><font size="+1" color="FFFFFF">Port Number:</font></td>
<td><input type="text" name="serverport" maxlength="30" size="20"></td>
</tr>
<tr height="8"></tr>

<tr>
<td></td><td><input type="submit" value="Add World"></td>
</tr>
<tr height="8"></tr>
</form>
</table>

</td>
<td align="left" valign="top"><img src="images/clear.gif" width="9" border="0"></td>			
<td align="left" valign="top" bgcolor="#EFEFEF"><img src="images/grey.gif" width="19" border="0"></td>
<td align="left" valign="top" bgcolor="#EFEFEF">
<!-- Body text here -->

Enter worldname, server name, and port number for new world.

<%@ include file="bottom.html" %>
</body>
</html>

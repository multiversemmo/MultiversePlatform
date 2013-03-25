<%@ page import="java.sql.*, java.util.*" %>
<html>
<head><title> User Login </title>
<%@ include file="style.html" %>
</head>
<body background="images/bkgd_sub.gif" bottommargin="0" leftmargin="0" marginheight="0" marginwidth="0" rightmargin="0" topmargin="0">

<%@ include file="logo.html" %>
User Login
</font></td>
<td align="left" valign="top"><img src="images/right_crnr.gif" width="19" height="41" border="0"></td>	
</tr>

<!-- Third row contains body text -->
<tr align="left" valign="top">
<td align="left" valign="top" background="images/stripes.gif">

<%
    String username = request.getParameter("username");
%>

<!-- Table with labels and entry fields -->
<table>
<tr height="8"></tr>

<form action="add.jsp" method="post">
<tr>
<td align="right"><font size="+1" color="FFFFFF">User Name:</font></td>
<td><input type="text" name="username" maxlength="15" size="15"
value="<%= username %>"></td>
</tr>
<tr height="8"></tr>

<tr>
<td align="right"><font size="+1" color="FFFFFF">Password:</font></td>
<td><input type="text" name="password" maxlength="15" size="15"></td>
</tr>
<tr height="8"></tr>

<tr>
<td></td><td><input type="submit" value="Login"></td>
</tr>
<tr height="8"></tr>
</form>
</table>

</td>
<td align="left" valign="top"><img src="images/clear.gif" width="9" border="0"></td>			
<td align="left" valign="top" bgcolor="#EFEFEF"><img src="images/grey.gif" width="19" border="0"></td>
<td align="left" valign="top" bgcolor="#EFEFEF">
<!-- Body text here -->

Enter your username and password.
<br><br><br><br>
<a href="login.jsp">Click here</a> if you forgot your password.<br><br>
<a href="mailto:info@multiverse.net">Send email</a> to request user account.

<%@ include file="bottom.html" %>
</body>
</html>

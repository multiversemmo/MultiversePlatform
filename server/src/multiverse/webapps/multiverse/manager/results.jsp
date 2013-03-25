<%@ page import="java.sql.*, java.util.*" %>
<html>
<head><title> Search Results </title>
<%@ include file="style.html" %>
</head>
<body background="images/bkgd_sub.gif" bottommargin="0" leftmargin="0" marginheight="0" marginwidth="0" rightmargin="0" topmargin="0">

<%@ include file="logo.html" %>
Search Results
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
    String searchString = request.getParameter("worldname");
    if(searchString == null) {
	searchString = "";
    }

    if(searchString.length() == 0) {
        query = "SELECT worldname, servername FROM worlds LIMIT 200";        
    }
    else
    {
        query = "SELECT worldname, servername FROM worlds WHERE worldname LIKE '%" +
                searchString +
                "%' LIMIT 200";
    }

    Statement stmt = con.createStatement();
    ResultSet rs = stmt.executeQuery(query);
%>

<!-- Table with labels and entry fields -->
<table>
<tr height="8"></tr>

<form action="results.jsp" method="post"  name="inputform">
<tr>
<td align="right"><font size="+1" color="FFFFFF">World Name:</font></td>
<td><input type="text" name="worldname" maxlength="15" size="15"
value="<%= searchString %>"></td>
</tr>
<tr height="8"></tr>

<tr>
<td></td><td><input type="submit" value="Search"></td>
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

query: <%= query %>

<table border=0 cellspacing=0 cellpadding=2>

<%
    int rows = 0;
    while(rs.next()){
	rows++;
	String worldname = rs.getString(1);
	String servername = rs.getString(2);
%>

<tr>
<td><a href="edit.jsp?worldname=<%= worldname %>"><%= worldname %></a></td>
<td><%= servername %></td>
</tr>

<%
    } // end while()

    // clean up
    if(rs != null) rs.close();
    if(stmt != null) stmt.close();
    if(con != null) con.close();
%>

</table>
<%= rows %> rows in result set

<%@ include file="bottom.html" %>
</body>
</html>

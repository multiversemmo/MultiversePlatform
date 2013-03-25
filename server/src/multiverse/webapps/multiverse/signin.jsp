<%@ page import="java.sql.*, java.util.*" %>
<html>
<head>
<title>Multiverse</title>
<style>
<!--
td {
	font: 10px verdana, arial narrow, arial, sans-serif;
	margin: 2px 0 10px 0;
	line-height: 1.25em;
	color: #383838;
}

td.white {
	font: 12px verdana, arial narrow, arial, sans-serif;
	margin: 2px 0 10px 0;
	line-height: 1.25em;	
	color: #FFFFFF;
}

a {
	font-family: verdana, arial narrow, arial, sans-serif;
	font-size: 12px;
	font-weight: bold;
	line-height: 1.2em;
	color: #4858FF;
	text-decoration: none;
}

td.head {
	font: 18px arial, sans-serif;
	margin: 2px 0 10px 0;
	line-height: 1.2em;
	color: #FFFFFF;
}

td.copy {
	vertical-align: top;
	font-family: verdana, sans-serif;
	font-size: 9px;
	color: #4C4C4C;
	font-weight: normal;
}	
-->
</style>

</head>

<body background="images/bkgd_sub.gif" bottommargin="0" leftmargin="0" marginheight="0" marginwidth="0" rightmargin="0" topmargin="0">

<!-- Outer table used to center content -->
<table width="100%" border="0" cellspacing="0" cellpadding="0" align="center">
<tr align="center" valign="top">
<td align="center" valign="top">		

<!-- Inner table used to format content -->
<table width="708" border="0" cellspacing="0" cellpadding="0">

<!-- Top row contains logo and spacers -->
<tr align="left" valign="top">
<td align="left" valign="top" rowspan="2"><img src="images/nologo_sub.gif" width="208" height="81" border="0"></td>
<td align="left" valign="top"><img src="images/clear.gif" width="9" height="40" border="0"></td>
<td align="left" valign="top"><img src="images/clear.gif" width="19" height="40" border="0"></td>
<td align="left" valign="top"><img src="images/clear.gif" width="453" height="40" border="0"></td>
<td align="left" valign="top"><img src="images/clear.gif" width="19" height="40" border="0"></td>
</tr>

<!-- Second row contains tab and content label -->		
<tr align="left" valign="top">		
<td align="left" valign="top"><img src="images/clear.gif" width="9" height="41" border="0"></td>
<td align="left" valign="top"><img src="images/left_crnr.gif" width="19" height="41" border="0"></td>
<td align="left" valign="middle" bgcolor="#EFEFEF"><font size="+1">

<!-- Page label goes here -->
User Sign In
</font></td>
<td align="left" valign="top"><img src="images/right_crnr.gif" width="19" height="41" border="0"></td>	
</tr>

<!-- Third row contains body text -->
<tr align="left" valign="top">
<td align="left" valign="top" background="images/stripes.gif">
<!--img src="images/clear.gif" width="208" border="0"-->

<!-- Table with labels and entry fields -->
<table>
<tr height="8"></tr>
<tr>
<td align="center" class="head">Sign In to the Multiverse Network</td>
</tr>

<tr height="8"></tr>
<tr>
<td class="white">Sign In to change your password and user profile for the Multiverse Network.</td>
</tr>

<tr height="48"></tr>
<tr>
<td align="left" class="white"><a href="reg1.html">Register</a> for a free Multiverse Network user name if you don't already have one.</td>
</tr>

<tr height="8"></tr>
</table>

</td>
<td align="left" valign="top"><img src="images/clear.gif" width="9" border="0"></td>			
<td align="left" valign="top" bgcolor="#EFEFEF"><img src="images/grey.gif" width="19" border="0"></td>
<td align="left" valign="top" bgcolor="#EFEFEF">

<%
    String connectString = getServletContext().getInitParameter("connect-string");
    String dbuser = getServletContext().getInitParameter("username");
    String dbpassword = getServletContext().getInitParameter("password");

    String processing = request.getParameter("processing");
    String username = request.getParameter("username");
    if (username == null) {
        username = "";
    }
    String password = request.getParameter("password");
    if (password == null) {
        password = "";
    }

    // Make sure database is operational
    // The newInstance() call is a work around for some 
    // broken Java implementations
    Class.forName("com.mysql.jdbc.Driver").newInstance();
    Connection con = DriverManager.getConnection(connectString, dbuser, dbpassword);
    if (con == null) {
        // transfer to JSP for database error
	pageContext.forward("/dberror.html");
        return;
    }

    // validate username
    String usernameHelp = "";
    String passwordHelp = "";
    int usernameLength = username.length();

    // See if all user data OK
    if (processing != null) {
        // Make sure we have User Name
        if (usernameLength == 0) {
            usernameHelp = "<br><font color=\"FF0000\">** (User Name missing.  It must contain between 3 & 16 letters or numbers and begin with a letter.)";
        }
        else {
            // See if user name exists
            Statement stmt = con.createStatement();
            String query = "SELECT username, password, email FROM user WHERE username = '" +
                    username +
                    "'";

            ResultSet rs = stmt.executeQuery(query);
            if(rs.next()){
                // User name does exist
                String dbUsername = rs.getString(1);
                String dbPassword = rs.getString(2);
                String dbEmail = rs.getString(3);

                if (dbPassword.equals(password)) {
                    // transfer to success JSP
                    pageContext.forward("/edit1.jsp");
                    return;
                }
                else {
                    passwordHelp = "<br><font color=\"FF0000\">** (Password incorrect.  Please enter correct password.)";
                }
            }
            else {
                usernameHelp = "<br><font color=\"FF0000\">** (User Name '" + username + "' does not exist.  Please enter valid User Name.)";
            }

            // Clean up database
            if(rs != null) rs.close();
            if(stmt != null) stmt.close();
        }
    }

    // Clean up database
    if(con != null) con.close();

    // Set instructions for form
    String formHelp = "";
    if( processing == null ) {
        formHelp = "Complete the form below to sign in to your account.";
    }
    else {
        formHelp = "Fix errors <font color=\"FF0000\">(highlighted in red)</font> and resubmit form.";
    }

%>

<!-- Body text here -->
<%= formHelp %>

<!-- Table with labels and entry fields -->
<table>
<tr height="8"></tr>

<form action="signin.jsp" method="post">
<input type="hidden" name="processing" value="yes">
<tr>
<td align="right" valign="top" width="200"><strong>User Name:</strong></td>
<td><input type="text" name="username" maxlength="15" size="15" value="<%= username %>">
<%= usernameHelp %></td>
</tr>
<tr height="8"></tr>

<tr>
<td align="right" valign="top"><strong>Password:</strong></td>
<td><input type="password" name="password" maxlength="15" size="15" value="<%= password %>">
<%= passwordHelp %></td>
</tr>
<tr height="8"></tr>

<tr>
<td></td><td><input type="submit" value="Submit"></td>
</tr>
<tr height="8"></tr>
</form>
</table>

<!-- Links here -->
<a href="tech.html">technology</a>  |  
<a href="team.html">team </a>  |  
<a href="work.html">cool place</a>	
</td>

<td align="left" valign="top" bgcolor="#EFEFEF"><img src="images/grey.gif" width="19" border="0"></td>
</tr>					
</table></td>

<!-- Spacer in next to bottom row of outer table -->
</tr>	
<tr align="center" valign="top" bgcolor="#060F0A">
<td align="center" valign="top"><img src="images/clear.gif" width="100%" height="60" border="0"></td>
</tr>

<!-- Copyright message in bottom row of outer table -->
<tr align="center" valign="top">
<td align="center" valign="top" class="copy"><br>&copy; Copyright Multiverse Network 2005. All rights reserved.</td>
</tr>						
</table>

</body>
</html>

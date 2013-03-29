<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html>
  <head>
    <title>Multiverse Login</title>
    <link rel="stylesheet" href="worldlogin.css">
    <script type="text/javascript" src="multiverse-devlogin.js"></script>
  </head>

<body bgcolor="#232323" marginheight="0" marginwidth="0" bottommargin="0" leftmargin="0" topmargin="0" onLoad="HandleLoad();">

<form>
<input type="hidden" id="mv_error_text" value="">
<input type="hidden" id="mv_status_text" value="">
</form>

<table width="800px" border="0" cellpadding=0 cellspacing=0 class="outertbl">
<tr>
  <td bgcolor="black" width="100%" colspan="2"><div class="devtextbig">
	  
      ERROR: World Name null Not Found!</div>

    
	</div></td>
</tr>
<tr>
  <td align="left" valign="middle" bgcolor="black" height="220">
  <a href="login.jsp"> 
  </td>

  
  <td rowspan="2">
  	 
       <div class="big_desc"> Error </div>
     
  </td>
</tr>

  <tr>
    <td id="login_table" valign="middle">
    <!-- inner table -->
    <table class="form-noindent" width="190px" cellpadding="3" cellspacing="3" align="center" border="0" id="inner_login_table">

    <form action="#" id="login_form" method="post" onSubmit="return false;">
    <tr>
      <td class="mvlabel">Username: <br>
      <input id="account" type="text" name="account" class="mvvalue"></td>
      
    </tr>
    
    <tr>
      <td class="mvlabel">Password: <br>
      <input id="password" type="password" name="password" class="mvvalue">

      <input type="hidden" id="world_id" value="null">
      </td>
    </tr>
    
    <tr>
      <td><input type="submit" id="login_button" value="Log In" class="mvbutton" onClick="Login()"></td>
    </tr>

  </form>
  </table>

  <!-- end inner table -->
  </td>
  </tr>
</table>
  <table bgcolor=#ffcc33 cellpadding="2" cellspacing="0" align="center" border="0" id="status_error_table" width=340px>
     <tr id="status_section">
        <td class="mvlabel" width=50px> Status: </td>

        <td id="status" class="statusmsg">status message </td>
      </tr>

       <tr id="error_section">
        <td class="mvlabel">Error: </td>
        <td id="error" class="errormsg">err message</td>
      </tr> 
    </table>
    
  </body>
</html>

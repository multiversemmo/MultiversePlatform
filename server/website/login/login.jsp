<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">

<html>
  <head>
    <title>Multiverse Login</title>
    <!--<link rel="stylesheet" href="login.css">-->
    <script type="text/javascript" src="multiverse-devlogin.js"></script>
    <script type="text/javascript" src="tooltip.js"></script>

    <script>
function setUnavailableWorlds() {
  //setUA("unavailable_world_example", 340, 300)
}

function setUA(id, left, top) {
  obj = document.getElementById(id );
  obj.style.left = left;
  obj.style.top = top;
  removeOption( id );
  obj.visibility = "visible";
}

//function removeOption( optionValue ){
//  var x=document.getElementById("world_id");
//  for (i=0; i<x.length; i++) {
//    if ( x.options[i].value == optionValue ) {
//       x.remove( i )
//    }
//  }
//}
</script> 
</head>

<body marginheight="0" marginwidth="0" bottommargin="0" leftmargin="0" topmargin="0" onLoad="setUnavailableWorlds(); ">

<form>
<input type="hidden" id="mv_error_text" value="">
<input type="hidden" id="mv_status_text" value="">

</form>

<table width="800px" border="0" cellpadding=0 cellspacing=0 class="outertbl">

<tr>
<!-- Begin Cell for Login Table and News -->
<td id="login_table" >

<!-- inner login table -->
<form action="#" id="login_form" method="post" onSubmit="return false;">
  <table class="form-noindent"  cellpadding="3" cellspacing="3" align="center" border=0 id="inner_login_table">
    <tr>

      <td width="100">  <span class="mvlabel">Username:</span>  </td>
      <td> <input id="account" type="text" name="account" class="mvvalue" value="guest"> </td>        
    </tr>
    
    <tr>
      <td width="100"> <span class="mvlabel">Password:</span> </td>

      <td> <input id="password" type="password" name="password" class="mvvalue" value="guest"> </td>
    </tr>
    
    <tr id="world_section">
      <td>  <span class="mvlabel">World: </span> </td>
      
      <td>  
      <select id="world_id" class="mvvalue">

      <option value="nyts">New York Times Square</option>

      </select>     
      </td>
    </tr>
    
    <tr>
      <td colspan="2" align="center">

      <input type="button" id="login_button" value="Log In" class="mvbutton" onClick="Login()">      </td>
    </tr>
  </table>
  </form>
  <!-- end inner login table -->

</td>
</tr>
</table>

</body>

</html>

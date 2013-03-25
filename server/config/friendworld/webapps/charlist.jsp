<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">

<%@ page import="java.util.*" %>

<%@ include file="execscript.jsp" %>

<%
    Integer token = (Integer)session.getAttribute("token");
    if (token == null) {
        // transfer to JSP for user login
        session.setAttribute("loginTargetPage", "charlist.jsp");
        response.sendRedirect("login.jsp");
        return;
    }

    String script = String.format(ServerScript.listChars, token);
    String result = ServerScript.execScript(script);
%>

<script language="javascript">
function chardelete(oid) {
  document.delform.oid.value = oid
  document.delform.submit()
}
</script>

<html>
  <head>
    <title>List Characters</title>
  </head>

  <body>
    <h1>List Characters</h1>

    <table border=1>
    <tr><td><b>Name</b></td><td><b>Oid</b></td></tr>

<%
    for (String line : result.split("\n")) {
        if (!line.equals("")) {
            String[] charData = line.trim().split(" ");
            out.print("<tr><td>" + charData[0] + "</td><td>" + charData[1] +
                      "</td><td><input type='button' value='delete' onClick='chardelete(" +
                      charData[1] + ")'></td></tr>");
        }
    }
%>

    </table>

    <form name="delform" action="chardel.jsp" method="post">
    <input type="hidden" name="oid">
    </form>

    <a href="charcreate.jsp">Create Character</a>
    <br>
    <a href="logout.jsp">Logout</a>
  </body>
</html>

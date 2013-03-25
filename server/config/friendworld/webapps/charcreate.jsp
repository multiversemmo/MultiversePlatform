<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">

<%@ include file="execscript.jsp" %>

<%
    String processing = request.getParameter("loginRefer");
    Integer token = (Integer)session.getAttribute("token");
    if (token == null) {
        // transfer to JSP for user login
        session.setAttribute("loginTargetPage", "charlist.jsp");
        response.sendRedirect("login.jsp");
        return;
    } else {
        %> token = <%= token %> <%
    }

    if (processing != null) {
      String name = ServerScript.validateArg(request.getParameter("character"));
      String sex = ServerScript.validateArg(request.getParameter("sex"));
      String script = String.format(ServerScript.createChar, token, name, sex);
      String result = ServerScript.execScript(script);
      response.sendRedirect("charlist.jsp");
    }
%>


<html>
  <head>
    <title>Character Creation</title>
  </head>

  <body>
    <h1>Character Creation</h1>

<form action="charcreate.jsp" method="post">
  <input type="hidden" name="loginRefer" value="yes">
  <table>
    <tr><td>Character Name <input type="text" name="character"></td></tr>
    <tr><td><table>
      <tr><td><input type="radio" name="sex" value="male" checked> Male</td>
          <td><input type="radio" name="sex" value="female"> Female</td></tr>
    </table></td></tr>
    <tr><td><input type="submit" value="Create Character"></td></tr>
  </table>
</form>

<a href="logout.jsp">Logout</a>
  </body>
</html>

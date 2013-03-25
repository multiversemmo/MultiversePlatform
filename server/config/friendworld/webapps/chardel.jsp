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

    String oid = ServerScript.validateArg(request.getParameter("oid"));
    String script = String.format(ServerScript.delChar, token, request.getParameter("oid"));
    String result = ServerScript.execScript(script);
    response.sendRedirect("charlist.jsp");
%>

<%= result %>

<html>
  <head>
    <title>Delete Character</title>
  </head>

  <body>
    <h1>Delete Character</h1>



    <hr>
  </body>
</html>

<html>
<head><title> Parameter Test </title></head>
<body>

<h2>Parameter Test</h2>

<%
    String connectString = getServletContext().getInitParameter("connect-string");
%>

connect-string = <%= connectString %>

</body>
</html>

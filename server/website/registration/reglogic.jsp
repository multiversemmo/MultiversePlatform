<html><head><title>Apache Tomcat/5.0.28 - Error report</title><style><!--H1 {font-family:Tahoma,Arial,sans-serif;color:white;background-color:#525D76;font-size:22px;} H2 {font-family:Tahoma,Arial,sans-serif;color:white;background-color:#525D76;font-size:16px;} H3 {font-family:Tahoma,Arial,sans-serif;color:white;background-color:#525D76;font-size:14px;} BODY {font-family:Tahoma,Arial,sans-serif;color:black;background-color:white;} B {font-family:Tahoma,Arial,sans-serif;color:white;background-color:#525D76;} P {font-family:Tahoma,Arial,sans-serif;background:white;color:black;font-size:12px;}A {color : black;}A.name {color : black;}HR {color : #525D76;}--></style> </head><body><h1>HTTP Status 500 - </h1><HR size="1" noshade="noshade"><p><b>type</b> Exception report</p><p><b>message</b> <u></u></p><p><b>description</b> <u>The server encountered an internal error () that prevented it from fulfilling this request.</u></p><p><b>exception</b> <pre>org.apache.jasper.JasperException: Unable to compile class for JSP

Generated servlet error:
/usr/share/tomcat/work/Catalina/www.multiverse.net/_/org/apache/jsp/consumer/reglogic_jsp.java:454: 'catch' without 'try'
    } catch (Throwable t) {
      ^


Generated servlet error:
/usr/share/tomcat/work/Catalina/www.multiverse.net/_/org/apache/jsp/consumer/reglogic_jsp.java:454: ')' expected
    } catch (Throwable t) {
                      ^


Generated servlet error:
/usr/share/tomcat/work/Catalina/www.multiverse.net/_/org/apache/jsp/consumer/reglogic_jsp.java:454: not a statement
    } catch (Throwable t) {
            ^


Generated servlet error:
/usr/share/tomcat/work/Catalina/www.multiverse.net/_/org/apache/jsp/consumer/reglogic_jsp.java:454: ';' expected
    } catch (Throwable t) {
                        ^


Generated servlet error:
/usr/share/tomcat/work/Catalina/www.multiverse.net/_/org/apache/jsp/consumer/reglogic_jsp.java:461: 'finally' without 'try'
    } finally {
      ^


Generated servlet error:
/usr/share/tomcat/work/Catalina/www.multiverse.net/_/org/apache/jsp/consumer/reglogic_jsp.java:34: 'try' without 'catch' or 'finally'
    try {
    ^


Generated servlet error:
/usr/share/tomcat/work/Catalina/www.multiverse.net/_/org/apache/jsp/consumer/reglogic_jsp.java:465: reached end of file while parsing
}
 ^
7 errors



	org.apache.jasper.compiler.DefaultErrorHandler.javacError(DefaultErrorHandler.java:84)
	org.apache.jasper.compiler.ErrorDispatcher.javacError(ErrorDispatcher.java:332)
	org.apache.jasper.compiler.Compiler.generateClass(Compiler.java:412)
	org.apache.jasper.compiler.Compiler.compile(Compiler.java:472)
	org.apache.jasper.compiler.Compiler.compile(Compiler.java:451)
	org.apache.jasper.compiler.Compiler.compile(Compiler.java:439)
	org.apache.jasper.JspCompilationContext.compile(JspCompilationContext.java:511)
	org.apache.jasper.servlet.JspServletWrapper.service(JspServletWrapper.java:295)
	org.apache.jasper.servlet.JspServlet.serviceJspFile(JspServlet.java:292)
	org.apache.jasper.servlet.JspServlet.service(JspServlet.java:236)
	javax.servlet.http.HttpServlet.service(HttpServlet.java:802)
</pre></p><p><b>note</b> <u>The full stack trace of the root cause is available in the Apache Tomcat/5.0.28 logs.</u></p><HR size="1" noshade="noshade"><h3>Apache Tomcat/5.0.28</h3></body></html>
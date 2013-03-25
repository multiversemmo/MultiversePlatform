__doc__ = """

(C) 2006 Mark Rees mark.john.rees at gmail dot com

A wsgi gateway for ASP.NET and IronPython

Dependecies:
    - IronPython 1.0.+
    - wsgiref library http://cheeseshop.python.org/pypi/wsgiref

"""
__description__ = "IronPython ASP.NET WSGI Gateway"
__license__ = "MIT"

from wsgiref.handlers import BaseHandler

traceon = False
def trace(*msgs):
    """Write trace message(s)
    This may only work under xsp2 as there may be issues
    writing to the stdout with IIS running as a service
    """
    if not traceon: return
    for msg in msgs:
        print msg

class OutputWrapper:
    def __init__(self, response):
        trace("OutputWrapper.__init__")
        self.response = response
        self.response.BufferOutput = False;
    def write(self, msg):
        trace("OutputWrapper.write")
        trace(msg)
        self.response.Write(msg)
    def flush(self):
        trace("OutputWrapper.flush")
        pass

class ErrorWrapper:
    def write(self, msg):
        trace("ErrorWrapper.write")
        trace(msg)
        pass
    def flush(self):
        trace("ErrorWrapper.flush")
        pass

class FePyWSGIHandler(BaseHandler):
    """IronPythonPython WSGI Handler"""
    def __init__(self, context, application, rootpath):
        trace("__init__")
        self.context = context
        self.application = application
        self.rootpath = rootpath
        self.request = context.Request
        self.response = context.Response
        self.stdin = file(self.request.InputStream)
        self.stdout = OutputWrapper(self.response)
        self.stderr = ErrorWrapper()
        self.base_env = []
        self.wsgi_multithread = True
        self.wsgi_multiprocess = False
        self.headers = None
        self.headers_sent = False
        self.run(application)

    def send_preamble(self):
        """Since ASP.NET sends preamble itself, do nothing"""
        trace("send_preamble")
        pass
    
    def send_headers(self):
        """Transmit headers"""
        trace("send_headers")
        self.cleanup_headers()
        self.headers_sent = True
        if not self.origin_server or self.client_is_modern():
            self.response.Status = self.status
            for header_name, header_value in self.headers.items():
                self.response.AddHeader(header_name, header_value)
            
    def _write(self, data):
        trace("_write")
        trace(data)
        self.stdout.write(data)

    def _flush(self):
        trace("_flush")
        pass

    def get_stdin(self):
        trace("get_stdin")
        return self.stdin

    def get_stderr(self):
        trace("get_stderr")
        return self.stderr

    def add_cgi_vars(self):
        trace("add_cgi_vars")
        environ = {}
        environ['wsgi.url_scheme'] = self.request.Url.Scheme
        # set standard CGI variables
        required_cgienv_vars = ['REQUEST_METHOD', 'SCRIPT_NAME',
                                'PATH_INFO', 'QUERY_STRING',
                                'CONTENT_TYPE', 'CONTENT_LENGTH',
                                'SERVER_NAME', 'SERVER_PORT',
                                'SERVER_PROTOCOL'
                                ]
        for cgivar in required_cgienv_vars:
            try:
                environ[cgivar] = self.request.ServerVariables[cgivar]
            except:
                raise AssertionError("missing CGI environment variable %s" % cgivar)

        http_cgienv_vars = self.request.ServerVariables['ALL_HTTP']
        for cgivar in http_cgienv_vars.split("\n"):
            pair = cgivar.split(":",1)
            try:
                environ[pair[0]] = pair[1]
            except:
                # Handle last list which is not a pair
                pass
        # Other useful CGI variables
        try:
            environ['REMOTE_USER'] = self.request.ServerVariables('REMOTE_USER')
        except:
            pass

        environ['SCRIPT_NAME'] = self.rootpath
        environ['PATH_INFO'] = self.request.Path[self.request.Path.index(self.rootpath) + len(self.rootpath):]
        environ['SERVER_SOFTWARE'] = self.request.ServerVariables['SERVER_SOFTWARE'];
        self.environ.update(environ)

def run_application(context, application, rootpath):
    FePyWSGIHandler(context, application, rootpath)

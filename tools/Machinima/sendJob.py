#
#
#  The Multiverse Platform is made available under the MIT License.
#
#  Copyright (c) 2012 The Multiverse Foundation
#
#  Permission is hereby granted, free of charge, to any person 
#  obtaining a copy of this software and associated documentation 
#  files (the "Software"), to deal in the Software without restriction, 
#  including without limitation the rights to use, copy, modify, 
#  merge, publish, distribute, sublicense, and/or sell copies 
#  of the Software, and to permit persons to whom the Software 
#  is furnished to do so, subject to the following conditions:
#
#  The above copyright notice and this permission notice shall be 
#  included in all copies or substantial portions of the Software.
#
#  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
#  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
#  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
#  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
#  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
#  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
#  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
#  OR OTHER DEALINGS IN THE SOFTWARE.
#
#  

#!/usr/bin/python

import socket
import sys

addOrderStart = "<add_order job_chain=\"renderscenechain\"><xml_payload>"
addOrderEnd = "</xml_payload></add_order>"

hostname = "64.13.140.178"
port = 4446

filename = sys.argv[1]
f = file(filename)
command = addOrderStart + f.read() + addOrderEnd

#create an INET, STREAMing socket
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((hostname, port))
s.send(command)

reply = ""
while reply.find('</spooler>') == -1:
	reply = reply + s.recv(4096)
	
s.close()

print reply

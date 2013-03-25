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
import os
import xml.dom
import xml.dom.minidom
import httplib
import time

def parse_spooler(spooler_node):
    answer_node = spooler_node.getElementsByTagName('answer')[0]
    ok_node = answer_node.getElementsByTagName('ok')[0]
    task_node = ok_node.getElementsByTagName('task')[0]
    task_id = task_node.getAttribute('id')
    return task_id

def sendJob(filename):
    """
    Send a job to the render system, and parse the job number from the reply
    @param filename: the name of the xml file with the request data
    @return: the job id from the render machine or None if there was an error
    @rtype: string
    """
    addOrderStart = "<add_order job_chain=\"renderscenechain\"><xml_payload>"
    addOrderEnd = "</xml_payload></add_order>"
    
    hostname = "render1"
    port = 4446

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

    # Right now, the reply contains a null
    # do some hackery to remove it
    null_index = reply.find('\0')
    if null_index != -1:
        reply = reply[0:null_index]
    print reply
    try:
        replyDom = xml.dom.minidom.parseString(reply)
        spooler_node = replyDom.getElementsByTagName('spooler')[0]
        answer_node = spooler_node.getElementsByTagName('answer')[0]
        ok_node = answer_node.getElementsByTagName('ok')[0]
        task_node = ok_node.getElementsByTagName('order')[0]
        task_id = task_node.getAttribute('id')
        return task_id
    except:
        print 'Unable to parse reply:'
        print reply
        return None

def sendRequests(folder, output_folder):
    result_hostname = 'facebook.multiverse.net'
    result_port = 8087
    result_folder = 'machinima'
    files = os.listdir(folder)
    tasks = {}
    for filename in files:
        if filename.endswith('.xml'):
            task_id = sendJob(os.path.join(folder, filename))
            if task_id is not None:
                tasks[task_id] = filename[0:-4] # strip off the .xml
                print 'Render job %s submitted with task id %s' % (filename, task_id)
    # TODO: Automatically check the status of the postcards, and when they are ready,
    # pull them locally
    # sleep for 30 seconds,  plus 20 seconds per postcard
    sleep_time = 30 + 20 * len(tasks)
    print 'Sleeping for %d seconds' % sleep_time
    time.sleep(sleep_time)
    conn = httplib.HTTPConnection(result_hostname, result_port)
    conn.connect()
    for key, value in tasks.items():
        conn.request('GET', '/%s/%s.png' % (result_folder, key))
        response = conn.getresponse()
        if response.status == 200:
            output_file = os.path.join(output_folder, '%s.png' % value)
            imgData = response.read()
            out = open(output_file, 'w')
            out.write(imgData)
            out.close()
            print 'Wrote image: %s' % output_file
        else:
            print 'Status = %d' % response.status
            print response.reason
    conn.close()

source_folder = ''
dest_folder = ''
if len(sys.argv) >= 2:
    source_folder = sys.argv[1]
    # default to setting dest folder to source folder
    dest_folder = sys.argv[1]
if len(sys.argv) >= 3:
    dest_folder = sys.argv[2]

# To generate sample poses:
#   sendXmlJobs.py sample_poses
# To generate sample postcards:
#   sendXmlJobs.py sample_postcards
sendRequests(source_folder, dest_folder)


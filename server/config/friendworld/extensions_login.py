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

import sys

from java.util import *
from java.lang import *
from java.net import *
from java.sql import *
from multiverse.mars import *
from multiverse.mars.core import *
from multiverse.mars.objects import *
from multiverse.mars.util import *
from multiverse.mars.plugins import *
from multiverse.server.math import *
from multiverse.server.plugins import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *
from multiverse.server.worldmgr import *


LoginPlugin.SecureToken = 1


# host running web database
webdb_host = "webdb.mv-places.com"
# for testing
#webdb_host = "localhost"


def getDomainHost():
    hostName = Engine.getMessageServerHostname()
    if hostName == 'localhost':
        try:
            localMachine = InetAddress.getLocalHost()
            hostName = localMachine.getHostName()
        except UnknownHostException:
            Log.error("getDomainHost: couldn't get host name from local IP address %s" % str(localMachine))
    Log.debug("getDomainHost: hostname = %s" % hostName)
    return hostName


def setDomainKey():
    # get the domain key and host (resolving 'localhost' if necessary)
    domainKey = SecureTokenManager.getInstance().getEncodedDomainKey()
    encodedKey = Base64.encodeBytes(domainKey)
    hostName = getDomainHost()

    # insert hostname and domainkey into web server database
    url = "jdbc:mysql://%s/friendworld?user=root&password=test" % webdb_host
    sql = "INSERT INTO security VALUES ('%s', '%s') ON DUPLICATE KEY UPDATE domainkey = '%s'" % (hostName, encodedKey, encodedKey)
    try:
        con = DriverManager.getConnection(url)
        stm = con.createStatement()
        res = stm.executeUpdate(sql)
        stm.close()
        con.close()
    except:
        errMsg = "Critical error in setDomainKey: %s" % sys.exec_info()[0]
        Log.error(errMsg)
        print(errMsg)
        System.exit(1)


setDomainKey()

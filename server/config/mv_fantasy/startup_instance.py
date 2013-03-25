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

from multiverse.server.plugins import *
from multiverse.server.objects import *
from multiverse.server.engine import *


template = Template("mv_fantasy template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/mv_fantasy.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load.py")

rc = InstanceClient.registerInstanceTemplate(template);

overrideTemplate = Template("mv_fantasy");
overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "default")

rc = InstanceClient.createInstance("mv_fantasy template", overrideTemplate);
Log.debug("startup_instance.py: createInstance result=" + str(rc))

Engine.getPlugin("Instance").setPluginAvailable(True)


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

import java.lang.Thread
import java.util.LinkedList
from multiverse.server.plugins import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *
from multiverse.server.engine import WorldLoaderOverride

class myWorldLoaderOverride(WorldLoaderOverride):
    def __init__(self):
        pass
    def adjustLightData(self, worldCollectionName, objectName, lightData):
        return True
    def adjustObjectTemplate(self, worldCollectionName, objectName, template):
        instanceOid = template.get(Namespace.WORLD_MANAGER, ":instance")
        roomStyle = None
        try:
            roomStyle = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "RoomStyle")
        except:
            pass
        if roomStyle is not None:
            template.put(Namespace.WORLD_MANAGER, "RoomStyle", roomStyle)
        accountId = None
        try:
            accountId = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "AccountId")
        except:
            pass
        if accountId is not None:
            template.put(Namespace.WORLD_MANAGER, "AccountId", accountId)
        props = None
        try:
            props = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "RoomItemsProps")
        except:
            pass
        if props is not None and props.containsKey(objectName):
            objProps = props[objectName]
            for key in objProps.keySet():
                value = objProps[key]
                template.put(Namespace.WORLD_MANAGER, key, value)
        return True
    def adjustRegion(self, worldCollectionName, objectName, region):
        return True
    def adjustRegionConfig(self, worldCollectionName, objectName, region, regionConfig):
        return True
    def adjustSpawnData(self, worldCollectionName, objectName, spawnData):
        return True

InstancePlugin.registerWorldLoaderOverrideClass("placesWorldLoaderOverride", myWorldLoaderOverride)


template = Template("friendworld template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/friendworld.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load_default.py")

rc = InstanceClient.registerInstanceTemplate(template);

overrideTemplate = Template()

overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "default") # instance name here
overrideTemplate.put(Namespace.INSTANCE, "populationLimit", Integer(100)) # instance name here

rc = InstanceClient.createInstance("friendworld template", overrideTemplate); # template name here
Log.info("startup_instance.py: createInstance 'default' #1 result=" + str(rc))
rc = InstanceClient.createInstance("friendworld template", overrideTemplate); # template name here
Log.info("startup_instance.py: createInstance 'default' #2 result=" + str(rc))

rc = InstanceClient.createInstance("friendworld template", overrideTemplate); # template name here
Log.info("startup_instance.py: createInstance 'default' #3 result=" + str(rc))
rc = InstanceClient.createInstance("friendworld template", overrideTemplate); # template name here
Log.info("startup_instance.py: createInstance 'default' #4 result=" + str(rc))

##########

template = Template("titanic template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/../titanic_world/titanic_world.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load_default.py")

rc = InstanceClient.registerInstanceTemplate(template);

overrideTemplate = Template()

overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "titanic") # instance name here
rc = InstanceClient.createInstance("titanic template", overrideTemplate); # template name here
Log.info("startup_instance.py: createInstance 'titanic' result=" + str(rc))

##########

template = Template("hip hop environment template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/hiphopEnvironment.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load_hiphopEnvironment.py")

rc = InstanceClient.registerInstanceTemplate(template)

overrideTemplate = Template()

overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "hiphopEnvironment") # instance name here
rc = InstanceClient.createInstance("hip hop environment template", overrideTemplate); # template name here
Log.info("startup_instance.py: createInstance 'hiphopEnvironment' result=" + str(rc))

##########

template = Template("hip hop room template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/hiphoproom.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load_room_hiphop.py")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_LOADER_OVERRIDE_NAME, "placesWorldLoaderOverride")

rc = InstanceClient.registerInstanceTemplate(template)

overrideTemplate = Template()

overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "hiphop") # instance name here

rc = InstanceClient.createInstance("hip hop room template", overrideTemplate) # template name here
Log.info("startup_instance.py: createInstance 'hiphoproom' result=" + str(rc))

##########

template = Template("hip hop room unfurnished template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/hiphoproom.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load_room_hiphop_unfurnished.py")

rc = InstanceClient.registerInstanceTemplate(template)

overrideTemplate = Template()

overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "hiphop_unfurnished") # instance name here

rc = InstanceClient.createInstance("hip hop room unfurnished template", overrideTemplate) # template name here
Log.info("startup_instance.py: createInstance 'hiphoproom_unfurnished' result=" + str(rc))

##########

template = Template("cute room template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/cuteroom.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load_room_cute.py")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_LOADER_OVERRIDE_NAME, "placesWorldLoaderOverride")

rc = InstanceClient.registerInstanceTemplate(template)

overrideTemplate = Template()

overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "cute") # instance name here
rc = InstanceClient.createInstance("cute room template", overrideTemplate) # template name here
Log.info("startup_instance.py: createInstance 'cuteroom' result=" + str(rc))

##########

template = Template("goth room template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/gothroom.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load_room_goth.py")

rc = InstanceClient.registerInstanceTemplate(template)

overrideTemplate = Template()

overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "goth") # instance name here
rc = InstanceClient.createInstance("goth room template", overrideTemplate) # template name here
Log.info("startup_instance.py: createInstance 'gothroom' result=" + str(rc))

##########

template = Template("metal room template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/metalroom.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load_room_metal.py")

rc = InstanceClient.registerInstanceTemplate(template)

overrideTemplate = Template()

overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "metal") # instance name here
rc = InstanceClient.createInstance("metal room template", overrideTemplate) # template name here
Log.info("startup_instance.py: createInstance 'metalroom' result=" + str(rc))

##########

template = Template("sports room template")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, "$WORLD_DIR/sportsroom.mvw")
template.put(Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, "$WORLD_DIR/instance_load_room_sports.py")

rc = InstanceClient.registerInstanceTemplate(template)

overrideTemplate = Template()

overrideTemplate.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, "sports") # instance name here
rc = InstanceClient.createInstance("sports room template", overrideTemplate) # template name here
Log.info("startup_instance.py: createInstance 'sportsroom' result=" + str(rc))

##########


class RoomInstanceTimeout(InstanceTimeout):
    def readyForTimeout(self,instance):
        return instance.getPlayerPopulation() == 0 and instance.getName().find("room-") == 0

instanceTimeout = RoomInstanceTimeout(60)
instanceTimeout.start()

class PopulationClass(InstancePlugin.PopulationChangeCallback):
    def __init__(self):
        self.popLock = LockFactory.makeLock("PopulationClass")
        self.recentPopulationNumbers = {}
        self.allPopulationNumbers = {}
        # Clear the table if it exists
        if (Engine.getDatabase().databaseContainsTable(Engine.getDBName(), "populations")):
            Engine.getDatabase().executeUpdate("DELETE FROM populations")
        else:
            # Now recreate it
            Engine.getDatabase().executeUpdate("CREATE TABLE populations (instance_id BIGINT, account_id INT, population INT, INDEX USING HASH (instance_id)) ENGINE = MEMORY")

    def onInstancePopulationChange(self, instanceOid, instanceName, population):
        #Log.info("PopulationClass.onInstancePopulationChange called: instance " + str(instanceOid) + ", name " + instanceName + ", pop " + str(population))
        #self.recentPopulationNumbers[instanceOid] = (population, instanceOid)
        #return
        try:
            if instanceName.find("room-") != 0:
                raise
            accountIdStr = instanceName[5:]
            accountId = Integer.valueOf(accountIdStr)
        except:
            Log.warn("PopulationClass.onInstancePopulationChange: For instanceOid " + str(instanceOid) + ", instanceName " + instanceName + " is not of the form 'room-nnnn'")
            return
        #Log.debug("PopulationClass.onInstancePopulationChange setting recent: instance " + str(instanceOid) + ", name " + instanceName + ", pop " + str(population))
        try:
            self.popLock.lock()
            self.recentPopulationNumbers[accountId] = (population, instanceOid)
        finally:
            self.popLock.unlock()

    def writePopulationToDatabase(self):
        # Uncommenting these lines allows testing when there is no web server.
        #Log.debug("PopulationClass.writePopulationToDatabase entered")
        statements = LinkedList()
        # Acquire the lock to copy the recentPopulationNumbers dictionary,
        # and then release it.
        try:
            self.popLock.lock()
            mostRecentPopulationNumbers = self.recentPopulationNumbers
            self.recentPopulationNumbers = {}
        finally:
            self.popLock.unlock()
        #Log.debug("PopulationClass.writePopulationToDatabase: " + str(len(mostRecentPopulationNumbers)) + " elements")
        # Iterate over the recent population changes elements.
        # If the instanceOid already exists in allPopulationNumbers and
        # the population is zero, remove the row and remove the element
        # of allPopulationNumbers; otherwise, create the update statement.
        # If it's not in allPopulationNumbers, create the insert statement.
        for accountId, (population, instanceOid) in mostRecentPopulationNumbers.items():
            if accountId in self.allPopulationNumbers:
                if (population == 0):
                    statements.add("DELETE FROM populations WHERE account_id = " + str(accountId) + ";")
                    del self.allPopulationNumbers[accountId]
                else:
                    statements.add("UPDATE populations SET population = " + str(population) + " WHERE instance_id = " + str(instanceOid) + ";")
            else:
                statements.add("INSERT INTO populations (account_id, instance_id, population) VALUES (" + str(accountId) + "," +
                     str(instanceOid) + "," + str(population) + ");")
                self.allPopulationNumbers[accountId] = (population, instanceOid)

        # If there is nothing to do, return
        if statements.size() == 0:
            return
        else:
            Engine.getDatabase().executeBatch(statements)
            if (Log.loggingDebug):
                batch = ""
                for i in range(statements.size() - 1):
                    batch += "\n" + statements.get(i)
                Log.debug("PopulationClass.writePopulationFields: ran SQL statements " + batch)

class PopulationRunnable(Thread):
    def __init__(self, intervalArg, populationClassArg):
        self.interval = intervalArg
        self.populationClass = populationClassArg

    def run(self):
        while True:
            Thread.sleep(self.interval)
            self.populationClass.writePopulationToDatabase()

populationClass = PopulationClass()
Engine.getPlugin("Instance").registerPopulationChangeCallback(populationClass)
# Start the thread with a write-out interval of 10 seconds
populationRunnable = PopulationRunnable(10 * 1000, populationClass)
populationRunnable.start()

Engine.getPlugin("Instance").setPluginAvailable(True)

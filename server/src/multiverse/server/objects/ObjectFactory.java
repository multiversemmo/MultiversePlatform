/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

package multiverse.server.objects;

import java.util.Map;
import java.util.HashMap;

import multiverse.server.math.*;
import multiverse.server.plugins.*;

/** Create objects for spawn generators.  Sub-class to customize
spawned objects.  Sub-classes should override
{@link #makeObject(multiverse.server.objects.SpawnData,long,multiverse.server.math.Point)}.
*/
public class ObjectFactory {

    /**
     * No-arg constructor used by WEObjFactory.
     */
    public ObjectFactory() {
    }

    /** Create objects using the named template.
    */
    public ObjectFactory(String template) {
	templateName = template;
    }

    /*
     * The overloadings of makeObject used when spawning an object not
     * defined in the world editor.
     */

    /** Create object using MobManagerPlugin.createObject().
        @deprecated
    */
    public ObjectStub makeObject(long instanceOid, Point loc) {
 	ObjectStub obj = MobManagerPlugin.createObject(templateName, instanceOid, loc, null);
	return obj;
    }

    /** Create object using MobManagerPlugin.createObject().
        @deprecated
    */
    public ObjectStub makeObject(long instanceOid, Template override) {
 	ObjectStub obj = MobManagerPlugin.createObject(templateName, override, instanceOid);
	return obj;
    }

    /** Create object at the given location.  The SpawnData
        template name overrides this ObjectFactory template name.
        Objects are created with {@link multiverse.server.plugins.MobManagerPlugin#createObject(String,long,multiverse.server.math.Point,multiverse.server.math.Quaternion)
        MobManagerPlugin.createObject()}.
        <p>
        Sub-classes should override this method to customize spawned objects.
    */
    public ObjectStub makeObject(SpawnData spawnData, long instanceOid, Point loc) {
        String tName = spawnData.getTemplateName();
        if (tName == null)
            tName = templateName;
        Quaternion orient = spawnData.getOrientation();
        ObjectStub obj = MobManagerPlugin.createObject(tName, instanceOid, loc,
            orient);
        return obj;
    }

    /** Create object at the given location.  The SpawnData
        template name overrides this ObjectFactory template name.
        The override template overrides the named template.
        Objects are created with {@link multiverse.server.plugins.MobManagerPlugin#createObject(java.lang.String,multiverse.server.objects.Template,java.lang.Long)
        MobManagerPlugin.createObject()}.
    */
    public ObjectStub makeObject(SpawnData spawnData, long instanceOid, Template override) {
        String tName = spawnData.getTemplateName();
        if (tName == null)
            tName = templateName;
        ObjectStub obj = MobManagerPlugin.createObject(tName, override, instanceOid);
        return obj;
    } 

    /** Get template for creating objects.
    */
    public String getTemplateName() {
        return templateName;
    }

    /** Set template for creating objects.
    */
    public void setTemplateName(String templateName) {
        this.templateName = templateName;
    }

    /** Register an object factory.  Object factories must be registered
        for remote spawn generator creation.
        @see multiverse.server.plugins.MobManagerClient#createSpawnGenerator 
    */
    public static void register(String factoryName, ObjectFactory factory)
    {
        factories.put(factoryName,factory);
    }

    /** Get registered object factory.
    */
    public static ObjectFactory getFactory(String factoryName)
    {
        return factories.get(factoryName);
    }

    protected String templateName = null;

    private static Map<String,ObjectFactory> factories =
        new HashMap<String,ObjectFactory>();

}

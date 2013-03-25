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

package multiverse.server.engine;

import multiverse.server.objects.LightData;
import multiverse.server.objects.Template;
import multiverse.server.objects.Region;
import multiverse.server.objects.RegionConfig;
import multiverse.server.objects.SpawnData;


/** Override objects created during world file loading.  Implement this
interface to modify or override objects created from world files and
world collections.  Each method is called prior to creating an object.
If the method returns false, the object is not created.  The method
may modify the object's initialization data (via Template, LightData,
SpawnData, etc).
<p>
Register world loader override classes with
{@link multiverse.server.plugins.InstancePlugin#registerWorldLoaderOverrideClass(java.lang.String,java.lang.Class) InstancePlugin.registerWorldLoaderOverrideClass(String,Class)}.
Set the instance's loader override by setting property
{@link multiverse.server.plugins.InstanceClient#TEMPL_LOADER_OVERRIDE_NAME InstanceClient.TEMPL_LOADER_OVERRIDE_NAME} to the loader override's registered name.
A new instance of the class will be created prior to loading the instance's
world file.
*/
public interface WorldLoaderOverride
{
    /** Modify or override point lights.
        @param worldCollectionName World collection file name.
        @param objectName Point light name.
        @param lightData Light data.
        @return True to create the object, false to skip it.
    */
    public boolean adjustLightData(String worldCollectionName,
        String objectName, LightData lightData);

    /** Modify or override object templates.  This method is called
        for static objects, marker-based particle effects, and marker-based
        sounds.
        <p>
        Particle effect names are constructed as "&lt;marker-name&gt;-&lt;particle-effect-name&gt;".
        Sound names are constructed as ""&lt;marker-name&gt;-&lt;sound-file-name&gt;".
        @param worldCollectionName World collection file name.
        @param objectName Static object name, particle effect name,
                or sound name.
        @param template Object template.
        @return True to create the object, false to skip it.
    */
    public boolean adjustObjectTemplate(String worldCollectionName,
        String objectName, Template template);

    /** Modify or override regions.  This method is called with region
        properties, but before adding RegionConfig.
        @param worldCollectionName World collection file name.
        @param objectName Region name.
        @param region
        @return True to create the object, false to skip it.
    */
    public boolean adjustRegion(String worldCollectionName,
        String objectName, Region region);

    /** Modify or override region configuration.  This method is called
        for each RegionConfig on a Region.  The following region features
        are included: sound, fog, directional light, ambient light.
        @param worldCollectionName World collection file name.
        @param objectName Region name.
        @param region Region object.
        @param regionConfig Region configuration.
        @return True to create the object, false to skip it.
    */
    public boolean adjustRegionConfig(String worldCollectionName,
        String objectName, Region region, RegionConfig regionConfig);

    /** Modify or override spawn generator.
        @param worldCollectionName World collection file name.
        @param objectName Spawn generator name (marker name).
        @param spawnData Spawn generator data.
        @return True to create the object, false to skip it.
    */
    public boolean adjustSpawnData(String worldCollectionName,
        String objectName, SpawnData spawnData);

}



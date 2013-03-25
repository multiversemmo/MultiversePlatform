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

import java.util.List;
import java.util.LinkedList;
import java.util.Map;
import java.util.HashMap;
import java.util.Collection;

import multiverse.server.objects.Marker;
import multiverse.server.pathing.PathInfo;
import multiverse.server.util.Log;
import multiverse.server.engine.*;

public class Instance extends Entity
{
    public Instance()
    {
        super();
    }

    public Instance(long oid)
    {
        super(oid);
        globalRegion.addConfig(roadConfig);
    }

    public String getName()
    {
        return name;
    }

    public void setName(String name)
    {
        this.name = name;
    }

    public String getTemplateName()
    {
        return templateName;
    }

    public void setTemplateName(String templateName)
    {
        this.templateName = templateName;
    }

    public String getWorldFileName()
    {
        return worldFileName;
    }

    public void setWorldFileName(String fileName)
    {
        worldFileName = fileName;
    }

    public String getInitScriptFileName()
    {
        return initScriptFileName;
    }

    public void setInitScriptFileName(String fileName)
    {
        initScriptFileName = fileName;
    }

    public String getLoadScriptFileName()
    {
        return loadScriptFileName;
    }

    public void setLoadScriptFileName(String fileName)
    {
        loadScriptFileName = fileName;
    }

    public static final int STATE_INIT = 0;
    public static final int STATE_GENERATE = 1;
    public static final int STATE_LOAD = 2;
    public static final int STATE_AVAILABLE = 3;
    public static final int STATE_UNLOAD = 4;
    public static final int STATE_DELETE = 5;

    public int getState()
    {
        return state;
    }

    public void setState(int state)
    {
        this.state = state;
    }

    public String getWorldLoaderOverrideName()
    {
        return worldLoaderOverrideName;
    }

    public void setWorldLoaderOverrideName(String loaderName)
    {
        worldLoaderOverrideName = loaderName;
    }

    public WorldLoaderOverride getWorldLoaderOverride()
    {
        return worldLoaderOverride;
    }

    public void setWorldLoaderOverride(WorldLoaderOverride loaderOverride)
    {
        worldLoaderOverride = loaderOverride;
    }

    public boolean loadWorldFile()
    {
        WorldFileLoader loader = new WorldFileLoader(this, worldFileName,
            worldLoaderOverride);
        return loader.load();
    }

    public boolean runInitScript()
    {
        if (initScriptFileName == null)
            return true;
        setCurrentInstance(this);
        try {
            ScriptManager scriptManager = new ScriptManager();
            scriptManager.init();
            scriptManager.runFileWithThrow(initScriptFileName);
            return true;
        }
        catch (Exception e) {
            // ignore, ScriptManager already logged
        }
        finally {
            setCurrentInstance(null);
        }
        return false;
    }

    public boolean runLoadScript()
    {
        if (loadScriptFileName == null)
            return true;
        setCurrentInstance(this);
        try {
            ScriptManager scriptManager = new ScriptManager();
            scriptManager.init();
            scriptManager.runFileWithThrow(loadScriptFileName);
            return true;
        }
        catch (Exception e) {
            // ignore, ScriptManager already logged
        }
        finally {
            setCurrentInstance(null);
        }
        return false;
    }

    public String getGlobalSkybox()
    {
        return globalSkybox ;
    }

    public void setGlobalSkybox(String skybox)
    {
        this.globalSkybox = skybox;
    }

    public Fog getGlobalFog()
    {
        return globalFog ;
    }

    public void setGlobalFog(Fog fog)
    {
        this.globalFog = fog;
    }

    public Color getGlobalAmbientLight()
    {
        return globalAmbientLight;
    }
    public void setGlobalAmbientLight(Color lightColor)
    {
        this.globalAmbientLight = lightColor;
    }

    public LightData getGlobalDirectionalLight()
    {
        return globalDirLightData;
    }
    public void setGlobalDirectionalLight(LightData lightData)
    {
        this.globalDirLightData = lightData;
    }

    public OceanData getOceanData()
    {
        return oceanData;
    }

    public void setOceanData(OceanData od)
    {
        this.oceanData = od;
    }

    public TerrainConfig getTerrainConfig()
    {
        return terrainConfig;
    }

    public void setTerrainConfig(TerrainConfig terrainConfig)
    {
        this.terrainConfig = terrainConfig;
    }

    public Region getGlobalRegion()
    {
        return globalRegion;
    }

    public RoadRegionConfig getRoadConfig()
    {
        return roadConfig;
    }

    public synchronized void addRegion(Region region)
    {
        regionList.add(region);
    }

    public synchronized Region getRegion(String regionName)
    {
        for (Region region : regionList) {
            if (region.getName().equals(regionName))
                return region;
        }
        return null;
    }

    public synchronized List<Region> getRegionList()
    {
        return new LinkedList<Region>(regionList);
    }

    public synchronized void addRegionConfig(String region)
    {
        regionConfig.add(region);
    }
    public synchronized List<String> getRegionConfig()
    {
        return new LinkedList<String>(regionConfig);
    }

    public synchronized Marker getMarker(String name)
    {
        return markers.get(name);
    }

    public synchronized void setMarker(String name, Marker marker)
    {
        markers.put(name, marker);
    }

    public synchronized void addSpawnData(SpawnData spawnData)
    {
        spawnGen.add(spawnData);
    }

    public synchronized List<SpawnData> getSpawnData() {
        return spawnGen;
    }
    
    public PathInfo getPathInfo()
    {
        return pathInfo;
    }

    public synchronized int changePlayerPopulation(int delta)
    {
        playerPopulation += delta;
        return playerPopulation;
    }

    public int getPlayerPopulation()
    {
        return playerPopulation;
    }

    public static Instance current()
    {
        if (loadingInstance.get() == null)
            Log.error("Instance.current() called in the wrong context");
        return loadingInstance.get();
    }

    public static long currentOid() 
    {
        Instance instance = loadingInstance.get();
        if (instance != null)
            return instance.getOid();
        else {
            Log.error("Instance.currentOid() called in the wrong context");
            return -1;
        }
    }

    static void setCurrentInstance(Instance instance)
    {
        loadingInstance.set(instance);
    }

    public Collection runMarkerSearch(SearchClause search,
            SearchSelection selection)
    {
        return markerSearch.runSearch(search, selection);
    }

    public Collection runRegionSearch(SearchClause search,
            SearchSelection selection)
    {
        return regionSearch.runSearch(search, selection);
    }

    class MarkerSearch implements Searchable
    {
        public Collection runSearch(SearchClause search,
            SearchSelection selection)
        {
            Matcher matcher = SearchManager.getMatcher(search, Marker.class);
            if (matcher == null)
                return null;

            List<Object> resultList = new LinkedList<Object>();
            for (Map.Entry<String,Marker> entry : markers.entrySet()) {
                Marker marker = entry.getValue();
                boolean rc = matcher.match(marker.getPropertyMapRef());
                if (rc)  {
                    selectProperties(entry.getKey(),marker,
                        selection,resultList);
                }
            }
            return resultList;
        }

        void selectProperties(String name, Marker marker,
                SearchSelection selection, List<Object> resultList)
        {
            if (selection.getResultOption() ==
                    SearchSelection.RESULT_KEY_ONLY) {
                resultList.add(name);
                return;
            }

            long propFlags = selection.getPropFlags();
            Marker result = new Marker();
            if ((propFlags & Marker.PROP_POINT) != 0)
                result.setPoint(marker.getPoint());
            if ((propFlags & Marker.PROP_ORIENTATION) != 0)
                result.setOrientation(marker.getOrientation());
            if ((propFlags & Marker.PROP_PROPERTIES) != 0)
                result.setProperties(marker.getPropertyMapRef());

            if (selection.getResultOption() == SearchSelection.RESULT_KEYED)
                resultList.add(new SearchEntry(name, result));
            else if ((propFlags & Marker.PROP_ALL) != 0)
                resultList.add(result);
        }

    }

    class RegionSearch implements Searchable
    {
        public Collection runSearch(SearchClause search,
            SearchSelection selection)
        {
            Matcher matcher = SearchManager.getMatcher(search, Region.class);
            if (matcher == null)
                return null;

            List<Object> resultList = new LinkedList<Object>();
            for (Region region : regionList) {
                boolean rc = matcher.match(region.getPropertyMapRef());
                if (rc)  {
                    selectProperties(region.getName(), region,
                        selection, resultList);
                }
            }
            return resultList;
        }

        void selectProperties(String name, Region region,
                SearchSelection selection, List<Object> resultList)
        {
            if (selection.getResultOption() ==
                    SearchSelection.RESULT_KEY_ONLY) {
                resultList.add(name);
                return;
            }

            long propFlags = selection.getPropFlags();
            Region result = new Region();
            result.setName(region.getName());
            result.setPriority(region.getPriority());
            if ((propFlags & Region.PROP_BOUNDARY) != 0)
                result.setBoundary(region.getBoundary());
            if ((propFlags & Region.PROP_PROPERTIES) != 0)
                result.setProperties(region.getPropertyMapRef());

            if (selection.getResultOption() == SearchSelection.RESULT_KEYED)
                resultList.add(new SearchEntry(name, result));
            else if ((propFlags & Region.PROP_ALL) != 0)
                resultList.add(result);
        }

    }

    transient private MarkerSearch markerSearch = new MarkerSearch();
    transient private RegionSearch regionSearch = new RegionSearch();

    transient private String globalSkybox = "";
    transient private Fog globalFog;
    transient private Color globalAmbientLight;
    transient private LightData globalDirLightData;
    transient private OceanData oceanData;
    transient private TerrainConfig terrainConfig;

    transient private Region globalRegion = new Region("Global Region");
    transient private RoadRegionConfig roadConfig = new RoadRegionConfig();

    transient private LinkedList<Region> regionList = new LinkedList<Region>();
    transient private LinkedList<String> regionConfig = new LinkedList<String>();
    transient private Map<String, Marker> markers = new HashMap<String, Marker>();
    transient private List<SpawnData> spawnGen = new LinkedList<SpawnData>();

    transient private PathInfo pathInfo = new PathInfo();

    private String name;
    private String worldFileName;
    private String initScriptFileName;
    private String loadScriptFileName;
    private String templateName;
    private String worldLoaderOverrideName;
    transient private int state = STATE_INIT;
    transient private WorldLoaderOverride worldLoaderOverride;

    transient private int playerPopulation;

    private static ThreadLocal<Instance> loadingInstance =
        new ThreadLocal<Instance>();
    
    private static final long serialVersionUID = 1L;
}


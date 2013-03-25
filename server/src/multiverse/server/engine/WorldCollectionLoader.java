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

import org.w3c.dom.*;
import java.io.Serializable;
import java.util.List;
import java.util.LinkedList;
import java.util.Map;
import java.util.HashMap;

import multiverse.server.util.Log;
import multiverse.server.util.XMLHelper;
import multiverse.server.util.MVRuntimeException;
import multiverse.server.objects.*;
import multiverse.server.math.*;
import multiverse.msgsys.Message;
import multiverse.server.plugins.WorldManagerClient;
import multiverse.server.plugins.ObjectManagerClient;
import multiverse.mars.plugins.AnimationClient;
import multiverse.server.pathing.*;

public class WorldCollectionLoader extends WorldFileLoader
{
    public WorldCollectionLoader(Instance instance,
        String worldCollectionFileName, WorldLoaderOverride override)
    {
        super(instance, worldCollectionFileName, override);
    }

    public boolean generate()
    {
        Node worldObjColNode = XMLHelper.getMatchingChild(worldDoc,
                "WorldObjectCollection");
        if (worldObjColNode == null) {
            Log.error("No <WorldObjectCollection>");
            return false;
        }
        
        // get the version #
        String version = XMLHelper.getAttribute(worldObjColNode, "Version");
        if ((version == null) || (! version.equals("2"))) {
            Log.error("Unsupported version number in file "+
                    worldFileName+": " + version);
            return false;
        }
        Log.debug("World collection '"+worldFileName+"' version: "+version);
        
        // point lights
        List<Node> pointLightNodes = XMLHelper.getMatchingChildren(worldObjColNode, "PointLight");
        if (pointLightNodes != null) {
            for (Node pointLightNode : pointLightNodes) {
                String lightName = XMLHelper.getAttribute(pointLightNode, "Name");
                String attenuationRange = XMLHelper.getAttribute(pointLightNode, "AttenuationRange");
                String attenuationConstant = XMLHelper.getAttribute(pointLightNode, "AttenuationConstant");
                String attenuationLinear = XMLHelper.getAttribute(pointLightNode, "AttenuationLinear");
                String attenuationQuadratic = XMLHelper.getAttribute(pointLightNode, "AttenuationQuadratic");
                Point lightLoc = getPoint(XMLHelper.getMatchingChild(pointLightNode, "Position"));
                Color specular = getColor(XMLHelper.getMatchingChild(pointLightNode, "Specular"));
                Color diffuse = getColor(XMLHelper.getMatchingChild(pointLightNode, "Diffuse"));
                
                LightData lightData = new LightData();
                lightData.setName(lightName);
                lightData.setAttenuationRange(Float.parseFloat(attenuationRange));
                lightData.setAttenuationConstant(Float.parseFloat(attenuationConstant));
                lightData.setAttenuationLinear(Float.parseFloat(attenuationLinear));
                lightData.setAttenuationQuadradic(Float.parseFloat(attenuationQuadratic));
                lightData.setSpecular(specular);
                lightData.setDiffuse(diffuse);
                lightData.setInitLoc(lightLoc);
                if (Log.loggingDebug)
                    Log.debug("LightData=" + lightData);
                
                // create a light object
                if (worldLoaderOverride.adjustLightData(worldFileName,
                        lightName, lightData)) {
                    Long lightOid = ObjectManagerClient.generateLight(
                        instance.getOid(), lightData);
                    if (Log.loggingDebug)
                        Log.debug("Generated light, oid=" + lightOid);
                    
                    // spawn the light.  A -1 return value means failure.
                    boolean rv = (WorldManagerClient.spawn(lightOid) >= 0);
                    if (Log.loggingDebug)
                        Log.debug("Light spawn rv=" + rv);
                }
            }
        }
        
        // objects
        List<Node> objectsList = XMLHelper.getMatchingChildren(worldObjColNode,
                "StaticObject");

        for (Node objNode : objectsList) {
            String name = XMLHelper.getAttribute(objNode, "Name");
            String mesh = XMLHelper.getAttribute(objNode, "Mesh");

            if (Log.loggingDebug)
                Log.debug("StaticObject " + name + " mesh '" + mesh + "'");
            Node posNode = XMLHelper.getMatchingChild(objNode, "Position");
            if (posNode == null) {
                Log.error("No <Position> node, name="+name);
                return false;
            }
            Point loc = getPoint(posNode);

            int perceptionRadius = 0;
            String radiusStr = XMLHelper.getAttribute(objNode,
                    "PerceptionRadius");
            if (radiusStr != null)
                perceptionRadius = (int)Float.parseFloat(radiusStr);

            Node scaleNode = XMLHelper.getMatchingChild(objNode, "Scale");
            MVVector scale = getVector(scaleNode);

            Node rotNode = XMLHelper.getMatchingChild(objNode, "Rotation");
            Quaternion orient = null;
            if (rotNode != null) {
                int rotation = getPoint(rotNode).getY();
                orient = Quaternion.fromAngleAxisDegrees(rotation,
                    new MVVector(0, 1, 0));
            } else {
                Node orientNode = XMLHelper.getMatchingChild(objNode, "Orientation");
                orient = getQuaternion(orientNode);
            }
            
            // Shadow settings
            boolean castShadow = false;
            boolean receiveShadow = false;
            String shadowStr = XMLHelper.getAttribute(objNode, 
                    "CastShadows");
            if (shadowStr != null)
                castShadow = shadowStr.equals("True") ? true : false;
            shadowStr = XMLHelper.getAttribute(objNode, "ReceiveShadows");
            if (shadowStr != null)
                receiveShadow = shadowStr.equals("True") ? true : false;

            // get the submeshes node
            DisplayContext dc = new DisplayContext(mesh);
            dc.setCastShadow(castShadow);
            dc.setReceiveShadow(receiveShadow);
            Node subMeshNode = XMLHelper.getMatchingChild(objNode, "SubMeshes");
            if (subMeshNode != null) {
                List<Node> subMeshInfoList = XMLHelper.getMatchingChildren(
                        subMeshNode, "SubMeshInfo");
                for (Node subMeshInfo : subMeshInfoList) {
                    String subMeshInfoName = XMLHelper.getAttribute(
                            subMeshInfo, "Name");
                    String subMeshInfoMaterial = XMLHelper.getAttribute(
                            subMeshInfo, "MaterialName");
                    if (Log.loggingDebug)
                        Log.debug("Submesh name=" + subMeshInfoName
                                  + ", material=" + subMeshInfoMaterial);
                    if (!XMLHelper.getAttribute(subMeshInfo, "Show")
                            .equals("True")) {
                        Log.warn("SubMesh is not visible - skipping, name="+subMeshInfoName);
                        continue;
                    }
                    dc.addSubmesh(new DisplayContext.Submesh(
                            subMeshInfoName, subMeshInfoMaterial));
                }
            }
            Node pathObjectsNode = XMLHelper.getMatchingChild(objNode, "PathData");
            PathData pathData = null;
            if (pathObjectsNode != null) {
                int pathVersion = (int)Float.parseFloat(XMLHelper.getAttribute(pathObjectsNode, "version"));
                List<PathObject> pathObjects = new LinkedList<PathObject>();
                List<Node> pathObjectNodes = XMLHelper.getMatchingChildren(
                        pathObjectsNode, "PathObject");
                for (Node pathObjectNode : pathObjectNodes) {
                    String modelName = XMLHelper.getAttribute(pathObjectNode, "modelName");
                    String type = XMLHelper.getAttribute(pathObjectNode, "type");
                    int firstTerrainIndex = (int)Float.parseFloat(XMLHelper.getAttribute(pathObjectNode, "firstTerrainIndex"));
                    List<PathPolygon> boundingPolygons = 
                        processPathPolygons("BoundingPolygon", pathObjectNode);
                    assert boundingPolygons.size() == 1;
                    List<PathPolygon> polygons = processPathPolygons("PathPolygons", pathObjectNode);
                    List<PathArc> portals = processPathArcs("PathPortals", pathObjectNode);
                    List<PathArc> arcs = processPathArcs("PathArcs", pathObjectNode);
                    PathPolygon boundingPolygon = boundingPolygons.get(0);
                    pathObjects.add(new PathObject(modelName, type, firstTerrainIndex, boundingPolygon, polygons, portals, arcs));
                    if (Log.loggingDebug)
                        Log.debug("Path object model name =" + modelName +
                            ", bounding polygon = " + boundingPolygon +
                            ", polygon count = " + polygons.size() +
                            ", portals count = " + portals.size() + 
                            ", arcs count = " + arcs.size());
                }
                if (pathObjects.size() > 0)
                    pathData = new PathData(pathVersion, pathObjects);
                if (Log.loggingDebug)
                    Log.debug("Read PathData for model object " + name);
            }
            if (Log.loggingDebug)
                Log.debug("StaticObject name=" + name + " mesh=" + mesh + " loc="
                          + loc + " scale=" + scale + " orient=" + orient
                          + " mesh=" + dc.getMeshFile() + " percRadius=" + perceptionRadius);

            // Sounds
            List<Node> sounds = XMLHelper.getMatchingChildren(objNode,
                    "Sound");
            List<SoundData> soundData = getSoundDataList(sounds);

            Node nameValuePairsNode =
                XMLHelper.getMatchingChild(objNode, "NameValuePairs");

            String anim = null;
            Template overrideTemplate = new Template();

            if (nameValuePairsNode != null) {
                Map<String, Serializable> props =
                    XMLHelper.nameValuePairsHelper(nameValuePairsNode);
                if (props.containsKey("animation"))
                    anim = (String) props.get("animation");
                for (Map.Entry<String, Serializable> entry : props.entrySet()) {
                    overrideTemplate.put(Namespace.WORLD_MANAGER,
                        entry.getKey(), entry.getValue());
                }
            }

            overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_NAME, name);
            overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_OBJECT_TYPE, WorldManagerClient.TEMPL_OBJECT_TYPE_STRUCTURE);
            overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, dc);
            overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_INSTANCE, instance.getOid());
            overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_LOC, loc);
            overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_ORIENT, orient);
            overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_SCALE, scale);
            overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_PERCEPTION_RADIUS, perceptionRadius);
            overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_FOLLOWS_TERRAIN, Boolean.FALSE);

            if (anim != null) {
                overrideTemplate.put(Namespace.WORLD_MANAGER, AnimationClient.TEMPL_ANIM, anim);
            }
            if (soundData.size() > 0)  {
                overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_SOUND_DATA_LIST, (Serializable)soundData);
            }
            List<Node> partEffectNodes = XMLHelper.getMatchingChildren(objNode, "ParticleEffect");
            LinkedList<LinkedList> particles = new LinkedList<LinkedList>();
            for (Node partEffectNode : partEffectNodes) {
                LinkedList<Object> particleData = new LinkedList<Object>();
                String peName = XMLHelper.getAttribute(partEffectNode, "ParticleEffectName");
                String velScale = XMLHelper.getAttribute(partEffectNode, "VelocityScale");
                String particleScale = XMLHelper.getAttribute(partEffectNode, "ParticleScale");
                String attachName = XMLHelper.getAttribute(partEffectNode, "AttachmentPoint");
                particleData.add(peName);
                particleData.add(attachName);
                particleData.add(Float.parseFloat(velScale));
                particleData.add(Float.parseFloat(particleScale));
                particles.add(particleData);
            }
            if (!particles.isEmpty()) {
                overrideTemplate.put(Namespace.WORLD_MANAGER, "StaticParticles", particles);
            }
            if (worldLoaderOverride.adjustObjectTemplate(worldFileName,
                    name, overrideTemplate)) {
                Long objOid =
                    ObjectManagerClient.generateObject(
                        ObjectManagerClient.BASE_TEMPLATE, overrideTemplate);
                if (objOid != null) {
                    WorldManagerClient.spawn(objOid);
                }
                else {
                    Log.error("Could not create static object="
                              + name + " mesh=" + mesh + " loc=" + loc);
                }
            
                if (pathData != null)
                    instance.getPathInfo().getPathDictionary().put(name, pathData);
            }

        }   // End of 'StaticObject'
    
        // get the regions (boundaries)
        List<Node> boundaryNodes = XMLHelper.getMatchingChildren(
                worldObjColNode, "Boundary");
        for (Node boundaryNode : boundaryNodes) {
            processBoundary(boundaryNode);
        }

        // get the roads - for now we place them all into a 'fake' region
        // FIXME: add roads to extent system

//## this boundary should be max geometry ??
        Boundary globalBoundary = new Boundary();
        globalBoundary.addPoint(new Point(-2000000000, 0, 2000000000));
        globalBoundary.addPoint(new Point(2000000000, 0, 2000000000));
        globalBoundary.addPoint(new Point(2000000000, 0, -2000000000));
        globalBoundary.addPoint(new Point(-2000000000, 0, -2000000000));

        instance.getGlobalRegion().setBoundary(globalBoundary);
        List<Node> roadNodes =
            XMLHelper.getMatchingChildren(worldObjColNode, "Road");
        if (Log.loggingDebug)
            Log.debug("Road count=" + roadNodes.size());
        for (Node roadNode : roadNodes) {
            Road road = processRoad(roadNode);
            instance.getRoadConfig().addRoad(road);
            if (Log.loggingDebug)
                Log.debug("Road: " + road + ", config=" +
                    instance.getRoadConfig());
        }

        // add the global directional light into the global region
        LightData globalDirLight = instance.getGlobalDirectionalLight();
        if (Log.loggingDebug)
            Log.debug("Global dir light: " + globalDirLight);
        if (globalDirLight != null) {
            RegionConfig lightRegionConfig = new RegionConfig(
                    LightData.DirLightRegionType);
            lightRegionConfig.setProperty("orient", globalDirLight
                    .getOrientation());
            lightRegionConfig.setProperty("specular", globalDirLight
                    .getSpecular());
            lightRegionConfig.setProperty("diffuse", globalDirLight
                    .getDiffuse());
            lightRegionConfig.setProperty("name", "dirLight_GLOBAL");
            instance.getGlobalRegion().addConfig(lightRegionConfig);
        }
        
        // add global ambient light into the global region
        Color globalAmbientLight = instance.getGlobalAmbientLight();
        if (Log.loggingDebug)
            Log.debug("Global ambient light: " + globalAmbientLight);
        if (globalAmbientLight != null) {
            RegionConfig ambientConfig = new RegionConfig(LightData.AmbientLightRegionType);
            ambientConfig.setProperty("color", globalAmbientLight);
            instance.getGlobalRegion().addConfig(ambientConfig);
        }

        // get the markers
        List<Node> markerNodes = XMLHelper.getMatchingChildren(
                worldObjColNode, "Waypoint");
        for (Node markerNode : markerNodes) {
            String name = XMLHelper.getAttribute(markerNode, "Name");
            Node posNode = XMLHelper.getMatchingChild(markerNode,
                    "Position");
            Point loc = getPoint(posNode);

            Node orientNode =
                XMLHelper.getMatchingChild(markerNode, "Orientation");
            Quaternion orient = getQuaternion(orientNode);
            if (Log.loggingDebug)
                Log.debug("Marker " + name + ", loc=" + loc);

            Marker marker = new Marker(loc,orient);
            Node nameValuePairsNode = XMLHelper.getMatchingChild(markerNode, "NameValuePairs");
            if (nameValuePairsNode != null) {
                marker.setProperties(XMLHelper.nameValuePairsHelper(nameValuePairsNode));
            }
            instance.setMarker(name, marker);

            // treat particle effect markers special
            List<Node> partEffectNodes = XMLHelper.getMatchingChildren(
                    markerNode, "ParticleEffect");
            for (Node partEffectNode : partEffectNodes) {
                String peName = XMLHelper.getAttribute(partEffectNode, "ParticleEffectName");
                String velScale = XMLHelper.getAttribute(partEffectNode, "VelocityScale");
                String particleScale = XMLHelper.getAttribute(partEffectNode, "ParticleScale");

                String objName = name + "-" + peName;
                Template overrideTemplate = new Template();
                overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_NAME, objName);
                overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_OBJECT_TYPE,
                                     WorldManagerClient.TEMPL_OBJECT_TYPE_STRUCTURE);
                overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_DISPLAY_CONTEXT,
                                     new DisplayContext("tiny_cube.mesh"));
                overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_INSTANCE, instance.getOid());
                overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_LOC, loc);
                overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_ORIENT, orient);
                overrideTemplate.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_FOLLOWS_TERRAIN,
                                     Boolean.FALSE);

                LinkedList<LinkedList> particles = new LinkedList<LinkedList>();
                LinkedList<Object> particleData = new LinkedList<Object>();
                particleData.add(peName);
                particleData.add("base");
                particleData.add(Float.parseFloat(velScale));
                particleData.add(Float.parseFloat(particleScale));
                particles.add(particleData);
                overrideTemplate.put(Namespace.WORLD_MANAGER, "StaticParticles", particles);

                if (worldLoaderOverride.adjustObjectTemplate(worldFileName,
                        objName, overrideTemplate)) {
                    Long fakeOid = ObjectManagerClient.generateObject(
                        ObjectManagerClient.BASE_TEMPLATE, overrideTemplate);
                    if (fakeOid != null) {
                        WorldManagerClient.spawn(fakeOid);
                    }
                    else {
                        Log.error("Could not create object for particle system="
                              + name + " particle=" + peName + " loc=" + loc);
                    }
                }
            } // end of <ParticleEffect>

            // Spawn generators
            List<Node> spawnGenNodes = XMLHelper.getMatchingChildren(
                    markerNode, "SpawnGen");
            for (Node spawnGenNode : spawnGenNodes) {
                String templateName = XMLHelper.getAttribute(spawnGenNode, "TemplateName");
                String spawnRadius = XMLHelper.getAttribute(spawnGenNode, "SpawnRadius");
                String numSpawns = XMLHelper.getAttribute(spawnGenNode, "NumSpawns");
                String respawnTime = XMLHelper.getAttribute(spawnGenNode, "RespawnTime");

                Integer spawnRadiusVal = new Integer(0);
                Integer numSpawnsVal = new Integer(1);
                Integer respawnTimeVal = new Integer(0);

                if (spawnRadius != null) {
                    spawnRadiusVal = Integer.parseInt(spawnRadius);
                }
                if (numSpawns != null) {
                    // Integer from WorldEditor
                    numSpawnsVal = Integer.parseInt(numSpawns);
                }
                if (respawnTime != null) {
                    // Integer from WorldEditor
                    respawnTimeVal = Integer.parseInt(respawnTime);
                }

                SpawnData spawnData = new SpawnData(name, templateName,
                    "WEObjFactory", instance.getOid(), loc, orient,
                    spawnRadiusVal, numSpawnsVal, respawnTimeVal);

                // Use the marker properties as the spawn gen properties
                if (nameValuePairsNode != null) {
                    spawnData.setPropertyMap(XMLHelper.nameValuePairsHelper(nameValuePairsNode));
                }

                if (worldLoaderOverride.adjustSpawnData(worldFileName,
                        name, spawnData))
                    instance.addSpawnData(spawnData);
            } // end of <SpawnGen>

            // Marker sounds
            // Create an object for each sound on the marker.  Each
            // sound has a different perception radius, so putting
            // multiple sounds on a single object would result in
            // notifying of sounds that can't yet be heard.
            List<Node> sounds = XMLHelper.getMatchingChildren(
                markerNode, "Sound");
            List<SoundData> soundData = getSoundDataList(sounds);

            for (SoundData data : soundData)  {
                Template overrideTemplate = new Template();
                int perceptionRadius= 25 * 1000;
                String maxAtten= data.getProperties().get("MaxAttenuationDistance");
                if (maxAtten != null) {
                    perceptionRadius= (int)Float.parseFloat(maxAtten);
                }

                String objName = name + "-" + data.getFileName();

                // create a 'fake' dc
                DisplayContext fakeDC = new DisplayContext("tiny_cube.mesh");
                overrideTemplate.put(Namespace.WORLD_MANAGER,
                                     WorldManagerClient.TEMPL_OBJECT_TYPE, 
                                     WorldManagerClient.TEMPL_OBJECT_TYPE_POINT_SOUND);
                overrideTemplate.put(Namespace.WORLD_MANAGER,
                                     WorldManagerClient.TEMPL_NAME, 
                                     objName);
                overrideTemplate.put(Namespace.WORLD_MANAGER,
                                     WorldManagerClient.TEMPL_DISPLAY_CONTEXT, fakeDC);
                overrideTemplate.put(Namespace.WORLD_MANAGER,
                                     WorldManagerClient.TEMPL_INSTANCE, instance.getOid());
                overrideTemplate.put(Namespace.WORLD_MANAGER,
                                     WorldManagerClient.TEMPL_LOC, loc);
                overrideTemplate.put(Namespace.WORLD_MANAGER,
                                     WorldManagerClient.TEMPL_SCALE, new MVVector(1, 1, 1));
                overrideTemplate.put(Namespace.WORLD_MANAGER,
                                     WorldManagerClient.TEMPL_PERCEPTION_RADIUS,
                                     perceptionRadius);
                List<SoundData> soundList= new LinkedList<SoundData>();
                soundList.add(data);
                overrideTemplate.put(Namespace.WORLD_MANAGER,
                                     WorldManagerClient.TEMPL_SOUND_DATA_LIST, 
                                     (Serializable)soundList);

                if (worldLoaderOverride.adjustObjectTemplate(worldFileName,
                        objName, overrideTemplate)) {
                    Long objOid = ObjectManagerClient.generateObject(
                        ObjectManagerClient.BASE_TEMPLATE, overrideTemplate);
                    if (objOid != null) {
                        WorldManagerClient.spawn(objOid);
                    }
                    else {
                        Log.error("Could not create marker="
                              + name + " soundFileName=" + data.getFileName());
                    }
                }
            } // end of <Sound>
        } // end of <Marker>

        // Terrain decals
        List<Node> terrainDecals = XMLHelper.getMatchingChildren(
            worldObjColNode, "TerrainDecal");
        for (Node terrainDecal : terrainDecals) {
            String decalName = XMLHelper.getAttribute(
                terrainDecal, "Name");
            String imageName = XMLHelper.getAttribute(
                terrainDecal, "ImageName");
            int posX = (int)Math.round(
                Double.parseDouble( XMLHelper.getAttribute(
                terrainDecal, "PositionX")) );
            int posZ = (int)Math.round(
                Double.parseDouble( XMLHelper.getAttribute(
                terrainDecal, "PositionZ")) );
            float sizeX = Float.parseFloat( XMLHelper.getAttribute(
                terrainDecal, "SizeX"));
            float sizeZ = Float.parseFloat( XMLHelper.getAttribute(
                terrainDecal, "SizeZ"));
            float rotation = Float.parseFloat( XMLHelper.getAttribute(
                terrainDecal, "Rotation"));
            // Integer from WorldEditor
            int priority = Integer.parseInt( XMLHelper.getAttribute(
                terrainDecal, "Priority"));
            int perceptionRadius = 0;
            String radiusStr = XMLHelper.getAttribute(
                terrainDecal, "PerceptionRadius");
            if (radiusStr != null)
                perceptionRadius = (int)Float.parseFloat(radiusStr);
            TerrainDecalData data = new TerrainDecalData(
                imageName, posX, posZ, sizeX, sizeZ,
                rotation, priority);
            Template overrideTemplate = new Template();
            overrideTemplate.put(Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_OBJECT_TYPE, 
                WorldManagerClient.TEMPL_OBJECT_TYPE_TERRAIN_DECAL);
            overrideTemplate.put(Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_NAME, 
                decalName);
            overrideTemplate.put(Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_INSTANCE, instance.getOid());
            overrideTemplate.put(Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_LOC,
                new Point(posX, 0, posZ));
            overrideTemplate.put(Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_SCALE, new MVVector(1, 1, 1));
            overrideTemplate.put(Namespace.WORLD_MANAGER,
                    WorldManagerClient.TEMPL_PERCEPTION_RADIUS,
                    perceptionRadius);
            overrideTemplate.put(Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_TERRAIN_DECAL_DATA, 
                data);

            Long objOid = ObjectManagerClient.generateObject(ObjectManagerClient.BASE_TEMPLATE, overrideTemplate);
            if (objOid != null) {
                WorldManagerClient.spawn(objOid);
            }
            else {
                Log.error("Could not create decal="
                      + decalName + " imageName=" + imageName);
            }
        
        }

        // Nested world collections
        processWorldCollections(worldObjColNode);

        return true;
    }

    private void processBoundary(Node boundaryNode)
    {
        // get name
        String name = XMLHelper.getAttribute(boundaryNode, "Name");
        Boundary boundary = new Boundary(name);

        // default priority set in Region.DEFAULT_PRIORITY
	// Integer from WorldEditor
        String priS = XMLHelper.getAttribute(boundaryNode, "Priority");
        Integer pri = (priS == null) ? null : Integer.parseInt(priS);
        
        // get points
        Node pointsNode = XMLHelper.getMatchingChild(boundaryNode, "PointCollection");
        List<Node> points = XMLHelper.getMatchingChildren(pointsNode, "Point");
        if (points == null) {
            Log.warn("No points for boundary, ignoring");
            return;
        }
        for (Node pointNode : points) {
            Point p = getPoint(pointNode);
            boundary.addPoint(p);
        }
        
        // the boundary data will be put into the region object
        Region region = new Region();
        region.setName(name);
        region.setPriority(pri);
        region.setBoundary(boundary);
        if (Log.loggingDebug)
            Log.debug("processBoundary: new region=" + region);
        
        Node nameValuePairsNode = XMLHelper.getMatchingChild(boundaryNode, "NameValuePairs");
        if (nameValuePairsNode != null) {
            region.setProperties(XMLHelper.nameValuePairsHelper(nameValuePairsNode));
        }

        if (! worldLoaderOverride.adjustRegion(worldFileName,
                name, region))
            return;

        // Sound region
	List<Node> sounds = XMLHelper.getMatchingChildren(boundaryNode,
		"Sound");
	List<SoundData> soundData = getSoundDataList(sounds);
	if (soundData.size() > 0)  {
	    SoundRegionConfig soundConfig = new SoundRegionConfig();
	    soundConfig.setSoundData(soundData);
            if (worldLoaderOverride.adjustRegionConfig(worldFileName,
                    name, region, soundConfig))
                region.addConfig(soundConfig);
	}

        // Fog region
        Node fogNode = getFogNode(boundaryNode);
        if (fogNode != null) {
            String nearS = XMLHelper.getAttribute(fogNode, "Near");
            String farS = XMLHelper.getAttribute(fogNode, "Far");
            
            Node colorNode = XMLHelper.getMatchingChild(fogNode, "Color");
            String redS = XMLHelper.getAttribute(colorNode, "R");
            String greenS = XMLHelper.getAttribute(colorNode, "G");
            String blueS = XMLHelper.getAttribute(colorNode, "B");

            int red = (int)(Float.parseFloat(redS) * 255);
            int green = (int)(Float.parseFloat(greenS) * 255);
            int blue = (int)(Float.parseFloat(blueS) * 255);
            int near = (int)Float.parseFloat(nearS);
            int far = (int)Float.parseFloat(farS);
            FogRegionConfig fogConfig = new FogRegionConfig();
            fogConfig.setColor(new Color(red, green, blue));
            fogConfig.setNear(near);
            fogConfig.setFar(far);
            if (Log.loggingDebug)
                Log.debug("Fog region: " + fogConfig);
            if (worldLoaderOverride.adjustRegionConfig(worldFileName,
                    name, region, fogConfig))
                region.addConfig(fogConfig);
        }

        // Direction light region
        Node dirLightNode = XMLHelper.getMatchingChild(boundaryNode, "DirectionalLight");
        if (dirLightNode != null) {
            MVVector dir = getVector(XMLHelper.getMatchingChild(dirLightNode, "Direction"));
            Color diffuse = getColor(XMLHelper.getMatchingChild(dirLightNode, "Diffuse"));
            Color specular = getColor(XMLHelper.getMatchingChild(dirLightNode, "Specular"));
            RegionConfig regionConfig = new RegionConfig(LightData.DirLightRegionType);
            Quaternion orient = MVVector.UnitZ.getRotationTo(dir);
            if (orient == null) {
                if (Log.loggingDebug)
                    Log.debug("Region light is near inverse, dir=" + dir);
                orient = new Quaternion(0,1,0,0);
            }
            regionConfig.setProperty("orient", orient);
            regionConfig.setProperty("specular", specular);
            regionConfig.setProperty("diffuse", diffuse);
	    String boundaryName = XMLHelper.getAttribute(boundaryNode, "Name");
	    regionConfig.setProperty("name", "dirLight_" + boundaryName);
            if (worldLoaderOverride.adjustRegionConfig(worldFileName,
                    name, region, regionConfig)) {
                region.addConfig(regionConfig);
                if (Log.loggingDebug)
                    Log.debug("Added dir light region: specular=" + specular +
                        " diffuse=" + diffuse + " dir=" + dir + " orient=" + orient);
            }
        }

        // Ambient light region
        Node ambientNode = XMLHelper.getMatchingChild(boundaryNode, "AmbientLight");
        if (ambientNode != null) {
            Color ambientColor = getColor(XMLHelper.getMatchingChild(ambientNode, "Color"));
            RegionConfig regionConfig = new RegionConfig(LightData.AmbientLightRegionType);
            regionConfig.setProperty("color", ambientColor);
            if (worldLoaderOverride.adjustRegionConfig(worldFileName,
                    name, region, regionConfig)) {
                region.addConfig(regionConfig);
                Log.debug("Added ambient light region: color="+ambientColor);
            }
        }

        // Water region
        Node waterNode = getWaterNode(boundaryNode);
        if (waterNode != null) {
            float height = Float.parseFloat(XMLHelper.getAttribute(waterNode, "Height"));
            String regionConfig = "<boundaries><boundary><name>" + name + "_WATER</name>";

            regionConfig += "<points>";
            for (Point point : boundary.getPoints()) {
                regionConfig += "<point x=\"" + point.getX() + "\" y=\"" + point.getZ() + "\" />";
            }
            regionConfig += "</points>";
            
            // water attribute
            regionConfig += "<boundarySemantic type=\"WaterPlane\">";
            regionConfig += "<height>" + height + "</height>";
            regionConfig += "<name>" + name + "_WATERNAME</name>";
            regionConfig += "</boundarySemantic>";
            
            // close it up
            regionConfig += "</boundary></boundaries>";
            
            if (Log.loggingDebug)
                Log.debug("processBoundary: waterRegion: " + regionConfig);
            instance.addRegionConfig(regionConfig);
        }

        // Forest region
        // forests are special right now because we dont want to wait until the
        // player is IN the boundary to give it the data, so we send over all
        // forests in a region config message, which is different from the new region msg.
        // right now this data goes directly to the client
        List<Node> forestsNode = XMLHelper.getMatchingChildren(boundaryNode, "Forest");
        if (forestsNode != null) {
            for (Node forestNode : forestsNode) {
                String forestXML = processTreeBoundary(boundary.getPoints(), forestNode);
                if (Log.loggingDebug)
                    Log.debug("processBoundary: Tree boundary: xml=" + forestXML);
                instance.addRegionConfig(forestXML);
            }
        }

        // Grass region
        // same special treatment for grass
        Node grassNode = XMLHelper.getMatchingChild(boundaryNode, "Grass");
        if (grassNode != null) {
            String grassXML = processGrassBoundary(boundary.getPoints(), grassNode);
            if (Log.loggingDebug)
                Log.debug("processBoundary: Grass boundary: xml=" + grassXML);
            instance.addRegionConfig(grassXML);
        }

        Message msg =
            new WorldManagerClient.NewRegionMessage(instance.getOid(), region);
        Engine.getAgent().sendBroadcast(msg);

        instance.addRegion(region);
    }
    
    private String processTreeBoundary(List<Point> points, Node forestNode)
    {
        String xml = "<boundaries><boundary>";
        
        // get name
        String name = XMLHelper.getAttribute(forestNode, "Name");
        xml += "<name>" + name + "_FOREST</name>";  // append the type to keep region names unique
        if (Log.loggingDebug)
            Log.debug("processTreeBoundary: name=" + name);

        // get points
        xml += "<points>";
        for (Point p : points) {
            xml += "<point x=\"" + p.getX() + "\" y=\"" + p.getZ() + "\" />";
        }
        xml += "</points>";

        xml += "<boundarySemantic type=\"SpeedTreeForest\">";
        // get seed
        String seed = XMLHelper.getAttribute(forestNode, "Seed");
        xml += "<seed>" + seed + "</seed>";
        if (Log.loggingDebug)
            Log.debug("processTreeBoundary: seed=" + seed);

        xml += "<name>" + name + "</name>";

        // get wind filename
        String windFile = XMLHelper.getAttribute(forestNode,
                "Filename");
        xml += "<windFilename>" + windFile + "</windFilename>";
        if (Log.loggingDebug)
            Log.debug("processTreeBoundary: windFile=" + windFile);

        // wind str
        String windStr = XMLHelper.getAttribute(forestNode,
                "WindSpeed");
        xml += "<windStrength>" + windStr + "</windStrength>";
        if (Log.loggingDebug)
            Log.debug("processTreeBoundary: windStrength=" + windStr);

        // wind dir
        Node windDir = XMLHelper.getMatchingChild(forestNode, "WindDirection");
        String dirX = XMLHelper.getAttribute(windDir, "x");
        String dirY = XMLHelper.getAttribute(windDir, "y");
        String dirZ = XMLHelper.getAttribute(windDir, "z");
        xml += "<windDirection x=\"" + dirX + "\" y=\"" + dirY + "\" z=\""
                + dirZ + "\" />";
        if (Log.loggingDebug)
            Log.debug("processTreeBoundary: windDir: x=" + dirX + ",y=" + dirY + ",z=" + dirZ);

        // tree types
        List<Node> treeTypes = XMLHelper.getMatchingChildren(forestNode,
                "Tree");
        // filename=\"AmericanHolly_RT.spt\"
        // size=\"16000\"
        // sizeVariance=\"1200\"
        // numInstances=\"24\
        if (treeTypes.isEmpty()) {
            Log.warn("processTreeBoundary: no trees in forest");
            return null;
        }

        for (Node treeType : treeTypes) {
            String fileName = XMLHelper.getAttribute(treeType,
                    "Filename");
            xml += "<treeType";
            xml += " filename=\"" + fileName + "\"";
            if (Log.loggingDebug)
                Log.debug("processTreeBoundary: TreeType Filename=" + fileName);

            String scale = XMLHelper.getAttribute(treeType, "Scale");
            xml += " size=\"" + scale + "\"";
            if (Log.loggingDebug)
                Log.debug("processTreeBoundary: scale size=" + scale);

            String scaleVariance = XMLHelper.getAttribute(treeType,
                    "ScaleVariance");
            xml += " sizeVariance=\"" + scaleVariance + "\"";
            if (Log.loggingDebug)
                Log.debug("processTreeBoundary: sizeVariance=" + scaleVariance);

            String instances = XMLHelper.getAttribute(treeType,
                    "Instances");
            xml += " numInstances=\"" + instances + "\" />";
            if (Log.loggingDebug)
                Log.debug("processTreeBoundary: instances=" + instances);
        }
        xml += "</boundarySemantic></boundary></boundaries>";

        return xml;
    }
    
    protected String processGrassBoundary(List<Point> points, Node grassNode)
    {
        String xml = "<boundaries><boundary>";
        String name = XMLHelper.getAttribute(grassNode, "Name");
        xml += "<name>" + name + "_GRASS</name>";  // append the type to keep region names unique
        if (Log.loggingDebug)
            Log.debug("processTreeBoundary: name=" + name);

        // get points
        xml += "<points>";
        for (Point p : points) {
            xml += "<point x=\"" + p.getX() + "\" y=\"" + p.getZ() + "\" />";
        }
        xml += "</points>";
        xml += "<boundarySemantic type=\"Vegetation\">";
        xml += "<name>" + name + "</name>";
        
        List<Node> plantTypes = XMLHelper.getMatchingChildren(grassNode, "PlantType");
        if (plantTypes == null) {
            throw new MVRuntimeException("no plant types in a grass boundary");
        }
        for (Node plantTypeNode : plantTypes) {
            String numInstances = XMLHelper.getAttribute(plantTypeNode, "Instances");
            String imageName = XMLHelper.getAttribute(plantTypeNode, "ImageName");
            String colorMultLow = XMLHelper.getAttribute(plantTypeNode, "ColorMultLow");
            String colorMultHi = XMLHelper.getAttribute(plantTypeNode, "ColorMultHi");
            String scaleWidthLow = XMLHelper.getAttribute(plantTypeNode, "ScaleWidthLow");
            String scaleWidthHi = XMLHelper.getAttribute(plantTypeNode, "ScaleWidthHi");
            String scaleHeightLow = XMLHelper.getAttribute(plantTypeNode, "ScaleHeightLow");
            String scaleHeightHi = XMLHelper.getAttribute(plantTypeNode, "ScaleHeightHi");
            String windMagnitude = XMLHelper.getAttribute(plantTypeNode, "WindMagnitude");
            String red = XMLHelper.getAttribute(plantTypeNode, "R");
            String green = XMLHelper.getAttribute(plantTypeNode, "G");
            String blue = XMLHelper.getAttribute(plantTypeNode, "B");
            
            
            xml += "<PlantType numInstances=\"" + numInstances + 
                "\" imageName=\""+ imageName +
                "\" atlasStartX=\"0\" atlasStartY=\"0\" atlasEndX=\"1\" atlasEndY=\"1\"" +
                " scaleWidthLow=\"" + scaleWidthLow + 
                "\" scaleWidthHi=\"" + scaleWidthHi +
                "\" scaleHeightLow=\"" + scaleHeightLow +
                "\" scaleHeightHi=\"" + scaleHeightHi +
                "\" colorMultLow=\"" + colorMultLow +
                "\" colorMultHi=\"" + colorMultHi +
                "\" windMagnitude=\"" + windMagnitude + 
                "\"><color r=\"" + red + "\" g=\"" + green + "\" b=\"" + blue + "\"/>" +
                "</PlantType>";
        }
        xml += "</boundarySemantic></boundary></boundaries>";
        return xml;
    }
    
    private List<SoundData> getSoundDataList(List<Node> sounds)
    {
        List<SoundData> soundData = new LinkedList<SoundData>();
        for (Node sound : sounds) {
            String fileName = XMLHelper.getAttribute(sound, "Filename");
            String typeStr = XMLHelper.getAttribute(sound, "Type");
            org.w3c.dom.NamedNodeMap attrMap = sound.getAttributes();
            Map<String,String> propertyMap= new HashMap<String,String>();
            for (int ii=0; ii < attrMap.getLength(); ii++) {
                Node attr= attrMap.item(ii);
                String attrName= attr.getNodeName();
                if (!attrName.equals("Filename") &&
                        !attrName.equals("Type")) {
                    propertyMap.put(attrName,attr.getNodeValue());
                }
            }
            soundData.add(
                new SoundData(fileName, typeStr, propertyMap));
        }
        return soundData;
    }

    private static Road processRoad(Node roadNode)
    {
        // get name
        String name = XMLHelper.getAttribute(roadNode, "Name");
        // Integer from WorldEditor
        Integer halfWidth = Integer.parseInt(XMLHelper.getAttribute(roadNode, "HalfWidth"));
        Road road = new Road(name);
        road.setHalfWidth(halfWidth);

        // get points node
        Node pointsNode = XMLHelper.getMatchingChild(roadNode, "PointCollection");

        // get all points
        List<Node> points = XMLHelper.getMatchingChildren(pointsNode, "Point");
        for (Node pointNode : points) {
            String x = XMLHelper.getAttribute(pointNode, "x");
            String y = XMLHelper.getAttribute(pointNode, "y");
            String z = XMLHelper.getAttribute(pointNode, "z");

            road.addPoint(new Point((int) Math.round(Double.parseDouble(x)),
                    (int) Math.round(Double.parseDouble(y)), (int) Math
                            .round(Double.parseDouble(z))));
        }
        return road;
    }

    // returns the fog node if it exists.
    static Node getFogNode(Node boundaryNode)
    {
        Node fogNode = XMLHelper.getMatchingChild(boundaryNode,
                "Fog");
        return fogNode;
    }

    static Node getWaterNode(Node boundaryNode) {
        return XMLHelper.getMatchingChild(boundaryNode, "Water");
    }

}

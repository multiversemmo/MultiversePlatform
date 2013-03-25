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

import javax.xml.parsers.*;
import org.xml.sax.SAXException;
import org.w3c.dom.*;
import java.io.IOException;
import java.io.File;
import java.util.List;
import java.util.LinkedList;

import multiverse.server.util.Log;
import multiverse.server.util.XMLHelper;
import multiverse.server.objects.*;
import multiverse.server.math.*;
import multiverse.server.pathing.*;
import multiverse.server.engine.TerrainConfig;
import multiverse.msgsys.Message;
import multiverse.server.plugins.WorldManagerClient;

public class WorldFileLoader
{
    public WorldFileLoader(Instance instance, String worldFileName,
        WorldLoaderOverride override)
    {
        this.instance = instance;
        this.worldFileName = worldFileName;
        this.worldLoaderOverride = override;
    }

    public void setWorldLoaderOverride(WorldLoaderOverride override)
    {
        worldLoaderOverride = override;
    }

    public WorldLoaderOverride getWorldLoaderOverride()
    {
        return worldLoaderOverride;
    }

    public boolean load()
    {
        if (! parse())
            return false;

        return generate();
    }

    public boolean parse()
    {
        try {
            DocumentBuilder builder = XMLHelper.makeDocBuilder();
            File xmlFile = new File(worldFileName);
            worldFileBasePath = xmlFile.getParent();

            worldDoc = builder.parse(xmlFile);
        }
        catch (IOException e) {
            Log.exception("WorldFileLoader.parse("+worldFileName+")", e);
            return false;
        } catch (SAXException e) {
            Log.exception("WorldFileLoader.parse("+worldFileName+")", e);
            return false;
        }
        return true;
    }

    public boolean generate()
    {
        Node worldNode = XMLHelper.getMatchingChild(worldDoc, "World");
        if (worldNode == null) {
            Log.error("No <World> node in file "+worldFileName);
            return false;
        }
        String worldName = XMLHelper.getAttribute(worldNode, "Name");
        if (worldName == null) {
            Log.error("No world name in file "+worldFileName);
            return false;
        }

        if (Log.loggingDebug)
            Log.debug("world name=" + worldName +
                " (file "+worldFileName+")");

        String fileVersion = XMLHelper.getAttribute(worldNode, "Version");
        if (fileVersion == null) {
            Log.error("No world file version");
            return false;
        }
        if (Log.loggingDebug)
            Log.debug("world file version=" + fileVersion);

        if (! fileVersion.equals("2")) {
            Log.error("Unsupported world file version in file " +
                worldFileName);
            return false;
        }

        // get the skybox
        Node skyboxNode = XMLHelper.getMatchingChild(worldNode, "Skybox");
        if (skyboxNode == null) {
            Log.debug("No <Skybox> node in file " + worldFileName);
        }
        else {
            String skybox = XMLHelper.getAttribute(skyboxNode, "Name");
            if (Log.loggingDebug)
                Log.debug("Global skybox=" + skybox);
            instance.setGlobalSkybox(skybox);
        }
        
        // get global fog
        Node globalFogNode = XMLHelper.getMatchingChild(worldNode,
            "GlobalFog");
        if (globalFogNode != null) {
            String near = XMLHelper.getAttribute(globalFogNode, "Near");
            String far = XMLHelper.getAttribute(globalFogNode, "Far");
            Color fogColor = getColor(
                XMLHelper.getMatchingChild(globalFogNode, "Color"));
            Fog fog = new Fog("global fog");
            fog.setStart((int)Float.parseFloat(near));
            fog.setEnd((int)Float.parseFloat(far));
            fog.setColor(fogColor);
            instance.setGlobalFog(fog);
            if (Log.loggingDebug)
                Log.debug("Global fog: " + fog);
        }

        // get global ambient light
        Node globalAmbientLightNode =
            XMLHelper.getMatchingChild(worldNode, "GlobalAmbientLight");
        if (globalAmbientLightNode != null) {
            Color lightColor = getColor(
                XMLHelper.getMatchingChild(globalAmbientLightNode,"Color"));
            instance.setGlobalAmbientLight(lightColor);
            if (Log.loggingDebug)
                Log.debug("Global ambient light: " + lightColor);
        }
        
        // get global directional light
        Node globalDirectionalLightNode = XMLHelper.getMatchingChild(worldNode, "GlobalDirectionalLight");
        if (globalDirectionalLightNode != null) {
            Color diffuseColor = getColor(XMLHelper.getMatchingChild(globalDirectionalLightNode, "Diffuse"));
            Color specularColor = getColor(XMLHelper.getMatchingChild(globalDirectionalLightNode, "Specular"));
            MVVector lightDir = getVector(XMLHelper.getMatchingChild(globalDirectionalLightNode, "Direction"));
            LightData lightData = new LightData();
            lightData.setName("globalDirLight");
            lightData.setDiffuse(diffuseColor);
            lightData.setSpecular(specularColor);
            lightData.setAttenuationRange(1000000);
            lightData.setAttenuationConstant(1);
            Quaternion q = MVVector.UnitZ.getRotationTo(lightDir);
            if (q == null) {
                if (Log.loggingDebug)
                    Log.debug("global light orient is near inverse, dir=" + lightDir);
                q = new Quaternion(0,1,0,0);
            }
            lightData.setOrientation(q);
            instance.setGlobalDirectionalLight(lightData);
            if (Log.loggingDebug)
                Log.debug("Global directional light: " + lightData);
        }
        
        Node pathObjectTypesNode = XMLHelper.getMatchingChild(worldNode, "PathObjectTypes");
        if (pathObjectTypesNode != null) {
            List<Node> pathObjectTypeNodes = XMLHelper.getMatchingChildren(
                pathObjectTypesNode, "PathObjectType");
            for (Node pathObjectTypeNode : pathObjectTypeNodes) {
                String potName = XMLHelper.getAttribute(pathObjectTypeNode, "name");
                float potHeight = Float.parseFloat(XMLHelper.getAttribute(pathObjectTypeNode, "height"));
                float potWidth = Float.parseFloat(XMLHelper.getAttribute(pathObjectTypeNode, "width"));
                float potMaxClimbSlope = Float.parseFloat(XMLHelper.getAttribute(pathObjectTypeNode, "maxClimbSlope"));
                instance.getPathInfo().getTypeDictionary().put(
                    potName,
                    new PathObjectType(potName, potHeight, potWidth, potMaxClimbSlope));
                if (Log.loggingDebug)
                    Log.debug("Path object type name=" + potName);
            }
        }

        // get the ocean
        Node oceanNode = XMLHelper.getMatchingChild(worldNode, "Ocean");
        if (oceanNode != null) {
            OceanData oceanData = new OceanData();
            
            String displayOcean = XMLHelper.getAttribute(oceanNode,
                "DisplayOcean");
            oceanData.displayOcean =
                displayOcean.equals("True") ? Boolean.TRUE : Boolean.FALSE;

            String useParams = XMLHelper.getAttribute(oceanNode, "UseParams");
            if (useParams != null) {
                oceanData.useParams =
                    useParams.equals("True") ? Boolean.TRUE : Boolean.FALSE;
            }
            
            String waveHeight = XMLHelper.getAttribute(oceanNode, "WaveHeight");
            if (waveHeight != null) {
                oceanData.waveHeight = Float.parseFloat(waveHeight);
            }
            
            String seaLevel = XMLHelper.getAttribute(oceanNode, "SeaLevel");
            if (seaLevel != null) {
                oceanData.seaLevel = Float.parseFloat(seaLevel);
            }
            
            String bumpScale = XMLHelper.getAttribute(oceanNode, "BumpScale");
            if (bumpScale != null) {
                oceanData.bumpScale = Float.parseFloat(bumpScale);
            }
            
            String bumpSpeedX = XMLHelper.getAttribute(oceanNode, "BumpSpeedX");
            if (bumpSpeedX != null) {
                oceanData.bumpSpeedX = Float.parseFloat(bumpSpeedX);
            }
            
            String bumpSpeedZ = XMLHelper.getAttribute(oceanNode, "BumpSpeedZ");
            if (bumpSpeedZ != null) {
                oceanData.bumpSpeedZ = Float.parseFloat(bumpSpeedZ);
            }
            
            String textureScaleX = XMLHelper.getAttribute(oceanNode, "TextureScaleX");
            if (textureScaleX != null) {
                oceanData.textureScaleX = Float.parseFloat(textureScaleX);
            }
            
            String textureScaleZ = XMLHelper.getAttribute(oceanNode, "TextureScaleZ");
            if (textureScaleZ != null) {
                oceanData.textureScaleZ = Float.parseFloat(textureScaleZ);
            }
            
            Node deepColorNode = XMLHelper.getMatchingChild(oceanNode, "DeepColor");
            if (deepColorNode != null) {
                oceanData.deepColor = getColor(deepColorNode);
            }
            
            Node shallowColorNode = XMLHelper.getMatchingChild(oceanNode, "ShallowColor");
            if (shallowColorNode != null) {
                oceanData.shallowColor = getColor(shallowColorNode);
            }
            
            instance.setOceanData(oceanData);
            if (Log.loggingDebug)
                Log.debug("Ocean: " + oceanData);
        }
        
        // Terrain configuration.  Only get from the world file if the
        // instance doesn't already have terrain config.  Terrain config
        // can be set by instance template or property.
        TerrainConfig terrainConfig = instance.getTerrainConfig();
        if (terrainConfig != null && Log.loggingDebug)
            Log.debug("Terrain: " + terrainConfig);
        if (terrainConfig == null) {
            Node terrainNode = XMLHelper.getMatchingChild(worldNode,
                    "Terrain");
            if (terrainNode != null) {
                String terrainXML = XMLHelper.toXML(terrainNode);
                if (Log.loggingDebug)
                    Log.debug("Terrain: xmlsize=" + terrainXML.length());
                
                // add in the terrain display
                Node terrainDisplay = XMLHelper.getMatchingChild(worldNode, "TerrainDisplay");
                if (terrainDisplay != null) {
                    String terrainDisplayXML = XMLHelper.toXML(terrainDisplay);
                    if (Log.loggingDebug)
                        Log.debug("TerrainDisplay: " + terrainDisplayXML);
                    terrainXML += terrainDisplayXML;
                }
                
                terrainConfig = new TerrainConfig();
                terrainConfig.setConfigType(TerrainConfig.configTypeXMLSTRING);
                terrainConfig.setConfigData(terrainXML);
                instance.setTerrainConfig(terrainConfig);
                if (Log.loggingDebug)
                    Log.debug("terrain has been set:" + terrainConfig);
            } else {
                Log.debug("No terrain in file");
            }
        }

        // read in filenames for world collections
        if (! processWorldCollections(worldNode))
            return false;

        // send out global region, including the
        // new road region info
        Message msg = new WorldManagerClient.NewRegionMessage(
            instance.getOid(), instance.getGlobalRegion());
        Engine.getAgent().sendBroadcast(msg);

        return true;
    }

    protected boolean processWorldCollections(Node node)
    {
        List<Node> worldCollections = XMLHelper.getMatchingChildren(node,
            "WorldCollection");
        for (Node worldCollectionNode : worldCollections) {
            String colFilename =
                XMLHelper.getAttribute(worldCollectionNode, "Filename");
            String fullFile = worldFileBasePath + File.separator + colFilename;
            if (Log.loggingDebug)
                Log.debug("Loading world collection " + fullFile);
            WorldCollectionLoader collectionLoader =
                new WorldCollectionLoader(instance, fullFile,
                    worldLoaderOverride);
            if (! collectionLoader.load())
                return false;
        }
        return true;
    }

    // Parses the CVPolygons or TerrainPolygons phrase, depending on kind
    protected static List<PathPolygon> processPathPolygons(String introducer,
        Node parentNode)
    {
        Node polyContainerNode = XMLHelper.getMatchingChild(parentNode, introducer);
        List<Node> polyNodes = XMLHelper.getMatchingChildren(polyContainerNode, "PathPolygon");
        LinkedList<PathPolygon> polys = new LinkedList<PathPolygon>();
        if (polyNodes == null)
            return polys;
        for (Node polyNode : polyNodes) {
            int index = (int)Float.parseFloat(XMLHelper.getAttribute(polyNode, "index"));
            String stringKind = XMLHelper.getAttribute(polyNode, "kind");
            byte polygonKind = PathPolygon.parsePolygonKind(stringKind);
            List<Node> cornerNodes = XMLHelper.getMatchingChildren(polyNode, "Corner");
            assert cornerNodes.size() >= 3;
            LinkedList<MVVector> corners = new LinkedList<MVVector>();
            for (Node corner : cornerNodes)
                corners.add(new MVVector(getPoint(corner)));
            polys.add(new PathPolygon(index, polygonKind, corners));
        }
        return polys;
    }

    // Parses the PathArcs phrase
    protected static List<PathArc> processPathArcs(String introducer,
        Node parentNode)
    {
        Node arcContainerNode = XMLHelper.getMatchingChild(parentNode, introducer);
        List<Node> arcNodes = XMLHelper.getMatchingChildren(arcContainerNode, "PathArc");
        LinkedList<PathArc> arcs = new LinkedList<PathArc>();
        if (arcNodes == null)
            return arcs;
        for (Node arcNode : arcNodes) {
            byte arcKind = PathArc.parseArcKind(XMLHelper.getAttribute(arcNode, "kind"));
            int poly1Index = (int)Float.parseFloat(XMLHelper.getAttribute(arcNode, "poly1Index"));
            int poly2Index = (int)Float.parseFloat(XMLHelper.getAttribute(arcNode, "poly2Index"));
            PathEdge edge = processPathEdge(arcNode);
            arcs.add(new PathArc(arcKind, poly1Index, poly2Index, edge));
        }
        return arcs;
    }

    // Parses a PathEdge
    protected static PathEdge processPathEdge(Node parentNode)
    {
        Node edgeNode = XMLHelper.getMatchingChild(parentNode, "PathEdge");
        return new PathEdge(new MVVector(getPoint(XMLHelper.getMatchingChild(edgeNode, "Start"))),
                            new MVVector(getPoint(XMLHelper.getMatchingChild(edgeNode, "End"))));
    }

    public static Color getColor(Node colorNode) {
        String redS = XMLHelper.getAttribute(colorNode, "R");
        String greenS = XMLHelper.getAttribute(colorNode, "G");
        String blueS = XMLHelper.getAttribute(colorNode, "B");
        Color color = new Color();
        color.setRed((int) (Float.parseFloat(redS) * 255));
        color.setGreen((int) (Float.parseFloat(greenS) * 255));
        color.setBlue((int) (Float.parseFloat(blueS) * 255));
        return color;
    }

    public static MVVector getVector(Node xyzNode) {
        String posX = XMLHelper.getAttribute(xyzNode, "x");
        String posY = XMLHelper.getAttribute(xyzNode, "y");
        String posZ = XMLHelper.getAttribute(xyzNode, "z");
        float x = Float.parseFloat(posX);
        float y = Float.parseFloat(posY);
        float z = Float.parseFloat(posZ);
        return new MVVector(x, y, z);
    }

    // pass in a node that has x y z attributes, returns a point
    public static Point getPoint(Node xyzNode) {
        String posX = XMLHelper.getAttribute(xyzNode, "x");
        String posY = XMLHelper.getAttribute(xyzNode, "y");
        String posZ = XMLHelper.getAttribute(xyzNode, "z");
        int x = (int) Math.round(Double.parseDouble(posX));
        int y = (int) Math.round(Double.parseDouble(posY));
        int z = (int) Math.round(Double.parseDouble(posZ));
        return new Point(x, y, z);
    }

    public static Quaternion getQuaternion(Node quatNode) {
        String x = XMLHelper.getAttribute(quatNode, "x");
        String y = XMLHelper.getAttribute(quatNode, "y");
        String z = XMLHelper.getAttribute(quatNode, "z");
        String w = XMLHelper.getAttribute(quatNode, "w");
        return new Quaternion(
                Float.parseFloat(x),
                Float.parseFloat(y),
                Float.parseFloat(z),
                Float.parseFloat(w));
    }

    protected Instance instance;
    protected String worldFileName;
    protected String worldFileBasePath;
    protected WorldLoaderOverride worldLoaderOverride;

    protected Document worldDoc;
}


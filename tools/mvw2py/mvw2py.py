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

import sys
from xml.dom import Node
from xml.dom.minidom import parse

staticObjects = []
markers = []
markerParticles = []
objectParticles = []
skybox = None
ambientColor = ('1.0', '1.0', '1.0')
dirLightDiffuse = ('1.0', '1.0', '1.0')
dirLightSpecular = ('0.0', '0.0', '0.0')
dirLightDir = ('1.0', '0.0', '0.0')
fogNear = '500000.0'
fogFar = '1000000.0'
fogColor = ('1.0', '1.0', '1.0')
cameraPosition = ('0.0', '0.0', '0.0')
cameraOrientation = ('1.0', '0.0', '0.0', '0.0')
terrain = None
terrainDisplay = None
collectionFiles = []
regions = []

names = {}
uniqueID = 0

def getUnique(base):
    global names
    global uniqueID
    
    name = base
    while name in names:
        name = base + str(uniqueID)
        uniqueID = uniqueID + 1
    names[name] = None
    return name    

def addStaticObject(node):
    name = node.getAttribute('Name')
    name = getUnique(name)
    mesh = node.getAttribute('Mesh')
    
    posNode = node.getElementsByTagName('Position')[0]
    posX = posNode.getAttribute('x')
    posY = posNode.getAttribute('y')
    posZ = posNode.getAttribute('z')

    scaleNode = node.getElementsByTagName('Scale')[0]
    scaleX = scaleNode.getAttribute('x')
    scaleY = scaleNode.getAttribute('y')
    scaleZ = scaleNode.getAttribute('z')
        
    orientNode = node.getElementsByTagName('Orientation')[0]
    orientX = orientNode.getAttribute('x')
    orientY = orientNode.getAttribute('y')
    orientZ = orientNode.getAttribute('z')
    orientW = orientNode.getAttribute('w')

    # collect object info into a tuple
    objInfo = (name, mesh, posX, posY, posZ, orientW, orientX, orientY, orientZ, scaleX, scaleY, scaleZ)
    
    # save object info
    staticObjects.append(objInfo)
    
    # collect any particle effects that are attached to the object
    for childNode in node.getElementsByTagName('ParticleEffect'):
         objectParticles.append((name,
                                 childNode.getAttribute('ParticleEffectName'),
                                 childNode.getAttribute('VelocityScale'),
                                 childNode.getAttribute('ParticleScale'),
                                 childNode.getAttribute('AttachmentPoint')))
    
#
# print the script line for the object
# objInfo is a tuple containing the object data in the following format:
#    (name, mesh, posX, posY, posZ, orientW, orientX, orientY, orientZ, scaleX, scaleY, scaleZ)
#    
def emitStaticObject(objInfo, indent):
    print indent + 'obj = ClientAPI.WorldObject.WorldObject(ClientAPI.GetLocalOID(), \"%s\", \"%s\", ClientAPI.Vector3(%s,%s,%s), False, ClientAPI.Quaternion(%s,%s,%s,%s), ClientAPI.Vector3(%s,%s,%s))' % objInfo
    print indent + 'StaticObjects[\"%s\"] = obj' % objInfo[0]
    
def dictToString(dict):
    s = '{ '
    for key in dict.keys():
        s = s + '\"%s\" : \"%s\",' % (key, dict[key])
    s = s + ' }'
    return s
    
def addMarker(node):
    name = node.getAttribute('Name')
    
    posNode = node.getElementsByTagName('Position')[0]
    posX = posNode.getAttribute('x')
    posY = posNode.getAttribute('y')
    posZ = posNode.getAttribute('z')

    orientNode = node.getElementsByTagName('Orientation')[0]
    orientX = orientNode.getAttribute('x')
    orientY = orientNode.getAttribute('y')
    orientZ = orientNode.getAttribute('z')
    orientW = orientNode.getAttribute('w')

    nvpDict = {}
    for childNode in node.getElementsByTagName('NameValuePair'):
        nvpDict[childNode.getAttribute('Name')] = childNode.getAttribute('Value')
    
    # collect marker info into a tuple
    markerInfo = (name, posX, posY, posZ, orientW, orientX, orientY, orientZ, dictToString(nvpDict))
    
    # save marker info
    markers.append(markerInfo)
    
    # collect any particle effects that are attached to markers
    for childNode in node.getElementsByTagName('ParticleEffect'):
        markerParticles.append((name,
                                childNode.getAttribute('ParticleEffectName'),
                                childNode.getAttribute('VelocityScale'),
                                childNode.getAttribute('ParticleScale')))
                                        
def addRegion(node):
    name = node.getAttribute('Name')
    points = []
    for pointNode in node.getElementsByTagName('Point'):
        posX = pointNode.getAttribute('x')
        posY = pointNode.getAttribute('y')
        posZ = pointNode.getAttribute('z')
        points.append((posX, posY, posZ))
        
    regions.append((name, points))
    
def emitRegion(regionInfo, indent):
    name, points = regionInfo
    regStr = indent + 'Regions[\"%s\"] = [' % name
    for point in points:
        regStr = regStr + 'ClientAPI.Vector3(%s,%s,%s), ' % point
    regStr = regStr + ']'
    print regStr
    
def emitMarker(markerInfo, indent):
    print indent + 'Markers[\"%s\"] = (ClientAPI.Vector3(%s,%s,%s), ClientAPI.Quaternion(%s,%s,%s,%s), %s)' % markerInfo

def emitScript(indent):
    print indent + '#'
    print indent + '# WARNING - this script is auto-generated from a world file.  Edit at your own risk'
    print indent + '#'
    
    print indent + 'import ClientAPI'
    print
    print indent + 'TerrainString = \"%s%s\"' % (terrain, terrainDisplay)
    print
    print indent + 'StaticObjects = {}'
    print
    print indent + 'Markers = {}'
    print
    print indent + 'Regions = {}'
    print
    emitMarkers(indent)
    print
    emitMarkerParticles(indent)
    print
    emitRegions(indent)
    print
    emitObjectParticles(indent)
    print
    
    emitSetupWorld(indent)
    print
    emitSetupCamera(indent)
    print
    emitSetupObjects(indent)

def emitSetupWorld(indent):
    print indent + 'def SetupWorld():'
    
    indent = indent + '    '
    
    emitSkybox(indent)
    emitGlobalFog(indent)
    emitGlobalAmbient(indent)
    emitGlobalDirectional(indent)

def emitSetupCamera(indent):
    print indent + 'def SetupCamera():'
    
    indent = indent + '    '
    
    emitCamera(indent)

def emitSetupObjects(indent):
    print indent + 'def SetupObjects():'
    
    indent = indent + '    '
    
    emitObjects(indent)
        
def emitObjects(indent):
    print indent + '#'
    print indent + '# Static Objects'
    print indent + '#'
    for objInfo in staticObjects:
        emitStaticObject(objInfo, indent)

def emitMarkers(indent):
    print indent + '#'
    print indent + '# Markers'
    print indent + '#'
    for markerInfo in markers:
        emitMarker(markerInfo, indent)

def emitMarkerParticles(indent):        
    print indent + '#'
    print indent + '# Marker attached particle systems'
    print indent + '#'
    print indent + 'markerParticles = ['
    for markerParticle in markerParticles:
        print indent + ' (\"%s\", \"%s\", %s, %s),' % markerParticle
    print indent + ' ]'

def emitRegions(indent):
    print indent + '#'
    print indent + '# Regions'
    print indent + '#'
    for regionInfo in regions:
        emitRegion(regionInfo, indent)
        
def emitObjectParticles(indent):        
    print indent + '#'
    print indent + '# Object attached particle systems'
    print indent + '#'
    print indent + 'objectParticles = ['
    for objectParticle in objectParticles:
        print indent + ' (\"%s\", \"%s\", %s, %s, \"%s\"),' % objectParticle
    print indent + ' ]'
    
def parseCollections(collectionFiles):
    for collectionFile in collectionFiles:
        collectionDom = parse(collectionFile)
        for topNode in collectionDom.documentElement.childNodes:
            if topNode.nodeType == Node.ELEMENT_NODE:
                if topNode.tagName == 'StaticObject':
                    addStaticObject(topNode)
                elif topNode.tagName == 'Waypoint':
                    addMarker(topNode)
                elif topNode.tagName == 'Boundary':
                    addRegion(topNode)

def addGlobalFog(node):
    global fogNear
    global fogFar
    global fogColor
    
    fogNear = node.getAttribute('Near')
    fogFar = node.getAttribute('Far')
    colorNode = node.getElementsByTagName('Color')[0]
    fogColor = (colorNode.getAttribute('R'), colorNode.getAttribute('G'), colorNode.getAttribute('B'))

def emitGlobalFog(indent):
    print indent + '#'
    print indent + '# Global Fog'
    print indent + '#'
    print indent + 'ClientAPI.FogConfig.FogColor = ClientAPI.ColorEx(%s,%s,%s)' % fogColor
    print indent + 'ClientAPI.FogConfig.FogNear = %s' % fogNear
    print indent + 'ClientAPI.FogConfig.FogFar = %s' % fogFar

def addGlobalAmbient(node):
    global ambientColor
    
    colorNode = node.getElementsByTagName('Color')[0]
    ambientColor = (colorNode.getAttribute('R'), colorNode.getAttribute('G'), colorNode.getAttribute('B'))

def emitGlobalAmbient(indent):
    print indent + '#'
    print indent + '# Global Ambient Light'
    print indent + '#'
    print indent + 'ClientAPI.AmbientLight.Color = ClientAPI.ColorEx(%s,%s,%s)' % ambientColor
        
def addGlobalDirectional(node):
    global dirLightDiffuse
    global dirLightSpecular
    global dirLightDir
    
    diffuseNode = node.getElementsByTagName('Diffuse')[0]
    dirLightDiffuse = (diffuseNode.getAttribute('R'), diffuseNode.getAttribute('G'), diffuseNode.getAttribute('B'))
    specularNode = node.getElementsByTagName('Specular')[0]
    dirLightSpecular = (specularNode.getAttribute('R'), specularNode.getAttribute('G'), specularNode.getAttribute('B'))
    dirNode = node.getElementsByTagName('Direction')[0]
    dirLightDir = (dirNode.getAttribute('x'), dirNode.getAttribute('y'), dirNode.getAttribute('z'))

def emitGlobalDirectional(indent):
    print indent + '#'
    print indent + '# Global Directional Light'
    print indent + '#'
    print indent + 'globalDirLight = ClientAPI.Light.Light(\"globalDirLight\")'
    print indent + 'globalDirLight.Type = ClientAPI.Light.LightType.Directional'
    print indent + 'globalDirLight.Direction = ClientAPI.Vector3(%s, %s, %s)' % dirLightDir
    print indent + 'globalDirLight.Diffuse = ClientAPI.ColorEx(%s, %s, %s)' % dirLightDiffuse
    print indent + 'globalDirLight.Specular = ClientAPI.ColorEx(%s, %s, %s)' % dirLightSpecular

def addSkybox(node):
    global skybox
    skybox = node.getAttribute('Name')
    
def emitSkybox(indent):
    if skybox is not None:
        print indent + '#'
        print indent + '# Skybox'
        print indent + '#'
        print indent + 'ClientAPI.World.SetSkyBox(\"%s\")' % skybox
    
def addCameraPosition(node):
    global cameraPosition
    
    cameraPosition = (node.getAttribute('x'), node.getAttribute('y'), node.getAttribute('z'))

def addCameraOrientation(node):
    global cameraOrientation
    
    cameraOrientation = (node.getAttribute('w'), node.getAttribute('x'), node.getAttribute('y'), node.getAttribute('z'))
    
def emitCamera(indent):
    print indent + '#'
    print indent + '# Camera'
    print indent + '#'
    print indent + 'camera = ClientAPI.GetPlayerCamera()'
    print indent + 'camera.Position = ClientAPI.Vector3(%s, %s, %s)' % cameraPosition
    print indent + 'camera.Orientation = ClientAPI.Quaternion(%s, %s, %s, %s)' % cameraOrientation

def addTerrain(node):
    global terrain
    terrain = '<Terrain>'
    for terrainNode in node.childNodes:
        if terrainNode.nodeType == Node.ELEMENT_NODE:
            terrain = terrain + '<%s>%s</%s>' % (terrainNode.tagName, terrainNode.firstChild.nodeValue, terrainNode.tagName)
            
    terrain = terrain + '</Terrain>'
    
def addTerrainDisplay(node):
    global terrainDisplay
    terrainDisplay = '<TerrainDisplay '
    for i in range(node.attributes.length):
        attrNode = node.attributes.item(i)
        terrainDisplay = terrainDisplay + '%s=\\\"%s\\\" ' % (attrNode.name, attrNode.value)
            
    terrainDisplay = terrainDisplay + '/>'

def addCollection(node):
    filename = node.getAttribute('Filename')
    collectionFiles.append(worldDir + filename)
    
def parseWorld(worldFile):
    worldDom = parse(worldFile)
    for topNode in worldDom.documentElement.childNodes:
        if topNode.nodeType == Node.ELEMENT_NODE:
            if topNode.tagName == 'GlobalFog':
                addGlobalFog(topNode)
            elif topNode.tagName == 'GlobalAmbientLight':
                addGlobalAmbient(topNode)
            elif topNode.tagName == 'GlobalDirectionalLight':
                addGlobalDirectional(topNode)
            elif topNode.tagName == 'Skybox':
                addSkybox(topNode)
            elif topNode.tagName == 'CameraPosition':
                addCameraPosition(topNode)
            elif topNode.tagName == 'CameraOrientation':
                addCameraOrientation(topNode)
            elif topNode.tagName == 'Terrain':
                addTerrain(topNode)
            elif topNode.tagName == 'TerrainDisplay':
                addTerrainDisplay(topNode)
            elif topNode.tagName == 'WorldCollection':
                addCollection(topNode)
            else:
                pass
                # print "Error parsing top level world tag: %s" % topNode.tagName
                    
worldFile = sys.argv[1]

# extract directory of world file
baseindex = worldFile.rfind('/')
if baseindex == -1:
    worldDir = ""
else:
    worldDir = worldFile[:baseindex+1]
    
parseWorld(worldFile)
parseCollections(collectionFiles)

emitScript('')


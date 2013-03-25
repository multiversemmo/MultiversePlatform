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
import os
import subprocess
import struct
import shutil
import tempfile

AtlasTool='c:\\Program Files (x86)\\NVIDIA Corporation\\Texture Atlas Tools\\DEMOS\\Direct3D9\\bin\\release\\AtlasCreationTool.exe'
TilePadTool='j:\\tree\\Tools\\TilePad\\TilePad\\bin\\Debug\\TilePad.exe'
DestAtlasDir='j:\\tree\\Media\\friendworld\\Textures'
DestAtlasName='FRW_furn_tile_atlas.dds'
AtlasInfoScript='j:\\tree\\Media\\friendworld\\Scripts\\FurnitureTiles.py'
PreviewImageset='j:\\tree\\Media\\friendworld\\Interface\\Imagesets\\FurniturePatterns.xml'
ImagesetName='FurniturePatterns'
ImagesetImageDir='j:\\tree\\Media\\friendworld\\Imagefiles'
ImagesetImageName='FurniturePatterns.dds'

AtlasTmpDirName=tempfile.mkdtemp()
PreviewTmpDirName=tempfile.mkdtemp()
#AtlasTmpDirName='c:\\cygwin\\home\\jsw\\tmpatlas'
#PreviewTmpDirName='c:\\cygwin\\home\\jsw\\tmppreview'

AtlasInfoFile='atlas.tai'

tileFiles = []

#
# read a dds file and extract the image width and height from it
#
def DDSSize(filename):
    ddsfile = open(filename, 'rb')
    ddsfile.seek(12)
    buf = ddsfile.read(8)
    (height, width) = struct.unpack('<II', buf)
    ddsfile.close()
    return width, height

tileInfoScriptHeader = [
    "import ClientAPI\n",
    "import MarsCommand\n",
    "import ColoredFurniture\n",
    "Tiles = {\n"
]

tileInfoScriptFooter = [
    "}\n",
    "ColoredFurniture.AtlasTexture = AtlasTexture\n",
    "ColoredFurniture.TileMap = Tiles\n"
]

#
# This function extracts the texture coordinates of the atlas tiles
#  and builds a python file containing a dictionary that can be used 
#  by client scripts to address the tiles in the atlas.  It needs to
#  correct for the padding that has been added to the textures.
#
def ConvertAtlasInfo(infoFileName, atlasWidth, atlasHeight):
    # compute the size of a single pixel in texture coordinates
    pixSizeX = 1.0 / atlasWidth
    pixSizeY = 1.0 / atlasHeight
    
    infoFile = open(infoFileName)
    lines = infoFile.readlines()
    
    infoScript = open(AtlasInfoScript, "w")
    infoScript.write('AtlasTexture = \"%s\"\n' % DestAtlasName)
    infoScript.writelines(tileInfoScriptHeader)
    for line in lines:
        # skip blank and comment lines
        if len(line) > 1 and line[0] != '#':
            (origfile, args) = line.split('\t\t')
            (tmp, tmp, tmp, xoffstr, yoffstr, tmp, widthstr, heightstr) = args.split(', ')
            xoff = float(xoffstr)
            yoff = float(yoffstr)
            width = float(widthstr)
            height = float(heightstr)
            # compute size of tile in pixels
            pixW = int(round(width * atlasWidth)) + 1
            pixH = int(round(height * atlasHeight)) + 1
            
            # compute width and height of the tile without the padding
            nopadW = ( pixW / 2 - 1 ) * pixSizeX
            nopadH = ( pixH / 2 - 1 ) * pixSizeY
            
            # compute the x and y offsets of the main tile, within the padding
            nopadX = xoff + pixW * pixSizeX / 4.0
            nopadY = yoff + pixH * pixSizeY / 4.0

            infoScript.write('  \"%s\" : (%f, %f, %f, %f),\n' % (origfile[:-4], nopadX, nopadY, nopadW, nopadH))
            
    infoScript.writelines(tileInfoScriptFooter)
    infoScript.close()
    infoFile.close()

#
# This function extracts the texture coordinates of the atlas tiles
#  and builds an imageset file.
#
def ConvertImageset(infoFileName, atlasWidth, atlasHeight):
    # compute the size of a single pixel in texture coordinates
    pixSizeX = 1.0 / atlasWidth
    pixSizeY = 1.0 / atlasHeight
    
    infoFile = open(infoFileName)
    lines = infoFile.readlines()
    
    imageset = open(PreviewImageset, "w")
    imageset.write('<?xml version=\"1.0\" ?>\n')
    imageset.write('<Imageset Name=\"%s\" Imagefile=\"%s\" NativeHorzRes=\"%d\" NativeVertRes=\"%d\" AutoScaled=\"false\">\n' % (ImagesetName, ImagesetImageName, atlasWidth, atlasHeight) )

    for line in lines:
        # skip blank and comment lines
        if len(line) > 1 and line[0] != '#':
            (origfile, args) = line.split('\t\t')
            (tmp, tmp, tmp, xoffstr, yoffstr, tmp, widthstr, heightstr) = args.split(', ')
            xoff = float(xoffstr)
            yoff = float(yoffstr)
            width = float(widthstr)
            height = float(heightstr)
            # compute size of tile in pixels
            pixW = int(round(width * atlasWidth))
            pixH = int(round(height * atlasHeight))
            
            pixX = int(round(xoff * atlasWidth))
            pixY = int(round(yoff * atlasHeight))
            
            imageset.write('  <Image Name="%s" XPos="%d" YPos="%d" Width="%d" Height="%d" />\n' % (origfile[:-4], pixX, pixY, pixW, pixH))
            
    imageset.write('</Imageset>\n')
    imageset.close()
    infoFile.close()

# pad the tiles and generate preview tiles for use in the client GUI
for i in range(1, len(sys.argv)):
    sourceFile = sys.argv[i]
    filename = os.path.basename(sourceFile)
    tileFiles.append(filename)
	
    # generate names of result files produced by TilePad
    tmpatlas = os.path.join(AtlasTmpDirName, filename)
    tmppreview = os.path.join(PreviewTmpDirName, filename) 
	
    # run the TilePad program, to generate the padded tile and preview tile
    tilePadCmd = [TilePadTool, sourceFile, tmpatlas, tmppreview]
    p = subprocess.Popen(tilePadCmd)
    tilePadResult =  p.wait()
    
# create the pattern atlas
createAtlasCmd = [AtlasTool, '-halftexel', '-width', '2048', '-height', '2048', '-o', 'atlas']
for f in tileFiles:
    createAtlasCmd.append(f)

p = subprocess.Popen(createAtlasCmd, cwd=AtlasTmpDirName)
atlasResult =  p.wait()

# extract the atlas width and height from the generated dds file
(ddswidth, ddsheight) = DDSSize(os.path.join(AtlasTmpDirName, 'atlas0.dds'))

# create the python script that contains the names and texture offsets of the
#  tiles, for use by the scripts that set up materials.
ConvertAtlasInfo(os.path.join(AtlasTmpDirName, AtlasInfoFile), ddswidth, ddsheight)

# Copy the atlas to the asset repository
shutil.copy(os.path.join(AtlasTmpDirName, 'atlas0.dds'), os.path.join(DestAtlasDir, DestAtlasName))

# create the preview atlas atlas for the gui
createPreviewCmd = [AtlasTool, '-nomipmap', '-width', '512', '-height', '512', '-o', 'atlas']
for f in tileFiles:
    createPreviewCmd.append(f)

p = subprocess.Popen(createPreviewCmd, cwd=PreviewTmpDirName)
previewResult =  p.wait()

# extract the preview atlas width and height from the generated dds file
(ddswidth, ddsheight) = DDSSize(os.path.join(PreviewTmpDirName, 'atlas0.dds'))

# create the imageset that contains the names and texture offsets of the
#  tiles, for use by the gui
ConvertImageset(os.path.join(PreviewTmpDirName, AtlasInfoFile), ddswidth, ddsheight)

# Copy the imageset atlas to the asset repository
shutil.copy(os.path.join(PreviewTmpDirName, 'atlas0.dds'), os.path.join(ImagesetImageDir, ImagesetImageName))

# clean up temp directories
shutil.rmtree(AtlasTmpDirName)
shutil.rmtree(PreviewTmpDirName)

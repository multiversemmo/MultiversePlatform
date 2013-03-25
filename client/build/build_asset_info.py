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

import re
import tarfile
import sys

from asset_info import *
from patch_tool import *

class Icon:
    def __init__(self, fileName):
        self.file = fileName

class SptConfig:
    def __init__(self, fileName):
        self.file = fileName

class Imagefile:
    def __init__(self, fileName):
        self.file = fileName

class Imageset:
    def __init__(self, fileName):
        self.file = fileName
        self.imagefiles = []

    def add_imagefile(self, imagefile):
        if imagefile not in self.imagefiles:
            self.imagefiles.append(imagefile)

class UiScript:
    def __init__(self, fileName):
        self.file = fileName

class UiFile:
    def __init__(self, fileName):
        self.file = fileName
        self.imagesets = []
        self.uiscripts = []

    def add_imageset(self, imageset):
        if imageset not in self.imagesets:
            self.imagesets.append(imageset)

    def add_uiscript(self, uiscript):
        if uiscript not in self.uiscripts:
            self.uiscripts.append(uiscript)

class UiModule:
    def __init__(self, fileName):
        self.file = fileName
        self.uifiles = []

    def add_uifile(self, uifile):
        if uifile not in self.uifiles:
            self.uifiles.append(file)


class Font:
    def __init__(self, fileName):
        self.file = fileName

class GpuProgram:
    def __init__(self, fileName):
        self.file = fileName

class Sound:
    def __init__(self, soundName, soundFile):
        self.name = soundName
        self.file = soundFile
      
class SpeedTree:
    def __init__(self, assetName, speedTreeName):
        self.name = assetName
        self.file = speedTreeName
        self.textures = []

    def add_texture(self, texture):
        if texture not in self.textures:
            self.textures.append(texture)
        
class Texture:
    def __init__(self, textureName):
        self.file = textureName

class Material:
    def __init__(self, materialName):
        self.file = materialName
        self.textures = []

    def add_texture(self, texture):
        if texture not in self.textures:
            self.textures.append(texture)

class Skeleton:
    def __init__(self, skeletonName):
        self.file = skeletonName

class Mesh:
    def __init__(self, meshName, meshFile):
        self.name = meshName
        self.file = meshFile
        self.skeleton = None
        
    def set_skeleton(self, skeleton):
        self.skeleton = skeleton

class ResourceRegistry:
    def __init__(self):
        self.meshes = {}
        self.skeletons = {}
        self.materials = {}
        self.textures = {}
        self.speedtrees = {}
        self.sounds = {}
        self.gpuprograms = {}
        self.fonts = {}
        self.uimodules = {}
        self.uifiles = {}
        self.uiscripts = {}
        self.imagesets = {}
        self.imagefiles = {}
        self.icons = {}
        self.sptconfigs = {}
        
    def add_resource(self, obj):
        container = None
        if obj.__class__ is Mesh:
            container = self.meshes
        elif obj.__class__ is Skeleton:
            container = self.skeletons
        elif obj.__class__ is Material:
            container = self.materials
        elif obj.__class__ is Texture:
            container = self.textures
        elif obj.__class__ is SpeedTree:
            container = self.speedtrees
        elif obj.__class__ is Sound:
            container = self.sounds
        elif obj.__class__ is GpuProgram:
            container = self.gpuprograms
        elif obj.__class__ is Font:
            container = self.fonts
        elif obj.__class__ is UiModule:
            container = self.uimodules
        elif obj.__class__ is UiFile:
            container = self.uifiles
        elif obj.__class__ is UiScript:
            container = self.uiscripts
        elif obj.__class__ is Imageset:
            container = self.imagesets
        elif obj.__class__ is Imagefile:
            container = self.imagefiles
        elif obj.__class__ is Icon:
            container = self.icons
        elif obj.__class__ is SptConfig:
            container = self.sptconfigs

        if obj.file not in container.keys():
            container[obj.file] = obj

        return container[obj.file]

    def _get_resource(self, container, name):
        if container.has_key(name):
            return container[name]
        return None
        
    def get_mesh(self, name):
        return self._get_resource(self.meshes, name)
    
    def get_skeleton(self, name):
        return self._get_resource(self.skeletons, name)

    def get_material(self, name):
        return self._get_resource(self.materials, name)

    def get_texture(self, name):
        return self._get_resource(self.textures, name)

    def get_speedtree(self, name):
        return self._get_resource(self.speedtrees, name)

    def get_sound(self, name):
        return self._get_resource(self.sounds, name)

    def get_gpuprogram(self, name):
        return self._get_resource(self.gpuprograms, name)

    def get_font(self, name):
        return self._get_resource(self.fonts, name)

    def get_uimodule(self, name):
        return self._get_resource(self.uimodules, name)

    def get_uifile(self, name):
        return self._get_resource(self.uifiles, name)

    def get_uiscript(self, name):
        return self._get_resource(self.uiscripts, name)

    def get_imageset(self, name):
        return self._get_resource(self.imagesets, name)

    def get_imagefile(self, name):
        return self._get_resource(self.imagefiles, name)

    def get_icon(self, name):
        return self._get_resource(self.icons, name)

    def get_sptconfig(self, name):
        return self._get_resource(self.sptconfigs, name)

    def write_to_tar(self, tar_file):
        media_assets = []
        # add the meshes (models)
        for name in self.meshes.keys():
            dir = "Media/Meshes/"
            media_assets.append(dir + name)

        # add the skeletons
        for name in self.skeletons.keys():
            dir = "Media/Skeletons/"
            media_assets.append(dir + name)

        # add the materials
        for name in self.materials.keys():
            dir = "Media/Materials/"
            media_assets.append(dir + name)

        # add the textures
        for name in self.textures.keys():
            dir = "Media/Textures/"
            media_assets.append(dir + name)

        # add the speedtree tree files
        for name in self.speedtrees.keys():
            dir = "Media/SpeedTree/"
            media_assets.append(dir + name)
    
        # add the sounds
        for name in self.sounds.keys():
            dir = "Media/Sounds/"
            media_assets.append(dir +  name)

        # add the gpu programs
        for name in self.gpuprograms.keys():
            dir = "Media/GpuPrograms/"
            media_assets.append(dir +  name)

        # add the fonts
        for name in self.fonts.keys():
            dir = "Media/Fonts/"
            media_assets.append(dir +  name)

        # add the ui modules (toc)
        for name in self.uimodules.keys():
            dir = "Media/Interface/FrameXML/"
            media_assets.append(dir +  name)

        # add the ui interface files (xml)
        for name in self.uifiles.keys():
            dir = "Media/Interface/FrameXML/"
            media_assets.append(dir +  name)

        # add the ui script files (py)
        for name in self.uiscripts.keys():
            dir = "Media/Interface/FrameXML/"
            media_assets.append(dir +  name)

        # add the imagesets
        for name in self.imagesets.keys():
            dir = "Media/Interface/Imagesets/"
            media_assets.append(dir +  name)

        # add the interface textures
        for name in self.imagefiles.keys():
            dir = "Media/Imagefiles/"
            media_assets.append(dir +  name)

        # add the icons
        for name in self.icons.keys():
            dir = "Media/Icons/"
            media_assets.append(dir +  name)

        # add the speedtree configs
        for name in self.sptconfigs.keys():
            dir = "Media/SpeedTree/"
            media_assets.append(dir +  name)

        self.write_to_tar_file(tar_file, media_assets)

    def write_to_manifest(self, manifest):
        # add the meshes (models)
        media_assets = []
        for name in self.meshes.keys():
            dir = "Media/Meshes/"
            media_assets.append(dir + name)
        self.add_to_manifest(manifest, media_assets)

        # add the skeletons
        media_assets = []
        for name in self.skeletons.keys():
            dir = "Media/Skeletons/"
            media_assets.append(dir + name)
        self.add_to_manifest(manifest, media_assets)

        # add the materials
        media_assets = []
        for name in self.materials.keys():
            dir = "Media/Materials/"
            media_assets.append(dir + name)
        self.add_to_manifest(manifest, media_assets)

        # add the textures
        media_assets = []
        for name in self.textures.keys():
            dir = "Media/Textures/"
            media_assets.append(dir + name)
        self.add_to_manifest(manifest, media_assets)

        # add the speedtree tree files
        media_assets = []
        for name in self.speedtrees.keys():
            dir = "Media/SpeedTree/"
            media_assets.append(dir + name)
        self.add_to_manifest(manifest, media_assets)
    
        # add the sounds
        media_assets = []
        for name in self.sounds.keys():
            dir = "Media/Sounds/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        # add the gpu programs
        media_assets = []
        for name in self.gpuprograms.keys():
            dir = "Media/GpuPrograms/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        # add the fonts
        media_assets = []
        for name in self.fonts.keys():
            dir = "Media/Fonts/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        # add the ui modules (toc)
        media_assets = []
        for name in self.uimodules.keys():
            dir = "Media/Interface/FrameXML/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        # add the ui interface files (xml)
        media_assets = []
        for name in self.uifiles.keys():
            dir = "Media/Interface/FrameXML/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        # add the ui script files (py)
        media_assets = []
        for name in self.uiscripts.keys():
            dir = "Media/Interface/FrameXML/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        # add the imagesets
        media_assets = []
        for name in self.imagesets.keys():
            dir = "Media/Interface/Imagesets/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        # add the interface textures
        media_assets = []
        for name in self.imagefiles.keys():
            dir = "Media/Imagefiles/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        # add the icons
        media_assets = []
        for name in self.icons.keys():
            dir = "Media/Icons/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        # add the speedtree configs
        media_assets = []
        for name in self.sptconfigs.keys():
            dir = "Media/SpeedTree/"
            media_assets.append(dir +  name)
        self.add_to_manifest(manifest, media_assets)

        self.asset_tree.dir_tree["Media"].print_all_entries(sys.stdout)
    

class AutoResourceRegistry(ResourceRegistry):
    def __init__(self, gd_dir):
        ResourceRegistry.__init__(self)
        # directories to find files
        # self.game_dir        = "c:/cygwin/home/mccollum/gd/"
        self.game_dir        = gd_dir
        self.material_prefix = self.game_dir + "Media/Materials/"
        self.imageset_prefix = self.game_dir + "Media/Interface/Imagesets/"
        self.uifile_prefix   = self.game_dir + "Media/Interface/FrameXML/"
        # patterns for matching in files
        self.uifile_pattern = re.compile("^([^\#\s]+)[\#\s]*.*$")
        self.texture_pattern = re.compile("^\s+texture\s+([^\s]+)\s*$")
        self.gpuprogram_pattern = re.compile("^\s+source\s+([^\s]+)\s*$")
        self.cubic_texture_pattern = re.compile("^\s+cubic_texture\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+$")
        self.script_pattern = re.compile("^.*<Script\s+file=\"([^\"]+)\".*$")
        self.interface_pattern = re.compile("^.*file=\"(Interface\\\\[^\"]*)\".*$")
        self.imagefile_pattern = re.compile("^.*Imagefile=\"([^\"]*)\".*$")
        self.asset_tree = AssetTree(self.game_dir)

    def add_resource(self, obj):
        resource_valid = 1
        if obj.__class__ is Material:
            if self.get_material(obj.file) is None:
                self._handle_material(obj)
        elif obj.__class__ is UiModule:
            if self.get_uimodule(obj.file) is None:
                self._handle_uimodule(obj)
        elif obj.__class__ is UiFile:
            if self.get_uifile(obj.file) is None:
                self._handle_uifile(obj)
        elif obj.__class__ is Imageset:
            if self.get_imageset(obj.file) is None:
                resource_valid = self._handle_imageset(obj)
        if resource_valid:
            return ResourceRegistry.add_resource(self, obj)
        return False

    def _handle_material(self, obj):
        texture_files = []
        gpuprogram_files = []
        filename = self.material_prefix + obj.file
        f = file(filename, "r")
        for line in f:
            # check for a normal texture
            tmp = self.texture_pattern.match(line)
            if tmp is not None:
                texture = tmp.group(1)
                if texture not in texture_files:
                    texture_files.append(texture)
            else:
                # check for a cubic_texture
                tmp = self.cubic_texture_pattern.match(line)
                if tmp is not None:
                    texture = tmp.group(1)
                    if texture not in texture_files:
                        texture_files.append(texture)
                    texture = tmp.group(2)
                    if texture not in texture_files:
                        texture_files.append(texture)
                    texture = tmp.group(3)
                    if texture not in texture_files:
                        texture_files.append(texture)
                    texture = tmp.group(4)
                    if texture not in texture_files:
                        texture_files.append(texture)
                    texture = tmp.group(5)
                    if texture not in texture_files:
                        texture_files.append(texture)
                    texture = tmp.group(6)
                    if texture not in texture_files:
                        texture_files.append(texture)
                else:
                    # check for a shader
                    tmp = self.gpuprogram_pattern.match(line)
                    if tmp is not None:
                        gpuprogram = tmp.group(1)
                        if gpuprogram not in gpuprogram_files:
                            gpuprogram_files.append(gpuprogram)
        f.close()
        # textures referenced by materials
        for name in texture_files:
            texture = self.get_texture(name)
            if texture is None:
                texture = self.add_resource(Texture(name))
        for name in gpuprogram_files:
            gpuprogram = self.get_gpuprogram(name)
            if gpuprogram is None:
                gpuprogram = self.add_resource(GpuProgram(name))

    def _handle_uimodule(self, obj):
        # ui layout files referenced by this module
        ui_files = []
        filename = self.uifile_prefix + obj.file
        f = file(filename, "r")
        for line in f:
            tmp = self.uifile_pattern.match(line)
            if tmp is not None:
                ui_file = tmp.group(1)
                if ui_file not in ui_files:
                    ui_files.append(ui_file)
        f.close()

        for name in ui_files:
            uifile = self.get_uifile(name)
            if uifile is None:
                uifile = self.add_resource(UiFile(name))
            obj.add_uifile(uifile)


    def _handle_uifile(self, obj):
        # scripts referenced by ui files
        script_files = []
        # imagesets referenced by ui files
        imageset_files = []
        filename = self.uifile_prefix + obj.file
        
        f = file(filename, "r")
        for line in f:
            tmp = self.script_pattern.match(line)
            if tmp is not None:
                script_file = tmp.group(1)
                # ignore any lua files for now
                if script_file.endswith(".lua"):
                    continue
                if script_file not in script_files:
                    script_files.append(script_file)
            else:
                tmp = self.interface_pattern.match(line)
                if tmp is not None:
                    tex = tmp.group(1)
                    # Ignore any mdx files for now
                    if tex.endswith(".mdx"):
                        continue
                    parts = tex.split("\\")
                    imageset_file = "%s%s" % (parts[1], ".xml")
                    if imageset_file not in imageset_files:
                        imageset_files.append(imageset_file)
        f.close()
        
        for name in script_files:
            uiscript = self.get_uiscript(name)
            if uiscript is None:
                uiscript = self.add_resource(UiScript(name))
            obj.add_uiscript(uiscript)

        for name in imageset_files:
            imageset = self.get_imageset(name)
            if imageset is None:
                imageset = self.add_resource(Imageset(name))
            obj.add_imageset(imageset)

    def _handle_imageset(self, obj):
        # interface textures referenced by imagesets
        interface_files = []
        filename = self.imageset_prefix + obj.file
        try:
            f = file(filename, "r")
        except IOError:
            return False
        for line in f:
            tmp = self.imagefile_pattern.match(line)
            if tmp is not None:
                interface = tmp.group(1)
                if interface not in interface_files:
                    interface_files.append(interface)
        f.close()
        for name in interface_files:
            imagefile = self.get_imagefile(name)
            if imagefile is None:
                imagefile = self.add_resource(Imagefile(name))
            obj.add_imagefile(interface)
        return True
        
    # helper method used by the asset library to add information about a set of
    # files to the manifest file
    def add_to_manifest(self, manifest, media_assets):
        for asset in media_assets:
            # build a tree structure that will match our directory structure
            self.asset_tree.add_asset_path(asset, asset)

    def write_to_tar_file(self, tar_file, media_assets):
        client_dir = self.game_dir
        for asset in media_assets:
            filename = client_dir + asset
            tar_file.add(filename, asset, False)
    
# Write the assets to the manifest file
def make_manifest_file(assets):
    manifest = file("MANIFEST", "wb")
    assets.write_to_manifest(manifest)
    manifest.flush()
    manifest.close()

# Write the assets to the assets.xml file for use by the map tool
def write_to_assets(assets, mesh_files, sound_files, speedtree_files, skyboxes):
    assets_file = file("assets.xml", "wb")
    assets_file.write("<Assets>\n")
    for name in mesh_files:
        mesh = assets.meshes[name]
        assets_file.write("  <Model name=\"%s\" assetName=\"%s\" subType=\"building\"/>\n" % (mesh.name, mesh.file))
    for name in sound_files:
        sound = assets.sounds[name]
        assets_file.write("  <Sound name=\"%s\" />\n" % sound.file)
    for name in speedtree_files:
        tree = assets.speedtrees[name]
        assets_file.write("  <SpeedTree name=\"%s\" assetName=\"%s\" />\n" % (tree.name, tree.file))
    for skyboxEntry in skyboxes:
        assets_file.write("  <Skybox name=\"%s\" assetName=\"%s\" />\n" % (skyboxEntry[0], skyboxEntry[2]))
    assets_file.write("  <SpeedWind name=\"Demo Wind\" assetName=\"demoWind.ini\" />\n")
    assets_file.write("</Assets>\n")
    assets_file.flush()
    assets_file.close()

# Cleanup the string so that it can be used as a wxs id field
def cleanup_string(str):
    pattern = re.compile("[^A-Za-z0-9_\.]+")
    return pattern.sub("_", str)
   

def write_to_wxs(assets):
    assets_file = file("media_assets.wxi", "wb")

    assets_file.write("<Include>\n")
    assets_file.write("          <Directory Id='InterfaceDir' Name='DUMMY' LongName='Interface'>\n")
    assets_file.write("            <Directory Id='FrameXMLDir' Name='DUMMY' LongName='FrameXML'>\n");
    assets_file.write("              <Component Id='InterfaceModules' Guid='18dc8d77-831d-4806-9f53-615dbb427c5c'>\n");
    for name in assets.uimodules.keys():
        assets_file.write("                <File Id='UiModule.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Interface/FrameXML/", name))
    assets_file.write("              </Component>\n")
    assets_file.write("              <Component Id='InterfaceLayouts' Guid='2183b338-bcec-49b9-aaee-4af21ea04c55'>\n");
    for name in assets.uifiles.keys():
        assets_file.write("                <File Id='UiFile.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Interface/FrameXML/", name))
    assets_file.write("              </Component>\n")
    assets_file.write("              <Component Id='InterfaceScripts' Guid='70206c7a-9737-41f1-a555-fdfcffaabd86'>\n")
    for name in assets.uiscripts:
        assets_file.write("                <File Id='Script.%s' DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Interface/FrameXML/", name))
    assets_file.write("              </Component>\n")
    assets_file.write("            </Directory>\n")
    assets_file.write("            <Directory Id='ImagesetDir' Name='DUMMY' LongName='Imagesets'>\n")
    assets_file.write("              <Component Id='InterfaceImagesets' Guid='3b3e1920-75b8-4333-8a46-4ff1e58f9714'>\n")
    for name in assets.imagesets:
        assets_file.write("                <File Id='Imageset.%s' DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Interface/Imagesets/", name))
    assets_file.write("              </Component>\n")
    assets_file.write("            </Directory>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='FontDir' Name='DUMMY' LongName='Fonts'>\n")
    assets_file.write("            <Component Id='Fonts' Guid='b494b490-7069-48e5-8085-cd4a8ce40d5a'>\n")
    for name in assets.fonts.keys():
        assets_file.write("              <File Id='Fonts.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Fonts/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='GpuProgramDir' Name='DUMMY' LongName='GpuPrograms'>\n")
    assets_file.write("            <Component Id='GpuPrograms' Guid='5467d1ba-bf47-4ec3-9c1e-4d201580289e'>\n")
    for name in assets.gpuprograms.keys():
        assets_file.write("              <File Id='GpuPrograms.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/GpuPrograms/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='ImagefileDir' Name='DUMMY' LongName='Imagefiles'>\n")
    assets_file.write("            <Component Id='Imagefiles' Guid='2a1478f6-3ecb-404a-8899-7f144d59718f'>\n")
    for name in assets.imagefiles.keys():
        assets_file.write("              <File Id='Imagefiles.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Imagefiles/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='MaterialDir' Name='DUMMY' LongName='Materials'>\n")
    assets_file.write("            <Component Id='Materials' Guid='13a78c62-77e3-4533-982d-08a6387022b4'>\n")
    for name in assets.materials.keys():
        assets_file.write("              <File Id='Materials.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Materials/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='MeshDir' Name='DUMMY' LongName='Meshes'>\n")
    assets_file.write("            <Component Id='Meshes' Guid='d5907809-b90e-49f5-baf9-acd2fe4976bc'>\n")
    for name in assets.meshes.keys():
        assets_file.write("              <File Id='Meshes.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Meshes/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='SkeletonDir' Name='DUMMY' LongName='Skeletons'>\n")
    assets_file.write("            <Component Id='Skeletons' Guid='2cd61b22-d9cb-4456-86c9-1dc1fd103996'>\n")
    for name in assets.skeletons.keys():
        assets_file.write("              <File Id='Skeletons.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Skeletons/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='SoundDir' Name='DUMMY' LongName='Sounds'>\n")
    assets_file.write("            <Component Id='Sounds' Guid='b29a33f8-7b5a-4426-84d3-96930ec2677e'>\n")
    for name in assets.sounds.keys():
        assets_file.write("              <File Id='Sounds.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Sounds/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='SpeedTreeDir' Name='DUMMY' LongName='SpeedTree'>\n")
    assets_file.write("            <Component Id='SpeedTree' Guid='692dd414-c4ef-4f89-af2b-5e630ab46ccf'>\n")
    for name in assets.speedtrees.keys():
        assets_file.write("              <File Id='SpeedTree.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/SpeedTree/", name))

    for name in assets.sptconfigs.keys():
        assets_file.write("              <File Id='SpeedTree.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/SpeedTree/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='TextureDir' Name='DUMMY' LongName='Textures'>\n")
    assets_file.write("            <Component Id='Textures' Guid='2b3ed4bf-9ebe-49a9-8a89-f73dbb9c1ac8'>\n")
    for name in assets.textures.keys():
        assets_file.write("              <File Id='Textures.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Textures/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")

    assets_file.write("          <Directory Id='IconsDir' Name='DUMMY' LongName='Icons'>\n")
    assets_file.write("            <Component Id='Icons' Guid='1666e6ce-f6dc-4157-bc6a-d6c95f6f5f48'>\n")
    for name in assets.icons.keys():
        assets_file.write("              <File Id='Icons.%s'  DiskId='1' Name='DUMMY' LongName='%s' src='%s%s' />\n" % \
                          (cleanup_string(name), name, "../../Media/Icons/", name))
    assets_file.write("            </Component>\n")
    assets_file.write("          </Directory>\n")


    assets_file.write("</Include>\n")
    
    assets_file.flush()
    assets_file.close()
    


# Write the assets to the tar manifest file
def make_tar_file(assets):
    tar_file = tarfile.open("media.tar", "w")
    assets.write_to_tar(tar_file)
    tar_file.close()

def build_assets(assets):
    all_mesh_files = palma_mesh_files + cogswell_mesh_files + equipment_mesh_files + misc_mesh_files + mob_mesh_files + tool_mesh_files
    for meshEntry in all_mesh_files:
        skeleton = None
        material = None
        mesh = assets.add_resource(Mesh(meshEntry[0], meshEntry[1]))
    
        if meshEntry[2] is not None:
            skeleton = assets.get_skeleton(meshEntry[2])
            if skeleton is None:
                skeleton = assets.add_resource(Skeleton(meshEntry[2]))

        mesh.set_skeleton(skeleton)

    for matEntry in material_files:
        assets.add_resource(Material(matEntry))
                
    for sptEntry in speedtree_files:
        tree = assets.add_resource(SpeedTree(sptEntry[0], sptEntry[1]))
        textures = sptEntry[2]
        if textures is not None:
            for textureName in textures:
                texture = assets.get_texture(textureName)
                if texture is None:
                    texture = assets.add_resource(Texture(textureName))
                tree.add_texture(texture)

    all_sound_files = environment_sound_files + mob_sound_files
    for soundEntry in all_sound_files:
        assets.add_resource(Sound(soundEntry[0], soundEntry[1]))

    for fontEntry in font_files:
        assets.add_resource(Font(fontEntry))

    for uiEntry in ui_module_files:
        assets.add_resource(UiModule(uiEntry))

    # Add the library.py directly
    assets.add_resource(UiScript("Library.py"))
    assets.add_resource(UiScript("StringMap.py"))
    # Add the cursor, dialog frame and tooltip imagesets directly
    assets.add_resource(Imageset("Cursor.xml"))
    assets.add_resource(Imageset("DialogFrame.xml"))
    assets.add_resource(Imageset("Tooltips.xml"))
    # Add the basic interface textures
    assets.add_resource(Texture("WindowsLook3.png"))
    assets.add_resource(Texture("MultiverseImageset.png"))
    assets.add_resource(Texture("loadscreen.jpg"))
    # Add the axiom icon
    assets.add_resource(Icon("AxiomIcon.ico"))
    # Add the speedtree config file
    assets.add_resource(SptConfig("demoWind.ini"))
    

def make_asset_file(assets, skyboxes):
    # get the names of the mesh files we want to include
    placeable_mesh_files = misc_mesh_files + cogswell_mesh_files
    mesh_files = []
    for tmp in placeable_mesh_files:
        if tmp[1] not in mesh_files:
            mesh_files.append(tmp[1])
    for tmp in tool_mesh_files:
        if tmp[1] not in mesh_files:
            mesh_files.append(tmp[1])
    # get the names of the speedtree files we want to include
    speedtree_files = assets.speedtrees.keys()
    # get the names of the sound files we want to include
    sound_files = []
    for tmp in environment_sound_files:
        if tmp[1] not in sound_files:
            sound_files.append(tmp[1])
    # write to the assets.xml file
    write_to_assets(assets, mesh_files, sound_files, speedtree_files, skyboxes)


def make_wxs_file(assets):
    write_to_wxs(assets)

def build_asset_info(gd_dir):
    assets = AutoResourceRegistry(gd_dir)
    # using the set of assets in variables, build the asset registry
    build_assets(assets)
    # make the MANIFEST file
    # make_manifest_file(assets)
    # make the assets.xml file
    make_asset_file(assets, skyboxes)
    # make the media_assets.wxs file
    make_wxs_file(assets)
    # make the tar file inclusion list
    make_tar_file(assets)

build_asset_info("c:/GameDevelopment/")


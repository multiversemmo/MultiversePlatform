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

import shutil
import os
import re

def winpath(str):
    return str.replace("/", "\\")

class model_conversion_tool:
    def __init__(self):
        self.game_dir           = "c:/GameDevelopment/"
        self.maya_dir           = "d:/Program Files/Alias/Maya7.0/bin/"
        self.export_dir         = "d:/Documents and Settings/mccollum/My Documents/maya/projects/default/"
        self.oxc_dir            = self.game_dir + "Tools/ogretools/"
        self.export_script      = self.game_dir + "MultiverseClient/build/export_script.mel"
        self.export_lib         = self.game_dir + "Tools/MayaExport/mel/export.mel"
        self.conversion_dir     = self.game_dir + "Tools/ConversionTool/bin/Debug/"
        self.models_dir         = self.game_dir + "Models/"
        self.media_dir          = self.game_dir + "Media/"
        self.game_dir_win       = winpath(self.game_dir)
        self.maya_dir_win       = winpath(self.maya_dir)
        self.export_script_win  = winpath(self.export_script)
        self.export_dir_win     = winpath(self.export_dir)
        self.media_dir_win      = winpath(self.media_dir)
        
    # Convert a model from the collada format to our binary format
    def convert_model(self, src_dir, media_dir, modelname, anims):
        ## Copy the source rig file to the local area
        shutil.copyfile(src_dir + modelname + ".dae", modelname + ".dae")
        ## Convert the rig file - generates mesh, material and skeleton files
        # ./ConversionTool.exe human_male.dae
        conversion_tool = 'ConversionTool.exe'
        xml_conversion_tool = self.oxc_dir + 'OgreXMLConverter.exe'
        args = [ conversion_tool ]
        args.append(modelname + ".dae")
        print args
        ## Generate the mesh/skeleton/material files for the composite mesh
        os.spawnv(os.P_WAIT, args[0], args)
        
        for anim in anims:
            ## Copy the source file locally
            fname = modelname + "_" + anim + ".dae"
            shutil.copyfile(src_dir + modelname + "_" + anim + ".dae", \
                            modelname + "_" + anim + ".dae")

        ## Now generate the skeleton with all of the animations
        if len(anims) > 0:
            ## Move the rig skeleton aside
            shutil.copyfile(modelname + ".skeleton.xml", modelname + "_rig.skeleton.xml")
            ## Combine the various skeleton files
            #  ./ConversionTool.exe --base_skeleton human_male_rig.skeleton.xml
            #                       --animation run human_male_run.dae
            #                       --out_skeleton human_male.skeleton.xml human_male.mesh.xml
            args = [ conversion_tool ]
            args.append('--base_skeleton')
            args.append(modelname + "_rig.skeleton.xml")
            for anim in anims:
                args.append('--animation')
                args.append(anim)
                args.append(modelname + "_" + anim + ".dae")
            args.append('--out_skeleton')
            args.append(modelname + ".skeleton.xml")
            args.append(modelname + ".mesh.xml")
            print args
            os.spawnv(os.P_WAIT, args[0], args)

        ## Convert the mesh xml to a mesh binary
        # ../../../ogretools/OgreXMLConverter ${MODEL}.mesh.xml
        args = [ xml_conversion_tool, modelname + ".mesh.xml" ]
        os.spawnv(os.P_WAIT, args[0], args)
        ## Copy the mesh into the media tree
        shutil.copyfile(modelname + ".mesh", media_dir + "Meshes/" + modelname + ".mesh")
        # shutil.copyfile(dst_dir + modelname + ".material", media_dir + "Materials/+ " modelname + ".material")
    
        if os.access(modelname + ".skeleton.xml", os.F_OK):
            ## Convert the skeleton xml to a skeleton binary
            args = [ xml_conversion_tool, modelname + ".skeleton.xml" ]
            os.spawnv(os.P_WAIT, args[0], args)
            ## Copy the skeleton into the media tree
            shutil.copyfile(modelname + ".skeleton", media_dir + "Skeletons/" + modelname + ".skeleton")
    
    # Method that actually invokes maya to export the models
    def export_models_helper(self):
        maya = self.maya_dir_win + "maya.exe"
        args = [ maya, '-nosplash', '-script', self.export_script_win ]
        os.spawnv(os.P_WAIT, args[0], args)
    
    # Remove the existing collada files so that we can recreate without a warning
    def remove_existing_models(self, modelinfo):
        for model in modelinfo:
            modelname = model['name']
            for anim in model['animations']:
                fname = '%s%s_%s.dae' % (self.export_dir, modelname, anim)
                if os.access(fname, os.F_OK):
                    os.remove(fname)
            fname = '%s%s.dae' % (self.export_dir, modelname)
            if os.access(fname, os.F_OK):
                os.remove(fname)
    
    # Generate a mel script that will export the models as collada files
    def generate_export_script(self, modelinfo):
        f = file(self.export_script, "wb");
        f.write('source "' + self.export_lib + '";\n')
        
        for model in modelinfo:
            str = '{ '
            first = True
            for anim in model['animations']:
                if first:
                    first = False
                else:
                    str = str + ', '
                str = str + '"' + anim + '"'
            str = str + ' }'
    
            line = 'convertAnimatedModel("%s", "%s%s", "%s", %s);\n' % (self.export_dir, self.models_dir, model['path'], model['name'], str)
            f.write(line)
    
        f.write('quit;\n')
        f.flush()
        f.close()
    
    # Convert the collada file to the binary format
    def convert_models(self, modelinfo):
        os.chdir(self.conversion_dir)
        for model in modelinfo:
            self.convert_model(self.export_dir_win, self.media_dir_win, model['name'], model['animations'])
    
    # Convenience method to limit the number of models exported
    def prune_modelinfo(self, modelinfo, models):
        pruned_modelinfo = []
        for entry in modelinfo:
            if entry['name'] in models:
                pruned_modelinfo.append(entry)
        return pruned_modelinfo
    
    # Crawl the models directory, generating the list of models and animations
    def build_modelinfo(self):
        modelinfo = []
        subdirs = [ "props", "mobs", "items", "buildings" ]
        for subdir in subdirs:
            models_path = self.models_dir + '/' + subdir
            model_dirs = os.listdir(models_path)
            if 'CVS' in model_dirs:
                model_dirs.remove('CVS')
            for model_dir in model_dirs:
                model_path = models_path + '/' + model_dir
                models = os.listdir(model_path)
                if 'CVS' in models:
                    models.remove('CVS')
                base_model = model_dir + ".ma"
                entry = {}
                modelinfo.append(entry)
                entry['name'] = model_dir
                entry['path'] = subdir + '/' + model_dir
                entry['animations'] = [ ]
                pattern = re.compile(model_dir + '_' + '(.*).ma')
                for model in models:
                    match = pattern.match(model)
                    if match:
                        anim = match.group(1)
                        entry['animations'].append(anim)
        return modelinfo
    
    def export_models(self, modelinfo):
        self.generate_export_script(modelinfo)
        self.remove_existing_models(modelinfo)
        self.export_models_helper()

ct = model_conversion_tool()
modelinfo = ct.build_modelinfo()
modelinfo = ct.prune_modelinfo(modelinfo, [ "human_female" ])
ct.export_models(modelinfo)
ct.convert_models(modelinfo)

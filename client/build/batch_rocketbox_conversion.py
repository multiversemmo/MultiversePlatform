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
import os
import re
import shutil

basic_maxscript = """
function export_model_files model_file_in model_file_out bip_files out_dir =
(
  loadMaxFile model_file_in
  model_name = getFilenameFile model_file_out
  out_file = out_dir + model_name + ".dae" as string
  -- export the model without any animation
  exportFile out_file #noPrompt

  for bip_file in bip_files do
  (
    -- reset the animation range
    animationRange = interval 0f 1f
    biped.loadBipFile $'Bip01'.controller bip_file
    animname = getFilenameFile bip_file
    out_file = out_dir + model_name + "_" + animname + ".dae" as string
    -- export the model with this animation
    exportFile out_file #noPrompt
  )
)
%s
quitMax #noprompt
"""

class model_converter:
    def __init__(self):
        self.game_dir           = "c:/GameDevelopment/"
        self.media_dir          = self.game_dir + "Media/"
        self.conversion_dir     = self.game_dir + "Tools/ConversionTool/bin/Debug/"
        self.conversion_tool    = self.conversion_dir + "ConversionTool.exe"

    def convert_model_alt(self, model_name, skel_file):
        # this variant of convert_model takes a skeleton name
        # make sure we are in the conversion dir, since we can only run from there
        os.chdir(self.conversion_dir)
        
        model_file = model_name + '.dae'
        args = [ self.conversion_tool, '--3ds', model_file, '--base_skeleton', skel_file ]

        print args
        os.spawnv(os.P_WAIT, args[0], args)

        ## Copy the mesh and material into the media tree
        shutil.copyfile(model_name + '.mesh', self.media_dir + 'Meshes/' + model_name + '.mesh')
        shutil.copyfile(model_name + '.material', self.media_dir + 'Materials/' + model_name + '.material')

    def convert_model_anims(self, model_name, animations):
        # make sure we are in the conversion dir, since we can only run from there
        os.chdir(self.conversion_dir)

        model_file = model_name + '.dae'
        skel_file = model_name + '.skeleton'

        if os.path.exists(skel_file):
            os.remove(skel_file)

        args = [ self.conversion_tool, '--3ds', model_file ]

        print args
        os.spawnv(os.P_WAIT, args[0], args)

        skel_file = model_name + '.skeleton'
        args = [ self.conversion_tool, '--3ds', '--base_skeleton', skel_file ]

        pattern = re.compile('(male|female)_' + '(.*)' + '_rb')
        for anim in animations:
            match = pattern.match(anim)
            if not match:
                continue
            anim_name = match.group(2)
            args.append("--animation")
            args.append(anim_name)
            args.append(model_name + '_' + anim + '.dae')
    
        args.append('--out_skeleton')
        args.append(skel_file)

        print args
        os.spawnv(os.P_WAIT, args[0], args)

        ## Copy the skeleton and mesh into the media tree
        shutil.copyfile(model_name + '.skeleton', self.media_dir + 'Skeletons/' + model_name + '.skeleton')
        # shutil.copyfile(model_name + '.mesh', self.media_dir + 'Meshes/' + model_name + '.mesh')
        
    def convert_model(self, model_name):
        anims = []
        model_files = os.listdir(self.conversion_dir)
        pattern = re.compile(model_name + '_' + '(.*)' + '.dae')
        for file_name in model_files:
            match = pattern.match(file_name)
            if match:
                anims.append(match.group(1))
        self.convert_model_anims(model_name, anims)

    def convert_model_lod(self, model_name, lod_suffixes, lod_distances):
        model_basename = model_name + lod_suffixes[0]
        model_file = model_basename + '.mesh'
        args = [ self.conversion_tool ]

        for i in range(1, len(lod_suffixes)):
            args.append('--manual_lod')
            args.append(str(lod_distances[i]))
            args.append(model_name + lod_suffixes[i] + '.mesh')
            
        args.append(model_file)

        print args
        os.spawnv(os.P_WAIT, args[0], args)

        ## Copy the mesh into the media tree
        shutil.copyfile(model_basename + '.mesh', self.media_dir + 'Meshes/' + model_basename + '.mesh')

class model_exporter:
    def __init__(self):
        self.max_dir            = "d:/Program Files/Autodesk/3dsmax8/"
        self.max_exe            = self.max_dir + "3dsmax.exe"
        self.game_dir           = "c:/GameDevelopment/"
        self.conversion_dir     = self.game_dir + "Tools/ConversionTool/bin/Debug/"

    def export_model_variant(self, model_dir, model_file, bip_files):
        model_file_in = model_file
        model_file_out = model_file
        if os.path.exists(model_dir + 'rb_' + model_file):
            model_file_in = 'rb_' + model_file
        self.export_model_basic(model_dir, model_file_in, model_file_out, bip_files)

    def export_model_basic(self, model_dir, model_file_in, model_file_out, bip_files):
        print 'model_dir: "%s"' % model_dir
        print 'model_file_in: "%s"' % model_file_in
        print 'model_file_out: "%s"' % model_file_out
        max_command = 'export_model_files "%s" "%s" %s "%s"' % \
                      (model_dir + model_file_in, model_dir + model_file_out,
                       self.get_maxscript_list(bip_files), self.conversion_dir)
        custom_maxscript = basic_maxscript % max_command
        script_file = self.conversion_dir + 'export.ms'
        f = open(script_file, 'w')
        f.write(custom_maxscript)
        f.close()
        args = [ self.max_exe, '-U', 'MAXScript',  script_file ]

        print "Exporting model from %s" % model_dir
        print args
        os.spawnv(os.P_WAIT, args[0], args)

    def get_maxscript_list(self, list):
        rv = '#('
        for i in range(0, len(list)):
            if i == len(list) - 1:
                rv = rv + '"' + list[i] + '"'
            else:
                rv = rv + '"' + list[i] + '", '
        rv = rv + ')'
        return rv

exporter = model_exporter()
converter = model_converter()


model_dirs_1 = [ [ 'c:/GameDevelopment/Assets/Models/mobs/Characters/rbox/',
                   [ 'business01_f', 'business04_f',
                     'casual06_f', 'casual07_f', 'casual08_f', 'casual12_f', 'casual13_f', 'casual15_f', 'casual16_f', 'casual19_f', 'casual21_f', 'casual23_f',
                     'nude01_f', 'nude02_f', 'nude03_f', 'nude04_f',
                     'sportive01_f', 'sportive02_f', 'sportive04_f', 'sportive05_f', 'sportive07_f' ]
                   ]
                 ]

model_animations_1 = [ [ 'c:/GameDevelopment/Assets/Animations/mobs/Characters/rbox/female/',
                         [ 'female_breakdown_01_rb.bip',
                           'female_cheer_rb.bip',
                           'female_claphands_rb.bip',
                           'female_dance_rb.bip',
                           'female_fight_rb.bip',
                           'female_idle_01_rb.bip',
                           'female_idle_02_rb.bip',
                           'female_listen_rb.bip',
                           'female_lookaround_rb.bip',
                           'female_run_rb.bip',
                           'female_shout_rb.bip',
                           'female_sitground_rb.bip',
                           'female_sitidle_rb.bip',
                           'female_strafeleft_rb.bip',
                           'female_straferight_rb.bip',
                           'female_talk_01_rb.bip',
                           'female_talk_02_rb.bip',
                           'female_throw_rb.bip',
                           'female_walk_rb.bip',
                           'female_work_assembly_rb.bip' ]
                         ]
                       ]
model_dirs_2 = [ [ 'c:/GameDevelopment/Assets/Models/mobs/Characters/rbox/',
                   [ 'business02_m', 'business03_m', 'business05_m', 'business06_m',
                     'casual02_m', 'casual03_m', 'casual04_m', 'casual06_m', 'casual07_m', 'casual10_m', 'casual16_m', 'casual21_m',
                     'nude01_m', 'nude02_m', 'nude04_m',
                     'soccerplayer01_m',
                     'sportive01_m', 'sportive05_m', 'sportive09_m' ]
                   ]
                 ]

model_animations_2 = [ [ 'c:/GameDevelopment/Assets/Animations/mobs/Characters/rbox/male/',
                         [ 'male_breakdown_01_rb.bip',
                           'male_cheer_rb.bip',
                           'male_claphands_rb.bip',
                           'male_dance_rb.bip',
                           'male_fight_rb.bip',
                           'male_idle_01_rb.bip',
                           'male_idle_02_rb.bip',
                           'male_listen_rb.bip',
                           'male_lookaround_rb.bip',
                           'male_run_rb.bip',
                           'male_shout_rb.bip',
                           'male_sitground_rb.bip',
                           'male_sitidle_rb.bip',
                           'male_strafeleft_rb.bip',
                           'male_straferight_rb.bip',
                           'male_talk_01_rb.bip',
                           'male_talk_02_rb.bip',
                           'male_throw_rb.bip',
                           'male_walk_rb.bip',
                           'male_work_assembly_rb.bip' ]
                         ]
                       ]
skeleton_templates = { 'rocketbox_f' : [ 'c:/GameDevelopment/Assets/Models/mobs/Characters/rbox/', 'business04_f', model_animations_1 ],
                       'rocketbox_m' : [ 'c:/GameDevelopment/Assets/Models/mobs/Characters/rbox/', 'casual04_m', model_animations_2 ]
                       }

do_export = True

lod_suffixes = [ '_mediumpoly', '_lowpoly' ]

if do_export:
    # Pass to export all animations
    for skeleton_entry in skeleton_templates.values():
        bip_files = []
        for entry in skeleton_entry[2]:
            dir_name = entry[0]
            for file_name in entry[1]:
                bip_files.append(dir_name + file_name)
        for lod_suffix in lod_suffixes:
            exporter.export_model_variant(skeleton_entry[0] + skeleton_entry[1] + '/', skeleton_entry[1] + lod_suffix + '.max', bip_files)

    # Export models
    model_dirs = model_dirs_1 + model_dirs_2
    for entry in model_dirs:
        base_dir_name = entry[0]
        for model_dir in entry[1]:
            full_model_dir = base_dir_name + model_dir + '/'
            for lod_suffix in lod_suffixes:
                model_file =  model_dir + lod_suffix + '.max'
                exporter.export_model_variant(full_model_dir, model_file, [])

do_convert = True

if do_convert:
    # Build the core skeletons
    for skeleton_name in skeleton_templates.keys():
        skeleton_value = skeleton_templates[skeleton_name]
        for lod_suffix in lod_suffixes:
            model_animations = skeleton_value[2]
            model_name = skeleton_value[1]
            animations = []
            for entry in skeleton_value[2]:
                for file_name in entry[1]:
                    animations.append(file_name[:-4])
            converter.convert_model_anims(model_name + lod_suffix, animations)
            model_skel_file = model_name + lod_suffix + '.skeleton'
            skel_file = skeleton_name + lod_suffix + '.skeleton'
            shutil.copyfile(converter.conversion_dir + model_skel_file, converter.conversion_dir + skel_file)

    dist = 0
    lod_step_distance = 10000
    lod_distances = []
    for lod_suffix in lod_suffixes:
        lod_distances.append(dist)
        dist = dist + lod_step_distance
    
    for entry in model_dirs_1:
        base_dir_name = entry[0]
        for model_dir in entry[1]:
            for lod_suffix in lod_suffixes:
                model_file =  model_dir + lod_suffix
                converter.convert_model_alt(model_file, 'rocketbox_f' + lod_suffix + '.skeleton')
            converter.convert_model_lod(model_dir, lod_suffixes, lod_distances);

    for entry in model_dirs_2:
        base_dir_name = entry[0]
        for model_dir in entry[1]:
            for lod_suffix in lod_suffixes:
                model_file =  model_dir + lod_suffix
                converter.convert_model_alt(model_file, 'rocketbox_m' + lod_suffix + '.skeleton')
            converter.convert_model_lod(model_dir, lod_suffixes, lod_distances);

    for skeleton_name in skeleton_templates.keys():
        for lod_suffix in lod_suffixes:
            skel_file = skeleton_name + lod_suffix + '.skeleton'
            shutil.copyfile(converter.conversion_dir + skel_file, converter.media_dir + 'Skeletons/' + skel_file)


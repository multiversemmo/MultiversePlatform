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

class model_converter:
    def __init__(self):
        self.game_dir           = "c:/GameDevelopment/"
        self.media_dir          = self.game_dir + "Media/"
        self.conversion_dir     = self.game_dir + "Tools/ConversionTool/bin/Debug/"
        self.conversion_tool    = self.conversion_dir + "ConversionTool.exe"

    def convert_model(self, model_name):
        model_file = model_name + '.dae'
        args = [ self.conversion_tool, '--3ds', model_file ]

        print args
        os.spawnv(os.P_WAIT, args[0], args)

        skel_file = model_name + '.skeleton'
        mesh_file = model_name + '.mesh'
        args = [ self.conversion_tool, '--3ds', '--base_skeleton', skel_file ]

        anims = []
        model_files = os.listdir(self.conversion_dir)
        pattern = re.compile(model_name + '_' + '(.*)' + '.dae')
        for file_name in model_files:
            match = pattern.match(file_name)
            if match:
                anims.append(match.group(1))

        for anim in anims:
            args.append("--animation")
            args.append(anim)
            args.append(model_name + '_' + anim + '.dae')
    
        args.append('--out_skeleton')
        args.append(skel_file)
        args.append(mesh_file)

        print args
        os.spawnv(os.P_WAIT, args[0], args)

        ## Copy the skeleton and mesh into the media tree
        shutil.copyfile(model_name + '.skeleton', self.media_dir + 'Skeletons/' + model_name + '.skeleton')
        shutil.copyfile(model_name + '.mesh', self.media_dir + 'Meshes/' + model_name + '.mesh')

mc = model_converter()
# mc.convert_model('business01_m_highpoly')
# mc.convert_model('business02_m_highpoly')
mc.convert_model('business01_f_highpoly')
mc.convert_model('business02_f_highpoly')
mc.convert_model('business03_f_highpoly')

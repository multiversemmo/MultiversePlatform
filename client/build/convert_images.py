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

class image_conversion_tool:
    def __init__(self):
        self.game_dir           = "c:/GameDevelopment/"
        # self.dxsdk_dir          = "c:/Documents and Settings/mccollum/Desktop/dxtex/Debug/"
        # self.dxsdk_dir        = "c:/Program Files/Microsoft DirectX 9.0 SDK (October 2005)/Utilities/Bin/x86/"
        self.dxsdk_dir          = "C:/PROGRA~1/MICROS~1.0SD/UTILIT~1/BIN/X86/"
        self.dxsdk_dir_win      = winpath(self.dxsdk_dir)

    def convert_all_images(self):
        self.convert_images(self.game_dir + "Media/Textures/")

    def convert_images(self, image_dir):
        images = os.listdir(image_dir)
        os.chdir(image_dir)
        for suffix in ['.tga', '.bmp', '.jpg', '.png']:
            pattern = re.compile('(.*)' + suffix)
            for image in images:
                match = pattern.match(image)
                if match:
                    basename = match.group(1)
                    self.convert_image(basename + suffix, basename + ".dds")

    def convert_image(self, infile, outfile):
        dxtex = self.dxsdk_dir_win + "DxTex.exe"
        args = [ dxtex, infile, "-m", "DXT5", outfile ]
        os.spawnv(os.P_WAIT, args[0], args)

ct = image_conversion_tool()
ct.convert_all_images()

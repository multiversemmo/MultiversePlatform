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

## This script will run through the media tree, and build a patch file based on the results
import sha
import time

from patch_tool import *

def add_assets(asset_tree):
    asset_tree.add_ignore("mv.patch")
    asset_tree.add_ignore("mv.patch.cur")
    asset_tree.add_ignore("AddOns/Fonts/.*")
    asset_tree.add_ignore("AddOns/GpuPrograms/.*")
    asset_tree.add_ignore("AddOns/Icons/.*")
    asset_tree.add_ignore("AddOns/Imagefiles/.*")
    asset_tree.add_ignore("AddOns/Interface/Imagesets/.*")
    asset_tree.add_ignore("AddOns/Interface/FrameXML/.*")
    asset_tree.add_ignore("AddOns/Materials/.*")
    asset_tree.add_ignore("AddOns/Meshes/.*")
    asset_tree.add_ignore("AddOns/Scripts/.*")
    asset_tree.add_ignore("AddOns/Skeletons/.*")
    asset_tree.add_ignore("AddOns/Sounds/.*")
    asset_tree.add_ignore("AddOns/SpeedTree/.*")
    asset_tree.add_ignore("AddOns/Textures/.*")
    asset_tree.add_ignore("AddOns/Physics/.*")

    asset_tree.add_asset_helper("")
                                      
        

def get_release():
    return "0.8.%d" % time.time()
    
f = file("mv.patch", "w")
asset_tree = AssetTree("c:/Program Files/Multiverse Client/Media/", "")
add_assets(asset_tree)
asset_tree.print_all_entries(f, get_release(), "http://update.multiverse.net/mvupdate.media/")
f.close()

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

## This script will run through the world asset tree, and build a patch file based on the results
import sha
import time
import re
import sha
import tarfile
import zipfile
import os.path
import sys

class AssetTree:
    def __init__(self, clientDir, dstPath):
        # client dir is essentially the source directory
        self.client_dir = clientDir
        self.dst_path = dstPath
        self.dir_tree = {}
        self.manifest_entries = []
        self.ignores = []
        self.ignore_patterns = []
        self.excludes = []
        self.exclude_patterns = []
        self.dir_pattern = re.compile(r'/')

    def make_tar_file(self, tarfile_path):
        tar_file = tarfile.open(tarfile_path, "w")
        self.write_to_tar(tar_file)
        tar_file.close()
        
    def write_to_tar(self, tar_file):
        # add directories as versions of the MultiverseClient directory
        # this means that the permissions will be based on that
        tar_file.add(self.client_dir, self.dst_path, False)
        for entry in self.manifest_entries:
            tar_file.add(self.client_dir + entry.src_path, entry.dst_path, False)
        for subdir_name in self.dir_tree.keys():
            self.dir_tree[subdir_name].write_to_tar(tar_file)

    def make_zip_file(self, zipfile_path):
        zip_file = zipfile.open(zipfile_path, "w")
        self.write_to_zip(zip_file)
        zip_file.close()
        
    def write_to_zip(self, zip_file):
        zip_info = zipfile.ZipInfo(self.dst_path)
        zip_info.external_attr = 16 # flag it as a directory
        zip_file.writestr(zip_info, '')
        for entry in self.manifest_entries:
            zip_file.write(self.client_dir + entry.src_path, entry.dst_path)
        for subdir_name in self.dir_tree.keys():
            self.dir_tree[subdir_name].write_to_zip(zip_file)

    # this should only be called on the root asset tree
    def add_asset_path(self, src_path, dst_path):
        # build a tree structure that will match our directory structure
        path_parts = self.dir_pattern.split(dst_path)
        self.add_asset(path_parts, src_path, dst_path)

    def add_ignore(self, pattern):
        # build a tree structure that will match our directory structure
        self.ignores.append(pattern)
        self.ignore_patterns.append(re.compile(pattern))


    def add_exclude(self, pattern):
        # build a tree structure that will match our directory structure
        self.excludes.append(pattern)
        self.exclude_patterns.append(re.compile(pattern))

    def add_asset(self, pathParts, src_path, dst_path):
        if len(pathParts) == 1:
            if os.path.isdir(self.client_dir + src_path):
                if not self.dir_tree.has_key(pathParts[0]):
                    self.dir_tree[pathParts[0]] = AssetTree(self.client_dir, self.dst_path + pathParts[0] + "/")
            elif os.path.isfile(self.client_dir + src_path):
                entry = ManifestEntry(src_path, dst_path, pathParts[0])
                entry.compute_digest(self.client_dir)
                self.manifest_entries.append(entry)
            else:
                print "Invalid asset entry: " + self.client_dir + src_path
        elif len(pathParts) > 1:
            if not self.dir_tree.has_key(pathParts[0]):
                self.dir_tree[pathParts[0]] = AssetTree(self.client_dir, self.dst_path + pathParts[0] + "/")
            self.dir_tree[pathParts[0]].add_asset(pathParts[1:], src_path, dst_path)

    def add_asset_helper(self, src_dir):
        for entry in os.listdir(self.client_dir + src_dir):
            src_path = src_dir + entry
            ignore = False
            # check to see if we should skip this
            for pattern in self.ignore_patterns:
                match = pattern.match(src_path)
                if match and match.end(0) == len(src_path):
                    ignore = True
                    # ignore this entry, but still recurse
                    break
            for pattern in self.exclude_patterns:
                match = pattern.match(src_path)
                if match and match.end(0) == len(src_path):
                    # ignore this entry, and do not recurse
                    return
            # ok, we don't ignore this..
            if os.path.isdir(self.client_dir + src_path):
                if not ignore:
                    self.add_asset_path(src_path, src_path)
                self.add_asset_helper(src_path + "/")
            elif os.path.isfile(self.client_dir + src_path):
                if not ignore:
                    self.add_asset_path(src_path, src_path)
            else:
                print "Invalid asset entry: " + self.client_dir + src_path
                
    def print_all_entries(self, f, rev, url):
        f.write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n")
        f.write("<patch_data revision=\"%s\" url=\"%s\">\n" % (rev, url))
        for entry in self.excludes:
            f.write("  <exclude pattern=\"%s\"/>\n" % entry)
        for entry in self.ignores:
            f.write("  <ignore pattern=\"%s\"/>\n" % entry)
        self.print_entries(f, "")
        f.write("</patch_data>\n")

    def print_entries(self, f, dir_name):
        for subdir_name in self.dir_tree.keys():
            f.write("  <entry name=\"%s\" kind=\"dir\" />\n" % (dir_name + subdir_name))
        for entry in self.manifest_entries:
            f.write("  <entry name=\"%s%s\"\n" % (dir_name, entry.fileName))
            f.write("         kind=\"file\" sha1_digest=\"%s\" size=\"%d\" />\n" % (entry.sha1Digest, entry.contentLength))
        for subdir_name in self.dir_tree.keys():
            self.dir_tree[subdir_name].print_entries(f, dir_name + subdir_name + "/")

class ManifestEntry:
    def __init__(self, src_path, dst_path, fileName):
        # src_path is the path of the source file relative to the client_dir
        self.src_path = src_path
        # dst_path is the path of the file relative to the install dir
        self.dst_path = dst_path
        # fileName is the last component of the path to which we install
        self.fileName = fileName
        self.sha1Digest = ""
        self.contentLength = 0

    def compute_digest(self, client_dir):
        md = sha.new()
        filename = client_dir + self.src_path
        f = file(filename, "rb")
        f.seek(0, 2)
        size = f.tell()
        f.seek(0, 0)
        while 1:
            data = f.read(4096)
            if len(data) == 0:
                break
            md.update(data)
        f.close()
        self.contentLength = size
        self.sha1Digest = md.hexdigest()


def add_assets(asset_tree):
    # the manifest file (and room for the local copy)
    asset_tree.add_ignore("mv.patch")
    asset_tree.add_ignore("mv.patch.cur")
    # in case they run the script from the assets dir
    asset_tree.add_ignore("build_manifest.py")
    # in case they run from a subversion checked out tree
    asset_tree.add_ignore(".*\.svn/.*")
    asset_tree.add_ignore(".*\.svn")
    # the various files in the addons directories
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

if len(sys.argv) == 1:
    print "Usage: %s [--notar] <path_to_world_assets> [<update_url> [<version_number>]]" % sys.argv[0]
    exit

world_asset_path = ""
update_url = "http://update.multiverse.net/mvupdate.media/"
version = get_release()

make_tar = True
arg_offset = 0
if sys.argv[1] == "--notar":
    make_tar = False
    arg_offset = 1

world_asset_path = sys.argv[1+arg_offset]
if len(sys.argv)-arg_offset >= 3:
    update_url = sys.argv[2+arg_offset]
if len(sys.argv)-arg_offset >= 4:
    version = sys.argv[3+arg_offset]

if not world_asset_path.endswith('/'):
    world_asset_path = world_asset_path + '/'

f = file(world_asset_path + "mv.patch", "w")
asset_tree = AssetTree(world_asset_path, "")
add_assets(asset_tree)
asset_tree.print_all_entries(f, version, update_url)
f.close()

# tar file is named after the folder its assets are located in
split_asset_path = os.path.split(world_asset_path)
if (split_asset_path[1] == ""):
    split_asset_path = os.path.split(split_asset_path[0])
filename = split_asset_path[1]

if make_tar:
    tar_file = tarfile.open(filename + ".tar", "w")
    asset_tree.write_to_tar(tar_file)
    tar_file.add(world_asset_path + "mv.patch", "mv.patch")
    tar_file.close()

#zip_file = zipfile.ZipFile(filename + '.zip', "w")
#asset_tree.write_to_zip(zip_file)
#zip_file.write(world_asset_path + "mv.patch", "mv.patch")
#zip_file.close()

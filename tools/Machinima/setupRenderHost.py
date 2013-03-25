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

#
# This script sets up a render server machine
#
import sys
import os
import subprocess
import shutil
import glob

# NOTE - if you change machinimaPath, you need to update BuildDirs()
machinimaPath = 'c:\\Multiverse\\Machinima\\'
treePath = 'c:\\Multiverse\\tree\\'

toolsToCopy = [ 'processJob.py', 'sendJob.py', 'mencoder.exe', 'machinima.sshkey.ppk', 'pscp.exe', 'MP4Box.exe' ]
toolSourceDir = os.path.join(treePath, 'Tools\\Machinima')
srcMediaDir = os.path.join(treePath, 'Media')

# map our internal world name to the public world name
# used when copying assets from our source tree to the production media area
worldNameMap = {
    'nyts' : 'times_square',
    'friendworld' : 'friendworld2'
}

# requires a full path name including drive spec
# returns a list of directory components
def SplitDirs(path):
    # split off the drive spec
    drive, path = os.path.splitdrive(path)
    
    # remove ending slash if it is present
    if path.endswith('\\'):
        path, empty = os.path.split(path)
    
    pathList = []
    while path is not None:
        path, element = os.path.split(path)
        if len(element) == 0:
            pathList.append(path)
            path = None
        else:
            pathList.append(element)
        
    pathList.append(drive)
    pathList.reverse()
    return pathList

def BuildPath(pathList, numDirs):
    path = os.path.join(pathList[0], pathList[1])
    for i in range(2,numDirs):
        path = os.path.join(path, pathList[i])
    return path

# make sure all the directories in the path exist
def MakePath(fullpath):
    pathList = SplitDirs(fullpath)
    for i in range(3, len(pathList) + 1):
        path = BuildPath(pathList, i)
        if not os.path.exists(path):
            print 'Making %s' % path
            os.mkdir(path)

def MakeDirIfNeeded(path):
    if not os.path.exists(path):
        os.mkdir(path, 0777)

def BuildDirs():    
    MakePath(machinimaPath)
    
    MakeDirIfNeeded(os.path.join(machinimaPath, 'Tools'))
    MakeDirIfNeeded(os.path.join(machinimaPath, 'Media'))
    MakeDirIfNeeded(os.path.join(machinimaPath, 'Jobs'))

def CopyTools():
    destPath = os.path.join(machinimaPath, 'Tools')
    for tool in toolsToCopy:
        srcPath = os.path.join(toolSourceDir, tool)
        shutil.copy(srcPath, destPath)

def CopyTree(src, dst):
  """Because shutil.copytree() fails if a directory already exists"""
  srcLen = len(src)
  for path, dirs, files in os.walk(src):
    if '.svn' in path:
      continue
    dstPath = path[srcLen:]
    for dir in dirs:
      if '.svn' in dir:
        continue
      dstDir = os.path.join(dst, dstPath, dir)
      MakePath(dstDir)
    for file in files:
      srcPath = os.path.join(path, file)
      subPath = srcPath[srcLen:]
      dstPath = os.path.join(dst, subPath)
      shutil.copy2(srcPath, dstPath)

def CopyMedia():
    sceneDirs = glob.glob(os.path.join(srcMediaDir, '*\\Machinima\\*\\'))
    for sceneDir in sceneDirs:
        dirList = SplitDirs(sceneDir)
        numElements = len(dirList)
        scene = dirList[numElements-1]
        world = worldNameMap[dirList[numElements-3]]
        destMediaDir = os.path.join(machinimaPath, 'Media\\')
        destDir = os.path.join(os.path.join(destMediaDir, world), scene)
        CopyTree(sceneDir, destDir)
        
    saMediaSrc = os.path.join(srcMediaDir, 'standalone\\')
    saMediaDest = os.path.join(destMediaDir, 'standalone\\')
    CopyTree(saMediaSrc, saMediaDest)
                
BuildDirs()
CopyTools()
CopyMedia()

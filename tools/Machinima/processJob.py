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
# This script runs a single machinima scene rendering job
#  It takes a single argument, which is the job directory.
#  That directory contains a file called args.xml, which
#  contains the data necessary to run the job.
#
import sys
import os
import subprocess
import time
import win32api
from xml.dom import Node
from xml.dom.minidom import parse

timeStart=time.time()

clientDir = 'c:\\Program Files\\Multiverse Client\\'
treeDir = 'c:\\Multiverse\\tree\\'
treeMediaDir = os.path.join(treeDir, 'Media')
prodDir = 'c:\\Multiverse\\Machinima\\'

clientBinDir = os.path.join(clientDir, 'bin')
toolDir = os.path.join(treeDir, 'Tools\\Machinima')
jobsDir = os.path.join(prodDir, 'Jobs')

# derived paths
encoderPath = os.path.join(toolDir, 'mencoder.exe')
saMediaDir = os.path.join(treeMediaDir, 'standalone')
clientPath = os.path.join(clientBinDir, 'MultiverseClient.exe')
mp4boxPath = os.path.join(toolDir, 'MP4Box.exe')

logConfigPath = os.path.join(toolDir, 'MachinimaLogConfig.xml')
jobConfigPath = None

# scp paths
scpPath = os.path.join(toolDir, 'pscp.exe')
keyFilePath = os.path.join(toolDir, 'machinima.sshkey.ppk')
#scpDest = 'multiverse@facebook.multiverse.net:/mnt/machinima/'
scpDest = 'multiverse@perf7.multiverse.net:/mnt/machinima/'

sceneName = None
worldName = None

renderFPS = 15
encodeBitrate = 3000
indexFrame = 0
soundtrack = None
imageType = 'png'
# assume a video will be made
doEncode = True

repositoryPathsByWorld = {
    'places1' : [ 'friendworld', 'nyts', 'common' ],
    'friendworld2' : [ 'friendworld', 'nyts', 'common' ],
    }
#
# Create necessary subdirectories for the job
#
def buildSubdirs():
    os.mkdir(os.path.join(jobDir, 'Scripts'))
    os.mkdir(os.path.join(jobDir, 'Screenshots'))

#
# Parse a set of properties from the argument xml
#
def parseProps(args, propTag):
    props = []
    propNodes = args.documentElement.getElementsByTagName(propTag)
    for propNode in propNodes:
        name = propNode.getAttribute('Name')
        value = propNode.getAttribute('Value')
        props.append((name, value))
    return props

def createSAScene():
    """Create a scene file, with the ScreenshotPath and any other required properties"""
    global sceneName
    f = open(os.path.join(jobDir, 'Scripts\\SAScene.py'), 'w')
    ssDir = os.path.join(jobDir, 'Screenshots')
    argsXml = os.path.join(jobDir, 'args.xml')
    f.write('import SASceneObjects\n')
    f.write('machinima = SASceneObjects.parseScene(r\"%s\")\n' % argsXml)
    f.write('ScreenshotPath = r\"%s/\"\n' % ssDir)
    
    # write scene properties - right now, we don't have any custom
    # properties, but I want to leave this, in case we add some
    props = None
    f.write('Properties = {')
    if props is not None:
        for kvp in props.items():
            f.write('  \"%s\" : \"%s\",\n' % kvp)
    f.write('}')
    
    f.close()

def createLogConfig():
    global jobConfigPath

    f = open(logConfigPath, "r")
    master = f.read()
    f.close()

    servant = master.replace("_jobID_", jobID)

    jobConfigFile = "JobLogConfig.%s.xml" % jobID
    jobConfigPath = os.path.join(jobDir, jobConfigFile)

    f = open(jobConfigPath, "w")
    f.write(servant)
    f.close()

def parseScene(args):
    global sceneName
    global worldName
    
    sceneNode = args.documentElement
    sceneName = sceneNode.getAttribute('Name')
    worldName = sceneNode.getAttribute('World')

def parseRender(args):
    global renderFPS
    
    renderNode = args.documentElement.getElementsByTagName('Render')[0]
    renderFPS = renderNode.getAttribute('FPS')

def parseEncode(args):
    global encodeBitrate
    global indexFrame
    global soundtrack
    global imageType
    global doEncode
        
    encodeElements = args.documentElement.getElementsByTagName('Encode')
    if len(encodeElements) == 0:
        # no video requested
        doEncode = False
	return

    encodeNode = encodeElements[0]
    encodeBitrate = encodeNode.getAttribute('BitRate')
    if encodeNode.hasAttribute('IndexFrame'):
        indexFrame = int(encodeNode.getAttribute('IndexFrame'))
    if encodeNode.hasAttribute('Soundtrack'):
        soundtrack = encodeNode.getAttribute('Soundtrack')
    if encodeNode.hasAttribute('ImageType'):
        imageType = encodeNode.getAttribute('ImageType')
    else:
        imageType = 'jpg'
    

def buildArgs():
    global worldName
    
    repository_paths = [ jobDir ]
    worldRepositoryPaths = [ worldName ]
    if repositoryPathsByWorld.has_key(worldName):
        worldRepositoryPaths = repositoryPathsByWorld[worldName]
        worldName = worldRepositoryPaths[0] # handle the remapping from friendworld2 to friendworld
    machinimaMediaDir = os.path.join(os.path.join(treeMediaDir, worldName), 'Machinima')
    worldSupplementalMediaDir = os.path.join(machinimaMediaDir, 'PostcardCharacters')
    sceneMediaDir = os.path.join(machinimaMediaDir, sceneName)
    repository_paths = [ jobDir, sceneMediaDir, worldSupplementalMediaDir, saMediaDir ]
    for entry in worldRepositoryPaths:
        repository_paths.append(os.path.join(treeMediaDir, entry))
    args = [clientPath, '--noupdate', '--standalone', '--display_config', '640x480x32', '--fixed_fps', renderFPS, '--log_config', jobConfigPath ]
    for repository_path in repository_paths:
        args.append('--repository_path')
        args.append(repository_path)
    return args

# Delete everything reachable from the directory named in 'top',
# assuming there are no symbolic links.
# CAUTION:  This is dangerous!  For example, if top == '/', it
# could delete all your disk files.    
def removeTree(top):
    for root, dirs, files in os.walk(top, topdown=False):
        for name in files:
            os.remove(os.path.join(root, name))
        for name in dirs:
            os.rmdir(os.path.join(root, name))
    os.rmdir(top)

#
# Poll the process for the specified duration.  If the process runs beyond
# 'timeout' seconds, kill it.
#
# Note that this only works when processJob.py is run with Windows Python.
#
# Returns None if the process was forcibly killed.
# Returns result of subprocess otherwise (0, 1, other).
#
def pollingWait(p, timeout=60):
  result = 0
  # poll for 'timeout' seconds
  while timeout > 0:
    time.sleep(1)
    result = p.poll()
    timeout = timeout - 1
    # if process ends within 'timeout' seconds, stop looping
    if result != None:
      print "process has ended within time limit"
      timeout = 0
  # if process is still running after 'timeout' seconds, kill it
  if p.poll() == None:
    print "process has timed out - killing"
    result = win32api.TerminateProcess(p._handle, 0)
  return result

# only argument is path of job directory
jobID = sys.argv[1]
jobDir = os.path.join(jobsDir, jobID)

# append a directory separator if one is not present
if not jobDir.endswith('\\'):
    jobDir = jobDir + '\\'

# set up sub directories
buildSubdirs()

# load the xml arguments
argsDom = parse(os.path.join(jobDir, 'args.xml'))

# parse the scene section of the xml
parseScene(argsDom)

# parse the render section of the xml
parseRender(argsDom)

# parse the encode section of the xml
parseEncode(argsDom)

# create the SAScene.py script
createSAScene()

# create exception config log
createLogConfig()

# build the client argument list
clientArgs = buildArgs()

print 'Client Args:'
for s in clientArgs:
    print s

# ******* client is started here *******

timeStartRender=time.time()
# run the client to perform the render
print "launching client"
p = subprocess.Popen(clientArgs, cwd=clientBinDir)
status = pollingWait(p)
print "client status =", status

# compute name and path of the output movie file 
movieBaseName = jobID + '.m4v'
movieFileName = os.path.join(jobDir, movieBaseName)
movieAVIName = jobID + '.avi'
movieAVIFileName = os.path.join(jobDir, movieAVIName)

# run the movie encoder
screenshotDir = os.path.join(jobDir, 'Screenshots')
encoderArgs = [ encoderPath, 'mf://*.jpg', '-mf', 'fps=' + str(renderFPS), '-o', movieAVIFileName, '-ovc', 'x264', '-x264encopts', 'bitrate=%s' % encodeBitrate]

# add sound args if a soundtrack is requested
if soundtrack is not None:
    sceneMediaDir = os.path.join(os.path.join(treeMediaDir, worldName), sceneName)
    sceneSoundsDir = os.path.join(sceneMediaDir, 'Sounds')
    fullSoundPath = os.path.join(sceneSoundsDir, soundtrack)
    encoderArgs.extend(['-oac', 'copy', '-audiofile', fullSoundPath])


# ******* encoder is started here *******

timeStartEncode=time.time()
if doEncode:
    print "launching encoder"
    p = subprocess.Popen(encoderArgs, cwd=screenshotDir)
    print "encoder status =", p.wait()
else:
    print "skipping encode (no video requested)"

#
# use MP4Box to convert the file from an avi mux to an m4v mux format
#

# ******* video stream is extracted here *******

timeStartVideo=time.time()
# extract video stream
if doEncode:
    extractVideoArgs = [ mp4boxPath, '-aviraw', 'video', movieAVIName ]
    print "extracting video stream"
    p = subprocess.Popen(extractVideoArgs, cwd=jobDir)
    print "video stream status =", p.wait()
else:
    print "skipping video stream (no video requested)"

# ******* audio stream is extracted here *******

timeStartAudio=time.time()
# extract audio stream
if soundtrack is not None:
    extractAudioArgs = [ mp4boxPath, '-aviraw', 'audio', movieAVIName ]
    print "extracting audio stream"
    p = subprocess.Popen(extractAudioArgs, cwd=jobDir)
    print "audio stream status =", p.wait()
else:
    print "skipping audio stream (none requested)"

# ******* remux is started here *******

timeStartRemux=time.time()
# remux the streams
if doEncode:
    videoStreamFile = jobID + '_video.h264'
    remuxArgs = [ mp4boxPath, '-add', videoStreamFile ]
    if soundtrack is not None:
        audioStreamFile = jobID + '_audio.mp3'
        remuxArgs += [ '-add', audioStreamFile ]
    remuxArgs += [ movieBaseName ]
    print "remuxing the streams"
    p = subprocess.Popen(remuxArgs, cwd=jobDir)
    print "remuxing status =", p.wait()
else:
    print "skipping remux (no video requested)"

timeMoveFile=time.time()
# move the movie file to the job directory
if doEncode:
    resultMoviePath = os.path.join(jobDir, movieBaseName)
    os.rename(movieFileName, resultMoviePath)

# move index image to job directory
indexFrameName = 'screenshot%05d.%s' % (indexFrame, imageType)
resultIndexName = jobID + '.' + imageType
resultIndexPath = os.path.join(jobDir, resultIndexName)
os.rename(os.path.join(screenshotDir, indexFrameName), resultIndexPath)

# copy result files to server
scpArgs = [ scpPath, '-q', '-i', keyFilePath ]
if doEncode:
    scpArgs += [ movieBaseName ]
scpArgs += [ resultIndexName, scpDest ]

print 'Scp Args:'
for s in scpArgs:
    print s

# ******* copy is started here *******

timeCopyFile=time.time()
print "copying files to server"
p = subprocess.Popen(scpArgs, cwd=jobDir)
print "server copy status =", p.wait()

timeCleanup=time.time()
# remove the job directory to clean up
removeTree(jobDir)
timeEnd=time.time()

print "time to configure =",timeStartRender-timeStart
print "time to render =",timeStartEncode-timeStartRender
print "time to encode =",timeStartVideo-timeStartEncode
print "time to stream video =",timeStartAudio-timeStartVideo
print "time to stream audio =",timeStartRemux-timeStartAudio
print "time to remux movie =",timeMoveFile-timeStartRemux
print "time to move file =",timeCopyFile-timeMoveFile
print "time to copy file =",timeCleanup-timeCopyFile
print "time to cleanup =",timeEnd-timeCleanup

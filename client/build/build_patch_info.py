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

import sha
import os
import sys

from patch_tool import *

def add_binary_assets(asset_tree, config):
    asset_tree.add_exclude("Worlds")

    asset_tree.add_ignore("patcher.exe")
    asset_tree.add_ignore("repair.exe")
    asset_tree.add_ignore("mv.patch")
    asset_tree.add_ignore("patcher.log")
    asset_tree.add_ignore(".*MultiverseClient.vshost.exe")
    asset_tree.add_ignore("custom")
    asset_tree.add_ignore("custom/.*")
    asset_tree.add_asset_path("build/patch_version.txt", "patch_version.txt")
#   asset_tree.add_asset_path("bin/Config/world_settings_sample.xml", "Config/world_settings_sample.xml")
    asset_tree.add_asset_path("bin/DefaultLogConfig.xml", "DefaultLogConfig.xml")
    asset_tree.add_asset_path("bin/Html/ClientError.css", "Html/ClientError.css")
    asset_tree.add_asset_path("bin/Html/logo.gif", "Html/logo.gif")
    asset_tree.add_asset_path("bin/Html/bad_media.htm", "Html/bad_media.htm")
    asset_tree.add_asset_path("bin/Html/bad_script.htm", "Html/bad_script.htm")
    asset_tree.add_asset_path("bin/Html/unable_connect_tcp_world.htm", "Html/unable_connect_tcp_world.htm")
    asset_tree.add_asset_path("bin/MultiverseImageset.xml", "MultiverseImageset.xml")
    asset_tree.add_asset_path("bin/logopicture.jpg", "logopicture.jpg")
    asset_tree.add_asset_path("bin/mvloadscreen.bmp", "mvloadscreen.bmp")

#   asset_tree.add_asset_path("bin/%s/libeay32.dll" % config, "bin/libeay32.dll")
#   asset_tree.add_asset_path("bin/%s/ssleay32.dll" % config, "bin/ssleay32.dll")
#   asset_tree.add_asset_path("bin/%s/dwTVC.exe" % config, "bin/dwTVC.exe")

    asset_tree.add_asset_path("bin/Imageset.xsd", "bin/Imageset.xsd")
    asset_tree.add_asset_path("bin/%s/Axiom.Engine.dll" % config, "bin/Axiom.Engine.dll")
    asset_tree.add_asset_path("bin/%s/Axiom.MathLib.dll" % config, "bin/Axiom.MathLib.dll")
    asset_tree.add_asset_path("bin/%s/Axiom.Platforms.Win32.dll" % config, "bin/Axiom.Platforms.Win32.dll")
    asset_tree.add_asset_path("bin/%s/Axiom.Plugins.CgProgramManager.dll" % config, "bin/Axiom.Plugins.CgProgramManager.dll")
    asset_tree.add_asset_path("bin/%s/Axiom.Plugins.ParticleFX.dll" % config, "bin/Axiom.Plugins.ParticleFX.dll")
    asset_tree.add_asset_path("bin/%s/Axiom.RenderSystems.DirectX9.dll" % config, "bin/Axiom.RenderSystems.DirectX9.dll")
#   asset_tree.add_asset_path("bin/%s/Axiom.RenderSystems.OpenGL.dll" % config, "bin/Axiom.RenderSystems.OpenGL.dll")
    asset_tree.add_asset_path("bin/%s/Axiom.SceneManagers.Multiverse.dll" % config, "bin/Axiom.SceneManagers.Multiverse.dll")
#   asset_tree.add_asset_path("bin/%s/Axiom.SceneManagers.Octree.dll" % config, "bin/Axiom.SceneManagers.Octree.dll")
    asset_tree.add_asset_path("bin/%s/HeightfieldGenerator.dll" % config, "bin/HeightfieldGenerator.dll")
    asset_tree.add_asset_path("bin/%s/LogUtil.dll" % config, "bin/LogUtil.dll")
    asset_tree.add_asset_path("bin/%s/TextureFetcher.dll" % config, "bin/TextureFetcher.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.AssetRepository.dll" % config, "bin/Multiverse.AssetRepository.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Base.dll" % config, "bin/Multiverse.Base.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.CollisionLib.dll" % config, "bin/Multiverse.CollisionLib.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Config.dll" % config, "bin/Multiverse.Config.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Generator.dll" % config, "bin/Multiverse.Generator.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Gui.dll" % config, "bin/Multiverse.Gui.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Interface.dll" % config, "bin/Multiverse.Interface.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.MathLib.dll" % config, "bin/Multiverse.MathLib.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Movie.dll" % config, "bin/Multiverse.Movie.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Network.dll" % config, "bin/Multiverse.Network.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Patcher.dll" % config, "bin/Multiverse.Patcher.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Serialization.dll" % config, "bin/Multiverse.Serialization.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Utility.dll" % config, "bin/Multiverse.Utility.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Voice.dll" % config, "bin/Multiverse.Voice.dll")
    asset_tree.add_asset_path("bin/%s/Multiverse.Web.dll" % config, "bin/Multiverse.Web.dll")
    asset_tree.add_asset_path("bin/%s/MultiverseClient.exe" % config, "bin/MultiverseClient.exe")
    asset_tree.add_asset_path("bin/%s/DirectShowWrapper.dll" % config, "bin/DirectShowWrapper.dll")

    # Other projects that are part of the solution
    asset_tree.add_asset_path("../Lib/FMOD/FMODWrapper/bin/%s/FMODWrapper.dll" % config, "bin/FMODWrapper.dll")
    asset_tree.add_asset_path("../Lib/SpeexWrapper/bin/%s/SpeexWrapper.dll" % config, "bin/SpeexWrapper.dll")

    # Axiom dependencies
    asset_tree.add_asset_path("../Axiom/Dependencies/Managed/ICSharpCode.SharpZipLib.dll", "bin/ICSharpCode.SharpZipLib.dll")
    asset_tree.add_asset_path("../Axiom/Dependencies/Managed/Tao.Cg.dll", "bin/Tao.Cg.dll")
    asset_tree.add_asset_path("../Axiom/Dependencies/Managed/Tao.DevIl.dll", "bin/Tao.DevIl.dll")
    asset_tree.add_asset_path("../Axiom/Dependencies/Managed/Tao.Platform.Windows.dll", "bin/Tao.Platform.Windows.dll")
    asset_tree.add_asset_path("../Axiom/Dependencies/Managed/log4net.dll", "bin/log4net.dll")
    asset_tree.add_asset_path("../Axiom/Dependencies/Native/cg.dll", "bin/cg.dll")
    asset_tree.add_asset_path("../Axiom/Dependencies/Native/devil.dll", "bin/devil.dll")
    asset_tree.add_asset_path("../Axiom/Dependencies/Native/ilu.dll", "bin/ilu.dll")
    asset_tree.add_asset_path("../Axiom/Dependencies/Native/ilut.dll", "bin/ilut.dll")

    # MultiverseClient dependencies
    asset_tree.add_asset_path("Dependencies/Managed/IronMath.dll", "bin/IronMath.dll")
    asset_tree.add_asset_path("Dependencies/Managed/IronPython.dll", "bin/IronPython.dll")
    asset_tree.add_asset_path("Dependencies/Native/msvcp71.dll", "bin/msvcp71.dll")
    asset_tree.add_asset_path("Dependencies/Native/msvcr71.dll", "bin/msvcr71.dll")

    # SpeedTree dependencies
    asset_tree.add_asset_path("../Lib/SpeedTree/bin/Release/SpeedTreeWrapper.dll", "bin/SpeedTreeWrapper.dll")
    asset_tree.add_asset_path("../Lib/SpeedTree/bin/Release/SpeedTreeRT.dll", "bin/SpeedTreeRT.dll")

    # FMOD dependencies
    asset_tree.add_asset_path("../Lib/FMOD/fmodex.dll", "bin/fmodex.dll")

    # Speex dependencies
    asset_tree.add_asset_path("../Lib/Speex/bin/libspeex.dll", "bin/libspeex.dll")
    asset_tree.add_asset_path("../Lib/Speex/bin/libspeexdsp.dll", "bin/libspeexdsp.dll")

#   asset_tree.add_asset_path("bin/%s/OggVorbisWrapper.dll" % config, "bin/OggVorbisWrapper.dll")


#   asset_tree.add_asset_path("bin/%s/ogg.dll" % config, "bin/ogg.dll")
#   asset_tree.add_asset_path("bin/%s/vorbis.dll" % config, "bin/vorbis.dll")
#   asset_tree.add_asset_path("bin/%s/vorbisfile.dll" % config, "bin/vorbisfile.dll")
#   asset_tree.add_asset_path("bin/%s/wrap_oal.dll" % config, "bin/wrap_oal.dll")

    asset_tree.add_asset_path("Scripts/Animation.py", "Scripts/Animation.py")
    asset_tree.add_asset_path("Scripts/AnimationState.py", "Scripts/AnimationState.py")
    asset_tree.add_asset_path("Scripts/Camera.py", "Scripts/Camera.py")
    asset_tree.add_asset_path("Scripts/CharacterCreation.py", "Scripts/CharacterCreation.py")
    asset_tree.add_asset_path("Scripts/ClientAPI.py", "Scripts/ClientAPI.py")
    asset_tree.add_asset_path("Scripts/Compositor.py", "Scripts/Compositor.py")
    asset_tree.add_asset_path("Scripts/Decal.py", "Scripts/Decal.py")
    asset_tree.add_asset_path("Scripts/EditableImage.py", "Scripts/EditableImage.py")
    asset_tree.add_asset_path("Scripts/GPUProgramType.py", "Scripts/GPUProgramType.py")
    asset_tree.add_asset_path("Scripts/HardwareCaps.py", "Scripts/HardwareCaps.py")
    asset_tree.add_asset_path("Scripts/Input.py", "Scripts/Input.py")
    asset_tree.add_asset_path("Scripts/Interface.py", "Scripts/Interface.py")
    asset_tree.add_asset_path("Scripts/Light.py", "Scripts/Light.py")
    asset_tree.add_asset_path("Scripts/Material.py", "Scripts/Material.py")
    asset_tree.add_asset_path("Scripts/Model.py", "Scripts/Model.py")
    asset_tree.add_asset_path("Scripts/MorphAnimationTrack.py", "Scripts/MorphAnimationTrack.py")
    asset_tree.add_asset_path("Scripts/MorphKeyFrame.py", "Scripts/MorphKeyFrame.py")
    asset_tree.add_asset_path("Scripts/Network.py", "Scripts/Network.py")
    asset_tree.add_asset_path("Scripts/NodeAnimationTrack.py", "Scripts/NodeAnimationTrack.py")
    asset_tree.add_asset_path("Scripts/NodeKeyFrame.py", "Scripts/NodeKeyFrame.py")
    asset_tree.add_asset_path("Scripts/ParticleSystem.py", "Scripts/ParticleSystem.py")
    asset_tree.add_asset_path("Scripts/Pass.py", "Scripts/Pass.py")
    asset_tree.add_asset_path("Scripts/PropertyAnimationTrack.py", "Scripts/PropertyAnimationTrack.py")
    asset_tree.add_asset_path("Scripts/PropertyKeyFrame.py", "Scripts/PropertyKeyFrame.py")
    asset_tree.add_asset_path("Scripts/SceneNode.py", "Scripts/SceneNode.py")
    asset_tree.add_asset_path("Scripts/SceneQuery.py", "Scripts/SceneQuery.py")
    asset_tree.add_asset_path("Scripts/SoundSource.py", "Scripts/SoundSource.py")
    asset_tree.add_asset_path("Scripts/SystemStatus.py", "Scripts/SystemStatus.py")
    asset_tree.add_asset_path("Scripts/Technique.py", "Scripts/Technique.py")
    asset_tree.add_asset_path("Scripts/TextureUnit.py", "Scripts/TextureUnit.py")
    asset_tree.add_asset_path("Scripts/Voice.py", "Scripts/Voice.py")
    asset_tree.add_asset_path("Scripts/World.py", "Scripts/World.py")
    asset_tree.add_asset_path("Scripts/WorldObject.py", "Scripts/WorldObject.py")

    asset_tree.add_asset_path("build/licenses/Tao.Cg.License.txt", "doc/Tao.Cg.License.txt")
    asset_tree.add_asset_path("build/licenses/Tao.DevIl.License.txt", "doc/Tao.DevIl.License.txt")
#   asset_tree.add_asset_path("build/licenses/Tao.OpenGl.License.txt", "doc/Tao.OpenGl.License.txt")
    asset_tree.add_asset_path("build/licenses/Tao.Platform.Windows.License.txt", "doc/Tao.Platform.Windows.License.txt")
    asset_tree.add_asset_path("build/licenses/ICSharpCode.SharpZipLib.License.txt", "doc/ICSharpCode.SharpZipLib.License.txt")
    asset_tree.add_asset_path("build/licenses/apache2.0.txt", "doc/apache2.0.txt")
    asset_tree.add_asset_path("build/licenses/cpl1.0.txt", "doc/cpl1.0.txt")
    asset_tree.add_asset_path("build/licenses/gpl2.0.txt", "doc/gpl2.0.txt")
    asset_tree.add_asset_path("build/licenses/lgpl2.1.txt", "doc/lgpl2.1.txt")
    asset_tree.add_asset_path("build/licenses/nvidia_license.txt", "doc/nvidia_license.txt")
#   asset_tree.add_asset_path("build/licenses/ogg_license.txt", "doc/ogg_license.txt")
    asset_tree.add_asset_path("build/licenses/third_party_software.txt", "doc/third_party_software.txt")
#   asset_tree.add_asset_path("build/licenses/vorbis_license.txt", "doc/vorbis_license.txt")

# Defaults
dir_win = "c:/cygwin/home/multiverse/svn_tree/trunk/MultiverseClient/"
dest_url = "http://update.multiverse.net/mvupdate.client/"
patch_file = "client_patch.tar"
config = "Debug"

for arg in sys.argv:
    if arg.startswith('--dir='):
        dir_win = arg.split('=')[1]
    elif arg.startswith('--dest_url='):
        dest_url = arg.split('=')[1]
    elif arg.startswith('--patch_file='):
        patch_file = arg.split('=')[1]
    elif arg.startswith('--config='):
        config = arg.split('=')[1]
    elif arg == '--help':
        print '%s: [--dir=<source_dir>] [--dest_url=<update_url>] [--patch_file=<patch_file>] [--config=<configuration>]' % sys.argv[0]
        sys.exit()

# Get the version
patcher = "%sPatcher/bin/%s/patcher.exe" % (dir_win, config)
# Default value of version, in case we can't run the patcher
version = "1.1.2920.33098"
try:
    output = os.popen(patcher + " --version").read()
    version = output.strip()
except:
    pass

# Build the patch_version.txt (for use by the patcher)
f = file("patch_version.txt", "w")
f.write(version + "\n")
f.close()

# Build the mv.patch manifest
f = file("mv.patch", "w")
asset_tree = AssetTree(dir_win, "")
add_binary_assets(asset_tree, config)
asset_tree.print_all_entries(f, version, dest_url)
f.close()

# Build the patch archive (which should be expanded on the update server)
# tar_file = tarfile.open(patch_file, "w:gz")
tar_file = tarfile.open(patch_file, "w")
asset_tree.write_to_tar(tar_file)
tar_file.add("mv.patch")
#tar_file.add("patch_version.txt")
tar_file.add(patcher, "patcher.exe")
tar_file.close()


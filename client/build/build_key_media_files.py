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
import time

from patch_tool import *

def add_assets(asset_tree):
    asset_tree.add_ignore("mv.patch")
    asset_tree.add_ignore("mv.patch.cur")

    # Directories for main assets
    asset_tree.add_asset_path("Fonts", "Fonts")
    asset_tree.add_asset_path("GpuPrograms", "GpuPrograms")
    asset_tree.add_asset_path("Icons", "Icons")
    asset_tree.add_asset_path("Imagefiles", "Imagefiles")
    asset_tree.add_asset_path("Interface/Imagesets", "Interface/Imagesets")
    asset_tree.add_asset_path("Interface/FrameXML", "Interface/FrameXML")
    asset_tree.add_asset_path("Scripts", "Scripts")
    asset_tree.add_asset_path("Textures", "Textures")
    asset_tree.add_asset_path("Meshes", "Meshes")
    asset_tree.add_asset_path("Skeletons", "Skeletons")
    asset_tree.add_asset_path("Materials", "Materials")
    asset_tree.add_asset_path("SpeedTree", "SpeedTree")
    asset_tree.add_asset_path("Sounds", "Sounds")
    asset_tree.add_asset_path("Physics", "Physics")
    asset_tree.add_asset_path("Particles", "Particles")

    # Directories for addon (local) assets
    asset_tree.add_asset_path("AddOns/Fonts", "AddOns/Fonts")
    asset_tree.add_asset_path("AddOns/GpuPrograms", "AddOns/GpuPrograms")
    asset_tree.add_asset_path("AddOns/Icons", "AddOns/Icons")
    asset_tree.add_asset_path("AddOns/Imagefiles", "AddOns/Imagefiles")
    asset_tree.add_asset_path("AddOns/Interface/Imagesets", "AddOns/Interface/Imagesets")
    asset_tree.add_asset_path("AddOns/Interface/FrameXML", "AddOns/Interface/FrameXML")
    asset_tree.add_asset_path("AddOns/Scripts", "AddOns/Scripts")
    asset_tree.add_asset_path("AddOns/Textures", "AddOns/Textures")
    asset_tree.add_asset_path("AddOns/Meshes", "AddOns/Meshes")
    asset_tree.add_asset_path("AddOns/Skeletons", "AddOns/Skeletons")
    asset_tree.add_asset_path("AddOns/Materials", "AddOns/Materials")
    asset_tree.add_asset_path("AddOns/SpeedTree", "AddOns/SpeedTree")
    asset_tree.add_asset_path("AddOns/Sounds", "AddOns/Sounds")
    asset_tree.add_asset_path("AddOns/Physics", "AddOns/Physics")
    asset_tree.add_asset_path("AddOns/Particles", "AddOns/Particles")
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

    asset_tree.add_asset_path("Scripts/Standalone.py", "Scripts/Standalone.py")

    # client gpu programs
    asset_tree.add_asset_path("GpuPrograms/Multiverse.program", "GpuPrograms/Multiverse.program")
    asset_tree.add_asset_path("GpuPrograms/DiffuseBump.cg", "GpuPrograms/DiffuseBump.cg")
    asset_tree.add_asset_path("GpuPrograms/DetailVeg.cg", "GpuPrograms/DetailVeg.cg")
    asset_tree.add_asset_path("GpuPrograms/Compound_Basic_Skinned.cg", "GpuPrograms/Compound_Basic_Skinned.cg")
    # common to the tools package
    asset_tree.add_asset_path("GpuPrograms/Trees.cg", "GpuPrograms/Trees.cg")
    asset_tree.add_asset_path("GpuPrograms/Ocean.cg", "GpuPrograms/Ocean.cg")
    asset_tree.add_asset_path("GpuPrograms/Terrain.cg", "GpuPrograms/Terrain.cg")
    # asset_tree.add_asset_path("GpuPrograms/HMETerrain.cg", "GpuPrograms/HMETerrain.cg")
    # end common

    # physics meshes
    asset_tree.add_asset_path("Meshes/unit_box.mesh", "Meshes/unit_box.mesh")
    asset_tree.add_asset_path("Meshes/unit_sphere.mesh", "Meshes/unit_sphere.mesh")
    asset_tree.add_asset_path("Meshes/unit_cylinder.mesh", "Meshes/unit_cylinder.mesh")
    # end physics

    # tool mesh
    # asset_tree.add_asset_path("Meshes/directional_marker.mesh", "Meshes/directional_marker.mesh")
    # end tool

    # key materials
    asset_tree.add_asset_path("Materials/DetailVeg.material", "Materials/DetailVeg.material")
    # asset_tree.add_asset_path("Materials/directional_marker.material", "Materials/directional_marker.material")
    asset_tree.add_asset_path("Materials/Multiverse.material", "Materials/Multiverse.material")
    asset_tree.add_asset_path("Materials/MVSMTerrain.material", "Materials/MVSMTerrain.material")
    asset_tree.add_asset_path("Materials/Ocean.material", "Materials/Ocean.material")
    asset_tree.add_asset_path("Materials/Trees.material", "Materials/Trees.material")
    asset_tree.add_asset_path("Materials/Water.material", "Materials/Water.material")
    # end key materials
    # physics
    asset_tree.add_asset_path("Materials/unit_box.material", "Materials/unit_box.material")
    asset_tree.add_asset_path("Materials/unit_sphere.material", "Materials/unit_sphere.material")
    asset_tree.add_asset_path("Materials/unit_cylinder.material", "Materials/unit_cylinder.material")
    # end physics

    # key textures
    asset_tree.add_asset_path("Textures/MultiverseImageset.png", "Textures/MultiverseImageset.png")
    asset_tree.add_asset_path("Textures/loadscreen.jpg", "Textures/loadscreen.jpg")
    # imageset for the DetailVegAtlas
    asset_tree.add_asset_path("Textures/DetailVeg.imageset", "Textures/DetailVeg.imageset")

    # asset_tree.add_asset_path("Textures/crosshatch-thin.dds", "Textures/crosshatch-thin.dds")
    # asset_tree.add_asset_path("Textures/directional_marker.dds", "Textures/directional_marker.dds")
    # asset_tree.add_asset_path("Textures/directional_marker_greyscale.dds", "Textures/directional_marker_greyscale.dds")
    # asset_tree.add_asset_path("Textures/directional_marker_orange.dds", "Textures/directional_marker_orange.dds")
    # asset_tree.add_asset_path("Textures/directional_marker_red.dds", "Textures/directional_marker_red.dds")
    # asset_tree.add_asset_path("Textures/directional_marker_yellow.dds", "Textures/directional_marker_yellow.dds")

    asset_tree.add_asset_path("Textures/noon_j_back.dds", "Textures/noon_j_back.dds")
    asset_tree.add_asset_path("Textures/noon_j_front.dds", "Textures/noon_j_front.dds")
    asset_tree.add_asset_path("Textures/noon_j_left.dds", "Textures/noon_j_left.dds")
    asset_tree.add_asset_path("Textures/noon_j_right.dds", "Textures/noon_j_right.dds")
    asset_tree.add_asset_path("Textures/noon_j_top.dds", "Textures/noon_j_top.dds")

    asset_tree.add_asset_path("Textures/mv_skybox0001.dds", "Textures/mv_skybox0001.dds")
    asset_tree.add_asset_path("Textures/mv_skybox0002.dds", "Textures/mv_skybox0002.dds")
    asset_tree.add_asset_path("Textures/mv_skybox0003.dds", "Textures/mv_skybox0003.dds")
    asset_tree.add_asset_path("Textures/mv_skybox0004.dds", "Textures/mv_skybox0004.dds")
    asset_tree.add_asset_path("Textures/mv_skybox0005.dds", "Textures/mv_skybox0005.dds")
    asset_tree.add_asset_path("Textures/mv_skybox0006.dds", "Textures/mv_skybox0006.dds")

    asset_tree.add_asset_path("Textures/splatting_grass.dds", "Textures/splatting_grass.dds")
    asset_tree.add_asset_path("Textures/splatting_rock.dds", "Textures/splatting_rock.dds")
    asset_tree.add_asset_path("Textures/splatting_sand.dds", "Textures/splatting_sand.dds")
    asset_tree.add_asset_path("Textures/splatting_snow.dds", "Textures/splatting_snow.dds")
    asset_tree.add_asset_path("Textures/sandy_path.dds", "Textures/sandy_path.dds")

    asset_tree.add_asset_path("Textures/DetailVegAtlas.dds", "Textures/DetailVegAtlas.dds")
    asset_tree.add_asset_path("Textures/WindowsLook3.png", "Textures/WindowsLook3.png")

    asset_tree.add_asset_path("Textures/Water02.dds", "Textures/Water02.dds")
    asset_tree.add_asset_path("Textures/waves.dds", "Textures/waves.dds")
    asset_tree.add_asset_path("Textures/White.dds", "Textures/White.dds")

    #end key textures

    # SpeedTree textures
    # common to the tools package
    asset_tree.add_asset_path("Textures/AmericanBoxwood_Composite.dds", "Textures/AmericanBoxwood_Composite.dds")
    asset_tree.add_asset_path("Textures/AmericanBoxwoodCluster_Composite.dds", "Textures/AmericanBoxwoodCluster_Composite.dds")
    asset_tree.add_asset_path("Textures/AppleTree_SelfShadow.dds", "Textures/AppleTree_SelfShadow.dds")
    asset_tree.add_asset_path("Textures/AppleTreeBark.dds", "Textures/AppleTreeBark.dds")
    asset_tree.add_asset_path("Textures/AppleTreeBarkNormals.dds", "Textures/AppleTreeBarkNormals.dds")
    asset_tree.add_asset_path("Textures/Azalea_Composite.dds", "Textures/Azalea_Composite.dds")
    asset_tree.add_asset_path("Textures/AzaleaPatch_Composite.dds", "Textures/AzaleaPatch_Composite.dds")
    asset_tree.add_asset_path("Textures/AzaleaPatchPink_Composite.dds", "Textures/AzaleaPatchPink_Composite.dds")
    asset_tree.add_asset_path("Textures/AzaleaPink_Composite.dds", "Textures/AzaleaPink_Composite.dds")
    asset_tree.add_asset_path("Textures/Beech_Composite.dds", "Textures/Beech_Composite.dds")
    asset_tree.add_asset_path("Textures/Beech_SelfShadow.dds", "Textures/Beech_SelfShadow.dds")
    asset_tree.add_asset_path("Textures/BeechBark.dds", "Textures/BeechBark.dds")
    asset_tree.add_asset_path("Textures/BeechBarkNormals.dds", "Textures/BeechBarkNormals.dds")
    asset_tree.add_asset_path("Textures/BeechFall_Composite.dds", "Textures/BeechFall_Composite.dds")
    asset_tree.add_asset_path("Textures/BeechWinter_Composite.dds", "Textures/BeechWinter_Composite.dds")
    asset_tree.add_asset_path("Textures/CurlyPalm_Composite.dds", "Textures/CurlyPalm_Composite.dds")
    asset_tree.add_asset_path("Textures/CurlyPalm_SelfShadow.dds", "Textures/CurlyPalm_SelfShadow.dds")
    asset_tree.add_asset_path("Textures/CurlyPalmBark.dds", "Textures/CurlyPalmBark.dds")
    asset_tree.add_asset_path("Textures/CurlyPalmBarkNormals.dds", "Textures/CurlyPalmBarkNormals.dds")
    asset_tree.add_asset_path("Textures/CurlyPalmCluster_Composite.dds", "Textures/CurlyPalmCluster_Composite.dds")
    asset_tree.add_asset_path("Textures/FraserFir_Composite.dds", "Textures/FraserFir_Composite.dds")
    asset_tree.add_asset_path("Textures/FraserFir_SelfShadow.dds", "Textures/FraserFir_SelfShadow.dds")
    asset_tree.add_asset_path("Textures/FraserFirBark.dds", "Textures/FraserFirBark.dds")
    asset_tree.add_asset_path("Textures/FraserFirBarkNormals.dds", "Textures/FraserFirBarkNormals.dds")
    asset_tree.add_asset_path("Textures/FraserFirCluster_Composite.dds", "Textures/FraserFirCluster_Composite.dds")
    asset_tree.add_asset_path("Textures/FraserFirCluster_SelfShadow.dds", "Textures/FraserFirCluster_SelfShadow.dds")
    asset_tree.add_asset_path("Textures/FraserFirClusterSnow_Composite.dds", "Textures/FraserFirClusterSnow_Composite.dds")
    asset_tree.add_asset_path("Textures/FraserFirSnow_Composite.dds", "Textures/FraserFirSnow_Composite.dds")
    asset_tree.add_asset_path("Textures/RDApple_Composite.dds", "Textures/RDApple_Composite.dds")
    asset_tree.add_asset_path("Textures/RDAppleApples_Composite.dds", "Textures/RDAppleApples_Composite.dds")
    asset_tree.add_asset_path("Textures/RDAppleSpring_Composite.dds", "Textures/RDAppleSpring_Composite.dds")
    asset_tree.add_asset_path("Textures/RDAppleWinter_Composite.dds", "Textures/RDAppleWinter_Composite.dds")
    asset_tree.add_asset_path("Textures/SugarPine_Composite.dds", "Textures/SugarPine_Composite.dds")
    asset_tree.add_asset_path("Textures/SugarPine_SelfShadow.dds", "Textures/SugarPine_SelfShadow.dds")
    asset_tree.add_asset_path("Textures/SugarPineBark.dds", "Textures/SugarPineBark.dds")
    asset_tree.add_asset_path("Textures/SugarPineBarkNormals.dds", "Textures/SugarPineBarkNormals.dds")
    asset_tree.add_asset_path("Textures/SugarPineWinter_Composite.dds", "Textures/SugarPineWinter_Composite.dds")
    asset_tree.add_asset_path("Textures/UmbrellaThorn_Composite.dds", "Textures/UmbrellaThorn_Composite.dds")
    asset_tree.add_asset_path("Textures/UmbrellaThorn_SelfShadow.dds", "Textures/UmbrellaThorn_SelfShadow.dds")
    asset_tree.add_asset_path("Textures/UmbrellaThornBark.dds", "Textures/UmbrellaThornBark.dds")
    asset_tree.add_asset_path("Textures/UmbrellaThornBarkNormals.dds", "Textures/UmbrellaThornBarkNormals.dds")
    asset_tree.add_asset_path("Textures/UmbrellaThornDead_Composite.dds", "Textures/UmbrellaThornDead_Composite.dds")
    asset_tree.add_asset_path("Textures/UmbrellaThornFlowers_Composite.dds", "Textures/UmbrellaThornFlowers_Composite.dds")
    asset_tree.add_asset_path("Textures/VenusTree_Composite.dds", "Textures/VenusTree_Composite.dds")
    asset_tree.add_asset_path("Textures/VenusTree_SelfShadow.dds", "Textures/VenusTree_SelfShadow.dds")
    asset_tree.add_asset_path("Textures/VenusTreeBark.dds", "Textures/VenusTreeBark.dds")
    asset_tree.add_asset_path("Textures/VenusTreeBarkNormals.dds", "Textures/VenusTreeBarkNormals.dds")
    asset_tree.add_asset_path("Textures/WeepingWillow_Composite.dds", "Textures/WeepingWillow_Composite.dds")
    asset_tree.add_asset_path("Textures/WeepingWillow_SelfShadow.dds", "Textures/WeepingWillow_SelfShadow.dds")
    asset_tree.add_asset_path("Textures/WeepingWillowBark.dds", "Textures/WeepingWillowBark.dds")
    asset_tree.add_asset_path("Textures/WeepingWillowBarkNormals.dds", "Textures/WeepingWillowBarkNormals.dds")
    asset_tree.add_asset_path("Textures/WeepingWillowFall_Composite.dds", "Textures/WeepingWillowFall_Composite.dds")
    asset_tree.add_asset_path("Textures/WeepingWillowWinter_Composite.dds", "Textures/WeepingWillowWinter_Composite.dds")
    # end common
    # end speedtree textures
    
    # SpeedTree trees
    # common to the tools package
    asset_tree.add_asset_path("SpeedTree/AmericanBoxwood_RT.spt", "SpeedTree/AmericanBoxwood_RT.spt")
    asset_tree.add_asset_path("SpeedTree/AmericanBoxwoodCluster_RT.spt", "SpeedTree/AmericanBoxwoodCluster_RT.spt")
    asset_tree.add_asset_path("SpeedTree/Azalea_RT.spt", "SpeedTree/Azalea_RT.spt")
    asset_tree.add_asset_path("SpeedTree/Azalea_RT_Pink.spt", "SpeedTree/Azalea_RT_Pink.spt")
    asset_tree.add_asset_path("SpeedTree/AzaleaPatch_RT.spt", "SpeedTree/AzaleaPatch_RT.spt")
    asset_tree.add_asset_path("SpeedTree/AzaleaPatch_RT_Pink.spt", "SpeedTree/AzaleaPatch_RT_Pink.spt")
    asset_tree.add_asset_path("SpeedTree/Beech_RT.spt", "SpeedTree/Beech_RT.spt")
    asset_tree.add_asset_path("SpeedTree/Beech_RT_Fall.spt", "SpeedTree/Beech_RT_Fall.spt")
    asset_tree.add_asset_path("SpeedTree/Beech_RT_Winter.spt", "SpeedTree/Beech_RT_Winter.spt")
    asset_tree.add_asset_path("SpeedTree/CurlyPalm_RT.spt", "SpeedTree/CurlyPalm_RT.spt")
    asset_tree.add_asset_path("SpeedTree/CurlyPalmCluster_RT.spt", "SpeedTree/CurlyPalmCluster_RT.spt")
    asset_tree.add_asset_path("SpeedTree/FraserFir_RT.spt", "SpeedTree/FraserFir_RT.spt")
    asset_tree.add_asset_path("SpeedTree/FraserFir_RT_Snow.spt", "SpeedTree/FraserFir_RT_Snow.spt")
    asset_tree.add_asset_path("SpeedTree/FraserFirCluster_RT.spt", "SpeedTree/FraserFirCluster_RT.spt")
    asset_tree.add_asset_path("SpeedTree/FraserFirCluster_RT_Snow.spt", "SpeedTree/FraserFirCluster_RT_Snow.spt")
    asset_tree.add_asset_path("SpeedTree/RDApple_RT.spt", "SpeedTree/RDApple_RT.spt")
    asset_tree.add_asset_path("SpeedTree/RDApple_RT_Apples.spt", "SpeedTree/RDApple_RT_Apples.spt")
    asset_tree.add_asset_path("SpeedTree/RDApple_RT_Spring.spt", "SpeedTree/RDApple_RT_Spring.spt")
    asset_tree.add_asset_path("SpeedTree/RDApple_RT_Winter.spt", "SpeedTree/RDApple_RT_Winter.spt")
    asset_tree.add_asset_path("SpeedTree/SugarPine_RT.spt", "SpeedTree/SugarPine_RT.spt")
    asset_tree.add_asset_path("SpeedTree/SugarPine_RT_Winter.spt", "SpeedTree/SugarPine_RT_Winter.spt")
    asset_tree.add_asset_path("SpeedTree/UmbrellaThorn_RT.spt", "SpeedTree/UmbrellaThorn_RT.spt")
    asset_tree.add_asset_path("SpeedTree/UmbrellaThorn_RT_Dead.spt", "SpeedTree/UmbrellaThorn_RT_Dead.spt")
    asset_tree.add_asset_path("SpeedTree/UmbrellaThorn_RT_Flowers.spt", "SpeedTree/UmbrellaThorn_RT_Flowers.spt")
    asset_tree.add_asset_path("SpeedTree/VenusTree_RT.spt", "SpeedTree/VenusTree_RT.spt")
    asset_tree.add_asset_path("SpeedTree/WeepingWillow_RT.spt", "SpeedTree/WeepingWillow_RT.spt")
    asset_tree.add_asset_path("SpeedTree/WeepingWillow_RT_Fall.spt", "SpeedTree/WeepingWillow_RT_Fall.spt")
    asset_tree.add_asset_path("SpeedTree/WeepingWillow_RT_Winter.spt", "SpeedTree/WeepingWillow_RT_Winter.spt")
    asset_tree.add_asset_path("SpeedTree/demoWind.ini", "SpeedTree/demoWind.ini")
    # end common
    # end trees

    # Interface
    asset_tree.add_asset_path("Fonts/MUFN____.TTF", "Fonts/MUFN____.TTF")
    asset_tree.add_asset_path("Interface/FrameXML/basic.toc", "Interface/FrameXML/basic.toc")
    asset_tree.add_asset_path("Interface/FrameXML/betaworld.toc", "Interface/FrameXML/betaworld.toc")
    asset_tree.add_asset_path("Interface/FrameXML/Library.py", "Interface/FrameXML/Library.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvActionBar.py", "Interface/FrameXML/MvActionBar.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvActionBar.xml", "Interface/FrameXML/MvActionBar.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvActionButton.xml", "Interface/FrameXML/MvActionButton.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvCharacter.py", "Interface/FrameXML/MvCharacter.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvCharacter.xml", "Interface/FrameXML/MvCharacter.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvChat.py", "Interface/FrameXML/MvChat.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvChat.xml", "Interface/FrameXML/MvChat.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvContainer.py", "Interface/FrameXML/MvContainer.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvContainer.xml", "Interface/FrameXML/MvContainer.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvDialog.py", "Interface/FrameXML/MvDialog.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvDialog.xml", "Interface/FrameXML/MvDialog.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvFonts.xml", "Interface/FrameXML/MvFonts.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvFrame.xml", "Interface/FrameXML/MvFrame.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvItemButton.xml", "Interface/FrameXML/MvItemButton.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvPlayer.py", "Interface/FrameXML/MvPlayer.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvPlayer.xml", "Interface/FrameXML/MvPlayer.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvStatus.py", "Interface/FrameXML/MvStatus.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvStatus.xml", "Interface/FrameXML/MvStatus.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvTarget.py", "Interface/FrameXML/MvTarget.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvTarget.xml", "Interface/FrameXML/MvTarget.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvTooltip.xml", "Interface/FrameXML/MvTooltip.xml")
    asset_tree.add_asset_path("Interface/FrameXML/MvUnit.py", "Interface/FrameXML/MvUnit.py")
    asset_tree.add_asset_path("Interface/FrameXML/MvUnit.xml", "Interface/FrameXML/MvUnit.xml")
    asset_tree.add_asset_path("Interface/Imagesets/MvButtons.xml", "Interface/Imagesets/MvButtons.xml")
    asset_tree.add_asset_path("Interface/Imagesets/MvChat.xml", "Interface/Imagesets/MvChat.xml")
    asset_tree.add_asset_path("Interface/Imagesets/MvQuestFrame.xml", "Interface/Imagesets/MvQuestFrame.xml")
    asset_tree.add_asset_path("Interface/Imagesets/ContainerFrame.xml", "Interface/Imagesets/ContainerFrame.xml")
    asset_tree.add_asset_path("Interface/Imagesets/QuestFrame.xml", "Interface/Imagesets/QuestFrame.xml")
    asset_tree.add_asset_path("Interface/Imagesets/CharacterFrame.xml", "Interface/Imagesets/CharacterFrame.xml")
    asset_tree.add_asset_path("Interface/Imagesets/Common.xml", "Interface/Imagesets/Common.xml")
    asset_tree.add_asset_path("Interface/Imagesets/DialogFrame.xml", "Interface/Imagesets/DialogFrame.xml")
    asset_tree.add_asset_path("Interface/Imagesets/TargetingFrame.xml", "Interface/Imagesets/TargetingFrame.xml")
    asset_tree.add_asset_path("Interface/Imagesets/ChatFrame.xml", "Interface/Imagesets/ChatFrame.xml")
    asset_tree.add_asset_path("Interface/Imagesets/Icons.xml", "Interface/Imagesets/Icons.xml")
    asset_tree.add_asset_path("Interface/Imagesets/Cursor.xml", "Interface/Imagesets/Cursor.xml")
    asset_tree.add_asset_path("Interface/Imagesets/PaperDollInfoFrame.xml", "Interface/Imagesets/PaperDollInfoFrame.xml")
    asset_tree.add_asset_path("Interface/Imagesets/Buttons.xml", "Interface/Imagesets/Buttons.xml")
    asset_tree.add_asset_path("Interface/Imagesets/Tooltips.xml", "Interface/Imagesets/Tooltips.xml")
    asset_tree.add_asset_path("Imagefiles/MvButtons.tga", "Imagefiles/MvButtons.tga")
    asset_tree.add_asset_path("Imagefiles/MvChatFrame.tga", "Imagefiles/MvChatFrame.tga")
    asset_tree.add_asset_path("Imagefiles/MvQuestFrame.tga", "Imagefiles/MvQuestFrame.tga")
    asset_tree.add_asset_path("Imagefiles/ContainerFrame.tga", "Imagefiles/ContainerFrame.tga")
    asset_tree.add_asset_path("Imagefiles/Buttons.tga", "Imagefiles/Buttons.tga")
    asset_tree.add_asset_path("Imagefiles/Icons.tga", "Imagefiles/Icons.tga")
    asset_tree.add_asset_path("Imagefiles/Tooltips.tga", "Imagefiles/Tooltips.tga")
    asset_tree.add_asset_path("Imagefiles/Cursor.tga", "Imagefiles/Cursor.tga")
    asset_tree.add_asset_path("Imagefiles/TargetingFrame.tga", "Imagefiles/TargetingFrame.tga")
    asset_tree.add_asset_path("Imagefiles/CharacterFrame.tga", "Imagefiles/CharacterFrame.tga")
    asset_tree.add_asset_path("Imagefiles/ChatFrame.tga", "Imagefiles/ChatFrame.tga")
    asset_tree.add_asset_path("Imagefiles/DialogFrame.tga", "Imagefiles/DialogFrame.tga")
    asset_tree.add_asset_path("Imagefiles/Common.tga", "Imagefiles/Common.tga")
    asset_tree.add_asset_path("Imagefiles/PaperDollInfoFrame.tga", "Imagefiles/PaperDollInfoFrame.tga")
    asset_tree.add_asset_path("Imagefiles/QuestFrame.tga", "Imagefiles/QuestFrame.tga")

    # Misc.
    asset_tree.add_asset_path("Icons/AxiomIcon.ico", "Icons/AxiomIcon.ico")

asset_tree = AssetTree("c:/Documents and Settings/mccollum/Desktop/sample/", "")
add_assets(asset_tree)

tar_file = tarfile.open("key_media_files.tar", "w")
asset_tree.write_to_tar(tar_file)
tar_file.close()


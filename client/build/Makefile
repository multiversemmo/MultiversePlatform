BUILD_DEST=$(DEPTH)/MultiverseClient/bin
PKG_DEST=$(DEPTH)/package
RUN_DEST=$(DEPTH)/MultiverseClient/bin/$(CONFIG)
CT_DEST=$(DEPTH)/Tools/ConversionTool/bin/$(CONFIG)
TG_DEST=$(DEPTH)/Tools/TerrainGenerator/bin/$(CONFIG)
MT_DEST=$(DEPTH)/Tools/MapTool/bin/$(CONFIG)
WE_DEST=$(DEPTH)/Tools/WorldEditor/bin/$(CONFIG)
TA_DEST=$(DEPTH)/Tools/TerrainAssembler/TerrainAssembler/bin/$(CONFIG)
MV_DEST=$(DEPTH)/Tools/ModelViewer/bin/$(CONFIG)
AXIOM_DEP_LIBDIR=$(DEPTH)/Axiom/Dependencies
MV_DEP_LIBDIR=$(DEPTH)/MultiverseClient/Dependencies
EXT_LIBDIR=$(DEPTH)/Lib
WIX_DIR=$(DEPTH)/Tools/wix20
FRAMEWORK_DIR="/cygdrive/c/Windows/Microsoft.NET/Framework"
SETUP_XML=mvsetup_build.xml
VS2008=true
ifdef VS2008
MV_SLN_FILE=Multiverse.sln
BUILD_CMD=$(FRAMEWORK_DIR)/v3.5/MSBuild.exe
else
MV_SLN_FILE=Multiverse_vs2005.sln
BUILD_CMD=$(FRAMEWORK_DIR)/v2.0.50727/MSBuild.exe
endif
#BUILD_FLAGS="/target:MultiverseClient"

## If we haven't defined a component url in the environment, use this default
ifndef COMPONENTS_URL
COMPONENTS_URL=http://update.multiverse.net/1.5-downloads/
#COMPONENTS_URL=http://sv1.multiverse.net/CruiseControl/client_builds/
endif

ifndef CONFIG
CONFIG=Debug
# CONFIG=Release
endif

ifndef PARTNER_TAG
INSTALLER_PACKAGE=client
else
# Use a different (custom) installer
INSTALLER_PACKAGE=client_${PARTNER_TAG}
# Change the location to which the files will be pushed.
COMPONENTS_URL=http://update.multiverse.net/1.5-downloads/${PARTNER_TAG}/
endif

DEPTH=../..
BOOTSTRAP_DIR=$(DEPTH)/Tools/build/wix/BootStrapper

.dummy: install install_other install_axiom install_cegui axiom cegui package setup

build: mv

install: install_other install_axiom_plugins install_axiom install_cegui
# install_runlibs

managed_clean:
	cd $(DEPTH) && find ./ -name Tao\*.dll -ok rm {} \;
	cd $(DEPTH) && find ./ -name ICSharpCode\*.dll -ok rm {} \;

axiom_clean:
	cd $(DEPTH) && find Axiom/            "(" -name "Axiom*.dll" -o -name "Axiom*.pdb" ")" -print -exec rm {} \;
	cd $(DEPTH) && find Lib/              "(" -name "Axiom*.dll" -o -name "Axiom*.pdb" ")" -print -exec rm {} \;
	cd $(DEPTH) && find MultiverseClient/ "(" -name "Axiom*.dll" -o -name "Axiom*.pdb" ")" -print -exec rm {} \;
	cd $(DEPTH) && find Tools/            "(" -name "Axiom*.dll" -o -name "Axiom*.pdb" ")" -print -exec rm {} \;
	cd $(DEPTH) && find mvsm/             "(" -name "Axiom*.dll" -o -name "Axiom*.pdb" ")" -print -exec rm {} \;

mv_clean:
	cd $(DEPTH) && find Axiom/            "(" -name "MultiverseClient*.exe" -o -name "MultiverseClient*.pdb" ")" -print -exec rm {} \;
	cd $(DEPTH) && find Lib/              "(" -name "MultiverseClient*.exe" -o -name "MultiverseClient*.pdb" ")" -print -exec rm {} \;
	cd $(DEPTH) && find MultiverseClient/ "(" -name "MultiverseClient*.exe" -o -name "MultiverseClient*.pdb" ")" -print -exec rm {} \;
	cd $(DEPTH) && find Tools/            "(" -name "MultiverseClient*.exe" -o -name "MultiverseClient*.pdb" ")" -print -exec rm {} \;
	cd $(DEPTH) && find mvsm/             "(" -name "MultiverseClient*.exe" -o -name "MultiverseClient*.pdb" ")" -print -exec rm {} \;

models:
	./run_models.sh

clean: axiom_clean mv_clean

$(DEPTH)/MultiverseClient/Multiverse.sln.proj: $(DEPTH)/MultiverseClient/Multiverse.sln
	export MsBuildEmitSolution=1 &&	$(BUILD_CMD) $< /t:ValidateSolutionConfiguration

# mv: $(DEPTH)/MultiverseClient/Multiverse.sln.proj
#	$(BUILD_CMD) $<

mv:
	$(BUILD_CMD) /property:Configuration=$(CONFIG) $(DEPTH)/MultiverseClient/$(MV_SLN_FILE)

mv2:
	$(BUILD_CMD) $(DEPTH)/Axiom/MathLib/Axiom.MathLib.csproj
	$(BUILD_CMD) $(DEPTH)/Axiom/Engine/Axiom.Engine.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Config/Multiverse.Config.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/CollisionLib/Multiverse.CollisionLib.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/MathLib/Multiverse.MathLib.csproj
	$(BUILD_CMD) $(DEPTH)/mvsm/Axiom.SceneManagers.Multiverse.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Gui/Multiverse.Gui.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Utility/Multiverse.Utility.csproj
	$(BUILD_CMD) $(DEPTH)/Lib/HeightfieldGenerator/HeightfieldGenerator.csproj
	$(BUILD_CMD) $(DEPTH)/Lib/AssetRepository/Multiverse.AssetRepository.csproj
	$(BUILD_CMD) $(DEPTH)/Lib/Voice/Multiverse.Voice.csproj
	$(BUILD_CMD) $(DEPTH)/Lib/SpeexWrapper/SpeexWrapper.csproj
	$(BUILD_CMD) $(DEPTH)/Axiom/SceneManagers/Octree/Axiom.SceneManagers.Octree.csproj
	$(BUILD_CMD) $(DEPTH)/Axiom/Plugins/CgProgramManager/Axiom.Plugins.CgProgramManager.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Network/Multiverse.Network.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Patcher/Multiverse.Patcher.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Serialization/Multiverse.Serialization.csproj
	$(BUILD_CMD) $(DEPTH)/Axiom/Plugins/ParticleFX/Axiom.Plugins.ParticleFX.csproj
	$(BUILD_CMD) $(DEPTH)/Axiom/Platforms/Win32/Axiom.Platforms.Win32.csproj
	$(BUILD_CMD) $(DEPTH)/Axiom/RenderSystems/DirectX9/Axiom.RenderSystems.DirectX9.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Interface/Multiverse.Interface.csproj
	$(BUILD_CMD) $(DEPTH)/Axiom/RenderSystems/OpenGL/Axiom.RenderSystems.OpenGL.csproj
	$(BUILD_CMD) $(DEPTH)/Lib/Generator/Multiverse.Generator.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Base/Multiverse.Base.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/BetaWorld/Multiverse.BetaWorld.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Interface/Multiverse.Interface.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/Patcher/StandalonePatcher.csproj
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/MultiverseClient.csproj

setup: $(BOOTSTRAP_DIR)/Packages/MultiverseClient/MultiverseClient.msi ${SETUP_XML}
	$(BUILD_CMD) ${SETUP_XML} /p:BootstrapDir=`cygpath -w $(DEPTH)/Tools/build/wix/BootStrapper/` /p:ComponentsUrl="${COMPONENTS_URL}"

maptool:
	# $(BUILD_CMD) $(DEPTH)/MultiverseClient/MultiverseClient.sln
	$(BUILD_CMD) $(DEPTH)/MultiverseClient/MultiverseClient.sln /target:MapTool

media_installer: 
	python build_asset_info.py

client_installer: setup
	tar cf client_dependencies.tar --directory="${BOOTSTRAP_DIR}/Packages/" MultiverseClient directx_9c_oct2005 DotNetFX

$(BOOTSTRAP_DIR)/Packages/MultiverseClient/MultiverseClient.msi: ${INSTALLER_PACKAGE}.msi
	cp ${INSTALLER_PACKAGE}.msi "$(BOOTSTRAP_DIR)/Packages/MultiverseClient/MultiverseClient.msi"

installer_files: media_installer client_installer

media_patch_files:
	python build_media_patch_info.py
	@echo 
	@echo You must now push media_patch.tar to the remote server and unpack 
	@echo the archive so that it will be available at:
	@echo "  http://update.multiverse.net/mvupdate.media/"
	@echo 

client_patch_files:
	python build_patch_info.py
	@echo 
	@echo You must now push client_patch.tar to the remote server and unpack 
	@echo the archive so that it will be available at:
	@echo "  http://update.multiverse.net/mvupdate.client/"
	@echo 

patch_files: media_patch_files client_patch_files

push_patch_files:
	scp media_patch.tar client_patch.tar $(DEPTH)/MultiverseClient/Patcher/bin/$(CONFIG)/patcher.exe sv1.multiverse.net:

axiom:
	# $(BUILD_CMD) $(DEPTH)/MultiverseClient/MultiverseClient.sln
	$(BUILD_CMD) $(DEPTH)/Axiom/Axiom.sln
cegui:
	# $(BUILD_CMD) $(DEPTH)/MultiverseClient/MultiverseClient.sln
	$(BUILD_CMD) $(DEPTH)/cegui-sharp/CrayzEdsSharpGui.sln

maya2ogre:
	$(BUILD_CMD) $(DEPTH)/Tools/MayaExport/MayaTools.sln /target:All /p:Configuration=$(CONFIG)

$(RUN_DEST): 
	mkdir $(RUN_DEST)

install_all_deps: install_deps install_we_deps install_ct_deps install_mv_deps install_tg_deps install_ta_deps

build_install_deps_script:
	# install the multiverse client dependencies
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(RUN_DEST)/
	cp $(MV_DEP_LIBDIR)/Managed/IronPython.dll $(RUN_DEST)/
	cp $(MV_DEP_LIBDIR)/Managed/IronMath.dll $(RUN_DEST)/
	# cp $(MV_DEP_LIBDIR)/Managed/Tao.OpenAl.dll $(RUN_DEST)/
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(RUN_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/OggVorbisWrapper/Release/OggVorbisWrapper.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/ogg/win32/Dynamic_Release/ogg.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/vorbis/win32/Vorbis_Dynamic_Release/vorbis.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/vorbis/win32/VorbisEnc_Dynamic_Release/vorbisenc.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/vorbis/win32/VorbisFile_Dynamic_Release/vorbisfile.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeWrapper.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeRT.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/FMOD/fmodex.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/Speex/bin/libspeex.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/Speex/bin/libspeexdsp.dll $(RUN_DEST)/

	# install the conversion tool dependencies
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(CT_DEST)/
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(CT_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(CT_DEST)/

	# install the model viewer dependencies
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(MV_DEST)/
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(MV_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(MV_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeWrapper.dll $(MV_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeRT.dll $(MV_DEST)/

	# install the world editor dependencies
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(MT_DEST)/
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(MT_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(MT_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeWrapper.dll $(MT_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeRT.dll $(MT_DEST)/

	# install the terrain generator dependencies
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(TG_DEST)/
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(TG_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(TG_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeWrapper.dll $(TG_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeRT.dll $(TG_DEST)/


install_deps: install_managed_deps install_native_deps install_speedtree_deps install_fmod_deps install_voice_deps install_dw_deps
	cp $(BUILD_DEST)/Imageset.xsd $(RUN_DEST)/

install_managed_deps: $(RUN_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(RUN_DEST)/
	cp $(MV_DEP_LIBDIR)/Managed/IronPython.dll $(RUN_DEST)/
	cp $(MV_DEP_LIBDIR)/Managed/IronMath.dll $(RUN_DEST)/
#	cp $(MV_DEP_LIBDIR)/Native/*.dll $(RUN_DEST)/

install_native_deps: $(RUN_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(RUN_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(RUN_DEST)/

install_ogg_deps: $(RUN_DEST)
	cp $(EXT_LIBDIR)/OggVorbisWrapper/Release/OggVorbisWrapper.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/ogg/win32/Dynamic_Release/ogg.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/vorbis/win32/Vorbis_Dynamic_Release/vorbis.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/vorbis/win32/VorbisEnc_Dynamic_Release/vorbisenc.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/vorbis/win32/VorbisFile_Dynamic_Release/vorbisfile.dll $(RUN_DEST)/

install_speedtree_deps: $(RUN_DEST)
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeWrapper.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeRT.dll $(RUN_DEST)/

install_fmod_deps: $(RUN_DEST)
	cp $(EXT_LIBDIR)/FMOD/fmodex.dll $(RUN_DEST)/

install_voice_deps: $(RUN_DEST)
	cp $(EXT_LIBDIR)/Speex/bin/libspeex.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/Speex/bin/libspeexdsp.dll $(RUN_DEST)/

install_dw_deps: $(RUN_DEST)
	cp $(EXT_LIBDIR)/DiamondWare/libeay32.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/DiamondWare/ssleay32.dll $(RUN_DEST)/
	cp $(EXT_LIBDIR)/DiamondWare/dwTVC.exe $(RUN_DEST)/

install_keys:
	regedit.exe /s /i $(DEPTH)/Lib/SpeedTree/SpeedTreeKeyMultiverseCurrent.reg


$(MT_DEST): 
	mkdir $(MT_DEST)

install_mt_deps: install_mt_managed_deps install_mt_native_deps install_mt_speedtree_deps

install_mt_managed_deps: $(MT_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(MT_DEST)/

install_mt_native_deps: $(MT_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(MT_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(MT_DEST)/

install_mt_speedtree_deps: $(MT_DEST)
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeWrapper.dll $(MT_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeRT.dll $(MT_DEST)/

install_we_deps: install_we_managed_deps install_we_native_deps install_we_speedtree_deps install_we_movie_deps

install_we_managed_deps: $(WE_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(WE_DEST)/

install_we_native_deps: $(WE_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(WE_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(WE_DEST)/
	cp $(DEPTH)/Tools/build/KeyBindings.xml $(WE_DEST)/
	cp $(DEPTH)/Tools/build/CommandEvents.xml $(WE_DEST)/
	cp $(DEPTH)/Tools/WorldEditor/build/NameValueTemplates/InstanceEntryRegion.xml $(WE_DEST)/NameValueTemplates/
	cp $(DEPTH)/Tools/WorldEditor/build/NameValueTemplates/MessageRegion.xml $(WE_DEST)/NameValueTemplates/
	cp $(DEPTH)/Tools/WorldEditor/build/NameValueTemplates/example.xml $(WE_DEST)/NameValueTemplates/

install_we_speedtree_deps: $(WE_DEST)
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeWrapper.dll $(WE_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeRT.dll $(WE_DEST)/

install_we_movie_deps: $(WE_DEST)
	cp $(BUILD_DEST)/../Base/bin/Debug/Multiverse.Base.dll $(WE_DEST)/
	cp $(BUILD_DEST)/../Network/bin/Debug/Multiverse.Network.dll $(WE_DEST)/
	cp $(BUILD_DEST)/../Interface/bin/Debug/Multiverse.Interface.dll $(WE_DEST)/
	cp $(BUILD_DEST)/../Movie/bin/Debug/Multiverse.Movie.dll $(WE_DEST)/
	cp $(BUILD_DEST)/../Movie/Codecs/DirectShowLib/bin/Debug/DirectShowWrapper.dll $(WE_DEST)/

install_ta_deps: install_ta_managed_deps install_ta_native_deps

install_ta_managed_deps: $(TA_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(TA_DEST)/

install_ta_native_deps: $(TA_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(TA_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(TA_DEST)/

$(MV_DEST): 
	mkdir $(MV_DEST)

install_mv_deps: install_mv_managed_deps install_mv_native_deps install_mv_speedtree_deps

install_mv_managed_deps: $(MV_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(MV_DEST)/

install_mv_native_deps: $(MV_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(MV_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(MV_DEST)/

install_mv_speedtree_deps: $(MV_DEST)
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeWrapper.dll $(MV_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeRT.dll $(MV_DEST)/
 
$(TG_DEST): 
	mkdir $(TG_DEST)

install_tg_deps: install_tg_managed_deps install_tg_native_deps install_tg_speedtree_deps

install_tg_managed_deps: $(TG_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(TG_DEST)/

install_tg_native_deps: $(TG_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(TG_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(TG_DEST)/

install_tg_speedtree_deps: $(TG_DEST)
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeWrapper.dll $(TG_DEST)/
	cp $(EXT_LIBDIR)/SpeedTree/bin/Release/SpeedTreeRT.dll $(TG_DEST)/

install_ct_deps: install_ct_managed_deps install_ct_native_deps

install_ct_managed_deps: $(CT_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Managed/*.dll $(CT_DEST)/

install_ct_native_deps: $(CT_DEST)
	cp $(AXIOM_DEP_LIBDIR)/Native/*.dll $(CT_DEST)/
	cp $(MV_DEP_LIBDIR)/Native/*.dll $(CT_DEST)/

# install_runlibs:
# 	cp $(BUILD_DEST)/*.dll $(RUN_DEST)/

# install_axiom:
# 	cp $(DEPTH)/Axiom/Engine/bin/$(CONFIG)/Axiom.Engine.dll $(BUILD_DEST)
# 	cp $(DEPTH)/Axiom/Engine/bin/$(CONFIG)/Axiom.MathLib.dll $(BUILD_DEST)
# 	cp $(DEPTH)/Axiom/SceneManagers/Octree/bin/$(CONFIG)/Axiom.SceneManagers.Octree.dll $(BUILD_DEST)
# 	cp $(DEPTH)/Axiom/RenderSystems/DirectX9/bin/$(CONFIG)/Axiom.RenderSystems.DirectX9.dll $(BUILD_DEST)
# 	cp $(DEPTH)/Axiom/Platforms/Win32/bin/$(CONFIG)/Axiom.Platforms.Win32.dll $(BUILD_DEST)

# install_axiom_plugins:
# 	cp $(DEPTH)/Axiom/RenderSystems/DirectX9/bin/$(CONFIG)/Axiom.RenderSystems.DirectX9.dll $(RUN_DEST)
# 	# cp $(DEPTH)/Axiom/RenderSystems/OpenGL/bin/$(CONFIG)/Axiom.RenderSystems.OpenGL.dll $(RUN_DEST)
# 	cp $(DEPTH)/Axiom/Plugins/CgProgramManager/bin/$(CONFIG)/Axiom.Plugins.CgProgramManager.dll $(BUILD_DEST)
# 	cp $(DEPTH)/Axiom/Plugins/ParticleFX/bin/$(CONFIG)/Axiom.Plugins.ParticleFX.dll $(BUILD_DEST)

# install_cegui:
# 	cp $(DEPTH)/cegui-sharp/CrayzEdsGui.Base/bin/$(CONFIG)/CrayzEdsGui.Base.dll $(BUILD_DEST)
# 	cp $(DEPTH)/cegui-sharp/CrayzEdsGui.Renderers.Axiom/bin/$(CONFIG)/CrayzEdsGui.Renderers.Axiom.dll $(BUILD_DEST)

# axiom_prebuild: axiom_prebuild.xml
# 	cp axiom_prebuild.xml $(DEPTH)/Axiom/prebuild.xml
# 	./dnpb.exe /target vs2005 /file $(DEPTH)/Axiom/prebuild.xml

# cegui_prebuild: cegui_prebuild.xml
# 	cp cegui_prebuild.xml $(DEPTH)/cegui-sharp/prebuild.xml
# 	./dnpb.exe /target vs2005 /file $(DEPTH)/cegui-sharp/prebuild.xml

# mv_prebuild: mv_prebuild.xml
# 	cp mv_prebuild.xml $(DEPTH)/MultiverseClient/prebuild.xml
# 	./dnpb.exe /target vs2005 /file $(DEPTH)/MultiverseClient/prebuild.xml

# mt_prebuild: mt_prebuild.xml
# 	cp mt_prebuild.xml $(DEPTH)/Tools/MapTool/prebuild.xml
# 	./dnpb.exe /target vs2005 /file $(DEPTH)/Tools/MapTool/prebuild.xml

# ct_prebuild: ct_prebuild.xml
# 	cp ct_prebuild.xml $(DEPTH)/Tools/ConversionTool/prebuild.xml
# 	./dnpb.exe /target vs2005 /file $(DEPTH)/Tools/ConversionTool/prebuild.xml

all_assets: sampleworld_assets mv_social_assets mv_fantasy_assets

sampleworld_assets: $(DEPTH)/Media/SampleAssets.assetlist
	cd $(DEPTH)/Tools/BaseAssetPackager/bin/Debug && ./BaseAssetPackager.exe --source_repository c:/svn_tree/trunk/Media/ --dest_repository c:/junk/sampleworld --worldassets_file ../../../../multiverse/config/sampleworld/sampleworld.worldassets --world_name sampleworld --assetlist_file ../../$<
	./build_manifest.py c:/junk/sampleworld

mv_social_assets: $(DEPTH)/Media/SocialWorldAssets.assetlist
	cd $(DEPTH)/Tools/AssetPackager/bin/Debug && ./AssetPackager.exe --worldassets_file ../../../../multiverse/config/mv_social/mv_social.worldassets --new_asset_repository c:/junk/mv_social --world_name mv_social --assetlist_file ../../$<
	./build_manifest.py c:/junk/mv_social

mv_fantasy_assets: $(DEPTH)/Media/FantasyWorldAssets.assetlist
	cd $(DEPTH)/Tools/AssetPackager/bin/Debug && ./AssetPackager.exe --worldassets_file ../../../../multiverse/config/mv_fantasy/mv_fantasy.worldassets --new_asset_repository c:/junk/mv_fantasy --world_name mv_fantasy --assetlist_file ../../$<
	./build_manifest.py c:/junk/mv_fantasy

les_assets: $(DEPTH)/Media/LES_worldAssets.assetlist
	cd $(DEPTH)/Tools/AssetPackager/bin/Debug && ./AssetPackager.exe --set_default_repository c:/svn_tree/trunk/Media --worldassets_file ../../../../Art_Dept_Dev_Repositories/les_world/ServerSideFiles/les_world.worldassets
	cd $(DEPTH)/Tools/AssetPackager/bin/Debug && ./AssetPackager.exe --worldassets_file ../../../../Art_Dept_Dev_Repositories/les_world/ServerSideFiles/les_world.worldassets --new_asset_repository c:/junk/les_world --world_name LES_world --assetlist_file ../../$<
	./build_manifest.py c:/junk/les_world

.dummy: installer build_all

build_all: build client_installer

epydoc:
	epydoc --html --parse-only --no-private -o ../epydoc ../Scripts/*.py

installer: ${INSTALLER_PACKAGE}.msi

media_assets.wxi:
	python build_asset_info.py

product.wixobj: ui.wxi product.wxi
client_mtv.wixobj: ui.wxi product.wxi

%.wixobj: %.wxs
	$(WIX_DIR)/candle -dConfiguration=$(CONFIG) $<

# We don't have any good way to determine when we need to rebuild the msi,
# so just rebuild whenever any dll is updated
# In our build scripts, where we care about partner tags, this isn't required,
# since those generally use a clean checkout
client.msi: ../bin/$(CONFIG)/*.dll

%.msi: %.wixobj
	$(WIX_DIR)/light -out $@ wixca.wixlib $<

fake_install:
	mkdir -p test/bin
	cp ../bin/$(CONFIG)/*.dll ../bin/$(CONFIG)/*.exe "C:/Program Files/Multiverse Client/bin/"

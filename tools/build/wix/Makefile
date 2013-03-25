#
# Version related variables.  You probably want to update some or all
# of these for each release.
#
MAJOR_VER=1
MINOR_VER=5
TOOLS_BOOTSTRAPPER_LABEL=Multiverse Tools 1.5RC2
TOOLS_RELEASE_NAME=1.5RC2

#
# These are the URLs for the 3 different distribution points, which are
# built into the bootstrappers.
#
SV1_BOOTSTRAPPER_URL=http://sv1.multiverse.net/devtools/
MVSECRET_BOOTSTRAPPER_URL=http://update.multiverse.net/mvsecret/
RELEASE_BOOTSTRAPPER_URL=http://update.multiverse.net/tools/

#
# This is all the AssemblyInfo.cs files for tools that need to have their
# version numbers whacked to match the release version.
#
ASSEMBLY_INFO_FILES=$(DEPTH)/Tools/ModelViewer/Properties/AssemblyInfo.cs \
$(DEPTH)/Tools/WorldEditor/Properties/AssemblyInfo.cs \
$(DEPTH)/Tools/TerrainGenerator/Properties/AssemblyInfo.cs \
$(DEPTH)/Tools/MosaicCreator/MosaicCreator/Properties/AssemblyInfo.cs \
$(DEPTH)/Tools/AssetImporter/Properties/AssemblyInfo.cs \
$(DEPTH)/Tools/AssetPackager/Properties/AssemblyInfo.cs \
${DEPTH}/Lib/AssetRepository/Properties/AssemblyInfo.cs \
${DEPTH}/Tools/MVImportTool/Properties/AssemblyInfo.cs

##
## You really don't want to be changing anything beyond this point unless
## you know what you are doing.
##

# CONFIG=Release
CONFIG=Debug

TOOLS_VERSION=$(MAJOR_VER)_$(MINOR_VER)
DEPTH=../../..

WIX_DIR=$(DEPTH)/Tools/wix
BUILD_CMD="/cygdrive/c/Windows/Microsoft.NET/Framework/v3.5/MSBuild.exe"
TOOLS_MSI=MultiverseTools-$(TOOLS_RELEASE_NAME)-$(VERSION_STRING).msi
SETUP_EXE=MultiverseToolsSetup-$(TOOLS_RELEASE_NAME)-$(VERSION_STRING).exe
LOCAL_PKG_DIR=$(DEPTH)/ThirdParty/BootStrapper/Packages
TOOLS_PKG_DIR=$(LOCAL_PKG_DIR)/MultiverseTools
GEN_VERSION_SOLUTION=../GenVersionNumber/GenVersionNumber.sln
GEN_VERSION_EXE=../GenVersionNumber/bin/Debug/GenVersionNumber.exe
ARCHIVE_FILES=publish/$(SETUP_EXE) publish/sv1-$(SETUP_EXE) publish/mvsecret-$(SETUP_EXE) $(TOOLS_MSI)

default: build_installer

#
# This is a trampoline target that builds the program that generates
# the version number, and then executes future targets with the version
# string set.
#
build_installer: build_gen_version_number build_installer_with_version

build_installer_with_version:  VERSION_STRING = $(shell ../GenVersionNumber/bin/Debug/GenVersionNumber.exe $(MAJOR_VER) $(MINOR_VER) )
build_installer_with_version: build_installer_real

#
# This target builds the bootstrapper and then archives the build products
#
build_installer_real: bootstrapper archive tag
	@echo Build Version Number: $(VERSION_STRING)
	@echo Tools MSI: $(TOOLS_MSI)
	@echo Release Bootstrapper: $(SETUP_EXE)

#
# This target copies the build products to an archive directory if the
# environment variable is set.
#
archive:
	if [ -n "$$TOOLS_ARCHIVE_DIR" -a -d "$$TOOLS_ARCHIVE_DIR" ] ; then cp $(ARCHIVE_FILES) $(TOOLS_ARCHIVE_DIR); echo $(VERSION_STRING) > $(TOOLS_ARCHIVE_DIR)/lastbuild;  fi
	scp $(TOOLS_MSI) sv1:/var/www/html/ToolBuilds/

tag:
	cd $(DEPTH) && svn cp -m "Tagging tools build $(VERSION_STRING)" . https://sv1.multiverse.net/svn/tags/Tools-$(TOOLS_RELEASE_NAME)-$(VERSION_STRING)

#
# This target forces a rebuild of the GenVersionNumber command
#
build_gen_version_number:
	$(BUILD_CMD) /t:clean ../GenVersionNumber/GenVersionNumber.sln
	$(BUILD_CMD) ../GenVersionNumber/GenVersionNumber.sln

#
# This target does a clean build of the tools
#
build_tools: update_assembly_versions
	$(BUILD_CMD) /t:clean ../MultiverseTools.sln
	$(BUILD_CMD) ../MultiverseTools.sln

#
# Generate the MSI file for the tools.
#
tools_msi: build_tools
	$(WIX_DIR)/candle.exe MultiverseTools.wxs
	$(WIX_DIR)/light.exe MultiverseTools.wixobj
	mv MultiverseTools.msi $(TOOLS_MSI)
	@echo "Built" $@

#
# Copy the tools msi into the bootstrapper dir
#
update_bootstrapper_packages: tools_msi
	rm -f $(TOOLS_PKG_DIR)/$(TOOLS_MSI)
	cp $(TOOLS_MSI) $(TOOLS_PKG_DIR)

#
# This target generates the two xml files that define the MultiverseTools
# package for the bootstrapper.  Since MSBuild doesn't do variable
# substitution on these files, we are using sed to do it.
#
gen_bootstrapper_files:
	sed -e 's/$$(TOOLS_MSI)/$(TOOLS_MSI)/' < $(TOOLS_PKG_DIR)/product.xml.proto > $(TOOLS_PKG_DIR)/product.xml
	sed -e 's/$$(TOOLS_BOOTSTRAPPER_LABEL)/$(TOOLS_BOOTSTRAPPER_LABEL)/' < $(TOOLS_PKG_DIR)/en/package.xml.proto > $(TOOLS_PKG_DIR)/en/package.xml
	
bootstrapper: gen_bootstrapper_files update_bootstrapper_packages
	$(BUILD_CMD) /property:BOOTSTRAPPER_URL=$(SV1_BOOTSTRAPPER_URL) /p:BootstrapDir=`cygpath -a -w $(DEPTH)/ThirdParty/BootStrapper/` MultiverseTools_build.xml
	mv publish/setup.exe publish/sv1-$(SETUP_EXE)
	$(BUILD_CMD) /property:BOOTSTRAPPER_URL=$(MVSECRET_BOOTSTRAPPER_URL) /p:BootstrapDir=`cygpath -a -w $(DEPTH)/ThirdParty/BootStrapper/` MultiverseTools_build.xml
	mv publish/setup.exe publish/mvsecret-$(SETUP_EXE)
	$(BUILD_CMD) /property:BOOTSTRAPPER_URL=$(RELEASE_BOOTSTRAPPER_URL) /p:BootstrapDir=`cygpath -a -w $(DEPTH)/ThirdParty/BootStrapper/` MultiverseTools_build.xml
	mv publish/setup.exe publish/$(SETUP_EXE)

GenVersionNumber:
	$(BUILD_CMD) /t:clean $(GEN_VERSION_SOLUTION)
	$(BUILD_CMD) $(GEN_VERSION_SOLUTION)

update_assembly_versions:
	for file in $(ASSEMBLY_INFO_FILES); do \
	 mv $$file $$file.tmp; \
	 sed -e '/AssemblyVersion/s/\"[0-9\.\*]*\"/\"$(VERSION_STRING)\"/' < $$file.tmp > $$file ; \
	done

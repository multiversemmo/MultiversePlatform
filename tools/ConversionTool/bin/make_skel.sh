#!/bin/sh

MODEL=$1

cd Debug

./ConversionTool.exe --build_skeleton ${MODEL}

../../../ogretools/OgreXMLConverter ${MODEL}.mesh.xml
cp -v ${MODEL}.mesh ../../../../Media/Meshes/${MODEL}_bones.mesh



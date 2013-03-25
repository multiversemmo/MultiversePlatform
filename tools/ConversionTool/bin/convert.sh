#!/bin/sh

MODEL=$1
MODEL_DIR="/c/Documents and Settings/mccollum/My Documents/maya/projects/default"

cp "${MODEL_DIR}"/"${MODEL}".dae Debug
cd Debug

## Scale by a factor of 300
# ./ConversionTool.exe --transform 300 0 0 0 0 300 0 0 0 0 300 0 0 0 0 1 ${MODEL}.dae

## Scale by a factor of 110, and rotate 90 degrees cw about the y axis
# ./ConversionTool.exe --transform 0 0 110 0 0 110 0 0 -110 0 0 0 0 0 0 1 ${MODEL}.dae

## Rotate 90 degrees about the y axis
# ./ConversionTool.exe --transform 0 0 -1 0 0 1 0 0 1 0 0 0 0 0 0 1 ${MODEL}.dae

## Scale by a factor of 10 (to offset the centimeter based export)
# ./ConversionTool.exe --transform 10 0 0 0 0 10 0 0 0 0 10 0 0 0 0 1 "${MODEL}.dae"

## Export the model without the transforms
./ConversionTool.exe "${MODEL}.dae"

../../../ogretools/OgreXMLConverter ${MODEL}.mesh.xml
../../../ogretools/OgreXMLConverter ${MODEL}.skeleton.xml
cp -v ${MODEL}.mesh ../../../../Media/Meshes/
cp -v ${MODEL}.skeleton ../../../../Media/Skeletons
cp -i ${MODEL}.material ../../../../Media/Materials


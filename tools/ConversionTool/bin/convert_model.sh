#!/bin/bash

MODEL=$1
MODEL_DIR="/c/Documents and Settings/mccollum/My Documents/maya/projects/default"

cd Debug

## Copy the model file to the local directory
cp "${MODEL_DIR}"/"${MODEL}".dae .
## Export the model without the transforms
./ConversionTool.exe "${MODEL}.dae"

../../../ogretools/OgreXMLConverter ${MODEL}.mesh.xml
cp -v ${MODEL}.mesh ../../../../Media/Meshes/${MODEL}.mesh
cp -i ${MODEL}.material ../../../../Media/Materials/${MODEL}.material

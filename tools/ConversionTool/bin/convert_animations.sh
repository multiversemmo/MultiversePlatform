#!/bin/bash

MODEL=$1
MODEL_DIR="/c/Documents and Settings/mccollum/My Documents/maya/projects/default"

cd Debug

if [ -z "${ANIMATIONS}" ]
    then ANIMATIONS="idle walk"
fi

## Copy the rig file to the local directory
cp "${MODEL_DIR}"/"${MODEL}".dae .
## Export the model without the transforms
./ConversionTool.exe "${MODEL}.dae"

for anim in ${ANIMATIONS}
do
    if [ -n ${MERGE_COLLADA} ]
    then
        ## Copy the model into the local directory
        cp "${MODEL_DIR}"/"${MODEL}_${anim}".dae ./"${MODEL}_${anim}_raw".dae
        ## Merge the rig data and animation data
        ./ConversionTool.exe --merge_collada "${MODEL}_${anim}".dae "${MODEL}.dae" "${MODEL}_${anim}_raw".dae
   else
        ## Copy the model into the local directory
        cp "${MODEL_DIR}"/"${MODEL}_${anim}".dae .
   fi
   ## Export the model without the transforms
   ./ConversionTool.exe --animation_name $anim "${MODEL}_${anim}.dae"
done

mv ${MODEL}.skeleton.xml ${MODEL}_rig.skeleton.xml
MERGE_FLAGS="--merge_animations ${MODEL}.skeleton.xml ${MODEL}_rig.skeleton.xml"
for anim in ${ANIMATIONS}
do
    MERGE_FLAGS="${MERGE_FLAGS} ${MODEL}_${anim}.skeleton.xml"
done
echo $MERGE_FLAGS
./ConversionTool.exe ${MERGE_FLAGS}

../../../ogretools/OgreXMLConverter ${MODEL}.mesh.xml
cp -v ${MODEL}.mesh ../../../../Media/Meshes/${MODEL}.mesh
cp -i ${MODEL}.material ../../../../Media/Materials/${MODEL}.material

if [ -n ${MERGE_COLLADA} ]
then
    ../../../ogretools/OgreXMLConverter ${MODEL}.skeleton.xml
    cp -v ${MODEL}.skeleton ../../../../Media/Skeletons
fi

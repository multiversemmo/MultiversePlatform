#!/bin/bash

if [ $# -lt 1 ]; then
    echo "specify directory name under Worlds (eg: mv_social)"
    exit 0
fi

WORLD_DIR=$1
WORLDS=../../Worlds/$WORLD_DIR
MEDIA=../../Media

date=`date "+%y%m%d_%H%M"`

for dir in "./Textures" "./Physics" "./GpuPrograms" "./Materials" "./Meshes" "./Skeletons" "./Sounds"; do
  for f in `cd ${WORLDS}; find $dir -type f |grep -v '.svn'`; do
    echo "copying ${f}"
    cp ${MEDIA}/${f} ${WORLDS}/${f}
  done
done

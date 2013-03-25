gamedev_dir="c:/GameDevelopment"
model_root="c:/GameDevelopment/Models"
mv_client_dir="${gamedev_dir}/MultiverseClient"
maya2ogre_dir="${gamedev_dir}/Tools/MayaExport/maya2ogre/bin/debug"
ogretools_dir="${gamedev_dir}/Tools/ogretools"

models="hero orc girl"
rigs="fantasy_rig nexus_tshirt_rig nexus_lsleeve_rig"
animations="idle run strike death wave walk"
props="sword portal"

models="
# Export a given mesh and skeleton, including animation dat"
rigs="kidgame_rig"
animations="idle"

scale=300
precision=8

export PATH="${PATH}":"${ogretools_dir}"
export MAYA_LOCATION=c:\\Program\ Files\\Alias\\Maya6.0

###
# Export a given mesh and skeleton, including animation data, and touch up the skeleton reference
###
export_mesh() {
  in_file=$1
  shift
  model=$1
  shift
  rig=$1
  shift
  rm -f "${model}_${rig}.mesh.xml"
  echo ./maya2ogre -in "${in_file}" -mesh "${model}_${rig}.mesh.xml" -t -n -mat "${model}_${rig}.material" -mprefix "${model}_${rig}." -skel "${model}.skeleton.xml" -vba -s "${scale}" -p "${precision}" -inanim "${model}.anim.xml" -b
  ./maya2ogre -in "${in_file}" -mesh "${model}_${rig}.mesh.xml" -t -n -mat "${model}_${rig}.material" -mprefix "${model}_${rig}." -skel "${model}.skeleton.xml" -vba -s "${scale}" -p "${precision}" -inanim "${model}.anim.xml" -b > "${model}_${rig}.mesh.out"

  sed -e s/"${model}.skeleton"/"${model}.skeleton.xml"/g < "${model}_${rig}.mesh.xml" > tmpfile && mv tmpfile "${model}_${rig}.mesh.xml"
}

###
# Export a given mesh for a prop (no animation or skeleton data)
###
export_prop() {
  in_file=$1
  shift
  model=$1
  shift
  echo ./maya2ogre -in "${in_file}" -mesh "${model}.mesh.xml" -t -n -mat "${model}.material" -mprefix "${model}." -s "${scale}" -p "${precision}"
  ./maya2ogre -in "${in_file}" -mesh "${model}.mesh.xml" -t -n -mat "${model}.material" -mprefix "${model}." -s "${scale}" -p "${precision}" > ${model}.mesh.out
}

###
# Export a given animation for a model
###
export_anim() {
  in_file=$1
  shift
  model=$1
  shift
  anim=$1
  shift
#  anim_length=$1
#  shift
#  ./maya2ogre -in "${in_file}" -afile "${model}_${anim}.anim.xml" -s "${scale}" -p "${precision}" -anim "${anim}" 0 "${anim_length}" 1
  echo ./maya2ogre -in "${in_file}" -afile "${model}_${anim}.anim.xml" -s "${scale}" -p "${precision}" -anim "${anim}" 0 1000 1 
  ./maya2ogre -in "${in_file}" -afile "${model}_${anim}.anim.xml" -s "${scale}" -p "${precision}" -anim "${anim}" 0 1000 1 > "${model}_${anim}.anim.out"
}

###
# Run through all the animations for all the models and export the animation
# data to xml files and build the concatenated animation xml file
###
export_animations() {
  for model in ${models}
  do
    cat /dev/null > ${model}.anim.xml
    for anim in ${animations}
    do
	  if [ -r "${model_root}/${model}/${model}_${anim}.ma" ]
	  then
        export_anim "${model_root}/${model}/${model}_${anim}.ma" "${model}" "${anim}"
        cat ${model}_${anim}.anim.xml >> ${model}.anim.xml
	  fi
    done
  done
}

###
# Variant that just run through all the animations for all the models and 
# builds the animation sets from already exported data.
###
concat_animations() {  
  for model in ${models}
  do
    cat /dev/null > ${model}.anim.xml
    for anim in ${animations}
	do
	  if [ -r "${model}_${anim}.anim.xml" ]
	  then
		cat ${model}_${anim}.anim.xml >> ${model}.anim.xml
	  fi
	done
  done
}

###
# Run through all the models and all the rigs for each model to export the mesh
###
export_meshes() {
  for model in ${models}
  do
    for rig in ${rigs}
    do
	  if [ -r "${model_root}/${model}/${model}_${rig}.ma" ]
	  then
	    export_mesh "${model_root}/${model}/${model}_${rig}.ma" "${model}" "${rig}"
	  fi
	done
  done
  for model in ${props}
  do
    export_prop "${model_root}/${model}/${model}.ma" "${model}"
  done
}

###
# Run the OgreXMLConverter to convert the xml skeleton into the ogre binary format
###
convert_xml() {
  for model in ${models}
  do
    ${ogretools_dir}/OgreXMLConverter "${model}".skeleton.xml
  done
}

###
# Simply copy the texture files into the appropriate directory
###
install_textures() {
  for model in ${models}
  do
    cp ${model_root}/${model}/*.jpg ${mv_client_dir}/bin/Media/Textures
  done

  for model in ${props}
  do
    cp ${model_root}/${model}/*.jpg ${mv_client_dir}/bin/Media/Textures
  done
}

###
# Install the textures, models, materials and skeleton data into the appropriate area
###
install_models() {
  install_textures

  for model in ${models}
  do
    for rig in ${rigs}
	do 
	  cp ${model}_${rig}.mesh.xml ${mv_client_dir}/bin/Media/Meshes
      cp ${model}_${rig}.material ${mv_client_dir}/bin/Media/Materials
    done
	if [ -r ${model}.skeleton ]
	then
      cp ${model}.skeleton ${mv_client_dir}/bin/Media/Skeletons
	fi
    cp ${model}.skeleton.xml ${mv_client_dir}/bin/Media/Skeletons
  done

  for model in ${props}
  do
    cp ${model}.material ${mv_client_dir}/bin/Media/Materials
    cp ${model}.mesh.xml ${mv_client_dir}/bin/Media/Meshes
  done
}
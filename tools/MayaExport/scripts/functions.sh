model_root="c:/Multiverse"
client_dir="c:/GameDevelopment/MultiverseClient"
ogretools_dir="c:/GameDevelopment/Tools/ogretools"

scale=100
precision=8

export PATH="${PATH}":"${ogretools_dir}"
export MAYA_LOCATION=c:\\Program\ Files\\Alias\\Maya6.0

export_mesh() {
  in_file=$1
  shift
  file_prefix=$1
  shift
  ./maya2ogre -in "${in_file}" -mesh "${file_prefix}.mesh.xml" -t -n -mat "${file_prefix}.material" -mprefix "${file_prefix}." -skel "${file_prefix}.skeleton.xml" -vba -c -s "${scale}" -p "${precision}" -inanim "${file_prefix}.anim.xml"

#  sed -e s/"${file_prefix}.skeleton"/"${file_prefix}.skeleton.xml"/g < "${file_prefix}.mesh.xml" > tmpfile && mv tmpfile "${file_prefix}.mesh.xml"
}

export_prop() {
  in_file=$1
  shift
  file_prefix=$1
  shift
  ./maya2ogre -in "${in_file}" -mesh "${file_prefix}.mesh.xml" -t -n -mat "${file_prefix}.material" -mprefix "${file_prefix}." -c -s "${scale}" -p "${precision}"
}

export_anim() {
  in_file=$1
  shift
  file_prefix=$1
  shift
  anim_name=$1
  shift
#  anim_length=$1
#  shift
#  ./maya2ogre -in "${in_file}" -afile "${file_prefix}.anim.xml" -s "${scale}" -p "${precision}" -anim "${anim_name}" 0 "${anim_length}" 1
  ./maya2ogre -in "${in_file}" -afile "${file_prefix}.anim.xml" -s "${scale}" -p "${precision}" -anim "${anim_name}" 0 1000 1
}

fix_files() {
    cp "${model_root}/HERO/Anim/idle/hero_idle_b.ma"         "${model_root}/HERO/Anim/idle/hero_idle.ma"

    cp "${model_root}/GIRL/Anim/idle/girl_idle_final.ma"     "${model_root}/GIRL/Anim/idle/girl_idle.ma"
    cp "${model_root}/GIRL/Anim/run/girl_run_final.ma"       "${model_root}/GIRL/Anim/run/girl_run.ma"
    cp "${model_root}/GIRL/Anim/strike/girl_strike_final.ma" "${model_root}/GIRL/Anim/strike/girl_strike.ma"
    cp "${model_root}/GIRL/Anim/death/girl_death_final.ma"   "${model_root}/GIRL/Anim/death/girl_death.ma"

    cp "${model_root}/ORC/Rigg/orc_rig_final.ma"             "${model_root}/ORC/Rigg/orc_rigg_final.ma"
}

export_animations() {
  for model in hero girl orc
  do
    cat /dev/null > ${model}.anim.xml
    for anim in idle run strike death
    do
      export_anim "${model_root}/${model}/Anim/${anim}/${model}_${anim}.ma" "${model}_${anim}" "${anim}"
      cat ${model}_${anim}.anim.xml >> ${model}.anim.xml
    done
  done
}

concat_animations() {  
  for model in hero girl orc
  do
    cat /dev/null > ${model}.anim.xml
    for anim in idle run strike death
    do
      cat ${model}_${anim}.anim.xml >> ${model}.anim.xml
    done
  done
}

export_meshes() {
  for model in hero girl orc
  do
    export_mesh "${model_root}/${model}/Rigg/${model}_Rigg_FINAL.ma" "${model}"
  done
  for model in sword
  do
    export_prop "${model_root}/hero/model/hero_sword_final.ma" "${model}"
  done
}

convert_xml() {
  for model in hero girl orc
  do
    ${ogretools_dir}/OgreXMLConverter "${model}".skeleton.xml
  done
}


install_models() {
  for model in orc hero girl
  do
    cp ${model}.material ${client_dir}/bin/Media/Materials
    cp ${model}.mesh.xml ${client_dir}/bin/Media/Meshes
    cp ${model}.skeleton ${client_dir}/bin/Media/Skeletons
  done

  for model in sword
  do
    cp ${model}.material ${client_dir}/bin/Media/Materials
    cp ${model}.mesh.xml ${client_dir}/bin/Media/Meshes
  done
}
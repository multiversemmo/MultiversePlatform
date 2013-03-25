#!/bin/sh

if [ $# -lt 7 ]
  then
    echo "usage: create_character <dbname> <dbhost> <dbuser> <dbpassword> <mv_id> <world_name> <character_name>"
    exit
fi

DBNAME=$1
DBHOST=$2
DBUSER=$3
DBPASSWORD=$4

#MVID must match the players account id in the account table
MVID=$5
MV_WORLDNAME=$6
CHARNAME=$7

# where the player will spawn at first
LOC="116000 0 3853000"
MESH="human_female.mesh"

# nude female
SUBMESH="bodyShape-lib.0 human_female.skin_material head_aShape-lib.0 human_female.head_a_material head_aShape-lib.1 human_female.skin_material hair_bShape-lib.0 human_female.hair_b_material"

# leather a female
#SUBMESH="bodyShape-lib.0 human_female.skin_material head_aShape-lib.0 human_female.head_a_material   hair_bShape-lib.0 	 human_female.hair_b_material head_aShape-lib.1 human_female.skin_material leather_a_tunicShape-lib.0 	 human_female.leather_a_material 	 leather_a_beltShape-lib.0 	 human_female.leather_a_material 	 leather_a_pantsShape-lib.0 	 human_female.leather_a_material leather_a_bootsShape-lib.0 	 human_female.leather_a_material leather_a_glovesShape-lib.0 	 human_female.leather_a_material leather_a_jewelShape-lib.0 	 human_female.leather_a_material"

# nude male 
#SUBMESH="bodyShape-lib.0 human_male.skin_material head_aShape-lib.0 human_male.head_a_material"

# brax armor female
#SUBMESH="bodyShape-lib.0 human_female.skin_material head_aShape-lib.1 human_female.skin_material head_aShape-lib.0 human_female.head_a_material hair_bShape-lib.0 human_female.hair_b_material braxBreastArmorShape-lib.0 human_female.brax_armor_material braxFront01ArmorShape-lib.0 human_female.brax_armor_material braxFront02ArmorShape-lib.0 human_female.brax_armor_material braxFront03ArmorShape-lib.0 human_female.brax_armor_material braxFront04ArmorShape-lib.0 human_female.brax_armor_material braxBack01ArmorShape-lib.0 human_female.brax_armor_material braxBack02ArmorShape-lib.0 human_female.brax_armor_material braxBack03ArmorShape-lib.0 human_female.brax_armor_material braxBack04ArmorShape-lib.0 human_female.brax_armor_material braxBack05ArmorShape-lib.0 human_female.brax_armor_material braxHipArmorShape-lib.0 human_female.brax_armor_material braxArmorStrapShape-lib.0 human_female.brax_armor_material braxHipStrapShape-lib.0 human_female.brax_armor_material braxShoulderArmorShape-lib.0 human_female.brax_armor_material braxPantsArmorShape-lib.0 human_female.brax_armor_material braxThighArmorShape-lib.0 human_female.brax_armor_material BraxClothBeltShape-lib.0 human_female.brax_armor_material braxBracersShape-lib.0 human_female.brax_armor_material"

java multiverse.scripts.CreateCharacter $DBHOST $DBNAME $DBUSER $DBPASSWORD "$CHARNAME" $MVID $LOC 0 0.9966 0 -0.0819 $MV_WORLDNAME $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH &

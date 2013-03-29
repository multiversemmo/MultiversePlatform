#!/bin/sh

WORLDID=3
LOC1="-10000000 0 -10000000"
LOC2="12000 0 0"
LOC="98212 0 3860592"
TOWNLOC="36000 0 -337000"
FORESTLOC="1653000 0 -430000"
ORCLOC="153000 0 3032000"

DBHOST=localhost
MESH="human_female.mesh"

# nude female
#SUBMESH="bodyShape-lib.0 human_female.skin_material head_aShape-lib.0 human_female.head_a_material"

# leather a female
#SUBMESH="bodyShape-lib.0 human_female.skin_material head_aShape-lib.0 human_female.head_a_material   hair_bShape-lib.0 	 human_female.hair_b_material head_aShape-lib.1 human_female.skin_material leather_a_tunicShape-lib.0 	 human_female.leather_a_material 	 leather_a_beltShape-lib.0 	 human_female.leather_a_material 	 leather_a_pantsShape-lib.0 	 human_female.leather_a_material leather_a_bootsShape-lib.0 	 human_female.leather_a_material leather_a_glovesShape-lib.0 	 human_female.leather_a_material leather_a_jewelShape-lib.0 	 human_female.leather_a_material"

# nude male 
#SUBMESH="bodyShape-lib.0 human_male.skin_material head_aShape-lib.0 human_male.head_a_material"

# brax armor female
SUBMESH="bodyShape-lib.0 human_female.skin_material head_aShape-lib.1 human_female.skin_material head_aShape-lib.0 human_female.head_a_material hair_bShape-lib.0 human_female.hair_b_material braxBreastArmorShape-lib.0 human_female.brax_armor_material braxFront01ArmorShape-lib.0 human_female.brax_armor_material braxFront02ArmorShape-lib.0 human_female.brax_armor_material braxFront03ArmorShape-lib.0 human_female.brax_armor_material braxFront04ArmorShape-lib.0 human_female.brax_armor_material braxBack01ArmorShape-lib.0 human_female.brax_armor_material braxBack02ArmorShape-lib.0 human_female.brax_armor_material braxBack03ArmorShape-lib.0 human_female.brax_armor_material braxBack04ArmorShape-lib.0 human_female.brax_armor_material braxBack05ArmorShape-lib.0 human_female.brax_armor_material braxHipArmorShape-lib.0 human_female.brax_armor_material braxArmorStrapShape-lib.0 human_female.brax_armor_material braxHipStrapShape-lib.0 human_female.brax_armor_material braxShoulderArmorShape-lib.0 human_female.brax_armor_material braxPantsArmorShape-lib.0 human_female.brax_armor_material braxThighArmorShape-lib.0 human_female.brax_armor_material BraxClothBeltShape-lib.0 human_female.brax_armor_material braxBracersShape-lib.0 human_female.brax_armor_material"

mysql multiverse -h $DBHOST -u root -pmv123 -e "delete from player_character where world_id=$WORLDID"

# you need to remove all players first
# delete from players;

# cedeno
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 Raf 2 $LOC1 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH &

sleep 2

# robin
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 mccollum 4 $LOC2 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH & 

sleep 2

# bill
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 Bill 1 $ORCLOC 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH & 

sleep 2

# corey
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 Corey 3 $TOWNLOC 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH & 

sleep 2

# tom
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 tomw 5 $LOC 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH & 

sleep 2

# jsw
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 jsw 6 $LOC 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH & 

sleep 2

# dan
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 danw 7 $LOC 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH & 
sleep 2

# chris
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 chris 8 $LOC 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 20 20 20 20 500 500 0 38 $SUBMESH & 

sleep 2

# david
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 darksuit 9 $LOC 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH & 

sleep 2

# chrischeung
java multiverse.scripts.CreateCharacter $DBHOST multiverse root mv123 Aelfryd 10 $LOC 0 0.9966 0 -0.0819 $WORLDID $MESH 10 12 50 10 10 10 10 50 50 0 38 $SUBMESH & 

sleep 2


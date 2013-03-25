#!/bin/sh

WORLD_LOGIN=localhost:5040
ACCOUNT_START=1100

ROOM_COUNT=10
BOTS_PER_ROOM=49
LOGIN_INTERVAL=1
DEST_INSTANCE="room"

TCP=--tcp

while [ $# != 0 ] ;
do
    case "$1" in
    --login) WORLD_LOGIN="$2" ; shift 2 ;;
    --account) ACCOUNT_START="$2" ; shift 2 ;;
    --rooms) ROOM_COUNT="$2" ; shift 2 ;;
    --bots-per-room) BOTS_PER_ROOM="$2" ; shift 2 ;;
    --interval) LOGIN_INTERVAL="$2" ; shift 2 ;;
    --name-prefix) NAME_PREFIX="$2" ; shift 2 ;;
    --instance) DEST_INSTANCE="$2" ; shift 2 ;;
    --) shift ; break ;;
    *) echo "Unknown option $1" ; exit 1 ;;
    esac
done

if [ "X"$MV_HOME = "X" ] ; then
    SCRIPT_HOME=.
else
    SCRIPT_HOME=$MV_HOME/src/multiverse/simpleclient
fi

# Default female props
#CHAR_PROPS=`cat <<EOF
#-P Sex=female
#-P Model=female_01
#-P BodyType=ntrl
#-P HeadDetail=
#-P SkinColor=euro
#-P HeadShape=euro_01
#-P HairStyle=bob
#-P HairColor=blond
#-P ClothesTorso=tank_n
#-P ClothesLegs=shorts_n
#-P Footwear=flats
#-P TorsoTattoo=none
#-P TorsoTattooSite=upper_back
#-P LegTattoo=none
#-P LegTattooSite=tramp_stamp
#-P AppearanceOverride=avatar
#EOF`

CHAR_PROPS=`cat <<EOF
-P Sex=female
-P Model=female_01
-P BodyType=ntrl,vlpt,athl
-P HeadDetail=
-P SkinColor=warm-tan,neutral-tan,cool-tan,brown,yellow,pink,dark-brown,yellow-ochre,apricot
-P HeadShape=euro_01,asia_01,afri_01,lati_01
-P HairStyle=bob,pigtails,short,rockabilly,goth,hat,bald
-P HairColor=blond,red,brown,black,purple
-P ClothesTorso=tank_n,tank_a,tank_v,swimsuit_n,goth_a_n,rockabilly_a_n,tokyopop_a_n,hiphop_a_n,swimsuit_a,goth_a_a,rockabilly_a_a,tokyopop_a_a,hiphop_a_a,swimsuit_v,goth_a_v,rockabilly_a_v,tokyopop_a_v,hiphop_a_v
-P ClothesLegs=shorts_n,shorts_a,shorts_v,swimsuit_n,goth_a_n,rockabilly_a_n,tokyopop_a_n,hiphop_a_n,swimsuit_a,goth_a_a,rockabilly_a_a,tokyopop_a_a,hiphop_a_a,swimsuit_v,goth_a_v,rockabilly_a_v,tokyopop_a_v,hiphop_a_v
-P Footwear=bare,flats,heels,goth_a,rockabilly_a,tokyopop_a,hiphop_a
-P TorsoTattoo=none,sunburst,roses,butterfly,ganesh,heart,koi,skull,skulltophat
-P TorsoTattooSite=upper_back,lower_back,neck_back,chest,stomach,left_arm,left_forearm,left_hand,right_arm,right_forearm,right_hand
-P LegTattoo=none,sunburst,roses,butterfly,ganesh,heart,koi,skull,skulltophat
-P LegTattooSite=tramp_stamp,left_thigh,left_calf,left_ankle,right_thigh,right_calf,right_ankle,pubic
-P AppearanceOverride=avatar
-P RoomStyle=cute
EOF`

# can't change room style
#-P RoomStyle=cute,hiphop

room=0
account=$ACCOUNT_START
player_letter=65

# Remove one from BOTS_PER_ROOM to account for room owner
let "BOTS_PER_ROOM -= 1"

while [ $room -lt $ROOM_COUNT ]; do

    if [ "X"$NAME_PREFIX = "X" ] ; then
	letter=\\`printf \\%03o $player_letter`
	letter=`printf $letter`
	name=$letter$letter$letter
    else
	name=$NAME_PREFIX
    fi
    echo STARTING room for account $account with name prefix $name

    instance=$account
    if [ $DEST_INSTANCE \!= "room" ]; then
	instance=$DEST_INSTANCE
    fi

    $SCRIPT_HOME/runplayerclients.sh $account 1 $LOGIN_INTERVAL -n $name --login $WORLD_LOGIN -X  "--instance $instance --polygon_region bot1" $CHAR_PROPS $TCP "$@"
    sleep 10
    let "account += 1"
    $SCRIPT_HOME/runplayerclients.sh $account $BOTS_PER_ROOM $LOGIN_INTERVAL -n $name --login $WORLD_LOGIN -X "--instance $instance --polygon_region bot1" $CHAR_PROPS $TCP "$@"

    let "account += BOTS_PER_ROOM"
    let "room += 1"
    let "player_letter += 1"

    sleeptime=$(python -c "print int($BOTS_PER_ROOM * $LOGIN_INTERVAL * 2.05)")
    sleep $sleeptime
done


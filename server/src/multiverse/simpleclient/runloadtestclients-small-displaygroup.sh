## need to add DisplayGroup

SERVER=shark:9040

./runlesclients.sh 66001 20 2 -s ../../../config/load_test_world/club_test_client.py -n club --login $SERVER &
sleep 40
sleep 2

./runlesclients.sh 61001 20 2 -P DisplayGroup=group1 -s ../../../config/load_test_world/group1_test_client.py -n group1 --login $SERVER &
sleep 40
sleep 2

./runlesclients.sh 62001 20 2 -P DisplayGroup=group2 -s ../../../config/load_test_world/group2_test_client.py -n group2 --login $SERVER &
sleep 40
sleep 2

./runlesclients.sh 63001 20 2 -P DisplayGroup=group3 -s ../../../config/load_test_world/group3_test_client.py -n group3 -P DisplayGroup=group3 --login $SERVER &
sleep 40
sleep 2

./runlesclients.sh 64001 20 2 -P DisplayGroup=group4 -s ../../../config/load_test_world/group4_test_client.py -n group4 -P DisplayGroup=group4 --login $SERVER &
sleep 40
sleep 2

./runlesclients.sh 65001 8 2 -s ../../../config/load_test_world/main_block_test_client.py -n main --login $SERVER &



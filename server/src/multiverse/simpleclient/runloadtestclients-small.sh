## need to add DisplayGroup

SERVER=shark:9040

./runlesclients.sh 56001 20 2 -s ../../../config/load_test_world/club_test_client.py -n club --login $SERVER &
sleep 40
sleep 2

./runlesclients.sh 51001 20 1 -s ../../../config/load_test_world/group1_test_client.py -n group1 --login $SERVER &
sleep 20
sleep 2

./runlesclients.sh 52001 20 2 -s ../../../config/load_test_world/group2_test_client.py -n group2 --login $SERVER &
sleep 40
sleep 2

./runlesclients.sh 53001 20 2 -s ../../../config/load_test_world/group3_test_client.py -n group3 --login $SERVER &
sleep 40
sleep 2

./runlesclients.sh 54001 20 2 -s ../../../config/load_test_world/group4_test_client.py -n group4 --login $SERVER &
sleep 40
sleep 2

./runlesclients.sh 55021 8 2 -s ../../../config/load_test_world/main_block_test_client.py -n main --login $SERVER &



## need to add DisplayGroup

SERVER=shark:9040

./runlesclients.sh 56001 30 1 -s ../../../config/load_test_world/club_test_client.py -n club --login $SERVER &
echo ==== Starting club group
sleep 30
sleep 2

./runlesclients.sh 55021 70 1 -s ../../../config/load_test_world/main_block_test_client.py -n main --login $SERVER &
echo ==== Starting main street group
sleep 70
sleep 5

./runlesclients.sh 51001 100 2 -s ../../../config/load_test_world/group1_test_client.py -n group1 --login $SERVER &
echo ==== Starting group1
sleep 200
sleep 5

./runlesclients.sh 52001 100 2 -s ../../../config/load_test_world/group2_test_client.py -n group2 --login $SERVER &
echo ==== Starting group2
sleep 200
sleep 5

./runlesclients.sh 53001 100 2 -s ../../../config/load_test_world/group3_test_client.py -n group3 --login $SERVER &
echo ==== Starting group3
sleep 200
sleep 5

./runlesclients.sh 54001 100 3 -s ../../../config/load_test_world/group4_test_client.py -n group4 --login $SERVER &
echo ==== Starting group4

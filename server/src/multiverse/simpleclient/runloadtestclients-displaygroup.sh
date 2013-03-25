## need to add DisplayGroup

if [ $# -eq 1 ]; then
    SERVER=$1
else     
    echo "Usage: $0 <server-host:port>"
    exit 1
fi           

PROTO=--tcp

./runlesclients.sh 71001 100 2 -P DisplayGroup=group1 -s ../../../config/load_test_world/group1_test_client.py -n group1 $PROTO --login $SERVER &
echo ==== Starting group1
sleep 200
sleep 5

./runlesclients.sh 73001 100 2 -P DisplayGroup=group3 -s ../../../config/load_test_world/group3_test_client.py -n group3 $PROTO --login $SERVER &
echo ==== Starting group3
sleep 200
sleep 5

./runlesclients.sh 76001 30 2 -s ../../../config/load_test_world/club_test_client.py -n club $PROTO --login $SERVER &
echo ==== Starting club group
sleep 30
sleep 2

./runlesclients.sh 75001 70 2 -s ../../../config/load_test_world/main_block_test_client.py -n main $PROTO --login $SERVER &
echo ==== Starting main street group
sleep 140
sleep 5

./runlesclients.sh 72001 100 2 -P DisplayGroup=group2 -s ../../../config/load_test_world/group2_test_client.py -n group2 $PROTO --login $SERVER &
echo ==== Starting group2
sleep 200
sleep 5

./runlesclients.sh 74001 100 3 -P DisplayGroup=group4 -s ../../../config/load_test_world/group4_test_client.py -n group4 $PROTO --login $SERVER &
echo ==== Starting group4
sleep 300
sleep 5

echo ==== DONE

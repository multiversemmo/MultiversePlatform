#!/bin/sh

mysql multiverse -h localhost -u root -pmv123 -e "delete from objstore where world_name='sampleworld'"

java multiverse.server.engine.Engine config/raf_work/fantasy_init.js config/betaworld/events.js config/betaworld/worldserver_handlers.js \
    config/betaworld/items_db.js \
    config/betaworld/create_quests.js \
    config/betaworld/save.js

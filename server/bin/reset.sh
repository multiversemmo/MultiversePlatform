#!/bin/bash

# delete all existing objects from the database
$MV_HOME/bin/clear_obj_store.sh

# create the default characters
$MV_HOME/bin/create_default_chars.sh

# disable this for now since we dont want to save all the items into the db
# we just load them up at startup time from scripts at the moment
# $MV_HOME/bin/init_world.sh

# kill any java processes still running
$MV_HOME/bin/kill.sh

#!/bin/bash

cd /cygdrive/c/Multiverse/tree

if [ -n "$1" ]
then
  if [ "$1" == "checkout" -o "$1" == "init" ]
  then
    svn co -N https://sv1.multiverse.net/svn/branches/facebook_branch .
  else
    echo "Unexpected argument: $1"
    exit
  fi
fi
svn up -N Media
svn up Media/friendworld
svn up Media/nyts
svn up Media/standalone
svn up Media/common
svn up -N Tools
svn up Tools/Machinima

#!/bin/bash

DOCTITLE="Multiverse Server API - Version 1.5 (beta)"
WINTITLE="Multiverse Server API - Version 1.5 (beta)"
echo "set titles"
FOOTER="<br>Copyright &copy; 2008 The Multiverse Network, Inc." 
HEADER="</em></td></tr><tr><td class='mv-version'>Version&nbsp;1.5</td><td class='mv-caveat'>APIs subject to change!</td></TR>"

SRCPATH="C:/work/source/src"
OUTPATH="C:/work/newjavadoc"

LIBROOT="C:/work/source/other"
CP="$LIBROOT/java-getopt-1.0.11.jar;$LIBROOT/rhino1_5R5/js.jar;$LIBROOT//jython.jar;$LIBROOT/log4j-1.2.14.jar;$LIBROOT/servlet-api.jar;$LIBROOT/bcel-5.2.jar;$LIBROOT/jsp-api.jar" 

PKGS="multiverse.mars:multiverse.msgsys:multiverse.server"
EXCL="multiverse.mars.eventhandlers:multiverse.mars.msghandlers:multiverse.server.eventhandlers:multiverse.server.network"
STYLESHEET="C:/work/source/mv-javadoc.css"

echo Classpath is $CP
echo OUTPATH is $OUTPATH
echo Sourcepath is $SRCPATH
echo 
javadoc -quiet -stylesheetfile "$STYLESHEET" -doctitle "$DOCTITLE" -windowtitle "$WINTITLE" -header "$HEADER" -footer "$FOOTER" -sourcepath "$SRCPATH" -classpath "$CP" -subpackages "$PKGS" -exclude "$EXCL" -d "$OUTPATH"

# PKGS="multiverse.entitymgr:multiverse.mars:multiverse.mars.events:multiverse.server.events:multiverse.msgsvr:multiverse.server:multiverse.simpleclient"
# EXCL="multiverse.mars.eventhandlers:multiverse.mars.msghandlers:multiverse.server.eventhandlers:multiverse.server.msghandlers:multiverse.server.network"

#!/bin/bash

MV_HOME=${MV_HOME:-"../../.."}
echo #MV_HOME
MV_BIN=${MV_HOME}/bin
DEFAULT_MV_PROPERTYFILE=${DEFAULT_MV_PROPERTYFILE:-"multiverse.properties"}
MV_PROPERTYFILE=${MV_PROPERTYFILE:-"${MV_BIN}/${DEFAULT_MV_PROPERTYFILE}"}

RHINO=${RHINO:-"${MV_HOME}/other/rhino1_5R5/js.jar"}
GETOPT=${GETOPT:-"${MV_HOME}/other/java-getopt-1.0.11.jar"}
JYTHON=${JYTHON:-"${MV_HOME}/other/jython.jar"}
LOG4J=${LOG4J:-"${MV_HOME}/other/log4j-1.2.14.jar"}
BCEL=${BCEL:-"${MV_HOME}/other/bcel-5.2.jar"}

# Determine if we should use .class files from the build hierarchy,
# or .jar files from the dist hierarchy.  To run the property getter
# before MV_JAR is set, we always use the dist version of the property
# getter.
USE_CLASS_FILES=${USE_CLASS_FILES:-$(java -cp ${MV_HOME}/dist/lib/multiverse.jar -Dmultiverse.propertyfile=${MV_PROPERTYFILE} multiverse.scripts.PropertyGetter multiverse.use_class_files)}

if  [ $USE_CLASS_FILES = "true" ]; then
	MV_JAR=${MV_JAR:-"${MV_HOME}/build"}
	MARS_JAR=${MARS_JAR:-"${MV_HOME}/build"}
	INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/inject"}
else
	MV_JAR=${MV_JAR:-"${MV_HOME}/dist/lib/multiverse.jar"}
	MARS_JAR=${MARS_JAR:-"${MV_HOME}/dist/lib/mars.jar"}
	INJECTED_JAR=${INJECTED_JAR:-"${MV_HOME}/dist/lib/injected.jar"}
fi

MV_COMMON=${MV_COMMON:-"${MV_HOME}/config/common"}

if [ $(uname -o) == "Cygwin" ]; then
    MV_CLASSPATH="$RHINO;$JDBC;$INJECTED_JAR;$MV_JAR;$MARS_JAR;$GETOPT;$JYTHON;$LOG4J;$BCEL."
else
    MV_CLASSPATH="${RHINO}:${JDBC}:${INJECTED_JAR}:${MV_JAR}:${MARS_JAR}:${GETOPT}:${JYTHON}:${LOG4J}:${BCEL}:."
fi

echo $MV_CLASSPATH
mkdir -p logs

echo "Run" > ./simpleclient.txt

echo MV_COMMON is "${MV_COMMON}"

mkdir -p logs

start=1
count=1
seconds=5

arg=1
while [ $# != 0 ]; do
    flag="$1"
    case "$flag" in
	-*) break
	    ;;
    esac
	if [ $arg = 1 ]; then
	    start=$flag
	fi
	if [ $arg = 2 ]; then
	    count=$flag
	fi
	if [ $arg = 3 ]; then
	    seconds=$flag
	fi
    shift
    arg=`expr $arg + 1`
done

echo start $start
echo count $count
echo seconds $seconds
echo FLAGS: "$@"

# Use SimpleClient plus start as the loggename, so different
# SimpleClients' log names don't collide
logname=SimpleClient$start

CHAR_PROPS="-P SkinColor=caucasian,asian,african_american -P HeadShape=caucasian_01,asian_01,african_american_01 -P HairStyle=pony,bob,layers,bob2 -P HairColor=blonde,red,brown,black -P ClothesTorso=sleeveless_white,sleeveless_purple,sleeveless_blue,strapless_purple,strapless_red,strapless_brown,leotard_blue,leotard_red,leotard_skull -P ClothesLegs=capris_black,capris_brown,capris_blue,short_skirt_leopard,short_skirt_red -P Tattoo=tattoo_01_arm,tattoo_01_back,tattoo_01_chest,tattoo_02_arm,tattoo_02_back,tattoo_02_chest,tattoo_03_arm,tattoo_03_back,tattoo_03_chest -P AppearanceOverride=avatar"

# runs the simple client to connect to the local server
exec java -client -Xmx128m -cp ${MV_CLASSPATH} -Dmultiverse.propertyfile=${MV_COMMON}/simpleclient.props -Dmultiverse.loggername=$logname multiverse.simpleclient.SimpleClient -t ${MV_COMMON}/typenumbers.txt -e ${MV_COMMON}/simpleclient.props -a $start --count $count -f ./simpleclient.txt --seconds-between $seconds $CHAR_PROPS "$@"

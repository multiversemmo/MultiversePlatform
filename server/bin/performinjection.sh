##*******************************************************************
##
## The Multiverse Platform is made available under the MIT License.
##
## Copyright (c) 2012 The Multiverse Foundation
## 
## Permission is hereby granted, free of charge, to any person 
## obtaining a copy of this software and associated documentation 
## files (the "Software"), to deal in the Software without restriction, 
## including without limitation the rights to use, copy, modify, 
## merge, publish, distribute, sublicense, and/or sell copies 
## of the Software, and to permit persons to whom the Software 
## is furnished to do so, subject to the following conditions:
## 
## The above copyright notice and this permission notice shall be 
## included in all copies or substantial portions of the Software.
## 
## THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
## EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
## OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
## NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
## HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
## WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
## FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
## OR OTHER DEALINGS IN THE SOFTWARE.
## 
## ********************************************************************

#!/bin/bash
export MV_HOME=".."
MV_BIN=${MV_BIN:-"${MV_HOME}/bin"}

# Optional: Set DEFAULT_MV_PROPERTYFILE env. variable to set the default property file, otherwise use multiverse.properties
# when no property file is specified as final argument on command line.
DEFAULT_MV_PROPERTYFILE=${DEFAULT_MV_PROPERTYFILE:-"multiverse.properties"}
MV_PROPERTYFILE=${MV_PROPERTYFILE:-"${MV_BIN}/${DEFAULT_MV_PROPERTYFILE}"}

DELETE_LOGS_ON_STARTUP=${DELETE_LOGS_ON_STARTUP:-$(java -cp ${MV_HOME}/dist/lib/multiverse.jar -Dmultiverse.propertyfile=${MV_PROPERTYFILE} multiverse.scripts.PropertyGetter multiverse.delete_logs_on_startup)}
MV_JAR=${MV_HOME}/build
LOG4J=${LOG4J:-"${MV_HOME}/other/log4j-1.2.14.jar"}
BCEL=${BCEL:-"${MV_HOME}/other/bcel-5.2.jar"}
MV_LOGS="${MV_HOME}/logs/inject"
MV_COMMON=${MV_COMMON:-"${MV_HOME}/config/common"}
if [ X"$1" != "X" ]; then
    MV_WORLDNAME=$1
else
    MV_WORLDNAME=${MV_WORLDNAME:-$(java -cp ${MV_HOME}/dist/lib/multiverse.jar -Dmultiverse.propertyfile=${MV_PROPERTYFILE} multiverse.scripts.PropertyGetter multiverse.worldname)}
fi
MV_WORLD=${MV_WORLD:-"$MV_HOME/config/$MV_WORLDNAME"}

if [ $(uname -o) == "Cygwin" ]; then
    MV_CLASSPATH="$MV_JAR;$LOG4J;$BCEL"
else
    MV_CLASSPATH="$MV_JAR:$LOG4J:$BCEL"
fi

JAVA_FLAGS="${JAVA_FLAGS} -cp ${MV_CLASSPATH} -Dmultiverse.logs=${MV_LOGS}"

if [ ! -d "${MV_LOGS}" ]; then
	mkdir -p "${MV_LOGS}"
fi
if [ X$DELETE_LOGS_ON_STARTUP = "Xtrue" ]; then
	rm "${MV_LOGS}"/*.out*
fi

rm -rf "${MV_HOME}"/inject/*

echo -en "Starting batch injection of marshalling methods ...\n"
java -Dmultiverse.log_level=0 $JAVA_FLAGS -ea multiverse.server.marshalling.InjectClassFiles -m "${MV_COMMON}"/mvmarshallers.txt -m "${MV_WORLD}"/worldmarshallers.txt -t "${MV_COMMON}"/typenumbers.txt -i "${MV_HOME}"/build -o "${MV_HOME}"/inject/

echo -en "Finished batch injection of marshalling methods\n"

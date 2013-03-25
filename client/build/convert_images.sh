#!/bin/bash
SRC_DIR="C:/tmp/Interface"
BLP_EXE="/cygdrive/c/tmp/blp2totga/BLP2toTGA.exe"
cd "${SRC_DIR}"
find ./ -name \*.tga -exec rm {} \;
echo         "  (list" > out.scm
for dir in `ls "${SRC_DIR}"`
do
	echo     "   (cons \"$dir\"" >> out.scm
	echo     "      '(" >> out.scm
	for file in `ls "${SRC_DIR}/${dir}/"*.blp`
	do
		fname=`echo "$file" | sed -e s'@.*/@@' | sed -e s'@\\..*@@'`
		"${BLP_EXE}" "${SRC_DIR}/${dir}/${fname}.blp"
		if [ "$?" -eq 0 ]
		then
			echo "        \"${fname}\"" >> out.scm
		fi
	done
	echo     "       ))" >> out.scm
done
echo         "     )" >> out.scm


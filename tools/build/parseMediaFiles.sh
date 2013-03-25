count=`wc -l meshs.txt | sed 's/meshs.txt//'`
echo $count
type=Meshes
echo "" > out.txt

COUNTER=1
while [ $COUNTER -le $count]; do
	head -$COUNTER meshs.txt | tail -1 | gawk "{ printf(\"<File Id='%s' DiskId='1' Name='FOO.TXT' LongName='%s' src='../../../Media/$type/%s' />\n\", \$0, \$0, \$0) }" >> out.txt
	let COUNTER=COUNTER+1
done

count=`wc -l materials.txt |sed 's/materials.txt//'`
echo $count
type=Materials

COUNTER=1
while [ $COUNTER -le $count]; do
	head -$COUNTER materials.txt | tail -1 | gawk "{ printf(\"<File Id='%s' DiskId='1' Name='FOO.TXT' LongName='%s' src='../../../Media/$type/%s' />\n\", \$0, \$0, \$0) }" >> out.txt
	let COUNTER=COUNTER+1
done

count=`wc -l skeletons.txt |sed 's/skeletons.txt//'`
echo $count
type=Skeletons

COUNTER=1
while [ $COUNTER -le $count]; do
	head -$COUNTER skeletons.txt | tail -1 | gawk "{ printf(\"<File Id='%s' DiskId='1' Name='FOO.TXT' LongName='%s' src='../../../Media/$type/%s' />\n\", \$0, \$0, \$0) }" >> out.txt
	let COUNTER=COUNTER+1
done

count=`wc -l textures.txt |sed 's/textures.txt//'`
echo $count
type=Textures

COUNTER=1
while [ $COUNTER -le $count]; do
	head -$COUNTER textures.txt | tail -1 | gawk "{ printf(\"<File Id='%s' DiskId='1' Name='FOO.TXT' LongName='%s' src='../../../Media/$type/%s' />\n\", \$0, \$0, \$0) }" >> out.txt
	let COUNTER=COUNTER+1
done
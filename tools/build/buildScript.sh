cd ./wix
../../wix/candle.exe MultiverseTools.wxs
../../wix/light.exe MultiverseTools.wixobj
cp MultiverseTools.msi ./BootStrapper/Packages/MultiverseTools/MultiverseTools.msi
rm -rf "/cygdrive/c/Program Files/Microsoft Visual Studio 8/SDK/v2.0/BootStrapper/Packages/MultiverseTools"
cp -r "BootStrapper/Packages/MultiverseTools" "/cygdrive/c/Program Files/Microsoft Visual Studio 8/SDK/v2.0/BootStrapper/Packages/"
echo "Open Command propt and type in the following commands:"
echo "cd \"C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\BootStrapper\Packages\MultiverseTools\""
echo "C:\\WINDOWS\\Microsoft.NET\\Framework\\v2.0.50727\\MSBuild.exe {WindowsTreeRootDir}\\Tools\\build\\wix\\MultiverseTools_build.xml"
echo "Setup.exe will be in {TreeRootDir}\\Tools\\build\wix\\publish"
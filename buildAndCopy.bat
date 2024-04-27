ECHO is on.
@echo off
echo Building Solution
set myDir=%CD%
cd Source

set newDir=%CD%
echo Building in: %newDir%
"C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" ArmorRocket.sln /t:Build /p:Configuration=Debug

cd C:\Users\bjupf\Desktop\RimWorldModding\ArmorRocket\Source\ArmorRocket\bin\Release
set copyPath = "%CD%/ArmorRocket.dll"

cd C:\Users\bjupf\Desktop\RimWorldModding\ArmorRocket\ArmorRocket\Common\Assemblies

copy "C:\Users\bjupf\Desktop\RimWorldModding\ArmorRocket\Source\ArmorRocket\bin\Debug\ArmorRocket.dll" "C:\Users\bjupf\Desktop\RimWorldModding\ArmorRocket\ArmorRocket\Common\Assemblies"
echo "Transfered Dll"

cd ..
cd ..
cd ..
echo Copying in: %myDir%

xcopy "C:\Users\bjupf\Desktop\RimWorldModding\ArmorRocket\ArmorRocket" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\ArmorRocket" /E /I /Y /Q
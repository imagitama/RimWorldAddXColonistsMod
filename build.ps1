# usage: .\build.ps1 path\to\rimworld\mods\folder

param (
    [string]$pathToMods
)

echo "Outputting to: $pathToMods\RimWorldAddXColonistsMod"

Push-Location

Set-Location "Source"

dotnet build

Pop-Location

echo "Copying..."

cp "Source\bin\Debug\net472\RimWorldAddXColonistsMod.dll" "Assemblies"

cp -r -Force "About" "${pathToMods}\RimWorldAddXColonistsMod\"
cp -r -Force "Assemblies" "${pathToMods}\RimWorldAddXColonistsMod\"
cp "README.md" "${pathToMods}\RimWorldAddXColonistsMod"
cp "LICENSE.md" "${pathToMods}\RimWorldAddXColonistsMod"

echo "Done"

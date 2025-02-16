param ([string] $mod)

if (Test-Path -Path "./temp") {
  Remove-Item -Path "./temp" -Recurse -Force
}
New-Item -ItemType Directory -Path "./temp" -Force
New-Item -ItemType Directory -Path "./temp/publish" -Force
New-Item -ItemType Directory -Path "./temp/output" -Force

if (-not (Test-Path -Path "./local")) {
  New-Item -ItemType Directory -Path "./local" -Force
}

if (-not (Test-Path -Path "./local/reloaded-tools")) {
  Write-Host "Downloading Reloaded Tools"
  Invoke-WebRequest -Uri "https://github.com/Reloaded-Project/Reloaded-II/releases/latest/download/Tools.zip" -OutFile "./temp/tools.zip"
  New-Item -ItemType Directory -Path "./local/reloaded-tools" -Force
  Expand-Archive -Path "./temp/tools.zip" -DestinationPath "./local/reloaded-tools"
  Remove-Item -Path "./temp/tools.zip" -Force
}

Write-Host "Publishing mod $mod"

dotnet restore ./$mod/$mod.csproj
dotnet clean ./$mod/$mod.csproj

$outputPath = Get-Item -Path "./temp/output" | Select-Object -ExpandProperty FullName
dotnet publish ./$mod/$mod.csproj -c Release --self-contained false -o "./temp/publish" /p:OutputPath="$outputPath"
Remove-Item -Path "./temp/output" -Recurse -Force
Get-ChildItem "./temp/publish" -Include *.pdb -Recurse | Remove-Item -Force -Recurse
Get-ChildItem "./temp/publish" -Include *.xml -Recurse | Remove-Item -Force -Recurse

Invoke-Expression "./local/reloaded-tools/Reloaded.Publisher.exe --modfolder ./temp/publish --packagename $mod --readmepath ./$mod/README.md --outputfolder ./temp/nuget --publishtarget NuGet"

Get-ChildItem -Path "./temp/publish" -Recurse

$apiKey = ""
if ($env:CI) {
  $apiKey = $env:NUGET_API_KEY
} else {
  $apiKey = (Get-Content -Path "./local/nuget.key").Trim()
}
$nugetFile = Get-ChildItem -Path "./temp/nuget" -Filter "$mod*.nupkg" | Select-Object -First 1

if (-not $env:CI) {
  Write-Host "Publishing to NuGet in 10 seconds..."
  Start-Sleep -Seconds 10
  Write-Host "aight no going back now"
}

dotnet nuget push -s ReloadedRepo -k $apiKey $nugetFile.FullName

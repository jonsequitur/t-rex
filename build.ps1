Param(
    [switch] $test
)

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1
$env:DOTNET_MULTILEVEL_LOOKUP = 0
$DotNetCliVersion = "2.1.300-preview1-008174"
$DotNetRoot = ".dotnet"
$DotNetInstallScript = Join-Path $DotNetRoot "dotnet-install.ps1"

function Create-Directory([string[]] $Path) {
    if (!(Test-Path -Path $Path)) {
        New-Item -Path $Path -Force -ItemType "Directory" | Out-Null
    }
}

Create-Directory $DotNetRoot
Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -UseBasicParsing -OutFile $DotNetInstallScript
Invoke-Expression -Command "$DotNetInstallScript -Version $DotNetCliVersion -InstallDir $DotNetRoot"

$env:PATH = "$DotNetRoot;$env:PATH"

dotnet build TRex.sln -r win10-x64
if ($test) {
    dotnet test TRexLib.Tests/TRexLib.Tests.csproj
}

$VersionSuffix = "devbuild"
if ($env:APPVEYOR_BUILD_VERSION -ne $Null -and $env:APPVEYOR_BUILD_VERSION -ne '') {
    $VersionSuffix=$env:APPVEYOR_BUILD_VERSION
}
dotnet pack t-rex/t-rex.csproj /p:AppendVersionSuffix=$VersionSuffix /p:packTool=true

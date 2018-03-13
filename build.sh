test=false
while [[ $# > 0 ]]; do
	lowerI="$(echo $1 | awk '{print tolower($0)}')"
	case $lowerI in
		--test)
			test=true
			shift 1
			;;
	esac
done

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_MULTILEVEL_LOOKUP=0
DotNetCliVersion="2.1.300-preview1-008174"
DotNetRoot=".dotnet"
DotNetInstallScript="$DotNetRoot/dotnet-install.sh"

function CreateDirectory() {
	if [ ! -d "$1" ]; then
		mkdir -p "$1"
	fi
}

CreateDirectory "$DotNetRoot"
curl "https://dot.net/v1/dotnet-install.sh" -sSL -o "$DotNetInstallScript"
bash "$DotNetInstallScript" --version $DotNetCliVersion --install-dir $DotNetRoot

export PATH="$DotNetRoot:$PATH"

dotnet build TRex.sln -r win10-x64
if [ $test == "true" ]; then
	dotnet test TRexLib.Tests/TRexLib.Tests.csproj
fi

VersionSuffix="devbuild"
if [ ! -z "$APPVEYOR_BUILD_VERSION" ]; then
	VersionSuffix="$APPVEYOR_BUILD_VERSION"
fi

dotnet pack t-rex/t-rex.csproj /p:AppendVersionSuffix=$VersionSuffix /p:packTool=true

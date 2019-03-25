function Get-ScriptDirectory {
    Split-Path -parent $PSCommandPath
}

dotnet pack /p:Version=0.0.0
dotnet tool uninstall -g t-rex
dotnet tool install -g --add-source "$(Get-ScriptDirectory)/bin/debug" --version 0.0.0 t-rex
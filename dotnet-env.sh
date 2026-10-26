# Source this file from bash or zsh to use the project's local SDK.
if [ -n "${ZSH_VERSION:-}" ]; then
    HCL_ENV_FILE="${(%):-%x}"
else
    HCL_ENV_FILE="${BASH_SOURCE[0]}"
fi
HCL_PROJECT_ROOT="$(cd "$(dirname "$HCL_ENV_FILE")" && pwd)"
export DOTNET_ROOT="$HCL_PROJECT_ROOT/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_HOME="$HCL_PROJECT_ROOT/.dotnet-cli"
export NUGET_PACKAGES="$HCL_PROJECT_ROOT/.nuget/packages"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
unset HCL_ENV_FILE HCL_PROJECT_ROOT

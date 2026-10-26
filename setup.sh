#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export DOTNET_ROOT="$PROJECT_ROOT/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_HOME="$PROJECT_ROOT/.dotnet-cli"
export NUGET_PACKAGES="$PROJECT_ROOT/.nuget/packages"
export DOTNET_CLI_TELEMETRY_OPTOUT=1

if [[ ! -x "$DOTNET_ROOT/dotnet" ]] || ! "$DOTNET_ROOT/dotnet" --list-sdks | grep -q '^8\.'; then
    command -v curl >/dev/null || { echo "curl is required to download the .NET installer." >&2; exit 1; }
    installer="$(mktemp "${TMPDIR:-/tmp}/hcl-dotnet-install.XXXXXX")"
    trap 'rm -f "$installer"' EXIT
    echo "Downloading Microsoft's .NET installer..."
    curl --fail --silent --show-error --location --retry 2 \
        https://dot.net/v1/dotnet-install.sh --output "$installer"
    bash "$installer" --channel 8.0 --install-dir "$DOTNET_ROOT" --no-path
fi

"$DOTNET_ROOT/dotnet" --info
for project in NumberGuessingGame SimpleCalculator UnitConverter ToDoList StudentGradeManager QuizApp BankAccountSimulator PasswordGenerator PersonalNotes WeatherApp ContactBook; do
    "$DOTNET_ROOT/dotnet" build "$PROJECT_ROOT/$project/$project.csproj"
done

echo "Setup complete. From the project root, run:"
echo "  source ./dotnet-env.sh"
echo "  dotnet run --project NumberGuessingGame"

#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$PROJECT_ROOT"
if [[ -x "$PROJECT_ROOT/.dotnet/dotnet" ]]; then
    source "$PROJECT_ROOT/dotnet-env.sh"
fi
command -v dotnet >/dev/null || { echo "Install the .NET 8 SDK with: bash setup.sh" >&2; exit 1; }
children=()
cleanup() {
    for child in "${children[@]}"; do kill "$child" 2>/dev/null || true; done
}
trap cleanup EXIT
trap 'exit 130' INT TERM

PROJECTS=(
    NumberGuessingGame
    SimpleCalculator
    UnitConverter
    ToDoList
    StudentGradeManager
    QuizApp
    BankAccountSimulator
    PasswordGenerator
    PersonalNotes
    WeatherApp
    ContactBook
)

for project in "${PROJECTS[@]}"; do
    dotnet build "$project" --nologo
done

port=5101
for project in "${PROJECTS[@]}"; do
    dotnet run --no-build --no-launch-profile --project "$project" --urls "http://localhost:$port" &
    children+=("$!")
    echo "$project: http://localhost:$port"
    port=$((port + 1))
done

echo "All eleven apps are starting on ports 5101–5111."
echo "Press Ctrl+C to stop all eleven apps."
while true; do
    for child in "${children[@]}"; do
        if ! kill -0 "$child" 2>/dev/null; then
            echo "An app stopped. Check the server output above." >&2
            exit 1
        fi
    done
    sleep 1
done

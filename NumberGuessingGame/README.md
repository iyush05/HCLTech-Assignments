# NumberGuessingGame

An independent C# / ASP.NET Core 8 app with a responsive browser frontend.

## Run in your browser

From the repository root, install the SDK once if needed with `bash setup.sh`.
Then run:

```sh
source ./dotnet-env.sh
cd NumberGuessingGame
dotnet run
```

Open **http://localhost:5101**. Stop the server with Ctrl+C.
If you installed the .NET 8 SDK system-wide, skip the `source` command.
The sidebar links use ports 5101–5108; start all eight apps to use those links.
You can run `bash run-all.sh` from the repository root to start all eight apps together.

## Browser features

Guess a server-held number from 1 to 100. Follow higher/lower hints, track attempts, and start a new round. The possible range narrows with each valid guess. Sessions expire after two hours of inactivity; reloading starts a new round. Tabs in the same browser share a round.

The frontend uses local HTML, CSS, and JavaScript in `wwwroot/`. The ASP.NET Core
endpoints in `Program.cs` perform the calculations and input validation. No npm,
external fonts, CDN assets, or third-party NuGet packages are required.
Error messages are announced to assistive technology; controls support keyboard
navigation and the layout adapts to mobile screens.

## Original console exercise

```sh
dotnet run -- --console
```

The original syntax, loop, condition, and method examples are preserved in
`ConsoleApp.cs`. The checks below describe the console interface; the same
validations apply to the browser controls.

## Manual checks

1. Enter `hello`, `0`, and `101`: each should be rejected without counting an attempt.
2. Guess `50`, then repeatedly halve the remaining range using the hints.
3. On a correct guess, verify the displayed count matches your valid guesses.
4. Enter `y` to replay: the attempt count starts over.
5. Enter `q` during a round: the game exits cleanly.


# SimpleCalculator

An independent C# / ASP.NET Core 8 app with a responsive browser frontend.

## Run in your browser

From the repository root, install the SDK once if needed with `bash setup.sh`.
Then run:

```sh
source ./dotnet-env.sh
cd SimpleCalculator
dotnet run
```

Open **http://localhost:5102**. Stop the server with Ctrl+C.
If you installed the .NET 8 SDK system-wide, skip the `source` command.
The sidebar links use ports 5101–5108; start all eight apps to use those links.
You can run `bash run-all.sh` from the repository root to start all eight apps together.

## Browser features

Choose addition, subtraction, multiplication, or division. Type into either operand or use the number pad; Next switches fields. Press Enter to calculate. History is held in the current page and Clear all resets it. The C# endpoint uses decimal arithmetic and rejects division by zero and overflow.

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

| Operation | First | Second | Expected result |
| --- | --- | --- | --- |
| `+` | 12.5 | 7.5 | 20 |
| `-` | 4 | 9 | -5 |
| `*` | -3 | 4 | -12 |
| `/` | 7 | 2 | 3.5 |
| `/` | 5 | 0 | Division-by-zero message; menu returns |
| `*` | 79228162514264337593543950335 | 2 | Range error; menu returns |

Also try an unsupported operation, `hello` at a number prompt, and `q` at each
type of prompt. Invalid input should not crash the program.


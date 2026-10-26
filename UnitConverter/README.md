# UnitConverter

An independent C# / ASP.NET Core 8 app with a responsive browser frontend.

## Run in your browser

From the repository root, install the SDK once if needed with `bash setup.sh`.
Then run:

```sh
source ./dotnet-env.sh
cd UnitConverter
dotnet run
```

Open **http://localhost:5103**. Stop the server with Ctrl+C.
If you installed the .NET 8 SDK system-wide, skip the `source` command.
The sidebar links use ports 5101–5108; start all eight apps to use those links.
You can run `bash run-all.sh` from the repository root to start all eight apps together.

## Browser features

Choose distance, weight, or temperature; pick a direction or swap units. Enter a value and click Convert. Formula and reference cards update with the category. Negative distance/weight, temperatures below absolute zero, nonfinite input, and overflowing results are rejected.

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

| Option | Input | Expected result (approximately) |
| --- | --- | --- |
| 1 | 1.609344 | 1 mile |
| 2 | 1 | 1.609344 kilometers |
| 3 | 0.45359237 | 1 pound |
| 4 | 1 | 0.45359237 kilograms |
| 5 | 0 | 32 degrees Fahrenheit |
| 6 | 212 | 100 degrees Celsius |

Also check invalid menu choices, text input, negative distance, Celsius below
`-273.15`, and `NaN`/`Infinity`. These should be rejected. Converting `1.7e308`
miles to kilometers should report an oversized result. Entering `q` should exit.


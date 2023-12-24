[![NuGet version](https://badge.fury.io/nu/ProcessOutputUtility.svg)](https://badge.fury.io/nu/ProcessOutputUtility)

# ProcessOutputUtility
Utility library related to process output.

# Getting Started
## Nuget Install
PM> Install-Package ProcessOutputUtility

## Example
```
using ProcessOutputUtility;
namespace Tests;
public async Task Test1Async()
{
    // ProcessInfo
	var processStartInfo = new ProcessStartInfo()
	{
		FileName = "cmd.exe",
		ArgumentList = { "/c", "echo start & echo end" },
	};
	var process = new Process() { StartInfo = processStartInfo };
    // Run Process
    var exitCode = await process.RunAsync(
		onReceived: (info) =>
		{
			if (!info.Eof)
			{
				var dateTimePrefix = info.Time.ToString("[yyyy/MM/dd HH:mm:ss.fff]");
				switch (info.Type)
				{
					case ProcessOutputType.Output:
						Console.WriteLine($"{dateTimePrefix}{info.Line}");
						break;
					case ProcessOutputType.Error:
						Console.Error.WriteLine($"{dateTimePrefix}{info.Line}");
						break;
				}
			}
			return ValueTask.CompletedTask;
		}, cancellationToken: default);
}
```

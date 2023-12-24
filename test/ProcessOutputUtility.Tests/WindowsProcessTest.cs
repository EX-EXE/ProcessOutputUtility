using System.Diagnostics;
using System.Text;
using ProcessOutputUtility;

namespace ProcessOutputUtility.Tests;

public class WindowsProcessTest
{
	[Fact]
	public async Task Test1Async()
	{
		var startInfo = new ProcessStartInfo()
		{
			FileName = "cmd.exe",
			ArgumentList = { "/c", "echo start & echo end" },
		};
		var process = new Process()
		{
			StartInfo = startInfo
		};
		var outputLines = new StringBuilder();
		await process.RunAsync((info)=>
		{
			if (!info.Eof)
			{
				outputLines.AppendLine(info.Line.Trim());
			}
			return ValueTask.CompletedTask;
		});
		var output = outputLines.ToString();
		Assert.True(output == $"start{Environment.NewLine}end{Environment.NewLine}");
	}

	[Fact]
	public async Task Test2Async()
	{
		var startInfo = new ProcessStartInfo()
		{
			FileName = "cmd.exe",
			ArgumentList = { "/c", "echo Error 1>&2 & echo Output" },
		};
		var process = new Process()
		{
			StartInfo = startInfo
		};
		var output = string.Empty;
		var error = string.Empty;
		await process.RunAsync((info) =>
		{
			if (!info.Eof)
			{
				switch (info.Type)
				{
					case ProcessOutputType.Output:
						output = info.Line.Trim();
						break;
					case ProcessOutputType.Error:
						error = info.Line.Trim();
						break;
				}
			}
			return ValueTask.CompletedTask;
		});
		Assert.True(output == $"Output");
		Assert.True(error == $"Error");
	}
}
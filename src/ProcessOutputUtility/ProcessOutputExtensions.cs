using System.Diagnostics;
using System.Threading.Channels;

namespace ProcessOutputUtility;

/// <summary>
/// OutputType
/// </summary>
public enum ProcessOutputType
{
	/// <summary>
	/// Output
	/// </summary>
	Output,

	/// <summary>
	/// Error
	/// </summary>
	Error,
}

/// <summary>
/// OutputType
/// </summary>
public class ProcessOutputInfo
{
	/// <summary>
	/// Time
	/// </summary>
	public DateTimeOffset Time { get; init; } = DateTimeOffset.Now;

	/// <summary>
	/// Type
	/// </summary>
	public ProcessOutputType Type { get; init; } = ProcessOutputType.Output;

	/// <summary>
	/// Line
	/// </summary>
	public string Line { get; set; } = string.Empty;

	/// <summary>
	/// EOF
	/// </summary>
	public bool Eof { get; set; } = false;
}

public static partial class ProcessExtensions
{
	/// <summary>
	/// Start And Wait Process.
	/// </summary>
	/// <param name="process"></param>
	/// <param name="timeProvider"></param>
	/// <param name="onReceivedOutput"></param>
	/// <param name="onReceivedError"></param>
	/// <param name="onReceivedException"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public static async ValueTask RunAsync(
		this Process process,
		TimeProvider timeProvider,
		Func<ProcessOutputInfo, ValueTask>? onReceivedOutput,
		Func<ProcessOutputInfo, ValueTask>? onReceivedError,
		Func<Exception, ValueTask>? onReceivedException,
		CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var outputChannel = Channel.CreateUnbounded<ProcessOutputInfo>();
		var outputReader = outputChannel.Reader;
		var outputWriter = outputChannel.Writer;
		var outputEventTask = Task.Run(async () =>
		{
			var info = (ProcessOutputInfo?)default;
			while (await outputReader.WaitToReadAsync(default).ConfigureAwait(false))
			{
				while (outputReader.TryRead(out info))
				{
					try
					{
						switch (info.Type)
						{
							case ProcessOutputType.Output:
								if (onReceivedOutput != null)
								{
									await onReceivedOutput.Invoke(info).ConfigureAwait(false);
								}
								break;
							case ProcessOutputType.Error:
								if (onReceivedError != null)
								{
									await onReceivedError.Invoke(info).ConfigureAwait(false);
								}
								break;
							default:
								throw new NotImplementedException($"NotImplemented {info.Type}.");
						}
					}
					catch (Exception ex)
					{
						if (onReceivedException != null)
						{
							try
							{
								await onReceivedException.Invoke(ex).ConfigureAwait(false);
							}
							catch { }
						}
					}
				}
			}
		}, cancellationToken);

		// ReceivedEvent
		DataReceivedEventHandler outputReceivedEventHandler = (sender, e) =>
		{
			outputWriter.TryWrite(new ProcessOutputInfo()
			{
				Time = timeProvider.GetLocalNow(),
				Type = ProcessOutputType.Output,
				Line = e.Data ?? string.Empty,
				Eof = e.Data == null
			});
		};
		DataReceivedEventHandler errorReceivedEventHandler = (sender, e) =>
		{
			outputWriter.TryWrite(new ProcessOutputInfo()
			{
				Time = timeProvider.GetLocalNow(),
				Type = ProcessOutputType.Error,
				Line = e.Data ?? string.Empty,
				Eof = e.Data == null
			});
		};
		process.OutputDataReceived += outputReceivedEventHandler;
		process.ErrorDataReceived += errorReceivedEventHandler;

		// ProcessStart
		try
		{
			process.EnableRaisingEvents = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			var started = process.Start();
			if (started)
			{
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				try
				{
					await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
				}
				finally
				{
					process.CancelErrorRead();
					process.CancelOutputRead();
				}
			}
		}
		finally
		{
			process.OutputDataReceived -= outputReceivedEventHandler;
			process.ErrorDataReceived -= errorReceivedEventHandler;
			// eventTask Wait
			outputWriter.Complete();
			outputEventTask.Wait();
		}
	}
}

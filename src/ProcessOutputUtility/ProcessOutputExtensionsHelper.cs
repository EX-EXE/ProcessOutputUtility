using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOutputUtility;

public static partial class ProcessExtensions
{
	public static ValueTask RunAsync(
		this Process process,
		CancellationToken cancellationToken = default)
	{
		return RunAsync(
			process,
			TimeProvider.System,
			null,
			null,
			null,
			cancellationToken);
	}

	public static ValueTask RunAsync(
		this Process process,
		Func<ProcessOutputInfo, ValueTask>? onReceived,
		CancellationToken cancellationToken = default)
	{
		return RunAsync(
			process,
			TimeProvider.System,
			onReceived,
			onReceived,
			null,
			cancellationToken);
	}

	public static ValueTask RunAsync(
		this Process process,
		Func<ProcessOutputInfo, ValueTask>? onReceived,
		Func<Exception, ValueTask>? onReceivedException,
		CancellationToken cancellationToken = default)
	{
		return RunAsync(
			process,
			TimeProvider.System,
			onReceived,
			onReceived,
			onReceivedException,
			cancellationToken);
	}

	public static ValueTask RunAsync(
		this Process process,
		Func<ProcessOutputInfo, ValueTask>? onReceivedOutput,
		Func<ProcessOutputInfo, ValueTask>? onReceivedError,
		Func<Exception, ValueTask>? onReceivedException,
		CancellationToken cancellationToken = default)
	{
		return RunAsync(
			process,
			TimeProvider.System,
			onReceivedOutput,
			onReceivedError,
			onReceivedException,
			cancellationToken);
	}
}

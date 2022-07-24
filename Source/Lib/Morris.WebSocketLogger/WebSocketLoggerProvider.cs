using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace Morris.WebSocketLogger;

[ProviderAlias(nameof(WebSocketLogger))]
public class WebSocketLoggerProvider : ILoggerProvider
{
	private readonly ConcurrentDictionary<string, WebSocketLogger> Loggers = new(StringComparer.OrdinalIgnoreCase);
	private readonly WebSocketLoggerConnection Connection;
	private readonly IOptionsMonitor<Configuration> Configuration;
	private volatile int IsDisposed;

	public WebSocketLoggerProvider(IOptionsMonitor<Configuration> configuration)
	{
		Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		Connection = new WebSocketLoggerConnection(configuration);
	}

	public ILogger CreateLogger(string categoryName)
	{
		if (IsDisposed != 0)
			throw new ObjectDisposedException($"{nameof(WebSocketLoggerProvider)} is already disposed");
		
		return Loggers.GetOrAdd(
			key: categoryName, 
			valueFactory: x => new WebSocketLogger(x, Configuration, Connection));
	}

	void IDisposable.Dispose()
	{
		if (Interlocked.CompareExchange(ref IsDisposed, 1, 0) != 0) return;
		Loggers.Clear();
		Connection.Dispose();
	}
}
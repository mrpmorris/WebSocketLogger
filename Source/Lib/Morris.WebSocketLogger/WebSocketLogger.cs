using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Morris.WebSocketLogger;

internal sealed class WebSocketLogger : ILogger
{
	private readonly string Name;
	private readonly IOptionsMonitor<Configuration> Configuration;
	private readonly WebSocketLoggerConnection Connection;

	public WebSocketLogger(
		string name,
		IOptionsMonitor<Configuration> configuration,
		WebSocketLoggerConnection connection)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		Connection = connection ?? throw new ArgumentNullException(nameof(connection));
	}

	public IDisposable BeginScope<TState>(TState state) => default!;
	public bool IsEnabled(LogLevel logLevel) => logLevel >= Configuration.CurrentValue.LogLevel;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		string message = $"[{eventId.Id,2}: {logLevel,-12}]  {Name} - {formatter(state, exception)}";
		_ = Connection.LogAsync(message);
	}
}
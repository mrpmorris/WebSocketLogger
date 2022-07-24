using Microsoft.Extensions.Logging;
using System;

namespace Morris.WebSocketLogger;

public class Configuration
{
	public Uri Uri { get; set; } = new Uri("wss://localhost:6510");
	public LogLevel LogLevel { get; set; } = LogLevel.Information;
}
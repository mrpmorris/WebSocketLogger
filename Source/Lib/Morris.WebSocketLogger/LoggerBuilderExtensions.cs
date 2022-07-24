using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;

namespace Morris.WebSocketLogger;

public static class LoggerBuilderExtensions
{
	public static ILoggingBuilder AddWebSocketLogger(this ILoggingBuilder builder)
	{
		builder.AddConfiguration();

		builder.Services.TryAddEnumerable(
			ServiceDescriptor.Singleton<ILoggerProvider, WebSocketLoggerProvider>());

		LoggerProviderOptions.RegisterProviderOptions
			<Configuration, WebSocketLoggerProvider>(builder.Services);

		return builder;
	}

	public static ILoggingBuilder AddWebSocketLogger(
		this ILoggingBuilder builder,
		Action<Configuration> configure)
	{
		builder.AddWebSocketLogger();
		builder.Services.Configure(configure);

		return builder;
	}	
}
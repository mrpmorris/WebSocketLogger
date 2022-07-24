using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Morris.WebSocketLogger;

internal sealed class WebSocketLoggerConnection : IDisposable
{
	private readonly IOptionsMonitor<Configuration> Configuration;
	private readonly Channel<string> SendQueue;
	private readonly CancellationTokenSource DisposedTokenSource = new();
	private ClientWebSocket? WebSocket;
	private CancellationToken DisposedToken => DisposedTokenSource.Token;

	public WebSocketLoggerConnection(IOptionsMonitor<Configuration> configuration)
	{
		Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		SendQueue = Channel.CreateBounded<string>(new BoundedChannelOptions(600));
		_ = Task.Run(EventLoop);
	}

	public async Task LogAsync(string? message)
	{
		if (message is not null && !DisposedToken.IsCancellationRequested)
			await SendQueue.Writer.WriteAsync(message, DisposedToken);
	}

	public void Dispose()
	{
		if (DisposedToken.IsCancellationRequested) return;

		DisposedTokenSource.Cancel();
		WebSocket? originalWebSocket = WebSocket;
		WebSocket = null;
		originalWebSocket?.Dispose();
	}

	private async Task EventLoop()
	{
		while (!DisposedToken.IsCancellationRequested)
		{
			string message = await SendQueue.Reader.ReadAsync(DisposedToken);

			if (DisposedToken.IsCancellationRequested) return;
			if (SendQueue.Reader.Count > 500) continue;

			try
			{
				if (WebSocket?.State != WebSocketState.Open)
				{
					Uri uri = Configuration.CurrentValue.Uri;
					WebSocket = new ClientWebSocket();
					await WebSocket.ConnectAsync(uri, CancellationToken.None);
					if (DisposedToken.IsCancellationRequested) return;
				}
			}
			catch (OperationCanceledException)
			{
				return;
			}
			catch (Exception)
			{
				// ignored
			}

			await SendMessageAsync(message);
		}
	}

	private Task SendMessageAsync(string message)
	{
		byte[] buffer = Encoding.UTF8.GetBytes(message);
		return WebSocket?.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage: true, DisposedToken) ?? Task.CompletedTask;
	}
}
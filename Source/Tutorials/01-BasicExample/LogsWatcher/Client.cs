using System.Net.WebSockets;
using System.Text;

namespace LogsWatcher;

public class Client
{
	private static int NextClientId = 0;
	private readonly WebSocket WebSocket;
	private readonly int ClientId;

	public Client(WebSocket webSocket)
	{
		WebSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
		ClientId = Interlocked.Increment(ref NextClientId);
	}

	public async ValueTask ListenAsync()
	{
		LogOutput.WriteLine(ClientId, "Connected");

		byte[] buffer = new byte[1024];

		try
		{
			while (WebSocket.State == WebSocketState.Open)
			{
				var builder = new StringBuilder();

				WebSocketReceiveResult readResult = null!;

				do
				{
					readResult = await WebSocket.ReceiveAsync(buffer, CancellationToken.None);

					if (WebSocket.State != WebSocketState.Open)
						return;

					string data = Encoding.UTF8.GetString(buffer, 0, readResult.Count);
					builder.Append(data);
				} while (!readResult!.EndOfMessage);

				string message = builder.ToString();

				LogOutput.WriteLine(ClientId, message);
			}
		}
		catch (Exception)
		{
			// ignored
		}
		finally
		{
			LogOutput.WriteLine(ClientId, "Disconnected");
		}
	}
}
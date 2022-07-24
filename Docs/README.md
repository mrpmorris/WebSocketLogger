# Morris.WebSocketLogger - documentation
![Logo](./../Images/small-logo.png)


## Instructions


### Creating an app that logs
1. Create a new web app.
2. Add a reference to Morris.WebSocketLogger
3. Add the following code to `Program.cs`

```c#
using Morris.WebSocketLogger;

// ...Other code here... 

builder.Services.AddLogging(x =>
{
	x.AddWebSocketLogger();
});
```

4. Edit HomeController.
5. In each action, make a call to `_logger.LogInformation(..some text...);`


### Create a logs watcher
1. Create a new web app
2. Add a reference to Morris.WebSocketLogger
3. Delete everything in the app except
  * Properties folder
  * appsettings.json
  * appsettings.Development.json
  * Program.cs
4. Create an output class (`LogOutput.cs`) to output received logs in a thread safe manner.

```c#
static class LogOutput
{
	private static readonly object SyncRoot = new();

	public static void WriteLine(int? clientId, string text)
	{
		string id =
			clientId is null
				? ""
				: $"{clientId}: ";
		
		lock (SyncRoot)
			Console.WriteLine($"{id}{text}");
	}
}
```
5. Create a `Client` class that listens for data from a WebSocket and logs to `LogOutput`. 

```c#
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
```

6. Finally, change `Program.cs` to list for WebSocket connections and creates a `Client` for each connection.

```c#
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();
app.UseWebSockets();
app.MapGet("/",
	async(ctx) =>
	{
		if (!ctx.WebSockets.IsWebSocketRequest)
		{
			ctx.Response.StatusCode = 400;
			return;
		}

		using WebSocket webSocket = await ctx.WebSockets.AcceptWebSocketAsync();
		await new Client(webSocket).ListenAsync();
	});
app.Run();
```

7. Run both apps, and watch the console output as you navigate between pages.


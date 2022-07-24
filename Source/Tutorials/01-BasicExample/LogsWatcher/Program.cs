using LogsWatcher;
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
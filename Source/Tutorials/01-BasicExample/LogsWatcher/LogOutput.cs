namespace LogsWatcher;

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
using System.Net.WebSockets;
using System.Text;
using System.Diagnostics;

public enum GlazeCommandType
{
	QUERY, COMMAND, SUB
}

public class GlazeClient
{
	ClientWebSocket client = new();
	CancellationTokenSource cts = new();
	Uri glazeUri = new("ws://localhost:6123");
	WebSocketReceiveResult result;

	string lastReply = "";

	public delegate void ReplyRecievedHandler(string reply);
	public event ReplyRecievedHandler REPLY_RECIEVED = (msg) => { };

	public GlazeClient()
	{
		client.ConnectAsync(glazeUri, cts.Token).Wait();
		Task.Run(async () => { await ReadToBuffer(); });
	}

	async Task ReadToBuffer()
	{
		byte[] buffer = new byte[4096 * 4];
		while ((result = await client.ReceiveAsync(buffer, cts.Token)).Count > 0)
		{
			lastReply += Encoding.UTF8.GetString(buffer, 0, result.Count);
			Array.Clear(buffer);
			if (result.EndOfMessage)
			{
				if (commandMode)
				{
					commandReplyRecieved = true;
				}
				else
				{
					REPLY_RECIEVED(lastReply);
					lastReply = "";
				}
			}
		}
	}

	bool commandMode = false;
	bool commandReplyRecieved = false;
	public async Task<string> SendCommand(string command)
	{
		commandMode = true;
		commandReplyRecieved = false;
		await client.SendAsync(Encoding.UTF8.GetBytes(command), WebSocketMessageType.Text, true, cts.Token);
		while (!commandReplyRecieved)
		{
			Debug.WriteLine("reached");
			await Task.Delay(500);
		}
		commandMode = false;
		string reply = lastReply;
		lastReply = "";
		return reply;
	}

	//static async Task Main()
	//{
	//	var client = new GlazeClient();
	//	string msg;
	//	Debug.Write("Enter: ");
	//	while ((msg = Debug.ReadLine()) != ":q")
	//	{
	//		string reply = await client.SendCommand(msg);
	//		Debug.WriteLine($"reply: {reply}");
	//		Debug.Write("Enter: ");
	//	}
	//}
}

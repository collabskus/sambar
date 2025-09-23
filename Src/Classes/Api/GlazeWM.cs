/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace sambar;

public partial class Api
{
	// dont initialize in fields anything that would block/hang the api instance
	// initialization, GlazeClient waits on reply, so init in separate thread or async 
	// function
	GlazeClient client;
	private async void GlazeInit()
	{
		if (Process.GetProcessesByName("glazewm").Length < 1) return;

		// The order is important, when sending subscription notification to glaze, the event 
		// handler must already be attached inorder to capture the response. We are processing 
		// this response  in GlazeEventHandler but we need all the active glaze workspaces 
		// to do so, therfore before sending the subscription run GetAllWorkspaces()
		client = new();
		client.REPLY_RECIEVED += GlazeEventHandler;
		await GetAllWorkspaces();
		await SubscribeToGlazeWMEvents();
		Logger.Log($"GlazeInit() => Workspaces: {workspaces.Count}");
	}

	public delegate void GlazeWorkspaceChangedHandler(Workspace workspace);
	public event GlazeWorkspaceChangedHandler GLAZE_WORKSPACE_CHANGED = (workspace) => { };

	public Workspace currentWorkspace = new();
	public List<Workspace> workspaces = new();

	private async Task GetAllWorkspaces()
	{
		string message = "query workspaces";
		Logger.Log("querying all workspaces");
		string reply = await client.SendCommand(message);
		Logger.Log($"SendCommand: {reply}");
		Message msg = JsonConvert.DeserializeObject<Message>(reply);
		if (msg.clientMessage == message)
		{
			int i = 0;
			foreach (Container workspace in msg.data.workspaces)
			{
				Workspace wksp = new();
				wksp.index = i;
				wksp.id = workspace.id;
				wksp.name = workspace.name;
				workspaces.Add(wksp);
				if (workspace.hasFocus == true)
				{
					currentWorkspace = wksp;
				}
				i++;
			}
		}
	}

	private void GlazeEventHandler(string message)
	{
		Logger.Log("glaze_event: " + message);
		Message msg = JsonConvert.DeserializeObject<Message>(message);
		switch (msg.messageType)
		{
			case "event_subscription":
				string focusedWorkspaceId = null;
				if (msg.data.focusedContainer.type == "window")
				{
					focusedWorkspaceId = msg.data.focusedContainer.parentId;
				}
				else if (msg.data.focusedContainer.type == "workspace")
				{
					focusedWorkspaceId = msg.data.focusedContainer.id;
				}
				currentWorkspace = workspaces.Where(wksp => wksp.id == focusedWorkspaceId).First();
				GLAZE_WORKSPACE_CHANGED(currentWorkspace);
				break;
		}
	}

	string? glazeSubscriptionId;
	private async Task SubscribeToGlazeWMEvents()
	{
		string command = $"sub --events focus_changed";
		string reply = await client.SendCommand(command);
		Logger.Log($"subscribe reply: {reply}");
		try
		{
			Message? replyMessage = JsonConvert.DeserializeObject<Message>(reply);
			glazeSubscriptionId = replyMessage?.data.subscriptionId;
			Logger.Log($"subscriptionId: {glazeSubscriptionId}");
		}
		catch (Exception ex)
		{
			Logger.Log($"[ JSON ERROR ]: {ex.Message}");
		}
	}

	internal async Task UnsubToGlazeWMEvents()
	{
		string command = $"unsub --id {glazeSubscriptionId}";
		string reply = await client.SendCommand(command);
		Logger.Log($"unsub reply: {reply}");
	}

	public async Task ChangeWorkspace(Workspace newWorkspace)
	{
		string message = $"command focus --workspace {newWorkspace.name}";
		await client.SendCommand(message);
	}

	private async void GlazeCleanup()
	{
		await UnsubToGlazeWMEvents();
	}
}

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
			Logger.Log("reached");
			await Task.Delay(500);
		}
		commandMode = false;
		string reply = lastReply;
		lastReply = "";
		return reply;
	}
}

public class Workspace
{
	public int index;
	public string id;
	public string name;
}

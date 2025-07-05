using System.Diagnostics;
using Newtonsoft.Json;

namespace sambar;

public partial class Api
{
    GlazeClient client = new();
    public async void GlazeInit()
    {
        await SubscribeToGlazeWMEvents();
		client.REPLY_RECIEVED += GlazeEventHandler;
		await GetAllWorkspaces();
    }

    public delegate void GlazeWorkspaceChangedHandler(Workspace workspace);
    public event GlazeWorkspaceChangedHandler GLAZE_WORKSPACE_CHANGED = (workspace) => { };

	public Workspace currentWorkspace = new();
    public List<Workspace> workspaces = new();

	public async Task GetAllWorkspaces()
	{
		string message = "query workspaces";
		Debug.WriteLine("querying all workspaces");
		string reply = await client.SendCommand(message);
		Debug.WriteLine($"SendCommand: {reply}");
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

	public void GlazeEventHandler(string message)
	{
		Debug.WriteLine("glaze_event: " + message);
		Message msg = JsonConvert.DeserializeObject<Message>(message);
		switch (msg.messageType)
		{
			case "event_subscription":
				string focusedWorkspaceId = null;
				if (msg.data.focusedContainer.type == "window")
				{
					focusedWorkspaceId = msg.data.focusedContainer.parentId;
				}
				else if (msg.data.focusedContainer.type == "workspace") {
					focusedWorkspaceId = msg.data.focusedContainer.id;	
				}
				currentWorkspace = workspaces.Where(wksp => wksp.id == focusedWorkspaceId).First();
				GLAZE_WORKSPACE_CHANGED(currentWorkspace);
				break;
		}
	}

	public async Task SubscribeToGlazeWMEvents()
	{
		string command = $"sub --events focus_changed";
		string reply = await client.SendCommand(command);
		Debug.WriteLine($"subscribe reply: {reply}");
		Message replyMessage = JsonConvert.DeserializeObject<Message>(reply);
		Debug.WriteLine($"subscriptionId: {replyMessage.data.subscriptionId}");
	}

	public async Task ChangeWorkspace(Workspace newWorkspace)
	{
		string message = $"command focus --workspace {newWorkspace.name}";
		await client.SendCommand(message);
	}
}

public class Workspace
{
	public int index;
	public string id;
	public string name;
	//public RoundedButton button;
}

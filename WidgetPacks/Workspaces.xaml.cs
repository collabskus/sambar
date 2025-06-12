using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.Json;
using Newtonsoft.Json;
using System.Diagnostics;

namespace sambar;

public class Workspace
{
	public int index;
	public string id;
	public string name;
	public RoundedButton button;
}

public partial class Workspaces : UserControl
{
	int currentUiWorkspace = 0;
	List<Workspace> workspaces = new();
	GlazeClient client;
	Brush buttonDefault = new SolidColorBrush(Colors.Orange);
	Brush buttonSelected = new SolidColorBrush(Colors.DarkGray);
	Brush buttonTextColor = new SolidColorBrush(Colors.White);
	public Workspaces()
	{
		InitializeComponent();
		//Win32.AttachDebug(-1);

		Task.Run(InitAsync);
	}

	public async Task InitAsync()
	{
		Debug.WriteLine("Glaze Client initialization");
		client = new();
		Debug.WriteLine("subscribing to glaze events");
		await SubscribeToGlazeWMEvents();
		Debug.WriteLine("subbing");
		client.REPLY_RECIEVED += GlazeEventHandler;
		Debug.WriteLine("GetAllWorkspaces();");
		await GetAllWorkspaces();

		await this.Dispatcher.InvokeAsync(() =>
		{
			WorkspaceBorder.CornerRadius = new(5);
			Debug.WriteLine($"reached: {workspaces.Count}");
			for (int i = 1; i <= workspaces.Count; i++)
			{
				RoundedButton btn = new();
				btn.text = $"{i}";
				btn.cornerRadius = new(5);
				btn.Margin = new(5);
				btn.Width = 30;
				btn.Height = 30;
				btn.RoundedButtonBorder.BorderThickness = new(0);
				btn.RoundedButtonBorder.BorderBrush = new SolidColorBrush(Colors.Black);
				btn.RoundedButtonTextBlock.Foreground = buttonTextColor;
				btn.MouseDown += WorkspaceButtonClicked;
				btn.Background = buttonDefault;
				workspaces[i - 1].button = btn;
				WorkspacePanel.Children.Add(btn);
			}
			workspaces[currentUiWorkspace].button.Background = buttonSelected;
			//Task.Run(async () => { await QueryGlazeAndSetFocusedWorkspaceOnUI(); });
			Debug.WriteLine("Workspace init finished");
		});
	}

	public void RedrawButtons(int index)
	{
		this.Dispatcher.Invoke(() =>
		{
			foreach (var wksp in workspaces)
			{
				wksp.button.Background = buttonDefault;
			}
			workspaces[index].button.Background = buttonSelected;
		});
	}

	public async Task ChangeWorkspace(Workspace newWorkspace)
	{
		string message = $"command focus --workspace {newWorkspace.name}";
		await client.SendCommand(message);
	}

	// for updating Glaze when buttons pressed
	bool buttonRedrawing = false;
	public void WorkspaceButtonClicked(object? sender, RoutedEventArgs e)
	{
		buttonRedrawing = true;
		var btn = sender as RoundedButton;
		string clickedBtnName = Convert.ToString(btn.text);
		Debug.WriteLine($"{clickedBtnName} pressed");
		Workspace clickedWorkspace = workspaces.Where(wksp => wksp.name == clickedBtnName).First();
		int clickedBtnIndex = clickedWorkspace.index;
		if (clickedBtnIndex != currentUiWorkspace)
		{
			RedrawButtons(clickedBtnIndex);
		}
		currentUiWorkspace = clickedBtnIndex;
		//Debug.WriteLine("WorkspaceButtonClicked");
		Task.Run(async () =>
		{
			await ChangeWorkspace(clickedWorkspace);
			await Task.Delay(3000);
			buttonRedrawing = false;
		});
	}

	public async Task GetAllWorkspaces()
	{
		string message = "query workspaces";
		Debug.WriteLine("querying all workspaces");
		string reply = await client.SendCommand(message);
		Debug.WriteLine($"SendCommand: {reply}");
		dynamic json = JsonConvert.DeserializeObject<dynamic>(reply);
		if (json["clientMessage"].Value == message)
		{
			int i = 0;
			foreach (var workspace in json["data"]["workspaces"])
			{
				Workspace wksp = new();
				wksp.index = i;
				wksp.id = workspace["id"].Value;
				wksp.name = workspace["name"].Value;
				workspaces.Add(wksp);
				i++;
			}
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
				Workspace focusedWorkspace = workspaces.Where(wksp => wksp.id == focusedWorkspaceId).First();
				if(focusedWorkspace != null) RedrawButtons(focusedWorkspace.index);
				break;
		}
	}
}

public class Workspace
{
	public int index;
	public string id;
	public string name;
	public RoundedButton button;
}

public class Workspaces: Widget 
{
	GlazeClient client;

	int currentUiWorkspace = 0;
	List<Workspace> workspaces = new();

	public Workspaces() : base()
	{
		index = 0;

		this.CornerRadius = Theme.WIDGET_CORNER_RADIUS;

        //panel.Orientation = Orientation.Horizontal;
        //panel.ClipToBounds = true;
        //this.Content = panel;

        Task.Run(InitAsync);

		//Api.Print($"READING GLOBALS: {inGlobals}");
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

		await this.Thread.InvokeAsync(() =>
		{
			Debug.WriteLine($"reached: {workspaces.Count}");
			StackPanelWithGaps panel = new(Theme.WIDGET_GAP, workspaces.Count);
			//StackPanel panel = new();
            panel.Orientation = Orientation.Horizontal;
            panel.ClipToBounds = true;
            this.Content = panel;
			for (int i = 1; i <= workspaces.Count; i++)
			{
				RoundedButton btn = new();
				btn.Text = $"{i}";
				btn.FontFamily = Theme.FONT_FAMILY;
				btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
				btn.Width = Theme.BUTTON_WIDTH;
				btn.Height = Theme.BUTTON_HEIGHT;
				btn.BorderThickness = Theme.BUTTON_BORDER_THICKNESS;
				btn.BorderBrush = Theme.BUTTON_BORDER_COLOR;
				btn.Foreground = Theme.TEXT_COLOR;
				btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
				btn.Background = Theme.BUTTON_BACKGROUND;
				btn.HoverEffect = true;
				btn.MouseDown += WorkspaceButtonClicked;
				workspaces[i - 1].button = btn;
				panel.Add(btn);
				//panel.Children.Add(btn);
			}
			workspaces[currentUiWorkspace].button.Background = Theme.BUTTON_PRESSED_BACKGROUND;
			Debug.WriteLine("Workspace init finished");
		});
	}

	public void RedrawButtons(int index)
	{
		this.Thread.Invoke(() =>
		{
			foreach (var wksp in workspaces)
			{
				wksp.button.Background = Theme.BUTTON_BACKGROUND;
			}
			workspaces[index].button.Background = Theme.BUTTON_PRESSED_BACKGROUND;
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
		string clickedBtnName = Convert.ToString(btn.Text);
		Debug.WriteLine($"{clickedBtnName} pressed");
		Workspace clickedWorkspace = workspaces.Where(wksp => wksp.name == clickedBtnName).First();
		int clickedBtnIndex = clickedWorkspace.index;
		if (clickedBtnIndex != currentUiWorkspace)
		{
			RedrawButtons(clickedBtnIndex);
		}
		currentUiWorkspace = clickedBtnIndex;
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
					currentUiWorkspace = i;
				}
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

//return new Workspaces();

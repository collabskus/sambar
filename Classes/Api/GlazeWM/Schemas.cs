// GlazeWM Reply Schemas

public class BorderDelta
{
	public Left left { get; set; }
	public Top top { get; set; }
	public Right right { get; set; }
	public Bottom bottom { get; set; }
	public BorderDelta() { }
}

public class Bottom
{
	public double amount { get; set; }
	public string unit { get; set; }
	public Bottom() { }
}
public class Left
{
	public double amount { get; set; }
	public string unit { get; set; }
	public Left() { }
}

public class Right
{
	public double amount { get; set; }
	public string unit { get; set; }
	public Right() { }
}

public class State
{
	public string type { get; set; }
	public State() { }
}

public class PrevState
{
	public string type { get; set; }
	public bool? centered { get; set; }
	public bool? shownOnTop { get; set; }
	public PrevState() { }
}

public class Top
{
	public double amount { get; set; }
	public string unit { get; set; }
	public Top() { }
}

public class FloatingPlacement
{
	public int left { get; set; }
	public int top { get; set; }
	public int right { get; set; }
	public int bottom { get; set; }
	public FloatingPlacement() { }
}

public class Container
{
	public string? type { get; set; }
	public string? id { get; set; }
	public string? name { get; set; }
	public string? parentId { get; set; }
	public bool? hasFocus { get; set; }
	public double? tilingSize { get; set; }
	public int? width { get; set; }
	public int? height { get; set; }
	public int? x { get; set; }
	public int? y { get; set; }
	public State? state { get; set; }
	public object? prevState { get; set; }
	public string? displayState { get; set; }
	public bool? isDisplayed { get; set; }
	public BorderDelta? borderDelta { get; set; }
	public FloatingPlacement? floatingPlacement { get; set; }
	public int? handle { get; set; }
	public string? title { get; set; }
	public string? className { get; set; }
	public string? processName { get; set; }
	public object? activeDrag { get; set; }
	public List<Container>? children { get; set; }
	public List<string>? childFocusOrder { get; set; }
	public string? tilingDirection { get; set; }
	public Container() { }
}

public class Data
{
	public string eventType { get; set; }
	public string subscriptionId { get; set; }
	public Container focusedContainer { get; set; }
	public List<Container> workspaces { get; set; }
	public Data() { }
}

/* Responses
 * ---------
 * */

public class Message
{
	public string messageType { get; set; }
	public string clientMessage { get; set; }
	public Data data { get; set; }
	public string? error { get; set; }
	public string? subscriptionId { get; set; }
	public bool success { get; set; }
	public Message() { }
}

/* SUBSCRIPTIONS
 * -------------
 * query: sub -e focus_changed
 * {"messageType":"client_response","clientMessage":"sub -e focus_changed","data":{"subscriptionId":"3a454bb1-baf2-4372-a6f8-872e27031cd9"},"error":null,"success":true}
 *
 * query: unsub --id {subscriptionId}
 * {"messageType":"client_response","clientMessage":"unsub --id 3a454bb1-baf2-4372-a6f8-872e27031cd9","data":null,"error":null,"success":true}
 *
 * EVENTS
 * ------
 * event: focus_changed
 * {"messageType":"event_subscription","data":{"eventType":"focus_changed","focusedContainer":{"type":"window","id":"ea59a01c-0032-43b2-95f9-2fb6b724959e","parentId":"cc84580a-7290-4186-a7e3-d86b6a5e4f54","hasFocus":true,"tilingSize":1.0,"width":1346,"height":748,"x":10,"y":10,"state":{"type":"tiling"},"prevState":null,"displayState":"hidden","borderDelta":{"left":{"amount":0.0,"unit":"pixel"},"top":{"amount":0.0,"unit":"pixel"},"right":{"amount":0.0,"unit":"pixel"},"bottom":{"amount":0.0,"unit":"pixel"}},"floatingPlacement":{"left":78,"top":48,"right":1289,"bottom":721},"handle":37946962,"title":"glazewm-go/command/client-messages.go at 9034d5a7debd13b518255dcb87e5ce331a43b7d1 · burgr033/glazewm-go — Mercury","className":"MozillaWindowClass","processName":"mercury","activeDrag":null}},"error":null,"subscriptionId":"3a454bb1-baf2-4372-a6f8-872e27031cd9","success":true}
 *
 *
 * */

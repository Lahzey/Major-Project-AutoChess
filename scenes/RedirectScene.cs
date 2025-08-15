using Godot;
using System;

public partial class RedirectScene : Node {
    
    [Export] public PackedScene ClientScene { get; set; }
    [Export] public PackedScene ServerScene { get; set; }
    
    public override void _Ready() {
        CallDeferred(MethodName.Redirect);
    }

    private void Redirect() {
        bool isServer = OS.HasFeature("dedicated_server");
        GetTree().ChangeSceneToPacked(isServer ? ServerScene : ClientScene);
    }
}
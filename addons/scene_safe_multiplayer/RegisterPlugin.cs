using Godot;

#if TOOLS
[Tool]
public partial class RegisterPlugin : EditorPlugin {
    public override void _EnterTree() {
        AddCustomType(
            "SceneSafeMpSpawner",
            "MultiplayerSpawner",
            GD.Load<CSharpScript>("res://addons/scene_safe_multiplayer/SceneSafeMpSpawner.cs"),
            GD.Load<Texture2D>("res://addons/scene_safe_multiplayer/MultiplayerSpawner.svg")
        );

        AddCustomType(
            "SceneSafeMpSynchronizer",
            "MultiplayerSynchronizer",
            GD.Load<CSharpScript>("res://addons/scene_safe_multiplayer/SceneSafeMpSynchronizer.cs"),
            GD.Load<Texture2D>("res://addons/scene_safe_multiplayer/MultiplayerSynchronizer.svg")
        );

        AddAutoloadSingleton("SceneSafeMultiplayer", "res://addons/scene_safe_multiplayer/SceneSafeMpManager.cs");
    }

    public override void _ExitTree() {
        RemoveCustomType("SceneSafeMpSpawner");
        RemoveCustomType("SceneSafeMpSynchronizer");
        RemoveAutoloadSingleton("SceneSafeMultiplayer");
    }
}
#endif
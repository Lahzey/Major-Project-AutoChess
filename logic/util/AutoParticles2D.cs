using Godot;

namespace MPAutoChess.logic.util;

public partial class AutoParticles2D : Node2D {
    
    [Export] public GpuParticles2D Particles2D { get; set; }
    [Export] public bool DisposeThis { get; set; } = true;

    public override void _Ready() {
        Particles2D.Emitting = true;
        Particles2D.Finished += () => {
            Particles2D.QueueFree();
            if (DisposeThis) {
                QueueFree();
            }
        };
    }
}
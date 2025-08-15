using System;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.projectile;

[ProtoContract]
public partial class Projectile : Node2D {

    [Export] [ProtoMember(1)] public float Speed { get; set; } = 10f;
    
    [ProtoMember(2)] public DamageSource Source { get; private set; }
    [ProtoMember(3)] public Target Target { get; private set; }
    private Action OnHit { get; set; } // not set on client
    private Func<Target> RecalculateTarget { get; set; } // not set on client
    
    private AnimatedSprite2D animatedSprite;
    
    public void Initialize(DamageSource source, Target target, Action onHit, Func<Target> recalculateTarget) {
        Source = source;
        Target = target;
        OnHit = onHit;
        RecalculateTarget = recalculateTarget;
        foreach (Node child in GetChildren()) {
            if (child is AnimatedSprite2D sprite) {
                animatedSprite = sprite;
                break;
            }
        }
    }

    public override void _Ready() {
        ZIndex = 100; // make sure projectiles are drawn above units
    }

    public override void _Process(double delta) {
        float toTravel = (float)(Speed * delta);

        if (!Target.IsValid()) {
            if (!ServerController.Instance.IsServer) return;
            Target = RecalculateTarget?.Invoke() ?? Target;
            if (!Target.IsValid()) return;
        }
        
        Vector2 targetPosition = Target.GetPosition();
        Vector2 position = Position;
        float distance = position.DistanceTo(targetPosition);
        if (distance > toTravel) {
            Vector2 direction = (targetPosition - position) / distance;
            Position = position + direction * toTravel;
            FaceTowards(direction);
        } else {
            Position = targetPosition;
            if (ServerController.Instance.IsServer) {
                OnHit?.Invoke();
                Destroy();
            }
        }
    }

    private void FaceTowards(Vector2 direction) {
        if (direction == Vector2.Zero) return;
        if (animatedSprite != null) animatedSprite.FlipH = direction.X < 0;
        Rotation = direction.Angle();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void Destroy() {
        QueueFree();
        if (ServerController.Instance.IsServer) {
            Rpc(MethodName.Destroy);
        }
    }
}
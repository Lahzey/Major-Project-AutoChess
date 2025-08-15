using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.util;

namespace MPAutoChess.logic.core.combat;

public partial class DamageNumbers : Control {
    
    private const double LIFETIME = 1.0f; // seconds
    private const float SPEED_PER_FONTSIZE = 15.0f;
    private static readonly Color PHYSICAL_COLOR = new Color("#7d4b1f");
    private static readonly Color MAGIC_COLOR = new Color("#3698c2");
    private static readonly Color PURE_COLOR = new Color("#ffffff");
    private static readonly Color HEALING_COLOR = new Color("#205221");
    
    public static DamageNumbers Instance { get; private set; }

    private Viewport viewport;
    private List<NumberLabel> numberLabels = new List<NumberLabel>();
    
    public override void _EnterTree() {
        Instance = this;
        viewport = GetViewport();
    }

    public override void _ExitTree() {
        if (Instance == this) Instance = null;
        viewport = null;
    }

    public override void _Process(double delta) {
        for (int i = numberLabels.Count - 1; i >= 0; i--) {
            NumberLabel numberLabel = numberLabels[i];
            numberLabel.lifetime -= delta;
            if (numberLabel.lifetime <= 0.0f) {
                numberLabel.label.QueueFree();
                numberLabels.RemoveAt(i);
            } else {
                // Update position to move upwards
                numberLabel.label.Position += numberLabel.direction * (float)delta * numberLabel.speed;
                numberLabel.label.Modulate = new Color(1f, 1f, 1f, (float) Mathf.Pow(numberLabel.lifetime / LIFETIME, 0.2));
            }
        }
    }

    public void ShowDamage(DamageInstance damageInstance) {
        Vector2 worldPosition = damageInstance.Target.GlobalPosition;
        Vector2 viewportPosition = CameraController.Instance.ToViewportPosition(worldPosition);

        int fontSize = FontSizeCalculator.GetFontSize(damageInstance.IsCrit ? FontSizeType.SUBTITLE : FontSizeType.NORMAL, viewport);
        Color color = damageInstance.Type switch {
            DamageType.PHYSICAL => PHYSICAL_COLOR,
            DamageType.MAGICAL => MAGIC_COLOR,
            DamageType.PURE => PURE_COLOR,
            DamageType.HEALING => HEALING_COLOR,
            _ => Colors.White
        };
        float direction = (FastRandom.FastRandomFloat() - 0.5f) * 0.3f;
        
        Label damageLabel = new Label();
        damageLabel.Position = viewportPosition;
        damageLabel.Text = damageInstance.FinalAmount.ToString("F0") + (damageInstance.IsCrit ? "!" : "");
        damageLabel.AddThemeFontSizeOverride("font_size", fontSize);
        damageLabel.AddThemeColorOverride("font_color", color);
        damageLabel.MouseFilter = MouseFilterEnum.Ignore; // just to make sure
        AddChild(damageLabel);
        
        numberLabels.Add(new NumberLabel {
            label = damageLabel,
            lifetime = LIFETIME,
            direction = new Vector2(direction, -1f + Mathf.Abs(direction)),
            speed = fontSize * SPEED_PER_FONTSIZE
        });
    }
    

    private class NumberLabel {
        public Label label;
        public double lifetime;
        public Vector2 direction;
        public float speed;
    }
}
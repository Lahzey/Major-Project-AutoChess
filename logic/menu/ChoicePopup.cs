using System;
using Godot;
using MPAutoChess.logic.util;

namespace MPAutoChess.logic.menu;

public partial class ChoicePopup : Control {
    
    public static ChoicePopup Instance { get; private set; }
    
    [Export] public Label TitleLabel { get; set; }
    [Export] public Container ChoicesContainer { get; set; }

    public override void _EnterTree() {
        Instance = this;
        MouseFilter = MouseFilterEnum.Ignore;
        Visible = false;
    }


    public void Open(string title, Choice[] choices, Action<int> onChoiceSelected) {
        foreach (Node child in ChoicesContainer.GetChildren()) {
            child.QueueFree();
        }
        
        TitleLabel.Text = title;
        TitleLabel.Visible = title != null;
        for (int i = 0; i < choices.Length; i++) {
            Choice choice = choices[i];
            VBoxContainer choiceBox = new VBoxContainer();
            choiceBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            if (choice.Icon != null) {
                TextureRect iconRect = new TextureRect();
                iconRect.Texture = choice.Icon;
                iconRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
                iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                iconRect.SizeFlagsVertical = SizeFlags.ExpandFill;
                choiceBox.AddChild(iconRect);
            }

            if (choice.Text != null) {
                Label textLabel = new Label();
                textLabel.Text = choice.Text;
                textLabel.HorizontalAlignment = HorizontalAlignment.Center;
                textLabel.AddChild(new AutoFontSize() { SizeType = FontSizeType.SUBTITLE });
                choiceBox.AddChild(textLabel);
            }

            int index = i; // capture the index for the event handler
            choiceBox.MouseFilter = MouseFilterEnum.Stop;
            choiceBox.MouseDefaultCursorShape = CursorShape.PointingHand;
            choiceBox.GuiInput += (InputEvent @event) => {
                if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left) {
                    onChoiceSelected(index);
                    Close();
                }
            };
            
            ChoicesContainer.AddChild(choiceBox);
        }

        Visible = true;
        MouseFilter = MouseFilterEnum.Pass;
    }

    private void Close() {
        foreach (Node child in ChoicesContainer.GetChildren()) {
            child.QueueFree();
        }
        MouseFilter = MouseFilterEnum.Ignore;
        Visible = false;
    }



    public struct Choice {
        public Texture2D Icon { get; set; }
        public string Text { get; set; }
        
        public Choice(Texture2D icon, string text) {
            Icon = icon;
            Text = text;
        }
    }
    
}
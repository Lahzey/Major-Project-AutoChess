using System.Collections.Generic;
using Godot;

namespace MPAutoChess.logic.menu;

public partial class Carousel : Control {
    [Export] public Control Container;
    [Export] public Button PrevButton;
    [Export] public Button NextButton;

    private List<Control> _children = new List<Control>();
    private int _currentIndex = 0;

    public override void _Ready() {
        if (Container == null) {
            GD.PrintErr("Container is not assigned.");
            return;
        }

        foreach (var child in Container.GetChildren()) {
            if (child is Control controlChild)
                _children.Add(controlChild);
        }

        if (_children.Count == 0) {
            GD.PrintErr("No children found in container.");
            return;
        }

        UpdateVisibility();

        if (PrevButton != null)
            PrevButton.Pressed += OnPrevPressed;

        if (NextButton != null)
            NextButton.Pressed += OnNextPressed;
    }

    private void OnPrevPressed() {
        if (_children.Count == 0) return;
        _currentIndex = (_currentIndex - 1 + _children.Count) % _children.Count;
        UpdateVisibility();
    }

    private void OnNextPressed() {
        if (_children.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % _children.Count;
        UpdateVisibility();
    }

    private void UpdateVisibility() {
        for (int i = 0; i < _children.Count; i++)
            _children[i].Visible = (i == _currentIndex);
    }
}
using Godot;
using MPAutoChess.logic.core.shop;

namespace MPAutoChess.logic.core.player;

public partial class PlayerUI : Control {

    [Export] public Control FreeSpace;
    [Export] public ShopUI ShopUI { get; set; }

}
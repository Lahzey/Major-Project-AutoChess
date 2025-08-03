using Godot;
using Godot.Collections;
using MPAutoChess.logic.core.stats;

namespace MPAutoChess.logic.core.item;

[GlobalClass]
public partial class ItemType : Resource {
    
    [Export] public string Name { get; set; }
    [Export] public ItemCategory Category { get; set; }
    [Export(PropertyHint.MultilineText)] public string Description { get; set; }
    [Export] public Texture2D Icon { get; set; }
    
    [Export] public ItemType CraftedFromA { get; set; }
    [Export] public ItemType CraftedFromB { get; set; }
    
    [Export] public Array<StatValue> Stats { get; set; }
    [Export] public CSharpScript EffectScript { get; set; }
    
}
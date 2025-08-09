using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.util;
using ProtoBuf;

namespace MPAutoChess.logic.core.item.consumable;

[ProtoContract(Surrogate = typeof(ConsumableSurrogate))]
public abstract class Consumable {
    
    protected static readonly Texture2D VALID_TARGET_CURSOR = ResourceLoader.Load<Texture2D>("res://assets/cursors/crosshair_target.svg");
    protected static readonly Texture2D INVALID_TARGET_CURSOR = ResourceLoader.Load<Texture2D>("res://assets/cursors/crosshair.svg");

    private static Dictionary<string, Consumable> consumablesByTypeName;

    public abstract Texture2D GetIcon();
    
    public abstract bool IsValidTarget(object target, int extraChoice);

    public abstract bool Consume(object target, int extraChoice);
    
    protected virtual object MapTarget(Node targetNode, out int extraChoice) {
        if (targetNode is ItemPanel itemPanel) {
            extraChoice = itemPanel.InventoryIndex;
            return itemPanel.Player.Inventory;
        } else {
            extraChoice = -1;
            return targetNode;
        }
    }

    protected virtual void RequestConsume(object target, int extraChoice) {
        if (target is IIdentifiable identifiable) PlayerController.Current.UseConsumable(this, identifiable, extraChoice);
        else if (target is Node node) PlayerController.Current.UseConsumable(this, node, extraChoice);
        else GD.PrintErr("Consumable target is not IIdentifiable or a Node: " + target);
    }
    
    public bool CanBeUsedOn(Node targetNode) {
        object target = MapTarget(targetNode, out int extraChoice);
        return IsValidTarget(target, extraChoice);
    }

    public void OnUse(Node targetNode) {
        object target = MapTarget(targetNode, out int extraChoice);
        if (!IsValidTarget(target, extraChoice)) return;
        RequestConsume(target, extraChoice);
    }

    public virtual string GetName() {
        return StringUtil.PascalToReadable(GetType().Name);
    }

    public static string GetTypeName(Type type) {
        return type.Name + " (in " + type.Namespace + ")";
    }
    
    public string GetTypeName() {
        return GetTypeName(GetType());
    }

    public static Consumable GetByTypeName(string typeName) {
        if (consumablesByTypeName == null) LoadAll();
        return consumablesByTypeName.GetValueOrDefault(typeName);
    }
    
    public static IEnumerable<Consumable> GetAll() {
        if (consumablesByTypeName == null) LoadAll();
        return consumablesByTypeName.Values;
    }
    
    private static void LoadAll() {
        consumablesByTypeName = new Dictionary<string, Consumable>();
        foreach (Type type in AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(assembly => assembly.GetTypes())
                     .Where(t => t.IsSubclassOf(typeof(Consumable)) && !t.IsAbstract)) {
            Consumable consumable = (Consumable)Activator.CreateInstance(type);
            consumablesByTypeName[GetTypeName(type)] = consumable;
        }
    }

    public static Consumable Get<T>() {
        return GetByTypeName(GetTypeName(typeof(T)));
    }

    public virtual Texture2D GetValidTargetCursor() {
        return VALID_TARGET_CURSOR;
    }

    public virtual Texture2D GetInvalidTargetCursor() {
        return INVALID_TARGET_CURSOR;
    }
}

[ProtoContract]
public class ConsumableSurrogate {
    
    [ProtoMember(1)] public string TypeName { get; set; }

    public static implicit operator ConsumableSurrogate(Consumable consumable) {
        if (consumable == null) return null;
        return new ConsumableSurrogate { TypeName = consumable.GetTypeName() };
    }

    public static implicit operator Consumable(ConsumableSurrogate surrogate) {
        if (surrogate == null) return null;
        return Consumable.GetByTypeName(surrogate.TypeName);
    }
    
}
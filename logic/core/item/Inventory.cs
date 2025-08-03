using System;
using MPAutoChess.logic.core.networking;
using ProtoBuf;

namespace MPAutoChess.logic.core.item;

[ProtoContract]
public class Inventory : IIdentifiable {
    public string Id { get; set; }
    
    public int Size => itemSlots.Length;

    [ProtoMember(1)] public bool AutoExpand { get; set; } = true;
    [ProtoMember(2)] public bool AutoShrink { get; set; } = true;
    [ProtoMember(4)]  public int ExpansionInterval { get; set; } = 10;

    [ProtoMember(5)] private int minSize;
    [ProtoMember(6)] private ItemSlot[] itemSlots;
    
    public Inventory() {} // for ProtoBuf serialization

    public Inventory(int initialSize) {
        minSize = initialSize;
        itemSlots = new ItemSlot[initialSize];
        for (int i = 0; i < initialSize; i++) {
            itemSlots[i] = new ItemSlot(null);
        }
    }

    private int CountFilledSlots() {
        int filledSlots = 0;
        foreach (ItemSlot slot in itemSlots) {
            if (slot.GetItem() != null) filledSlots++;
        }

        return filledSlots;
    }

    private bool TryExpand() {
        if (!AutoExpand) return false;
        int interval = Math.Max(ExpansionInterval, 1);
        Resize(Size + interval);
        ServerController.Instance.OnChange(this);
        return true;
    }

    private bool TryShrink() {
        if (!AutoShrink) return false;
        int interval = Math.Max(ExpansionInterval, 1);
        int emtpySlots = Size - CountFilledSlots();
        bool hasShrunk = false;
        while (interval >= emtpySlots) {
            Resize(Size - interval);
            emtpySlots -= interval;
            hasShrunk = true;
        }
        return hasShrunk;
    }

    public void Resize(int newSize) {
        if (newSize == Size) return;
        ItemSlot[] newSlots = new ItemSlot[newSize];
        if (newSize > Size) { // expanding inventory
            // copy all existing slots
            Array.Copy(itemSlots, newSlots, itemSlots.Length);
            
            // add new slots
            for (int i = itemSlots.Length; i < newSize; i++) {
                newSlots[i] = new ItemSlot(null);
            }
        } else { // shrinking inventory
            int filledSlots = CountFilledSlots();
            
            // drop any items that won't fit in the new inventory size (starting from the end)
            while (filledSlots < newSize) {
                for (int i = itemSlots.Length - 1; i >= 0; i--) {
                    if (itemSlots[i].GetItem() == null) continue;
                    itemSlots[i].SetItem(null);
                    filledSlots--;
                    goto EndWhile; // c# does not support continuing (or breaking) outer loops, so this is a workaround
                }
                throw new Exception("Failed to find empty slots to drop, CountFilledSlots is likely returning an incorrect result."); // if we don't do this we end up in an infinite loop
                EndWhile:
                continue;
            }
            
            // if new size is bigger than the filled slots, we keep some empty slots at the start
            int emptySlotsToKeep = newSize - filledSlots;
            int newIndex = 0;
            foreach (ItemSlot slot in itemSlots) {
                if (slot.GetItem() == null) {
                    if (emptySlotsToKeep <= 0) continue;
                    emptySlotsToKeep--;
                }
                newSlots[newIndex] = slot; // if we exceed array bounds here that means emptySlotsToKeep is calculated incorrectly, so getting an exception is preferable
                newIndex++;
            }
        }
        
        itemSlots = newSlots;
        ServerController.Instance.OnChange(this);
    }

    public bool AddItem(Item item, bool allowExpansion = true) {
        for (int i = 0; i < Size; i++) {
            ItemSlot slot = itemSlots[i];
            if (slot.GetItem() != null) continue;
            slot.SetItem(item);
            ServerController.Instance.OnChange(this);
            return true;
        }
        if (!allowExpansion) return false;

        if (TryExpand()) {
            return AddItem(item, false); // now it should fit, but block further expansion attempts just in case to prevent infinite loops
        }
        
        return false;
    }
    
    public Item? GetItem(int index) {
        if (index >= Size) throw new IndexOutOfRangeException();
        return itemSlots[index].GetItem();
    }
    
    public Item? ReplaceItem(int index, Item? item) {
        if (index >= Size) throw new IndexOutOfRangeException();
        Item? oldItem = itemSlots[index].GetItem();
        itemSlots[index].SetItem(item);
        ServerController.Instance.OnChange(this);
        return oldItem;
    }
}
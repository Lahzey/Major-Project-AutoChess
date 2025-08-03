using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.item;

public interface IItemEffect {

    public void Apply(UnitInstance unit);

    public void Remove(UnitInstance unit);

}
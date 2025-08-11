using System.Collections.Generic;
using MPAutoChess.logic.core.combat;

namespace MPAutoChess.logic.core.events;

public class CombatEndEvent : Event {

    public Combat Combat { get; private set; }
    public CombatResult Result { get; private set; }

    public CombatEndEvent(Combat combat, CombatResult result) {
        Combat = combat;
        Result = result;
    }

}
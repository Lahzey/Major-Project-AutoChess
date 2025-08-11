using System.Collections.Generic;
using MPAutoChess.logic.core.combat;

namespace MPAutoChess.logic.core.events;

public class CombatStartEvent : Event {

    public Combat Combat { get; private set; }

    public CombatStartEvent(Combat combat) {
        Combat = combat;
    }

}
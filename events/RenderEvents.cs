using RecompOne.Runtime.Events;

namespace Recompiled;

//todo: add other rendering related events
public sealed class TilemapRenderedEvent : GameEvent
{
    public int ScrollX;
    public int ScrollY;
}

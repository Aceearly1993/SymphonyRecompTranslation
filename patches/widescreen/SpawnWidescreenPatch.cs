using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;

namespace Recompiled;

//have to recalculate spawning and processing of entities otherwise THE FUCKING TRANSITION DOOR DISAPEARS
//its 1 am im so tired
public static partial class WidescreenPatch
{
    const uint ScrollDeltaX = 0x80097908; //from dec ref

    const int SpawnSlackLeft = 64;
    const int SpawnSlackRight = 320;

    const uint StageEntities = 0x800762D8;
    const int EntitySize = 0xBC;
    const int EntityIdOffset = 0x26;

    const int LayoutPosX = 0;
    const int LayoutEntityId = 4;
    const int LayoutRoomIndex = 6;

    const int RangeSlackClose = 0x40;
    const int RangeSlackFar = 0x140;
    const uint StageOverlayHeader = 0x80180000; //stage can be stale overlay (not fully overwriten so the recomp still marks it as loaded so head it via the header isntead

    //fn calls
    static Action<CpuContext, IMemory>? _createRight;
    static Action<CpuContext, IMemory>? _createLeft;
    static Action<CpuContext, IMemory>? _fromLayout;
    static uint _spawnUpdate;

    static int SpawnExtra() => Math.Max(0, StageMargin() - SpawnSlackLeft);

    static bool NameIs(string emitted, string name)
    {
        if (emitted.StartsWith(name, StringComparison.Ordinal)) return true;
        int us = emitted.IndexOf('_');
        return us > 0 && string.CompareOrdinal(emitted, us + 1, name, 0, name.Length) == 0;
    }

    static Action<CpuContext, IMemory>? Find(IOverlay overlay, string name) //should integrate on wraper?
    {
        foreach (var (_, fn) in overlay.Functions)
            if (NameIs(fn.Method.Name, name)) return fn;
        return null;
    }

    static void EnsureSpawnFns(IMemory m)
    {
        uint update = m.ReadU32(StageOverlayHeader);
        if (update == _spawnUpdate) return;

        _spawnUpdate = update;
        _createRight = null;
        _createLeft = null;
        _fromLayout = null;

        foreach (var ov in Dispatcher.ActiveNames)
        {
            if (!Dispatcher.Overlays.TryGetValue(ov, out var overlay)) continue;
            if (!overlay.Functions.ContainsKey(update)) continue;

            _createRight = Find(overlay, "CreateEntitiesToTheRight");
            _createLeft = Find(overlay, "CreateEntitiesToTheLeft");
            _fromLayout = Find(overlay, "CreateEntityFromLayout");
            return;
        }
    }

    static void Spawn(Action<CpuContext, IMemory>? fn, CpuContext c, IMemory m, int posX)
    {
        if (fn == null) return;
        var snap = c.Snapshot();
        c.A0 = (uint)(short)posX;
        fn(c, m);
        c.Restore(snap);
    }


    static int ScrollX(IMemory m) => (short)m.ReadU16(TilemapScrollXHi);

    public static void PostInitRoomEntities(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        int extra = SpawnExtra();
        if (extra == 0) return;

        EnsureSpawnFns(m);
        int scroll = ScrollX(m);

        int right = scroll + SpawnSlackRight + extra;

        Spawn(_createRight, c, m, right);
        Spawn(_createLeft, c, m, scroll - SpawnSlackLeft - extra);
        Spawn(_createRight, c, m, right);
    }


    public static void PreCreateEntityWhenInHorizontalRange(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        int extra = SpawnExtra();
        if (extra == 0) return;

        EnsureSpawnFns(m);


        if (_fromLayout == null) return;

        uint obj = c.A0;
        int scroll = ScrollX(m);

        int close = Math.Max(0, scroll - RangeSlackClose);
        int far = scroll + RangeSlackFar;
        int posX = (short)m.ReadU16(obj + LayoutPosX);
        if (posX >= close && posX <= far) return;

        int wideClose = Math.Max(0, scroll - RangeSlackClose - extra);
        int wideFar = far + extra;
        if (posX < wideClose || posX > wideFar) return;

        ushort entityId = m.ReadU16(obj + LayoutEntityId);
        int kind = entityId & 0xE000;
        if (kind != 0x0000 && kind != 0xA000) return;

        uint entity = StageEntities + (uint)((m.ReadU16(obj + LayoutRoomIndex) & 0xFF) * EntitySize);
        if (kind == 0x0000 && m.ReadU16(entity + EntityIdOffset) != 0) {
            return;
        }
        var snap = c.Snapshot();
        c.A0 = entity;
        c.A1 = obj;
        _fromLayout(c, m);

        c.Restore(snap);


    }


    public static void PostUpdateRoomPosition(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        int extra = SpawnExtra();
        if (extra == 0) return;
        EnsureSpawnFns(m);

        int delta = (int)m.ReadU32(ScrollDeltaX);
        int scroll = ScrollX(m);

        if (delta > 0) Spawn(_createRight, c, m, scroll + SpawnSlackRight + extra);
        else if (delta < 0) Spawn(_createLeft, c, m, scroll - SpawnSlackLeft - extra);
    }


}


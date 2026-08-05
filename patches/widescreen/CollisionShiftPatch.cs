using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace Recompiled;


//hit detection only makes stuff on 256 view area, so to avoid reimplementing the entire function im making a "hacky" way that works by changing the view area to be relative to the player! its a stupid fix but it should work
//stuff outside this area still despawn, but now its player relative
public static partial class WidescreenPatch
{
    const int ShiftEntityEnd = 192;
    const int ShiftPoolFirst = ShiftEntityEnd;
    const int ShiftPoolEnd = 256;

    const uint ShiftPrimDrawMode = 0x32;
    const uint ShiftPrimX0 = 0x08;
    const uint ShiftPrimX1 = 0x14;
    const uint ShiftPrimX2 = 0x20;
    const uint ShiftPrimX3 = 0x2C;
    const int ShiftDrawHide = 0x08;
    const uint ShiftPrimHead = 0x800973F8;

    static int _hitShift;
    static readonly ushort[] _shiftPoolIds = new ushort[ShiftPoolEnd - ShiftPoolFirst];
    static readonly List<uint> _shiftHiddenPrims = [];

    static int HitShift(IMemory m)
    {
        int playerX = (short)m.ReadU16(PedAt(0) + PedPosXHi);
        if (playerX > 256) return 256 - playerX;
        if (playerX < 0) return -playerX;
        return 0;
    }

    static void ShiftX(IMemory m, uint addr, int by) =>
        m.WriteU16(addr, (ushort)(short)((short)m.ReadU16(addr) + by)); //shift it to the thing

    public static void PreHitDetection(CpuContext c, IMemory m)
    {
        _hitShift = 0;
        if (OriginalAspect) return;
        if (StageMargin() == 0) return;

        int shift = HitShift(m);
        if (shift == 0) return;
        _hitShift = shift;

        for (int i = 0; i < ShiftEntityEnd; i++) ShiftX(m, PedAt(i) + PedPosXHi, shift);

        for (int i = ShiftPoolFirst; i < ShiftPoolEnd; i++)
            _shiftPoolIds[i - ShiftPoolFirst] = m.ReadU16(PedAt(i) + PedEntityId);

        _shiftHiddenPrims.Clear();
        uint prim = PrimBufAddr + m.ReadU32(ShiftPrimHead) * PrimStride;
        for (int guard = 0; prim != 0 && guard < 1024; guard++)
        {
            if (m.ReadU16(prim + ShiftPrimDrawMode) == ShiftDrawHide) _shiftHiddenPrims.Add(prim);
            prim = m.ReadU32(prim);
        }
    }

    public static void PostHitDetection(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        int shift = _hitShift;
        if (shift == 0) return;
        _hitShift = 0;

        for (int i = 0; i < ShiftEntityEnd; i++) ShiftX(m, PedAt(i) + PedPosXHi, -shift);

        for (int i = ShiftPoolFirst; i < ShiftPoolEnd; i++)
        {
            uint e = PedAt(i);
            ushort id = m.ReadU16(e + PedEntityId);
            if (id == 0 || id == _shiftPoolIds[i - ShiftPoolFirst]) continue;
            ShiftX(m, e + PedPosXHi, -shift);
        }

        foreach (uint prim in _shiftHiddenPrims)
        {
            if (m.ReadU16(prim + ShiftPrimDrawMode) == ShiftDrawHide) continue;
            ShiftX(m, prim + ShiftPrimX0, -shift);
            ShiftX(m, prim + ShiftPrimX1, -shift);
            ShiftX(m, prim + ShiftPrimX2, -shift);
            ShiftX(m, prim + ShiftPrimX3, -shift);
        }
        _shiftHiddenPrims.Clear();
    }
}

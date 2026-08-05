using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace Recompiled;

public static partial class WidescreenPatch
{
    const uint GameStepAddr = 0x80073060;
    const uint StageIdAddr = 0x800974A0;
    const uint GameOverPrimAddr = 0x8013640C;

    public static void PostHandleGameOver(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        int margin = StageMargin();
        if (margin == 0) return;
        uint step = m.ReadU32(GameStepAddr);
        if (step < 2 || step > 8) return;
        if (m.ReadU32(StageIdAddr) == 0x1F) return;

        uint red = PrimBufAddr + m.ReadU32(GameOverPrimAddr) * PrimStride;
        if (m.ReadU16(red + 0x1A) != 8) return;
        uint left = m.ReadU32(red);
        if (left == 0 || (short)m.ReadU16(left + 0x08) != 0) return;

        SetPrimX(m, red, -margin, 0xFF + margin);
        uint right = m.ReadU32(left);
        SetPrimX(m, left, -margin, 0x80);
        if (right == 0) return;
        SetPrimX(m, right, 0x80, 0x100 + margin);

        int wide = 0x100 + margin * 2;
        uint prim = m.ReadU32(right);
        for (int i = 0; i < 0x100 && prim != 0; i++, prim = m.ReadU32(prim))
            SetPrimX(m, prim, -margin + i * wide / 0x100, -margin + (i + 1) * wide / 0x100);
    }

    static void SetPrimX(IMemory m, uint prim, int x0, int x1)
    {
        m.WriteU16(prim + 0x08, (ushort)(short)x0);
        m.WriteU16(prim + 0x20, (ushort)(short)x0);
        m.WriteU16(prim + 0x14, (ushort)(short)x1);
        m.WriteU16(prim + 0x2C, (ushort)(short)x1);
    }

    const uint PlayerHudAddr = 0x8013796C;
    const uint BossBarStateAddr = 0x8003C744;
    const uint PlayableCharAddr = 0x8003C9A0;

    static bool RichterHudActive(IMemory m) =>
        m.ReadU32(StageIdAddr) == 0x1F || m.ReadU32(PlayableCharAddr) != 0;

    static uint HudPrim(IMemory m, uint indexAddr) =>
        PrimBufAddr + m.ReadU32(indexAddr) * PrimStride;

    static void ShiftPrimX(IMemory m, uint prim, int dx)
    {
        m.WriteU16(prim + 0x08, (ushort)((short)m.ReadU16(prim + 0x08) + dx));
        m.WriteU16(prim + 0x14, (ushort)((short)m.ReadU16(prim + 0x14) + dx));
        m.WriteU16(prim + 0x20, (ushort)((short)m.ReadU16(prim + 0x20) + dx));
        m.WriteU16(prim + 0x2C, (ushort)((short)m.ReadU16(prim + 0x2C) + dx));
    }
    static int _hudMargin;

    public static void PostDrawHud(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        int margin = StageMargin();
        _hudMargin = (margin == 0 || RichterHudActive(m)) ? 0 : margin;
        if (margin == 0 || RichterHudActive(m)) return;
        for (uint prim = HudPrim(m, PlayerHudAddr + 4); prim != 0; prim = m.ReadU32(prim))
            ShiftPrimX(m, prim, -margin);
    }
    static void FixHudFrame(IMemory m)
    {
        if (RichterHudActive(m)) return;
        int delta = StageMargin() - _hudMargin;
        if (delta == 0) return;
        uint prim = HudPrim(m, PlayerHudAddr + 4);
        for (int i = 0; i < 6 && prim != 0; i++, prim = m.ReadU32(prim))
            ShiftPrimX(m, prim, -delta);
        _hudMargin += delta;
    }

    public static void PostDrawHudSubweapon(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        FixHudFrame(m);
        int margin = StageMargin();
        if (margin == 0 || RichterHudActive(m)) return;
        uint prim = HudPrim(m, PlayerHudAddr + 4);
        for (int i = 0; i < 6 && prim != 0; i++)
            prim = m.ReadU32(prim);
        for (int i = 0; i < 8 && prim != 0; i++, prim = m.ReadU32(prim))
            ShiftPrimX(m, prim, -margin);
    }

    public static void PostDrawRichterHud(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        int margin = StageMargin();
        if (margin == 0) return;
        uint prim = HudPrim(m, PlayerHudAddr + 4);
        for (int i = 0; prim != 0; i++, prim = m.ReadU32(prim))
            ShiftPrimX(m, prim, i == 1 || i == 3 || i == 4 ? margin : -margin);
        for (uint col = HudPrim(m, PlayerHudAddr + 8); col != 0; col = m.ReadU32(col))
            ShiftPrimX(m, col, margin);
    }

    public static void PostDrawRichterHudSubweapon(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        int margin = StageMargin();
        if (margin == 0) return;
        if (m.ReadU32(BossBarStateAddr) == 5) return;
        uint prim = m.ReadU32(HudPrim(m, PlayerHudAddr + 4));
        if (prim == 0) return;
        ShiftPrimX(m, prim, margin);
        prim = m.ReadU32(prim);
        prim = m.ReadU32(prim);
        ShiftPrimX(m, prim, margin);
        prim = m.ReadU32(prim);
        ShiftPrimX(m, prim, margin);
        prim = m.ReadU32(prim);
        prim = m.ReadU32(prim);
        prim = m.ReadU32(prim);
        if (m.ReadU16(prim + 0x32) != 8)
            ShiftPrimX(m, prim, -margin);
        prim = m.ReadU32(prim);
        uint crash = m.ReadU32(PlayerHudAddr + 0x24);
        if (crash - 10 <= 3 || crash - 60 <= 3)
            ShiftPrimX(m, prim, -margin);
    }
}

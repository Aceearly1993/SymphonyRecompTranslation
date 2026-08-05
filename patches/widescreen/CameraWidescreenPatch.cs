using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace Recompiled;

public static partial class WidescreenPatch
{
    const uint TilemapScrollXHi = 0x8007308E;
    const uint TilemapX = 0x800730C0;
    const uint TilemapWidth = 0x800730C8;
    const uint CameraAnchorX = 0x8009740C;
    const uint PlayerXWorld = 0x800973F0;
    const uint PlayerPosXHi = 0x800733DA;

    const uint StageId = 0x800974A0;
    const uint RoomLoadPos = 0x801375C0;
    static bool _camColDisabled;
    static int _disabledStage;

  //capture the camera intended pos so it can be set correctly without causing alucard to teleport weird
    static int _pA0, _pWorld, _pPosX;
    static uint _prevDest, _prevDestY;

    public static void PreCamCol(CpuContext c, IMemory m)
    {
        _pA0 = (int)c.A0;
        _pWorld = (int)m.ReadU32(PlayerXWorld);
        _pPosX = (short)m.ReadU16(PlayerPosXHi);
    }

    static int WideMargin(IMemory m, out int x, out int w)
    {
        x = (int)m.ReadU32(TilemapX);
        w = (int)m.ReadU32(TilemapWidth);
        int margin = StageMargin();
        if (margin == 0) return 0;
        return Math.Min(margin, Math.Max(0, ((w - x) - 256) / 2));
    }

    public static void PostCamCol(CpuContext c, IMemory m)
    {
        if (_pA0 != 1) return;
        if (_camColDisabled)
        {
            if (m.ReadU16(StageId) == _disabledStage) return;
            _camColDisabled = false;
        }
        int margin = WideMargin(m, out int x, out int w);
        if (margin == 0) return;

        uint dx = m.ReadU32(RoomLoadPos);
        uint dy = m.ReadU32(RoomLoadPos + 4);
        if (dx != _prevDest || dy != _prevDestY)
        {
            _prevDest = dx;
            _prevDestY = dy;
            m.WriteU16(TilemapScrollXHi, (ushort)(short)(_pWorld - _pPosX));
            m.WriteU16(PlayerPosXHi, (ushort)(short)_pPosX);
            return;
        }

        int world = (int)m.ReadU32(PlayerXWorld);
        int anchor = (int)m.ReadU32(CameraAnchorX);
        int wide = Math.Clamp(world - anchor, x + margin, w - 256 - margin);
        m.WriteU16(TilemapScrollXHi, (ushort)(short)wide);
        m.WriteU16(PlayerPosXHi, (ushort)(short)(world - wide));
    }

    public static void MarkCameraPan(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return;
        _camColDisabled = true;
        _disabledStage = m.ReadU16(StageId);
    }
}

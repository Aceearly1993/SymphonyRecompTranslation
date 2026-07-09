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
    static bool _camColDisabled;
    static int _disabledStage;

    public static void PostCamCol(CpuContext c, IMemory m)
    {
        if (_camColDisabled)
        {
            if (m.ReadU16(StageId) == _disabledStage) return;
            _camColDisabled = false;
        }
        int margin = StageMargin();
        if (margin == 0) return;
        int x = (int)m.ReadU32(TilemapX);
        int w = (int)m.ReadU32(TilemapWidth);
        int room = w - x;
        margin = Math.Min(margin, Math.Max(0, (room - 256) / 2));
        if (margin == 0) return;
        int world = (int)m.ReadU32(PlayerXWorld);
        int anchor = (int)m.ReadU32(CameraAnchorX);
        int wide = Math.Clamp(world - anchor, x + margin, w - 256 - margin);
        m.WriteU16(TilemapScrollXHi, (ushort)(short)wide);
        m.WriteU16(PlayerPosXHi, (ushort)(short)(world - wide));
    }

    public static void MarkCameraPan(CpuContext c, IMemory m)
    {
        _camColDisabled = true;
        _disabledStage = m.ReadU16(StageId);
    }
}

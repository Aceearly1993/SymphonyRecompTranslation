using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace Recompiled;

public static partial class WidescreenPatch
{
    const uint EntPrimIndex = 0x64;
    const uint PrimDrawMode = 0x32;
    const int DrawHide = 0x08;
    const int DraculaArenaX = 255;

    static bool _vortexUp;
    static int _vortexStage = -1;

    public static void MarkVortexUp(CpuContext c, IMemory m)
    {
        _vortexUp = true;
        _vortexStage = m.ReadU16(StageId);
    }

    static bool VortexActive(IMemory m) => _vortexUp && m.ReadU16(StageId) == _vortexStage;

    static void HideEntityPrims(CpuContext c, IMemory m)
    {
        uint self = c.A0;
        int index = (int)m.ReadU32(self + EntPrimIndex);
        if (index < 0) return;

        uint prim = PrimBufAddr + (uint)index * PrimStride;
        for (int guard = 0; prim != 0 && guard < 1024; guard++)
        {
            m.WriteU16(prim + PrimDrawMode, DrawHide);
            prim = m.ReadU32(prim);
        }
    }

    public static bool CloudsBattleHook(CpuContext c, IMemory m)
    {
        if (OriginalAspect) return true;
        if (!VortexActive(m) && S32(m, PlayerXWorld) > DraculaArenaX) return true; //no >= dumbass
        HideEntityPrims(c, m);
        return false;
    }

}

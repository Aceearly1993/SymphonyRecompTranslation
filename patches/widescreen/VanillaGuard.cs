using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace Recompiled;

//i could do it by changing the function signature but this way is more fun (and easier to not rbeak stuff by accident)
//doing this so rsl works, but it will require the original aspect ratio, because the patches maked for the widescreen and camera accidentaly fixes rsl
public static partial class WidescreenPatch
{

    //if screen is using original aspect itll skip executing the patches, this fixes the behaviour
    static bool Wide(System.Action<CpuContext, IMemory> impl, CpuContext c, IMemory m)
    {
        if (OriginalAspect) return true; //return true for using original func
        impl(c, m);
        return false; //false for skip so wont execute it again
    }

    public static bool ConfigStageClipWide(CpuContext c, IMemory m) => Wide(ConfigStageClip, c, m);
    public static bool RenderTilemapWideHook(CpuContext c, IMemory m) => Wide(RenderTilemapWide, c, m);
    public static bool SetTitleDisplayBufferWide(CpuContext c, IMemory m) => Wide(SetTitleDisplayBuffer, c, m);
    public static bool SetTitleDisplayBuffer256Wide(CpuContext c, IMemory m) => Wide(SetTitleDisplayBuffer256, c, m);
    public static bool InitFadeWide(CpuContext c, IMemory m) => Wide(InitFade, c, m);
    public static bool SetFadeWidthWide(CpuContext c, IMemory m) => Wide(SetFadeWidth, c, m);
    public static bool VortexWideHook(CpuContext c, IMemory m) => Wide(VortexWide, c, m);
    public static bool CloudsWideHook(CpuContext c, IMemory m) => Wide(CloudsWide, c, m);
    public static bool TitleFadeoutWideHook(CpuContext c, IMemory m) => Wide(TitleFadeoutWide, c, m);
    public static bool TransparentWaterWide_no3Hook(CpuContext c, IMemory m) => Wide(TransparentWaterWide_no3, c, m);
    public static bool TransparentWaterWide_np3Hook(CpuContext c, IMemory m) => Wide(TransparentWaterWide_np3, c, m);
    public static bool WaterSurface801C12B0Wide_no4Hook(CpuContext c, IMemory m) => Wide(WaterSurface801C12B0Wide_no4, c, m);
    public static bool LavaGlowWide_catHook(CpuContext c, IMemory m) => Wide(LavaGlowWide_cat, c, m);
    public static bool CamColWide(CpuContext c, IMemory m) => Wide(CamCol, c, m);
}

using System.Reflection;
using RecompOne.Runtime.Events;
using RecompOne.Runtime.Hle;
using RecompOne.Runtime.Memory;
using StbImageSharp;
using Sotn;

namespace Recompiled;

//this patch iss a good example of doing stuff on the gpu outside the ps1 vram path
public static class ThroneLeftFill
{
    const int WorldX = -256; //its left of block 0 so its actually blok -1
    const int WorldY = 0;
    const int Size = 256;
    const uint VortexUpdateAddr = 0x801BE1B4u;

    const uint CurrentBufferPtr = 0x8006C37C;
    const uint BackbufferX = 0x8006C39C;
    const uint BackbufferY = 0x8006C3A0;
    const uint TilemapAddr = 0x80073084;
    const uint OtOfs = 0x474;
    const int OtSize = 0x200;
    public static int OrderOffset = 40;

    static int _image = -1;
    static bool _failed;

    public static void Register() => Event.AddListener<TilemapRenderedEvent>(OnTilemapRendered);

    static void OnTilemapRendered(TilemapRenderedEvent e)
    {
        if (_failed || WidescreenPatch.OriginalAspect) return;
        if (Game.StageId != Stage.Prologue) return;
        if (VortexActive()) return;

        var m = e.Memory;
        if (m == null) return;
        if (_image < 0 && !Load()) return;

        if (m.ReadU32(TilemapAddr + 0x20) == 0 || m.ReadU32(TilemapAddr + 0x00) == 0) return;

        GpuPrims.SetOrderingTable(m.ReadU32(CurrentBufferPtr) + OtOfs, OtSize);

        int x = (int)m.ReadU32(BackbufferX) + WorldX - e.ScrollX;
        int y = (int)m.ReadU32(BackbufferY) + WorldY - e.ScrollY;
        int order = (int)m.ReadU32(TilemapAddr + 0x18) - OrderOffset;

        GpuPrims.Sprite(order, _image, x, y, Size, Size);
    }

    static bool VortexActive()
    {
        foreach (var ent in Entities.All())
            if (ent.Update == VortexUpdateAddr) return true;
        return false;
    }

    static bool Load()
    {
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            var res = Array.Find(asm.GetManifestResourceNames(),
                n => n.EndsWith(".extension.st0_throne_left.png", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("st0_throne_left.png not embedded");

            using var stream = asm.GetManifestResourceStream(res)
                ?? throw new InvalidOperationException($"couldnt open {res}");

            var img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            _image = GpuPrims.RegisterImage(img.Data, img.Width, img.Height);
            if (_image < 0) return false;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ThroneLeftFill] had exception {ex.Message}");
            _failed = true;
            return false;
        }
    }
}

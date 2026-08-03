using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace Recompiled;

//makes so alucard shield and wepon dont automatically despawn for being outside the view area
public static partial class WidescreenPatch
{
    const uint PlayerEntityTable = 0x800733D8;
    const int PlayerEntityStride = 0xBC;
    const uint PedPosXHi = 0x02;
    const uint PedPosYHi = 0x06;
    const uint PedEntityId = 0x26;
    const uint PedFlags = 0x34;
    const int PedEnd = 64;
    const int PedTableEnd = 256;

    static int _pedShift;
    static readonly ushort[] _pedOuterId = new ushort[PedTableEnd - PedEnd];

    static uint PedAt(int index) => PlayerEntityTable + (uint)(index * PlayerEntityStride);

    public static void PreUpdatePlayerEntities(CpuContext c, IMemory m)
    {
        int margin = StageMargin();

        if (margin == 0) return;

        int shift = HitShift(m);
        if (shift == 0) return;
        _pedShift = shift;
        for (int i = 0; i < PedEnd; i++) ShiftX(m, PedAt(i) + PedPosXHi, shift);

        for (int i = PedEnd; i < PedTableEnd; i++)
            _pedOuterId[i - PedEnd] = m.ReadU16(PedAt(i) + PedEntityId);
    }

    public static void PostUpdatePlayerEntities(CpuContext c, IMemory m)
    {
        int shift = _pedShift;
        if (shift == 0) return;
        _pedShift = 0;

        for (int i = 0; i < PedEnd; i++) ShiftX(m, PedAt(i) + PedPosXHi, -shift);



        for (int i = PedEnd; i < PedTableEnd; i++)
        {
            ushort id = m.ReadU16(PedAt(i) + PedEntityId);
            if (id == 0 || id == _pedOuterId[i - PedEnd]) continue;
            ShiftX(m, PedAt(i) + PedPosXHi, -shift);
        }

    }
}

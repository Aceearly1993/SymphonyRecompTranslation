using RecompOne.Runtime.Memory;
using System.Text;

namespace Recompiled;

public static class Tracker
{
    public const uint RelicBase = 0x80097964;
    public const uint EquipBodyBase = 0x80097A33;

    public const byte RelicFlagFound = 0x01;
    public const byte RelicFlagActive = 0x02;

    public enum Kind : byte { Relic, KeyItem }
    //public enum Kind : byte { Relic, KeyItem, HandItem }

    public sealed class Entry
    {
        public string Name { get; }
        public uint[] Addresses { get; }
        public Kind Kind { get; }
        public string Icon { get; }

        public Entry(string name, uint address, Kind kind)
        {
            Name = name;
            Addresses = [address];
            Kind = kind;
            Icon = name;
        }

        public Entry(string name, uint[] addresses, Kind kind, string? icon = null)
        {
            Name = name;
            Addresses = addresses;
            Kind = kind;
            Icon = icon ?? name;
        }

        public uint Address => Addresses[0];
    }

    static Entry Relic(int index, string name) => new(name, RelicBase + (uint)index, Kind.Relic);

    public static readonly Entry[] Relics =
    [
        Relic(0, "Soul of Bat"),
        Relic(1, "Fire of Bat"),
        Relic(2, "Echo of Bat"),
        Relic(3, "Force of Echo"),
        Relic(4, "Soul of Wolf"),
        Relic(5, "Power of Wolf"),
        Relic(6, "Skill of Wolf"),
        Relic(7, "Form of Mist"),
        Relic(8, "Power of Mist"),
        Relic(9, "Gas Cloud"),
        Relic(10, "Cube of Zoe"),
        Relic(11, "Spirit Orb"),
        Relic(12, "Gravity Boots"),
        Relic(13, "Leap Stone"),
        Relic(14, "Holy Symbol"),
        Relic(15, "Faerie Scroll"),
        Relic(16, "Jewel of Open"),
        Relic(17, "Merman Statue"),
        Relic(18, "Bat Card"),
        Relic(19, "Ghost Card"),
        Relic(20, "Faerie Card"),
        Relic(21, "Demon Card"),
        Relic(22, "Sword Card"),
    ];

    public static readonly Entry[] JpRelics =
    [
        Relic(23, "Sprite Card"),
        Relic(24, "Nose Devil Card"),
    ];

    public static readonly Entry[] VladRelics =
    [
        Relic(25, "Heart of Vlad"),
        Relic(26, "Tooth of Vlad"),
        Relic(27, "Rib of Vlad"),
        Relic(28, "Ring of Vlad"),
        Relic(29, "Eye of Vlad"),
    ];

    public static readonly Entry[] KeyItems =
    [
        new("Gold Ring", 0x80097A7B, Kind.KeyItem),
        new("Silver Ring", 0x80097A7C, Kind.KeyItem),
        new("Spike Breaker", 0x80097A41, Kind.KeyItem),
        new("Holy Glasses", 0x80097A55, Kind.KeyItem),
        new("Library Card", 0x80097A30, Kind.KeyItem),
        new("Thrust Sword", new uint[] { 0x800979E9, 0x800979EC, 0x800979EF, 0x800979F1, 0x800979F5 }, Kind.KeyItem, "Claymore"),
    ];

    //broken/todo
    //public static readonly Entry[] HandItems = BuildRange(EquipHandBase, HandItemCount, Kind.HandItem);
    //public static readonly Entry[] BodyItems = BuildRange(EquipBodyBase, BodyItemCount, Kind.KeyItem);
    //
    //static Entry[] BuildRange(uint invBase, int count, Kind kind)
    //{
    //    var arr = new Entry[count];
    //    for (int i = 0; i < count; i++)
    //        arr[i] = new("", invBase + (uint)i, kind);
    //    return arr;
    //}
    //public static (uint ptrAddr, int stride, int iconOff, uint invBase) DefInfo(Kind kind) => kind switch
    //{
    //    Kind.Relic => (0x8003C850u, 0x10, 0x08, RelicBase),
    //    Kind.KeyItem => (0x8003C834u, 0x1E, 0x18, EquipBodyBase),
    //    Kind.HandItem => (0x8003C830u, 0x32, 0x2C, EquipHandBase),
    //    _ => (0u, 0, 0, 0u),
    //};
    //public static bool IsRam(uint addr) => addr >= 0x80010000 && addr < 0x80200000;

    public static byte RawValue(IMemory m, Entry e) => m.ReadU8(e.Address);

    public static bool IsOwned(IMemory m, Entry e)
    {
        if (e.Kind == Kind.Relic)
            return (m.ReadU8(e.Address) & RelicFlagFound) != 0;

        foreach (var a in e.Addresses)
            if (m.ReadU8(a) != 0) return true;
        return false;
    }

    public static bool IsActive(IMemory m, Entry e) =>
        e.Kind == Kind.Relic && (m.ReadU8(e.Address) & RelicFlagActive) != 0;

    public static int CountOwned(IMemory m, Entry[] items)
    {
        int n = 0;
        foreach (var e in items)
            if (IsOwned(m, e)) n++;
        return n;
    }

    //this doesnt work at all
    //public static string ResolveName(IMemory m, Entry e)
    //{
    //    if (!string.IsNullOrEmpty(e.Name)) return e.Name;
    //
    //    var (ptrAddr, stride, _, invBase) = DefInfo(e.Kind);
    //    uint defsBase = m.ReadU32(ptrAddr);
    //    if (!IsRam(defsBase)) return "";
    //
    //    uint entryAddr = defsBase + (e.Address - invBase) * (uint)stride;
    //    uint namePtr = m.ReadU32(entryAddr);
    //    if (!IsRam(namePtr)) return "";
    //
    //    var sb = new StringBuilder();
    //    for (int i = 0; i < 32; i++)
    //    {
    //        byte c = m.ReadU8(namePtr + (uint)i);
    //        if (c == 0) break;
    //        sb.Append((char)c);
    //    }
    //    return sb.ToString().Trim();
    //}
}

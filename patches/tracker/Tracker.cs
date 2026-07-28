using RecompOne.Runtime.Memory;
using System;
using System.Text;
using Sotn;

namespace Recompiled;

public static class Tracker
{
    public const uint RelicBase = 0x80097964;
    public const uint EquipHandBase = 0x8009798A;
    public const uint EquipBodyBase = 0x80097A33;

    public const byte RelicFlagFound = 0x01;
    public const byte RelicFlagActive = 0x02;

    public enum Kind : byte { Relic, KeyItem, HandItem, BodyItem }

    public sealed class Entry
    {
        public string Name { get; }
        public uint[] Addresses { get; }
        public Kind Kind { get; }
        public string Icon { get; }
        public int ItemId { get; }

        public Entry(string name, uint address, Kind kind)
        {
            Name = name;
            Addresses = [address];
            Kind = kind;
            Icon = name;
            ItemId = -1;
        }

        public Entry(string name, uint[] addresses, Kind kind, string? icon = null)
        {
            Name = name;
            Addresses = addresses;
            Kind = kind;
            Icon = icon ?? name;
            ItemId = -1;
        }

        public Entry(string name, uint address, Kind kind, int itemId)
        {
            Name = name;
            Addresses = [address];
            Kind = kind;
            Icon = name;
            ItemId = itemId;
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

    public static readonly Entry[] HandItems = BuildItems<HandItem>(Kind.HandItem, EquipHandBase);
    public static readonly Entry[] BodyItems = BuildItems<BodyItem>(Kind.BodyItem, EquipBodyBase);

    static Entry[] BuildItems<T>(Kind kind, uint countBase) where T : struct, Enum
    {
        uint hi = countBase + (uint)(kind == Kind.HandItem ? 169 : 90);
        var exclude = new System.Collections.Generic.HashSet<int>();
        foreach (var e in KeyItems)
            foreach (var a in e.Addresses)
                if (a >= countBase && a < hi) exclude.Add((int)(a - countBase));

        var vals = Enum.GetValues<T>();
        var list = new System.Collections.Generic.List<Entry>(vals.Length);
        foreach (var v in vals)
        {
            int id = Convert.ToInt32(v);
            if (id == 0 || exclude.Contains(id)) continue;
            list.Add(new Entry(Spaced(v.ToString()!), countBase + (uint)id, kind, id));
        }
        return list.ToArray();
    }

    static string Spaced(string name)
    {
        var sb = new StringBuilder(name.Length + 8);
        for (int i = 0; i < name.Length; i++)
        {
            char ch = name[i];
            if (i > 0 && char.IsUpper(ch) && (!char.IsUpper(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                sb.Append(' ');
            sb.Append(ch);
        }
        return sb.ToString();
    }

    public static byte RawValue(IMemory m, Entry e) => m.ReadU8(e.Address);

    public static bool IsOwned(IMemory m, Entry e) => e.Kind switch
    {
        Kind.Relic => Inventory.HasRelic((int)(e.Address - RelicBase)),
        Kind.HandItem => Inventory.HasHandItem(e.ItemId),
        Kind.BodyItem => Inventory.HasBodyItem(e.ItemId),
        _ => AnyAddrSet(m, e),
    };

    static bool AnyAddrSet(IMemory m, Entry e)
    {
        foreach (var a in e.Addresses)
            if (m.ReadU8(a) != 0) return true;
        return false;
    }

    public static bool IsActive(IMemory m, Entry e) =>
        e.Kind == Kind.Relic && Inventory.IsRelicActive((int)(e.Address - RelicBase));

    public static int CountOwned(IMemory m, Entry[] items)
    {
        int n = 0;
        foreach (var e in items)
            if (IsOwned(m, e)) n++;
        return n;
    }
}

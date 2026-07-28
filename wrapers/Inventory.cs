using RecompOne.Runtime.Memory;

namespace Sotn;
public static class Inventory
{
    static IMemory M => RecompOne.Runtime.Runtime.Mem!;
    static uint S => Game.StatusAddr;

    const uint RelicsOff = 0x000;
    const uint SpellsOff = 0x01E;
    const uint HandCountOff = 0x026;
    const uint BodyCountOff = 0x0CF;
    const uint SpellsLearntOff = 0x238;
    const uint SubWeaponOff = 0x298;
    const uint WornEquipOff = 0x29C;
    const uint AttackHandsOff = 0x2B8;

    public const int RelicCount = 30;
    public const int SpellCount = 8;
    public const int HandItemCount = 169;
    public const int BodyItemCount = 90;
    public const int WornEquipCount = 5;

    //hnd
    public static int GetHandCount(int id) => M.ReadU8(S + HandCountOff + (uint)id);
    public static void SetHandCount(int id, int n) => M.WriteU8(S + HandCountOff + (uint)id, (byte)n);
    public static bool HasHandItem(int id) => GetHandCount(id) > 0;
    public static void AddHandItem(int id, int n = 1) => SetHandCount(id, System.Math.Clamp(GetHandCount(id) + n, 0, 255));
    public static void RemoveHandItem(int id, int n = 1) => AddHandItem(id, -n);

    public static int GetHandCount(HandItem item) => GetHandCount((int)item);
    public static void SetHandCount(HandItem item, int n) => SetHandCount((int)item, n);
    public static bool HasHandItem(HandItem item) => HasHandItem((int)item);
    public static void AddHandItem(HandItem item, int n = 1) => AddHandItem((int)item, n);
    public static void RemoveHandItem(HandItem item, int n = 1) => RemoveHandItem((int)item, n);

    //bdy
    public static int GetBodyCount(int id) => M.ReadU8(S + BodyCountOff + (uint)id);
    public static void SetBodyCount(int id, int n) => M.WriteU8(S + BodyCountOff + (uint)id, (byte)n);
    public static bool HasBodyItem(int id) => GetBodyCount(id) > 0;
    public static void AddBodyItem(int id, int n = 1) => SetBodyCount(id, System.Math.Clamp(GetBodyCount(id) + n, 0, 255));
    public static void RemoveBodyItem(int id, int n = 1) => AddBodyItem(id, -n);

    public static int GetBodyCount(BodyItem item) => GetBodyCount((int)item);
    public static void SetBodyCount(BodyItem item, int n) => SetBodyCount((int)item, n);
    public static bool HasBodyItem(BodyItem item) => HasBodyItem((int)item);
    public static void AddBodyItem(BodyItem item, int n = 1) => AddBodyItem((int)item, n);
    public static void RemoveBodyItem(BodyItem item, int n = 1) => RemoveBodyItem((int)item, n);

    //rlc
    public static byte GetRelic(int id) => M.ReadU8(S + RelicsOff + (uint)id);
    public static void SetRelic(int id, byte value) => M.WriteU8(S + RelicsOff + (uint)id, value);
    public static bool HasRelic(int id) => GetRelic(id) != 0;

    public static bool IsRelicActive(int id) => (GetRelic(id) & 2) != 0;

    public static byte GetRelic(Relic relic) => GetRelic((int)relic);
    public static bool HasRelic(Relic relic) => HasRelic((int)relic);
    public static bool IsRelicActive(Relic relic) => IsRelicActive((int)relic);
    public static void GiveRelic(Relic relic, bool on) => SetRelic((int)relic, (byte)(on ? 3 : 0));

    //spll/
    public static byte GetSpell(int id) => M.ReadU8(S + SpellsOff + (uint)id);
    public static void SetSpell(int id, byte value) => M.WriteU8(S + SpellsOff + (uint)id, value);
    public static uint SpellsLearnt { get => M.ReadU32(S + SpellsLearntOff); set => M.WriteU32(S + SpellsLearntOff, value); }
    public static bool HasSpell(Spell spell) => (SpellsLearnt & (1u << (int)spell)) != 0;

    public static void SetSpellLearned(Spell spell, bool on)
    {
        uint mask = 1u << (int)spell;
        SpellsLearnt = on ? SpellsLearnt | mask : SpellsLearnt & ~mask;

        uint learnt = SpellsLearnt;
        int slot = 0;
        for (int id = 0; id < SpellCount; id++)
            if ((learnt & (1u << id)) != 0)
                SetSpell(slot++, (byte)(id | 0x80));
        for (; slot < SpellCount; slot++)
            SetSpell(slot, 0);
    } //fixed

    public static uint GetWornEquipment(int slot) => M.ReadU32(S + WornEquipOff + (uint)(slot * 4));
    public static void SetWornEquipment(int slot, uint id) => M.WriteU32(S + WornEquipOff + (uint)(slot * 4), id);
    public static uint RightHand { get => M.ReadU32(S + AttackHandsOff); set => M.WriteU32(S + AttackHandsOff, value); }
    public static uint LeftHand { get => M.ReadU32(S + AttackHandsOff + 4); set => M.WriteU32(S + AttackHandsOff + 4, value); }
    public static uint SubWeapon { get => M.ReadU32(S + SubWeaponOff); set => M.WriteU32(S + SubWeaponOff, value); }
}

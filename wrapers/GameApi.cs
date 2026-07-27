using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;

namespace Sotn;

public static class GameApi
{
    public const uint FreePrimitivesAddr = 0x8003C7B4u;
    public const uint AllocPrimitivesAddr = 0x8003C7B8u;
    public const uint CheckCollisionAddr = 0x8003C7BCu;
    public const uint GetFreeEntityAddr = 0x8003C7CCu;
    public const uint PlaySfxAddr = 0x8003C7DCu;
    public const uint SetFadeModeAddr = 0x8003C7ECu;
    public const uint CreateEntFactoryAddr = 0x8003C7F4u;
    public const uint DealDamageAddr = 0x8003C828u;
    public const uint AddHeartsAddr = 0x8003C838u;
    public const uint AddToInventoryAddr = 0x8003C84Cu;
    public const uint InitStatsAndGearAddr = 0x8003C854u;
    public const uint LearnSpellAddr = 0x8003C898u;

    static IMemory M => RecompOne.Runtime.Runtime.Mem!;

    public static uint Call(CpuContext c, IMemory m, uint funcAddr, uint a0 = 0, uint a1 = 0, uint a2 = 0, uint a3 = 0)
    {
        var snap = c.Snapshot();
        c.A0 = a0;
        c.A1 = a1;
        c.A2 = a2;
        c.A3 = a3;
        Dispatcher.Call(c, m, funcAddr);
        uint ret = c.V0;
        c.Restore(snap);
        return ret;
    }

    public static uint Call(uint funcAddr, uint a0 = 0, uint a1 = 0, uint a2 = 0, uint a3 = 0)
    {
        var c = RecompOne.Runtime.Runtime.Cpu;
        var m = RecompOne.Runtime.Runtime.Mem;
        if (c == null || m == null) return 0;
        return Call(c, m, funcAddr, a0, a1, a2, a3);
    }

    //add game calls here
    public static uint CallApi(uint apiSlot, uint a0 = 0, uint a1 = 0, uint a2 = 0, uint a3 = 0)=> Call(M.ReadU32(apiSlot), a0, a1, a2, a3);
    public static uint CallApi(CpuContext c, IMemory m, uint apiSlot, uint a0 = 0, uint a1 = 0, uint a2 = 0, uint a3 = 0) => Call(c, m, m.ReadU32(apiSlot), a0, a1, a2, a3);

    public static void FreePrimitives(int primIndex) => CallApi(FreePrimitivesAddr, (uint)primIndex);
    public static Entity GetFreeEntity(int start, int end) => new(CallApi(GetFreeEntityAddr, (uint)start, (uint)end));
    public static Entity CreateFactory(Entity self, uint flags, int arg2) => new(CallApi(CreateEntFactoryAddr, self.Addr, flags, (uint)arg2));
    public static void PlaySfx(int sfxId) => CallApi(PlaySfxAddr, (uint)sfxId);
    public static void AddHearts(int amount) => CallApi(AddHeartsAddr, (uint)amount);
    public static void LearnSpell(Spell spell) => CallApi(LearnSpellAddr, (uint)spell);
    public static void AddToInventory(int id, int kind) => CallApi(AddToInventoryAddr, (uint)id, (uint)kind);
    public static int DealDamage(Entity enemy, Entity attacker) => (int)(ushort)CallApi(DealDamageAddr, enemy.Addr, attacker.Addr);
}

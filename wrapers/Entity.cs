using RecompOne.Runtime.Memory;

namespace Sotn;

public sealed class Entity
{
    public const int Stride = 0xBC;
    const int FlagDead = 0x100;
    const int FlagHasPrims = 0x800000;
    const int FlagNotAnEnemy = 0x01000000;

    public readonly uint Addr;
    public Entity(uint addr) => Addr = addr;

    static IMemory M => RecompOne.Runtime.Runtime.Mem!;

    public bool IsValid => Addr != 0;

    public int PosXRaw { get => (int)M.ReadU32(Addr + 0x00); set => M.WriteU32(Addr + 0x00, (uint)value); }
    public int PosYRaw { get => (int)M.ReadU32(Addr + 0x04); set => M.WriteU32(Addr + 0x04, (uint)value); }
    public int PosX { get => PosXRaw >> 16; set => PosXRaw = value << 16; }
    public int PosY { get => PosYRaw >> 16; set => PosYRaw = value << 16; }
    public int VelocityX { get => (int)M.ReadU32(Addr + 0x08); set => M.WriteU32(Addr + 0x08, (uint)value); }
    public int VelocityY { get => (int)M.ReadU32(Addr + 0x0C); set => M.WriteU32(Addr + 0x0C, (uint)value); }
    public ushort FacingLeft { get => M.ReadU16(Addr + 0x14); set => M.WriteU16(Addr + 0x14, value); }
    public ushort Palette { get => M.ReadU16(Addr + 0x16); set => M.WriteU16(Addr + 0x16, value); }
    public byte BlendMode { get => M.ReadU8(Addr + 0x18); set => M.WriteU8(Addr + 0x18, value); }
    public byte DrawFlags { get => M.ReadU8(Addr + 0x19); set => M.WriteU8(Addr + 0x19, value); }
    public short ScaleX { get => (short)M.ReadU16(Addr + 0x1A); set => M.WriteU16(Addr + 0x1A, (ushort)value); }
    public short ScaleY { get => (short)M.ReadU16(Addr + 0x1C); set => M.WriteU16(Addr + 0x1C, (ushort)value); }
    public short Rotate { get => (short)M.ReadU16(Addr + 0x1E); set => M.WriteU16(Addr + 0x1E, (ushort)value); }
    public ushort ZPriority { get => M.ReadU16(Addr + 0x24); set => M.WriteU16(Addr + 0x24, value); }
    public ushort EntityId => M.ReadU16(Addr + 0x26);
    public uint Update { get => M.ReadU32(Addr + 0x28); set => M.WriteU32(Addr + 0x28, value); }
    public ushort Step { get => M.ReadU16(Addr + 0x2C); set => M.WriteU16(Addr + 0x2C, value); }
    public ushort StepSub { get => M.ReadU16(Addr + 0x2E); set => M.WriteU16(Addr + 0x2E, value); }
    public ushort Params { get => M.ReadU16(Addr + 0x30); set => M.WriteU16(Addr + 0x30, value); }
    public ushort RoomIndex => M.ReadU16(Addr + 0x32);
    public int Flags { get => (int)M.ReadU32(Addr + 0x34); set => M.WriteU32(Addr + 0x34, (uint)value); }
    public ushort EnemyId => M.ReadU16(Addr + 0x3A);
    public ushort HitboxState { get => M.ReadU16(Addr + 0x3C); set => M.WriteU16(Addr + 0x3C, value); }
    public short HitPoints { get => (short)M.ReadU16(Addr + 0x3E); set => M.WriteU16(Addr + 0x3E, (ushort)value); }
    public short Attack { get => (short)M.ReadU16(Addr + 0x40); set => M.WriteU16(Addr + 0x40, (ushort)value); }
    public ushort AttackElement { get => M.ReadU16(Addr + 0x42); set => M.WriteU16(Addr + 0x42, value); }
    public byte HitboxWidth { get => M.ReadU8(Addr + 0x46); set => M.WriteU8(Addr + 0x46, value); }
    public byte HitboxHeight { get => M.ReadU8(Addr + 0x47); set => M.WriteU8(Addr + 0x47, value); }
    public byte NFramesInvincibility { get => M.ReadU8(Addr + 0x49); set => M.WriteU8(Addr + 0x49, value); }
    public uint Anim { get => M.ReadU32(Addr + 0x4C); set => M.WriteU32(Addr + 0x4C, value); }
    public ushort Pose { get => M.ReadU16(Addr + 0x50); set => M.WriteU16(Addr + 0x50, value); }
    public short PoseTimer { get => (short)M.ReadU16(Addr + 0x52); set => M.WriteU16(Addr + 0x52, (ushort)value); }
    public short AnimSet { get => (short)M.ReadU16(Addr + 0x54); set => M.WriteU16(Addr + 0x54, (ushort)value); }
    public short AnimCurFrame { get => (short)M.ReadU16(Addr + 0x56); set => M.WriteU16(Addr + 0x56, (ushort)value); }
    public int PrimIndex { get => (int)M.ReadU32(Addr + 0x64); set => M.WriteU32(Addr + 0x64, (uint)value); }
    public byte Opacity { get => M.ReadU8(Addr + 0x6C); set => M.WriteU8(Addr + 0x6C, value); }

    public bool IsAlive => Update != 0;
    public bool IsDead => (Flags & FlagDead) != 0;
    public bool IsEnemy => IsAlive && (Flags & FlagNotAnEnemy) == 0;

    public void Kill() => HitPoints = 0; //not sure if this is right

    //game has various copies of Destroy entity, so use my own its simpler since its all duplicates
    public void Destroy()
    {
        if (Addr == 0) return;
        if ((Flags & FlagHasPrims) != 0)
            GameApi.FreePrimitives(PrimIndex);
        for (uint i = 0; i < Stride; i += 4)
            M.WriteU32(Addr + i, 0);
    }
}

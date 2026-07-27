using RecompOne.Runtime.Memory;

namespace Sotn;

public static class Player
{
    static IMemory M => RecompOne.Runtime.Runtime.Mem!;
    static uint S => Game.StatusAddr;

    public static PlayableCharacter Character => Game.Character;
    public static bool IsAlucard => Character == PlayableCharacter.Alucard;
    public static bool IsRichter => Character == PlayableCharacter.Richter;

    public static Entity Entity => new(Game.EntitiesAddr);

    public static int PosX { get => Entity.PosX; set { var e = Entity; e.PosX = value; } }
    public static int PosY { get => Entity.PosY; set { var e = Entity; e.PosY = value; } }

    public static int Hp { get => (int)M.ReadU32(S + 0x23C); set => M.WriteU32(S + 0x23C, (uint)value); }
    public static int HpMax { get => (int)M.ReadU32(S + 0x240); set => M.WriteU32(S + 0x240, (uint)value); }
    public static int Hearts { get => (int)M.ReadU32(S + 0x244); set => M.WriteU32(S + 0x244, (uint)value); }
    public static int HeartsMax { get => (int)M.ReadU32(S + 0x248); set => M.WriteU32(S + 0x248, (uint)value); }
    public static int Mp { get => (int)M.ReadU32(S + 0x24C); set => M.WriteU32(S + 0x24C, (uint)value); }
    public static int MpMax { get => (int)M.ReadU32(S + 0x250); set => M.WriteU32(S + 0x250, (uint)value); }
    public static int Strength { get => (int)M.ReadU32(S + 0x254); set => M.WriteU32(S + 0x254, (uint)value); }
    public static int Constitution { get => (int)M.ReadU32(S + 0x258); set => M.WriteU32(S + 0x258, (uint)value); }
    public static int Intelligence { get => (int)M.ReadU32(S + 0x25C); set => M.WriteU32(S + 0x25C, (uint)value); }
    public static int Luck { get => (int)M.ReadU32(S + 0x260); set => M.WriteU32(S + 0x260, (uint)value); }
    public static int StrengthTotal => (int)M.ReadU32(S + 0x274);
    public static int ConstitutionTotal => (int)M.ReadU32(S + 0x278);
    public static int IntelligenceTotal => (int)M.ReadU32(S + 0x27C);
    public static int LuckTotal => (int)M.ReadU32(S + 0x280);
    public static int Level { get => (int)M.ReadU32(S + 0x284); set => M.WriteU32(S + 0x284, (uint)value); }
    public static int Exp { get => (int)M.ReadU32(S + 0x288); set => M.WriteU32(S + 0x288, (uint)value); }
    public static int Gold { get => (int)M.ReadU32(S + 0x28C); set => M.WriteU32(S + 0x28C, (uint)value); }
    public static int KillCount { get => (int)M.ReadU32(S + 0x290); set => M.WriteU32(S + 0x290, (uint)value); }
    public static int SubWeapon { get => (int)M.ReadU32(S + 0x298); set => M.WriteU32(S + 0x298, (uint)value); }
}

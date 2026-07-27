using RecompOne.Runtime.Memory;

namespace Recompiled;

public static class Cheats
{
    public const uint Player = 0x800733D8;
    public const uint Pads = 0x80097490;
    public const uint GameStateAddr = 0x8003C734;

    public const uint PosX = Player + 0x00;
    public const uint PosY = Player + 0x04;
    public const uint VelX = Player + 0x08;
    public const uint VelY = Player + 0x0C;

    public const uint PlayerXWorld = 0x800973F0;
    public const uint PlayerYWorld = 0x800973F4;

    public const uint InvincTimer = 0x80072F1A;
    public const ushort InvincValue = 0x7FFF;

    public const uint DebugPlayer = 0x80098850;

    public const uint CastleMap = 0x8006BB74;
    public const int CastleMapSize = 0x800;

    public const int GamePlay = 2;
    public const int One = 0x10000;

    public const ushort PadCross = 0x0040;

    public static bool InPlay()
    {
        var m = RecompOne.Runtime.Runtime.Mem;
        return m != null && m.ReadU32(GameStateAddr) == GamePlay;
    }
}

using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;
using Sotn;

namespace Recompiled;

public static class MovementCheat
{
    public static bool SpeedOverride;
    public static float SpeedMul = 1f;

    public static bool JumpOverride;
    public static float JumpStrength = 6f;

    public static bool InfiniteJump;
    public static bool NoClip;
    public static bool Invincible;

    const int One = 0x10000;
    const uint PadCross = 0x0040;
    const uint PadTappedOff = 0x31C;

    static int _prevVelY;
    static int _prevX;
    static bool _invincWasOn;
    static bool _noClipWasOn;
    static bool _teleportPending;
    static int _tpX;
    static int _tpY;

    public static void PreEngine(CpuContext c, IMemory m)
    {
        if (!Cheats.InPlay()) return;
        _prevX = Player.Entity.PosXRaw;
    }

    public static void PostEngine(CpuContext c, IMemory m)
    {
        if (!Cheats.InPlay()) return;
        var p = Player.Entity;

        if (Invincible)
        {
            m.WriteU16(Cheats.InvincTimer, Cheats.InvincValue);
            _invincWasOn = true;
        }
        else if (_invincWasOn)
        {
            m.WriteU16(Cheats.InvincTimer, 0);
            _invincWasOn = false;
        }

        if (NoClip)
        {
            m.WriteU32(Cheats.DebugPlayer, 1);
            _noClipWasOn = true;
        }
        else if (_noClipWasOn)
        {
            m.WriteU32(Cheats.DebugPlayer, 0);
            _noClipWasOn = false;
        }

        if (SpeedOverride && SpeedMul != 1f)
        {
            int dx = p.PosXRaw - _prevX;
            p.PosXRaw = _prevX + (int)(dx * SpeedMul);
        }

        int velY = p.VelocityY;
        uint tapped = m.ReadU32(Game.PlayerStateAddr + PadTappedOff);

        if (InfiniteJump && (tapped & PadCross) != 0)
        {
            velY = -(int)(JumpStrength * One);
            p.VelocityY = velY;
        }
        else if (JumpOverride && _prevVelY >= 0 && velY < 0)
        {
            velY = -(int)(JumpStrength * One);
            p.VelocityY = velY;
        }

        _prevVelY = velY;
    }

    public static void PreCamera(CpuContext c, IMemory m)
    {
        WidescreenPatch.PreCamCol(c, m);
        if (_teleportPending)
        {
            _teleportPending = false;
            m.WriteU32(Cheats.PlayerXWorld, (uint)_tpX);
            m.WriteU32(Cheats.PlayerYWorld, (uint)_tpY);
            var p = Player.Entity;
            p.VelocityX = 0;
            p.VelocityY = 0;
        }
    }

    public static void RequestTeleport(int x, int y)
    {
        _tpX = x;
        _tpY = y;
        _teleportPending = true;
    }
}

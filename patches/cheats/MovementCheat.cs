using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

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

    static int _prevVelY;
    static bool _invincWasOn;
    static int _savedVelX;
    static int _scaledVelX;
    static bool _velXScaled;
    static bool _teleportPending;
    static int _tpX;
    static int _tpY;

    public static void PrePhysics(CpuContext c, IMemory m)
    {
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

        if (SpeedOverride && SpeedMul != 1f)
        {
            _savedVelX = (int)m.ReadU32(Cheats.VelX);
            _scaledVelX = (int)(_savedVelX * SpeedMul);
            m.WriteU32(Cheats.VelX, (uint)_scaledVelX);
            _velXScaled = true;
        }

        int curVelY = (int)m.ReadU32(Cheats.VelY);
        ushort tapped = m.ReadU16(Cheats.Pads + 4);

        if (InfiniteJump && (tapped & Cheats.PadCross) != 0)
        {
            curVelY = -(int)(JumpStrength * Cheats.One);
            m.WriteU32(Cheats.VelY, (uint)curVelY);
        }
        else if (JumpOverride && _prevVelY >= 0 && curVelY < 0)
        {
            curVelY = -(int)(JumpStrength * Cheats.One);
            m.WriteU32(Cheats.VelY, (uint)curVelY);
        }

        _prevVelY = curVelY;
    }

    public static void PostPhysics(CpuContext c, IMemory m)
    {
        if (!_velXScaled) return;
        _velXScaled = false;
        if ((int)m.ReadU32(Cheats.VelX) == _scaledVelX)
            m.WriteU32(Cheats.VelX, (uint)_savedVelX);
    }

    public static bool NoClipCancel(CpuContext c, IMemory m) => !NoClip;

    public static void PreCamera(CpuContext c, IMemory m)
    {
        WidescreenPatch.PreCamCol(c, m);
        if (_teleportPending)
        {
            _teleportPending = false;
            m.WriteU32(Cheats.PlayerXWorld, (uint)_tpX);
            m.WriteU32(Cheats.PlayerYWorld, (uint)_tpY);
            m.WriteU32(Cheats.VelX, 0);
            m.WriteU32(Cheats.VelY, 0);
        }
    }

    public static void RequestTeleport(int x, int y)
    {
        _tpX = x;
        _tpY = y;
        _teleportPending = true;
    }
}

using System.Numerics;
using ImGuiNET;
using RecompOne.Runtime.Host.Window;

namespace Recompiled;

public sealed class MovementCheatPanel : IPanel
{
    public string Name => "Movement Cheats";
    public bool IsOpen { get; set; }

    int _inX;
    int _inY;

    public void Draw()
    {
        ImGui.SetNextWindowSize(new Vector2(320, 420), ImGuiCond.FirstUseEver);
        bool open = IsOpen;
        if (!ImGui.Begin(Name, ref open))
        {
            IsOpen = open;
            ImGui.End();
            return;
        }

        var m = RecompOne.Runtime.Runtime.Mem;
        if (m == null || !Cheats.InPlay())
        {
            ImGui.TextDisabled("Not in gameplay.");
            IsOpen = open;
            ImGui.End();
            return;
        }

        int curX = (int)m.ReadU32(Cheats.PlayerXWorld);
        int curY = (int)m.ReadU32(Cheats.PlayerYWorld);

        ImGui.SeparatorText("Position (room)");
        ImGui.Text($"Current:  X {curX}   Y {curY}");
        ImGui.InputInt("New X", ref _inX);
        ImGui.InputInt("New Y", ref _inY);
        if (ImGui.Button("Teleport"))
            MovementCheat.RequestTeleport(_inX, _inY);
        ImGui.SameLine();
        if (ImGui.Button("Copy current"))
        {
            _inX = curX;
            _inY = curY;
        }

        ImGui.SeparatorText("Speed"); //sp
        float speed = (int)m.ReadU32(Cheats.VelX) / (float)Cheats.One;
        ImGui.Text($"Velocity X: {speed:0.00} px/f   (default 1.00x)");
        ImGui.Checkbox("Override speed", ref MovementCheat.SpeedOverride);
        ImGui.SliderFloat("Multiplier", ref MovementCheat.SpeedMul, 0.1f, 5f, "%.2fx");

        ImGui.SeparatorText("Jump");
        float velY = (int)m.ReadU32(Cheats.VelY) / (float)Cheats.One;
        ImGui.Text($"Velocity Y: {velY:0.00} px/f");
        ImGui.SliderFloat("Strength", ref MovementCheat.JumpStrength, 1f, 16f, "%.1f px/f");
        ImGui.Checkbox("Override jump", ref MovementCheat.JumpOverride);

        ImGui.SeparatorText("Toggles");
        ImGui.Checkbox("Infinite jump (fly)", ref MovementCheat.InfiniteJump);
        ImGui.Checkbox("No clip", ref MovementCheat.NoClip);
        ImGui.Checkbox("Invincible", ref MovementCheat.Invincible);

        IsOpen = open;
        ImGui.End();
    }
}

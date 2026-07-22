using ImGuiNET;
using RecompOne.Runtime.Host.Window;

namespace Recompiled;

public static class CheatMenu
{
    public static void Register()
    {
        PanelManager.Register(new MovementCheatPanel());
        MenuRegistry.Register("Cheats", DrawItems, "Misc");
    }

    static void DrawItems()
    {
        Toggle<MovementCheatPanel>("Movement");
    }

    static void Toggle<T>(string label) where T : class, IPanel
    {
        var panel = PanelManager.Get<T>();
        if (panel == null) return;
        if (ImGui.MenuItem(label, null, panel.IsOpen))
            panel.IsOpen = !panel.IsOpen;
    }
}

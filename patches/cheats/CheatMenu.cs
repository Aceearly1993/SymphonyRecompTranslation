using ImGuiNET;
using RecompOne.Runtime.Host.Window;

namespace Recompiled;

public static class CheatMenu
{
    public static void Register()
    {
        PanelManager.Register(new MovementCheatPanel());
        PanelManager.Register(new StatsCheatPanel());
        PanelManager.Register(new InventoryCheatPanel());
        MenuRegistry.Register("Cheats", DrawItems, "Misc", 300);
    }

    static void DrawItems()
    {
        Toggle<MovementCheatPanel>("Movement");
        Toggle<StatsCheatPanel>("Stats");
        Toggle<InventoryCheatPanel>("Inventory");
    }

    static void Toggle<T>(string label) where T : class, IPanel
    {
        var panel = PanelManager.Get<T>();
        if (panel == null) return;
        if (ImGui.MenuItem(label, null, panel.IsOpen))
            panel.IsOpen = !panel.IsOpen;
    }
}

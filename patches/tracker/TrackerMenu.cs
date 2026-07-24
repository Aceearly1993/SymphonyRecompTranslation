using ImGuiNET;
using RecompOne.Runtime.Host.Window;

namespace Recompiled;

public static class TrackerMenu
{
    public static void Register()
    {
        PanelManager.Register(new TrackerOverlayPanel());
        MenuRegistry.Register("Fun", DrawItems, "Misc");
    }

    static void DrawItems()
    {
        Toggle<TrackerOverlayPanel>("Tracker Overlay");
    }

    static void Toggle<T>(string label) where T : class, IPanel
    {
        var panel = PanelManager.Get<T>();
        if (panel == null) return;
        if (ImGui.MenuItem(label, null, panel.IsOpen))
            panel.IsOpen = !panel.IsOpen;
    }
}

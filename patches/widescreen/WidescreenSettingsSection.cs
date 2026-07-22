using ImGuiNET;
using RecompOne.Runtime.Hle;
using RecompOne.Runtime.Host.Window;

namespace Recompiled;

public static class WidescreenSettings
{
    static readonly (string Label, float Value)[] _presets =
    [
        ("4:3", 4f / 3f),
        ("16:10", 16f / 10f),
        ("16:9", 16f / 9f),
        ("21:9", 21f / 9f),
    ];

    public static void Register()
    {
        SettingsRegistry.Extend("display", Draw);
    }

    static void Draw()
    {
        ImGui.Spacing();

        float aspect = RecompOne.Runtime.Runtime.View.GetFloat("WidescreenAspect", 16f / 9f);
        int selected = MatchPreset(aspect);

        ImGui.TextUnformatted("Widescreen Aspect Ratio");
        if (ImGui.BeginCombo("##aspect-preset", selected >= 0 ? _presets[selected].Label : "Custom"))
        {
            for (int i = 0; i < _presets.Length; i++)
            {
                if (ImGui.Selectable(_presets[i].Label, selected == i))
                    Apply(_presets[i].Value);
            }
            if (ImGui.Selectable("Custom", selected < 0))
            {
            }
            ImGui.EndCombo();
        }

        float custom = aspect;
        if (ImGui.SliderFloat("##aspect-custom", ref custom, 1.0f, 2.5f, "%.3f : 1"))
            Apply(custom);

        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
        ImGui.TextWrapped("Controls the widescreen rendering ratio. 4:3 keeps the original framing.");
        ImGui.PopStyleColor();

        ImGui.Spacing();
        bool pillarBoxing = WidescreenPatch.PillarBoxing;
        if (ImGui.Checkbox("pillar boxing", ref pillarBoxing))
            ApplyPillarBoxing(pillarBoxing);

        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
        ImGui.TextWrapped("Keeps the original top/bottom black bars in the stage view. Turn off to use the full vertical resolution.");
        ImGui.PopStyleColor();
    }

    static void ApplyPillarBoxing(bool pillarBoxing)
    {
        WidescreenPatch.PillarBoxing = pillarBoxing;
        RecompOne.Runtime.Runtime.View.SetBool("WidescreenPillarBoxing", pillarBoxing);
        RecompOne.Runtime.Runtime.SaveView();
    }

    static int MatchPreset(float aspect)
    {
        for (int i = 0; i < _presets.Length; i++)
            if (MathF.Abs(_presets[i].Value - aspect) < 0.001f) return i;
        return -1;
    }

    const float FourThirds = 4f / 3f;

    static void Apply(float aspect)
    {
        aspect = Math.Clamp(aspect, 1.0f, 3.0f);
        bool wasFourThirds = MathF.Abs(WidescreenPatch.StageAspect - FourThirds) < 0.001f;
        bool nowFourThirds = MathF.Abs(aspect - FourThirds) < 0.001f;
        
        if (wasFourThirds && !nowFourThirds) //fourthirdslolfunnyname
            RecompOne.Runtime.Runtime.ShowNotice("Different Aspect Ratios are not fully supported yet, this WILL cause problems, use it at your own risk");
        
        RecompOne.Runtime.Runtime.View.SetFloat("WidescreenAspect", aspect);
        Display.TargetAspect = aspect;
        if (Display.WideAspect > 0f) Display.WideAspect = aspect;
        WidescreenPatch.StageAspect = aspect;
        RecompOne.Runtime.Runtime.SaveView();
    }
}

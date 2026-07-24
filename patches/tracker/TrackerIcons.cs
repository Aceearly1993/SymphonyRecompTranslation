using System.Numerics;
using System.Reflection;
using System.Text;
using ImGuiNET;
using RecompOne.Runtime.Host;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Recompiled;

public static class TrackerIcons
{
    static readonly Dictionary<string, uint> _textures = new();
    static readonly HashSet<string> _missing = new();
    static string[]? _resourceNames;

    public static bool TryGetTexture(Tracker.Entry entry, out uint texId, out Vector2 uv0, out Vector2 uv1)
    {
        uv0 = Vector2.Zero;
        uv1 = Vector2.One;
        texId = Load(entry.Icon);
        return texId != 0;
    }

    static uint Load(string name)
    {
        if (_textures.TryGetValue(name, out var cached)) return cached;
        if (_missing.Contains(name)) return 0;

        var bytes = ReadAsset(name);
        if (bytes == null)
        {
            _missing.Add(name);
            return 0;
        }

        uint tex = 0;
        try
        {
            using var img = Image.Load<Rgba32>(bytes);
            var rgba = new byte[img.Width * img.Height * 4];
            img.CopyPixelDataTo(rgba);
            tex = HostWindow.UploadTexture(rgba, img.Width, img.Height);
        }
        catch
        {
            tex = 0;
        }

        if (tex == 0)
        {
            _missing.Add(name);
            return 0;
        }

        _textures[name] = tex;
        return tex;
    }

    static byte[]? ReadAsset(string name)
    {
        var asm = Assembly.GetExecutingAssembly();
        _resourceNames ??= asm.GetManifestResourceNames();
        string suffix = ".tracker." + Pascal(name) + ".png";

        foreach (var res in _resourceNames)
        {
            if (!res.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) continue;
            using var s = asm.GetManifestResourceStream(res);
            if (s == null) return null;
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
        return null;
    }

    static string Pascal(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        foreach (var p in parts)
            sb.Append(char.ToUpperInvariant(p[0])).Append(p.AsSpan(1));
        return sb.ToString();
    }

    public static void DrawIcon(Tracker.Entry entry, bool owned, Vector2 size)
    {
        var pos = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();

        if (TryGetTexture(entry, out uint texId, out var uv0, out var uv1) && texId != 0)
        {
            uint tint = owned ? 0xFFFFFFFFu : 0xFF585858u;
            dl.AddImage((nint)texId, pos, pos + size, uv0, uv1, tint);
            ImGui.Dummy(size);
            return;
        }

        uint fill = owned ? 0xFF3C3C3Cu : 0xFF1C1C1Cu;
        uint border = owned ? 0xFF9A9A9Au : 0xFF3A3A3Au;
        dl.AddRectFilled(pos, pos + size, fill, 3f);
        dl.AddRect(pos, pos + size, border, 3f);
        ImGui.Dummy(size);
    }
}

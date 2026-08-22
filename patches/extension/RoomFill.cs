using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Events;
using RecompOne.Runtime.Hle;
using RecompOne.Runtime.Memory;
using StbImageSharp;
using Sotn;

namespace Recompiled;

public sealed class RoomFillRect
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }
}

public sealed class RoomFill
{
    public string Overlay { get; set; } = "";
    public int Room { get; set; }
    public RoomFillRect? Rect { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
    public string Image { get; set; } = "";
    public string? Tiles { get; set; }
    public int OrderOffset { get; set; } = 40;
    public bool InOriginalAspect { get; set; }
    public string? HideWhenEntity { get; set; }
    public string? Note { get; set; }

    [JsonIgnore] public uint HideEntityAddr;
    [JsonIgnore] public ushort[]? TileData;
}

public static class RoomFills
{
    const uint CurrentBufferPtr = 0x8006C37C;
    const uint BackbufferX = 0x8006C39C;
    const uint BackbufferY = 0x8006C3A0;
    const uint TilemapAddr = 0x80073084;
    const uint OverlayHeader = 0x80180000;
    const uint TilemapLeft = TilemapAddr + 0x2C;
    const uint OtOfs = 0x474; //offst check if correct, valid
    const int OtSize = 0x200;
    const int Screen = 256;
    const int TilesPerScreen = 16;

    const uint ClutIdsAddr = 0x8003C104;

    sealed class Loaded
    {
        public int Image = -1;
        public int Width, Height;
        public bool Failed;
    }

    static readonly List<RoomFill> _fills = [];
    static readonly Dictionary<string, Loaded> _cache = [];
    static readonly Dictionary<string, string> _files = [];
    static readonly Dictionary<string, (byte[] Rgba, int Width, int Height)> _raw = [];
    static bool _loaded;
    static bool _anyOriginalAspect;
    static uint _overlayKey;
    static string? _overlayName;

    public static Func<RoomFill, string?>? ImageResolver;
    public static Func<RoomFill, bool>? Visible;

    public static IReadOnlyList<RoomFill> Fills
    {
        get
        {
            EnsureConfig();
            return _fills;
        }
    }

    public static void Register()
    {
        Event.AddListener<TilemapRenderedEvent>(OnTilemapRendered);
        Event.AddListener<TilemapLayerDrawnEvent>(OnLayerDrawn);
    }

    //in cases like on the trone filling for st0, some packs may want to change the tile set,and since it uses an image this points are made for replacing!
    public static void SetImage(string name, string path)
    {
        _files[name] = path;
        _raw.Remove(name);
        _cache.Remove(name);
    }

    public static void SetImage(string name, byte[] rgba, int width, int height)
    {
        _raw[name] = (rgba, width, height);
        _files.Remove(name);
        _cache.Remove(name);
    }

    public static void ResetImage(string name)
    {
        _files.Remove(name);
        _raw.Remove(name);
        _cache.Remove(name);
    }

    static void OnTilemapRendered(TilemapRenderedEvent e)
    {
        EnsureConfig();
        if (_fills.Count == 0) return;
        if (WidescreenPatch.OriginalAspect && !_anyOriginalAspect) return;

        var m = e.Memory;
        if (m == null) return;
        if (m.ReadU32(TilemapAddr + 0x20) == 0 || m.ReadU32(TilemapAddr + 0x00) == 0) return;

        int left = (int)m.ReadU32(TilemapLeft);
        int top = (int)m.ReadU32(TilemapLeft + 4);
        int right = (int)m.ReadU32(TilemapLeft + 8);
        int bottom = (int)m.ReadU32(TilemapLeft + 12);

        bool candidate = false;
        foreach (var fill in _fills)
            if (InRoom(fill, left, top, right, bottom)) { candidate = true; break; }
        if (!candidate) return;

        string? overlay = CurrentOverlay(m);
        bool table = false;
        int order = (int)m.ReadU32(TilemapAddr + 0x18);
        int bx = (int)m.ReadU32(BackbufferX);
        int by = (int)m.ReadU32(BackbufferY);

        foreach (var fill in _fills)
        {
            if (!InRoom(fill, left, top, right, bottom)) continue;
            if (!string.Equals(fill.Overlay, overlay, StringComparison.OrdinalIgnoreCase)) continue;
            if (fill.HideEntityAddr != 0 && EntityActive(fill.HideEntityAddr)) continue;
            if (Visible != null && !Visible(fill)) continue;

            if (fill.TileData != null) continue;

            var image = Resolve(fill);
            if (image == null) continue;

            if (!table)
            {
                GpuPrims.SetOrderingTable(m.ReadU32(CurrentBufferPtr) + OtOfs, OtSize);
                table = true;
            }

            int x = bx + fill.TileX * Screen - e.ScrollX;
            int y = by + fill.TileY * Screen - e.ScrollY;
            GpuPrims.Sprite(order - fill.OrderOffset, image.Image, x, y, image.Width, image.Height);
        }
    }

    static void OnLayerDrawn(TilemapLayerDrawnEvent e)
    {
        EnsureConfig();
        if (_fills.Count == 0 || !e.Foreground) return;

        var m = e.Memory;
        if (m == null) return;

        int left = (int)m.ReadU32(TilemapLeft);
        int top = (int)m.ReadU32(TilemapLeft + 4);
        int right = (int)m.ReadU32(TilemapLeft + 8);
        int bottom = (int)m.ReadU32(TilemapLeft + 12);

        bool candidate = false;
        foreach (var fill in _fills)
            if (fill.TileData != null && InRoom(fill, left, top, right, bottom)) { candidate = true; break; }
        if (!candidate) return;

        string? overlay = CurrentOverlay(m);

        foreach (var fill in _fills)
        {
            if (fill.TileData == null) continue;
            if (!InRoom(fill, left, top, right, bottom)) continue;
            if (!string.Equals(fill.Overlay, overlay, StringComparison.OrdinalIgnoreCase)) continue;
            if (fill.HideEntityAddr != 0 && EntityActive(fill.HideEntityAddr)) continue;
            if (Visible != null && !Visible(fill)) continue;

            DrawTiles(m, e, fill);
        }
    }

    static void DrawTiles(IMemory m, TilemapLayerDrawnEvent e, RoomFill fill)
    {
        if (e.GfxPage == 0 || e.GfxIndex == 0 || e.ClutTable == 0) return;

        int baseX = e.BackbufferX + fill.TileX * Screen - e.ScrollX;
        int baseY = e.BackbufferY + fill.TileY * Screen - e.ScrollY;
        var tiles = fill.TileData!;
        uint sprites = e.Sprites;

        for (int ty = 0; ty < TilesPerScreen; ty++)
        {
            int y0 = baseY + ty * 16;
            for (int tx = 0; tx < TilesPerScreen; tx++)
            {
                ushort tile = tiles[ty * TilesPerScreen + tx];
                if (tile == 0) continue;
                if (sprites >= e.MaxSprites) break;

                byte g = m.ReadU8(e.GfxIndex + tile);
                byte page = m.ReadU8(e.GfxPage + tile);
                byte clutIdx = m.ReadU8(e.ClutTable + tile);
                ushort clut = m.ReadU16(ClutIdsAddr + (e.ClutBase + clutIdx) * 2);

                uint prim = e.Pool + sprites * 16;
                AddPrim(m, e.Ot, e.Order + page, prim, 3);
                m.WriteU32(prim + 4, e.Cmd << 24 | 0x808080);
                m.WriteU32(prim + 8, (uint)(ushort)y0 << 16 | (ushort)(baseX + tx * 16));
                m.WriteU32(prim + 12, (uint)clut << 16 | (uint)(byte)(g & 0xF0) << 8 | (byte)(g << 4));
                sprites++;
            }
        }

        e.Sprites = sprites;
    }

    static void AddPrim(IMemory m, uint ot, int index, uint prim, uint length)
    {
        if ((uint)index >= OtSize) return;
        uint entry = ot + (uint)index * 4;
        uint old = m.ReadU32(entry);
        m.WriteU32(prim, length << 24 | (old & 0xFFFFFFu));
        m.WriteU32(entry, (old & 0xFF000000u) | (prim & 0xFFFFFFu));
    }

    static bool InRoom(RoomFill fill, int left, int top, int right, int bottom)
    {
        if (!fill.InOriginalAspect && WidescreenPatch.OriginalAspect) return false;
        if (fill.Rect is not { } r) return true;
        return r.Left == left && r.Top == top && r.Right == right && r.Bottom == bottom;
    }

    static string? CurrentOverlay(IMemory m)
    {
        uint key = m.ReadU32(OverlayHeader);
        if (key == _overlayKey) return _overlayName;

        _overlayKey = key;
        _overlayName = null;
        var active = Dispatcher.ActiveNames;
        for (int i = active.Length - 1; i >= 0; i--)
        {
            if (!Dispatcher.Overlays.TryGetValue(active[i], out var overlay)) continue;
            if (!overlay.Functions.ContainsKey(key)) continue;
            _overlayName = overlay.Name;
            break;
        }
        return _overlayName;
    }

    static bool EntityActive(uint update)
    {
        foreach (var ent in Entities.All())
            if (ent.Update == update) return true;
        return false;
    }

    static Loaded? Resolve(RoomFill fill)
    {
        string name = ImageResolver?.Invoke(fill) ?? fill.Image;
        if (string.IsNullOrEmpty(name)) return null;

        if (_cache.TryGetValue(name, out var cached))
            return cached.Failed ? null : cached;

        var loaded = new Loaded();
        _cache[name] = loaded;
        try
        {
            byte[] rgba;
            int width, height;

            if (_raw.TryGetValue(name, out var raw))
            {
                (rgba, width, height) = raw;
            }
            else
            {
                using var stream = Open(name);
                var img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                rgba = img.Data;
                width = img.Width;
                height = img.Height;
            }

            loaded.Image = GpuPrims.RegisterImage(rgba, width, height);
            loaded.Width = width;
            loaded.Height = height;
            if (loaded.Image < 0) loaded.Failed = true;
        }
        catch (Exception ex)
        {
            loaded.Failed = true;
            Console.Error.WriteLine($"[RoomFills] cant load '{name}': {ex.Message}");
        }
        return loaded.Failed ? null : loaded;
    }

    static Stream Open(string name)
    {
        if (_files.TryGetValue(name, out var path))
            return File.OpenRead(path);

        var asm = Assembly.GetExecutingAssembly();
        string suffix = ".extension." + name;
        var res = Array.Find(asm.GetManifestResourceNames(),
                      n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                  ?? throw new FileNotFoundException($"{name} is not e,beded");
        return asm.GetManifestResourceStream(res)
               ?? throw new FileNotFoundException($"cannot open {res}");
    }

    static void EnsureConfig()
    {
        if (_loaded) return;
        _loaded = true;

        try
        {
            using var stream = Open("room_fills.json");
            var doc = JsonSerializer.Deserialize<Config>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });

            foreach (var fill in doc?.Fills ?? [])
            {
                fill.TileData = ParseTiles(fill.Tiles);
                if (fill.TileData == null && string.IsNullOrWhiteSpace(fill.Image)) continue;
                if (!string.IsNullOrWhiteSpace(fill.HideWhenEntity))
                {
                    string text = fill.HideWhenEntity!.Trim();
                    if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) text = text[2..];
                    uint.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out fill.HideEntityAddr);
                }
                _anyOriginalAspect |= fill.InOriginalAspect;
                _fills.Add(fill);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[RoomFills] cannot read room_fills.json, it is malformated?: {ex.Message}");
        }
    }

    static ushort[]? ParseTiles(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        const int count = TilesPerScreen * TilesPerScreen;
        if (text!.Length < count * 4) return null;

        var data = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            if (!ushort.TryParse(text.AsSpan(i * 4, 4), System.Globalization.NumberStyles.HexNumber,
                    null, out data[i]))
                return null;
        }
        return data;
    }

    sealed class Config
    {
        public List<RoomFill>? Fills { get; set; }
    }
}

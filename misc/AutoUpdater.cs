using System;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Host.Window;

namespace Recompiled;

public static class AutoUpdater //should be generic enough to use on other recomps
{
    const string Repo = "BlackLabelHQ/SymphonyRecomp";
    const string ReleasesUrl = "https://github.com/" + Repo + "/releases/latest";
    const string ApiUrl = "https://api.github.com/repos/" + Repo + "/releases/latest";
    const string ApplyArg = "--apply-update";
    const string UserAgent = "SymphonyRecomp-AutoUpdater";
    const string EnabledKey = "AutoUpdateEnabled";
    const string SkipTagKey = "AutoUpdateSkipTag";
    const string PopupId = "##autoupdate";

    enum Phase { Idle, Checking, Available, Downloading, Applying, Failed }

    static volatile Phase _phase = Phase.Idle;
    static volatile string _latestTag = "";
    static volatile string _assetUrl = "";
    static volatile string _error = "";
    static volatile bool _dismissed;
    static volatile bool _popupOpen;
    static CancellationTokenSource? _cancel;
    static long _downloaded;
    static long _total;

    public static string? CurrentTag
    {
        get
        {
            var v = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (string.IsNullOrEmpty(v)) return null;
            int plus = v.IndexOf('+');
            if (plus >= 0) v = v[..plus];
            return v.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? v : null;
        }
    }

    static string InstallDir => AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
    static string ExePath => Environment.ProcessPath ?? "";
    static string WorkDir => Path.Combine(Path.GetTempPath(), "SymphonyRecomp-update");
    static string StagingDir => Path.Combine(WorkDir, "staging");
    static string ApplierDir => Path.Combine(WorkDir, "applier");

    public static bool HandleRelaunch(string[] args)
    {
        if (args.Length < 5 || args[0] != ApplyArg) return false;

        string staging = args[1];
        string target = args[2];
        string exe = args[4];
        _ = int.TryParse(args[3], out int pid);

        Log($"waiting for pid {pid} to exit before writing to {target}");
        WaitForExit(pid);
        try
        {
            CopyTree(staging, target);
            Log("files replaced, relaunching");
        }
        catch (Exception e)
        {
            Log($"failed to apply update: {e.Message}");
        }

        MakeExecutable(exe);
        try
        {
            Process.Start(new ProcessStartInfo(exe) { WorkingDirectory = target, UseShellExecute = false });
        }
        catch (Exception e)
        {
            Log($"failed to relaunch: {e.Message}");
        }

        return true;
    }

    public static void Register()
    {
        TryDelete(WorkDir);
        CleanStaleFiles();
        PanelManager.Register(new UpdatePanel());
        MenuRegistry.Register("Updates", DrawMenuItems, null, 500);

        if (CurrentTag == null)
        {
            Log("development build, update check skipped");
            return;
        }

        if (!ConfigManager.View.GetBool(EnabledKey, true))
        {
            Log($"running {CurrentTag}, update check disabled in settings");
            return;
        }

        _phase = Phase.Checking;
        Log($"running {CurrentTag}, checking {Repo} for updates...");
        Task.Run(CheckAsync);
    }

    static async Task CheckAsync()
    {
        try
        {
            using var http = NewClient(TimeSpan.FromSeconds(15));
            using var doc = JsonDocument.Parse(await http.GetStringAsync(ApiUrl));
            var root = doc.RootElement;

            string tag = root.GetProperty("tag_name").GetString() ?? "";
            if (string.IsNullOrEmpty(tag))
            {
                Log("the latest release have no tag");
                _phase = Phase.Idle;
                return;
            }

            if (tag == CurrentTag)
            {
                Log($"up to date, {tag} is the latest release");
                _phase = Phase.Idle;
                return;
            }

            if (tag == ConfigManager.View.GetString(SkipTagKey))
            {
                Log($"{tag} is available but is skipped, staying on {CurrentTag}");
                _phase = Phase.Idle;
                return;
            }

            string suffix = AssetSuffix();
            foreach (var asset in root.GetProperty("assets").EnumerateArray())
            {
                string name = asset.GetProperty("name").GetString() ?? "";
                if (!name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) continue;
                _assetUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                break;
            }

            if (string.IsNullOrEmpty(_assetUrl))
            {
                Log($"{tag} is available but it ships no *{suffix} build for this platform, is this an error?");
                _phase = Phase.Idle;
                return;
            }

            _latestTag = tag;
            _phase = Phase.Available;
            Log($"update available: {tag} (running {CurrentTag})");
        }
        catch (Exception e)
        {
            Log($"update check failed: {e.Message}");
            _phase = Phase.Idle;
        }
    }

    static void Log(string message) => Console.WriteLine($"[AutoUpdater] {message}");

    static async Task UpdateAsync(CancellationToken token)
    {
        try
        {
            _phase = Phase.Downloading;
            Log($"downloading {_latestTag} from {_assetUrl}");
            TryDelete(WorkDir);
            Directory.CreateDirectory(WorkDir);

            string suffix = AssetSuffix();
            string archive = Path.Combine(WorkDir, "release" + suffix[(suffix.IndexOf('.'))..]);

            using (var http = NewClient(TimeSpan.FromMinutes(30)))
            using (var response = await http.GetAsync(_assetUrl, HttpCompletionOption.ResponseHeadersRead, token))
            {
                response.EnsureSuccessStatusCode();
                _total = response.Content.Headers.ContentLength ?? 0;
                _downloaded = 0;

                await using var src = await response.Content.ReadAsStreamAsync(token);
                await using var dst = File.Create(archive);
                var buffer = new byte[81920];
                int read;
                while ((read = await src.ReadAsync(buffer, token)) > 0)
                {
                    await dst.WriteAsync(buffer.AsMemory(0, read), token);
                    Interlocked.Add(ref _downloaded, read);
                }
            }

            Log($"downloaded {Interlocked.Read(ref _downloaded) / 1048576f:0.0} MB, extracting");
            Directory.CreateDirectory(StagingDir);
            Extract(archive, StagingDir);

            string exeName = Path.GetFileName(ExePath);
            string staged = Path.Combine(StagingDir, exeName);
            if (!File.Exists(staged)) throw new FileNotFoundException($"{exeName} not found in the release archive");

            Directory.CreateDirectory(ApplierDir);
            string applier = Path.Combine(ApplierDir, exeName);
            File.Copy(staged, applier, true);
            MakeExecutable(applier);

            _phase = Phase.Applying;
            Log($"restarting to apply {_latestTag} into {InstallDir}");

            Process.Start(new ProcessStartInfo(applier)
            {
                WorkingDirectory = ApplierDir,
                UseShellExecute = false,
                ArgumentList =
                {
                    ApplyArg,
                    StagingDir,
                    InstallDir,
                    Environment.ProcessId.ToString(),
                    ExePath,
                },
            });

            Environment.Exit(0);
        }
        catch (OperationCanceledException)
        {
            Log("download cancelled");
            TryDelete(WorkDir);
            _dismissed = true;
            _phase = Phase.Available;
            _popupOpen = false;
        }
        catch (Exception e)
        {
            Log($"update failed: {e.Message}");
            _error = e.Message;
            _phase = Phase.Failed;
        }
    }

    static void DrawMenuItems()
    {
        ImGui.TextDisabled(StatusLine());
        ImGui.Separator();

        bool enabled = ConfigManager.View.GetBool(EnabledKey, true);
        if (ImGui.MenuItem("Check on startup", null, enabled))
        {
            ConfigManager.View.SetBool(EnabledKey, !enabled);
            ConfigManager.SaveView(PanelManager.Panels);
        }

        if (ImGui.MenuItem("Check for updates now", null, false, _phase is Phase.Idle or Phase.Available))
        {
            if (_phase == Phase.Available) _dismissed = false;
            else
            {
                ConfigManager.View.SetString(SkipTagKey, "");
                _dismissed = false;
                _phase = Phase.Checking;
                Log("manual update check requested");
                Task.Run(CheckAsync);
            }
        }

        if (ImGui.MenuItem("Open releases page")) OpenUrl(ReleasesUrl);
    }

    static string StatusLine() => _phase switch
    {
        Phase.Checking => "Checking for updates...",
        Phase.Available => $"{_latestTag} available",
        Phase.Downloading => $"Downloading {_latestTag}...",
        Phase.Applying => "Restarting to update...",
        Phase.Failed => "Last update attempt failed",
        _ => CurrentTag == null ? "Deevlopment build" : $"Running {CurrentTag}",
    };

    internal static bool ShouldShowUi =>
        !_dismissed && _phase is Phase.Available or Phase.Downloading or Phase.Applying or Phase.Failed;

    internal static void CloseUi()
    {
        if (ShouldShowUi) _dismissed = true;
    }

    internal static void Draw()
    {
        var phase = _phase;

        if (!_popupOpen)
        {
            _popupOpen = true;
            ImGui.OpenPopup(PopupId);
        }

        var vp = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(vp.GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(440, 0), ImGuiCond.Appearing);

        if (!ImGui.BeginPopupModal(PopupId, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar)) return;

        switch (phase)
        {
            case Phase.Available: DrawAvailable(); break;
            case Phase.Downloading: DrawDownloading(); break;
            case Phase.Applying: DrawApplying(); break;
            case Phase.Failed: DrawFailed(); break;
            default: ClosePopup(); break;
        }

        ImGui.EndPopup();
    }

    static void DrawAvailable()
    {
        CenteredWrapped($"{_latestTag} is available.\nYou are running {CurrentTag}.");
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (!CanWriteInstallDir())
        {
            ImGui.TextWrapped("This folder is not writable, so the update cannot be installed automatically. Download it manually or move the game somewhere you can write to.");
            ImGui.Spacing();
            if (ImGui.Button("Open releases page", new Vector2(-1, 0))) OpenUrl(ReleasesUrl);
            if (ImGui.Button("Not now", new Vector2(-1, 0))) DismissForSession();
            return;
        }

        ImGui.TextWrapped("The game will close, replace itself and reopen. your saved data will be kept");
        ImGui.Spacing();

        if (ImGui.Button("Download and install", new Vector2(-1, 0)))
        {
            _cancel = new CancellationTokenSource();
            Task.Run(() => UpdateAsync(_cancel.Token));
        }

        if (ImGui.Button("Not now", new Vector2(-1, 0))) DismissForSession();

        if (ImGui.Button($"Skip {_latestTag}", new Vector2(-1, 0)))
        {
            ConfigManager.View.SetString(SkipTagKey, _latestTag);
            ConfigManager.SaveView(PanelManager.Panels);
            Log($"{_latestTag} skipped, it will not be offered again");
            DismissForSession();
        }
    }

    static void DrawDownloading()
    {
        long total = Interlocked.Read(ref _total);
        long done = Interlocked.Read(ref _downloaded);

        CenteredWrapped($"Downloading {_latestTag}...");
        ImGui.Spacing();
        ImGui.ProgressBar(total > 0 ? done / (float)total : 0f, new Vector2(-1, 0),
            total > 0
                ? $"{done / 1048576f:0.0} / {total / 1048576f:0.0} MB"
                : $"{done / 1048576f:0.0} MB");
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Cancel", new Vector2(-1, 0))) _cancel?.Cancel();
    }

    static void DrawApplying()
    {
        CenteredWrapped($"Installing {_latestTag}.\nThe game will close and reopen in a moment.");
        ImGui.Spacing();
        ImGui.ProgressBar(1f, new Vector2(-1, 0), "restarting");
    }

    static void DrawFailed()
    {
        CenteredWrapped("The update could not be installed.");
        ImGui.Spacing();
        ImGui.TextWrapped(_error);
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Retry", new Vector2(-1, 0)))
        {
            _cancel = new CancellationTokenSource();
            Task.Run(() => UpdateAsync(_cancel.Token));
        }

        if (ImGui.Button("Open releases page", new Vector2(-1, 0))) OpenUrl(ReleasesUrl);

        if (ImGui.Button("Close", new Vector2(-1, 0)))
        {
            _phase = Phase.Available;
            DismissForSession();
        }
    }

    static void CenteredWrapped(string text)
    {
        float avail = ImGui.GetContentRegionAvail().X;
        foreach (var line in text.Split('\n'))
        {
            if (line.Length == 0) { ImGui.Spacing(); continue; }
            float off = (avail - ImGui.CalcTextSize(line).X) * 0.5f;
            if (off > 0) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + off);
            ImGui.TextUnformatted(line);
        }
    }

    static void DismissForSession()
    {
        _dismissed = true;
        ClosePopup();
    }

    static void ClosePopup()
    {
        _popupOpen = false;
        ImGui.CloseCurrentPopup();
    }

    static HttpClient NewClient(TimeSpan timeout)
    {
        var http = new HttpClient { Timeout = timeout };
        http.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        return http;
    }

    static string AssetSuffix()
    {
        string arch = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "x64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"windows-{arch}.zip";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return $"macos-{arch}.zip";
        return $"linux-{arch}.tar.gz";
    }

    static void Extract(string archive, string dir)
    {
        if (archive.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            ZipFile.ExtractToDirectory(archive, dir, true);
            return;
        }

        using var fs = File.OpenRead(archive);
        using var gz = new GZipStream(fs, CompressionMode.Decompress);
        TarFile.ExtractToDirectory(gz, dir, true);
    }

    static void CopyTree(string source, string target)
    {
        foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, dir)));

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(source, file);
            if (IsUserFile(relative)) continue;
            CopyWithRetry(file, Path.Combine(target, relative));
        }
    }

    static bool IsUserFile(string relative)
    {
        string name = Path.GetFileName(relative);
        if (name.Equals("settings.json", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.Equals("interface.ini", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)) return true;

        string root = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
        return root.Equals("disc", StringComparison.OrdinalIgnoreCase)
            || root.Equals("mods", StringComparison.OrdinalIgnoreCase);
    }

    static void CopyWithRetry(string source, string target)
    {
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                File.Copy(source, target, true);
                MakeExecutable(target);
                return;
            }
            catch (IOException) when (attempt < 40)
            {
                Thread.Sleep(250);
            }
            catch (IOException)
            {
                MoveAside(target);
                File.Copy(source, target, true);
                MakeExecutable(target);
                return;
            }
        }
    }

    const string StaleSuffix = ".pending-delete";

    static void MoveAside(string target)
    {
        for (int n = 0; ; n++)
        {
            string aside = $"{target}{StaleSuffix}{(n == 0 ? "" : n.ToString())}";
            if (File.Exists(aside)) continue;
            File.Move(target, aside);
            return;
        }
    }

    static void CleanStaleFiles()
    {
        try
        {
            foreach (string f in Directory.GetFiles(InstallDir, "*" + StaleSuffix + "*", SearchOption.AllDirectories))
                try { File.Delete(f); } catch { }
        }
        catch
        {
        }
    }

    static void WaitForExit(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            process.WaitForExit(30000);
        }
        catch
        {
        }

        Thread.Sleep(500);
    }

    static void MakeExecutable(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        try
        {
            File.SetUnixFileMode(path, File.GetUnixFileMode(path) |
                UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
        }
        catch
        {
        }
    }

    static bool CanWriteInstallDir()
    {
        try
        {
            string probe = Path.Combine(InstallDir, ".update-probe");
            File.WriteAllText(probe, "");
            File.Delete(probe);
            return true;
        }
        catch
        {
            return false;
        }
    }

    static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
        }
    }

    static void TryDelete(string dir)
    {
        try
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
        catch
        {
        }
    }
}

public sealed class UpdatePanel : IFloatingPanel
{
    public string Name => "Update";

    public bool IsOpen
    {
        get => AutoUpdater.ShouldShowUi;
        set { if (!value) AutoUpdater.CloseUi(); }
    }

    public void Draw() => AutoUpdater.Draw();
}

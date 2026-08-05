using RecompOne.Runtime.Memory;
using Recompiled;

if (AutoUpdater.HandleRelaunch(args)) return 0;

RecompOne.Runtime.Runtime.SetStartupNotice(
    "SymphonyRecomp is in a beta state and its not yet finished, you will " +
    "experience game breaking bugs and issues, please report then to help us " +
    "improve the project.\nThis project is not affiliated with konami or sony.\n\nthanks for playing",
    "SymphonyRecomp",
    "SymphonyRecompBetaAck");

DiscCheck.Register();
WidescreenPatch.Register();
WidescreenSettings.Register();
ThroneLeftFill.Register();
CheatMenu.Register();
QualityOfLifeMenu.Register();
TrackerMenu.Register();
RandoMenu.Register();
AutoUpdater.Register();
HelpMenu.Register();

var title = AutoUpdater.CurrentTag is { } tag ? $"SymphonyRecomp {tag}" : "SymphonyRecomp"; //get version too

var iconAsm = System.Reflection.Assembly.GetExecutingAssembly();
if (Array.Find(iconAsm.GetManifestResourceNames(), n => n.EndsWith(".SymphonyRecomp.ico", StringComparison.OrdinalIgnoreCase)) is { } iconRes)
{
    using var iconStream = iconAsm.GetManifestResourceStream(iconRes)!;
    using var iconMem = new MemoryStream();
    iconStream.CopyTo(iconMem);
    RecompOne.Runtime.Runtime.SetIcon(iconMem.ToArray());
}

var m = new PSMemory();
Entry.Run(m, args.Length > 0 ? args[0] : null, title);
return 0;

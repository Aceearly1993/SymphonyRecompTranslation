using RecompOne.Runtime.Memory;
using Recompiled;

if (AutoUpdater.HandleRelaunch(args)) return 0;

RecompOne.Runtime.Runtime.SetStartupNotice(
    "SymphonyRecomp is a 'RE'comp (NOT A 'DE'comp) of the PSX version of Castlevania: Symphony of the Night.\n\nThis recomp project is in a beta state and it is not yet finished. You will " +
    "experience game breaking bugs and issues, please report any bugs on the GitHub or in the BlackLabelHQ Discord Server to help us " +
    "improve the project.\n\nThis project is not affiliated with Konami or Sony.\n\nThanks for playing!",
    "SymphonyRecomp!",
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

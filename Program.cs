using RecompOne.Runtime.Memory;
using Recompiled;

RecompOne.Runtime.Runtime.SetStartupNotice(
    "SymphonyRecomp is in a beta state and its not yet finished, you will " +
    "experience game breaking bugs and issues, please report then to help us " +
    "improve the project\n\nthanks for playing",
    "SymphonyRecomp",
    "SymphonyRecompBetaAck");

WidescreenPatch.Register();
WidescreenSettings.Register();
CheatMenu.Register();
QualityOfLifeMenu.Register();
TrackerMenu.Register();

var m = new PSMemory();
Entry.Run(m, args.Length > 0 ? args[0] : null);
return 0;

# 

IF YOU ARE RUNNING 0.3b OR EARLIER YOU NEED TO MANUALLY DOWNLOAD THIS UPDATE FROM THE RELEASE PAGE!!! THE AUTO-UPDATER WILL NOT WORK PROPERLY

# Changes

- fixes an issue with the mod's hooking system, mods should work correctly now
- fixed inverted castle having problems with the widescreen patches and transition patches
- reduced minimum opengl requirement to 3.3 (4.5 will still be used when available, it will be eventually droped if gl3.3 is stable enough)
- added vsync option
- fixes RSL bug being accidentaly patched (oops) game nows uses original behaviour when using original aspect ratio
- fixed alucard's rendering overlays having an incorrect offset
- added an patch to extend the view on st0(dracula fight as richter) using the tiles from TOP(catle top as alucard), its just an image not an new tileset
- added shader mod (crt-geom included, you can put custom shaders, feel fre to open a pr to include other shaders!)
- reworked Recompone's rendering backend
- some of the screen fades and flashes now display correctly to your selected aspect ratio
- added linux arm64 build

## 0.4.1b
- fixed some overlay functions duplicate not being properly named causing issues with the widescreen patches not having all necessary functions on inverted castle (wich was suposed to be fixed in 0.4b) this issue is now fixed

## 0.4.2b
- fixed colission problems on some bosses
- added an script to run it on imutable systems (can work on other systems too)
- randomizer fixes

## 0.4.3b
- fixed some runtime graphic backend issues
- reworked top bar
- fixed japanese familiars being in the wrong order
- update various panels to have clearer language

# removed
- bordeless fullscreen was removed, im not very sure how to correctly implement it im sorry, exclusive fs was made a bit better (wont auto-minimize anymore)

# known bugs

- display scale can cause issues with the gui
- ogl 3.3 does not display the photo post richter dracula battle correctly

# UNTESTED

- OGL 3.3 support has not yet been fully tested and can have minor issues

# WARNING

from 0.4b onwards you are required to have dotnet 10 installed, if you dont the application will fail to launch, from now on every changelog will include the section bellow for new users

the reason betwen this decision is to reduce file size and fix the modding system, it doesnt work in single file publish, having it self contained creates a montain of files, so removing it from self contained both reduces the ammount of files and the size of the application

# Dependencies

You're required to install [dotnet 10 runtime](https://dotnet.microsoft.com/pt-br/download/dotnet/10.0) to run the recomp, otherwise the game will not open, ideally also make sure to install [OpenAL](https://www.openal.org/downloads/), as of the current version no further dependencies are required

you can find instructions downloading dotnet 10 for linux systems that dont have it on their packages [here](https://wiki.archlinux.org/title/.NET), or you can run "run.sh" wich will install dotent if not available and execute sotn for you

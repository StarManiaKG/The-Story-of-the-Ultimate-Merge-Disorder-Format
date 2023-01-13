**System requirements:**
- 2.4 GHz CPU or faster (multi-core recommended)
- Windows 7, 8 10, 11; Linux
- Graphics card with OpenGL 3.2 support

**Required software on Windows/Wine:**
- [Microsoft .Net Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472)

**Building on Linux:**
# Compiling
These instructions are for Debian-based distros and were tested with Debian 10 and Ubuntu 18.04. For others it should be similar.

__Note:__ this is experimental. None of the developers are using Linux as a desktop OS, so you're pretty much on your own if you encounter any problems with running the application.

- Install Mono. The `mono-complete` package from the Debian repo doesn't include `msbuild`, so you have to install `mono-complete` by following the instructions on the Mono project's website: https://www.mono-project.com/download/stable/#download-lin
- Install additional required packages: `sudo apt install make g++ git libx11-dev mesa-common-dev`
- Go to a directory of your choice and clone the repository (it'll automatically create an `UltimateDoomBuilder` directory in the current directory): `git clone https://github.com/jewalky/UltimateDoomBuilder.git`
- Compile UZB: `cd UltimateDoomBuilder && make`
- Run UZB: `cd Build && ./builder`

# Using Wine
alan please add details

**Links:**
- [Official thread link](https://forum.zdoom.org/viewtopic.php?f=232&t=66745)
- [Git builds at DRDTeam.org](https://devbuilds.drdteam.org/ultimatedoombuilder/) 

More detailed info can be found in the **editor documentation** (Refmanual.chm)


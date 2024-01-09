## This project is still in development. There are NO public compiled releases (yet).
# Heaven Studio

(WIP) A tool to create playable Rhythm Heaven custom remixes, with many customization options.

<p>
  <a href="https://discord.gg/2kdZ8kFyEN">
    <img src="https://img.shields.io/discord/945450048832040980?color=5865F2&label=Heaven%20Studio&logo=discord&logoColor=white" alt="Discord">
  </a>
</p>

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

[YouTube Channel](https://www.youtube.com/channel/UCAb3R-5qyXWOEj8B4vibhxQ)

[itch.io Page](https://rheavenstudio.itch.io/heaven-studio)

[Documentation](https://rheavenstudio.github.io/)

![image](https://github.com/RHeavenStudio/HeavenStudio/assets/43734252/c32ef9a3-2fd1-40df-b088-968950b2adab)



## Prebuilt Binaries
This project is still in development, so there are currently no release builds yet. GitHub Actions creates experimental builds on each commmit, but minimal support will be provided.

### Release 1 alpha builds
These builds do not include some of the features from Nightly, instead focusing on stability. These builds will eventually culminate in the first stable release of Heaven Studio.
- [Windows](https://nightly.link/RHeavenStudio/HeavenStudio/workflows/build/release_1/StandaloneWindows64-build.zip)
- [Linux](https://nightly.link/RHeavenStudio/HeavenStudio/workflows/build/release_1/StandaloneLinux64-build.zip)
- [MacOS](https://nightly.link/RHeavenStudio/HeavenStudio/workflows/build/release_1/StandaloneOSX-build.zip)

### Nightly builds
These builds include experimental new features that will not be included in Release 1
- [Windows](https://nightly.link/RHeavenStudio/HeavenStudio/workflows/build/master/StandaloneWindows64-build.zip)
- [Linux](https://nightly.link/RHeavenStudio/HeavenStudio/workflows/build/master/StandaloneLinux64-build.zip)
- [MacOS](https://nightly.link/RHeavenStudio/HeavenStudio/workflows/build/master/StandaloneOSX-build.zip)


#### Important Notes:
- On MacOS and Linux builds you may [experience bugs with audio-related tasks](https://github.com/RHeavenStudio/HeavenStudio/issues/72), but in most cases Heaven Studio works perfectly.
- On MacOS you'll need to have Discord open in the background for now, there's a bug that causes the DiscordSDK library to crash when the rich presence is updated while Discord is not open in the background.


## Self-Building

Heaven Studio is made in [Unity 2021.3.21](https://unity.com/releases/editor/whats-new/2021.3.21),
and programmed with [Visual Studio Code](https://code.visualstudio.com/).

Build Instructions: [BUILD.md](https://github.com/megaminerjenny/HeavenStudio/blob/master/BUILD.md) (or the more maintained [documentation page](https://rheavenstudio.github.io/docs-contributing/setup/introduction))

## Other information
Rhythm Heaven is the intellectual property of Nintendo. This program is NOT endorsed nor sponsored in any way by Nintendo. All used properties of Nintendo (such as names, audio, graphics, etc.) in this software are not intended to maliciously infringe trademark rights. All other trademarks and assets are property of their respective owners. This is a community project and this is available for others to use according to the GPL-3.0 license, without charge.

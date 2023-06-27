## This project is still in development. There are NO public compiled releases (yet).
# Heaven Studio

(WIP) A tool to create playable Rhythm Heaven custom remixes, with many customization options.

<p>
  <a href="https://discord.gg/2kdZ8kFyEN">
    <img src="https://img.shields.io/discord/945450048832040980?color=5865F2&label=Heaven%20Studio&logo=discord&logoColor=white" alt="Discord">
  </a>
</p>

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

[Progress Spreadsheet](https://docs.google.com/spreadsheets/d/1NXxIeL4nsdjChrxAZTPpk20QOKEdQWGbXIhT4TclB6k/edit?usp=sharing)

[YouTube Channel](https://www.youtube.com/channel/UCAb3R-5qyXWOEj8B4vibhxQ)

[Bug Report Form](https://docs.google.com/forms/d/e/1FAIpQLSfu0p2ZjrfwwEvpLgJ4Hw-AAN3jh4QNSEk0U7mjDvwiIkoRAw/viewform?usp=pp_url)

[Documentation](https://rheavenstudio.github.io/)

![prCapture](https://user-images.githubusercontent.com/43734252/212429715-1971929f-fee1-442f-8ba2-694e1732590a.png)


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
- MP3 audio with variable bitrate encoding may [desync when seeking in the editor](https://github.com/RHeavenStudio/HeavenStudio/issues/490). Either use MP3s with constant bitrate encoding or use one of our other supported formats (OGG Vorbis, WAV...)
- On MacOS and Linux builds you may [experience bugs with audio-related tasks](https://github.com/RHeavenStudio/HeavenStudio/issues/72), but in most cases Heaven Studio works perfectly.
- On MacOS you'll need to have Discord open in the background for now, there's a bug that causes the DiscordSDK library to crash when the rich presence is updated while Discord is not open in the background.


## Self-Building

Heaven Studio is made in [Unity 2021.3.21](https://unity.com/releases/editor/whats-new/2021.3.21),
and programmed with [Visual Studio Code](https://code.visualstudio.com/).

Build Instructions: [BUILD.md](https://github.com/megaminerjenny/HeavenStudio/blob/master/BUILD.md) (or the more maintained [documentation page](https://rheavenstudio.github.io/docs-contributing/setup/introduction))

## Other information
Rhythm Heaven is the intellectual property of Nintendo. This program is NOT endorsed nor sponsored in any way by Nintendo. All used properties of Nintendo (such as names, audio, graphics, etc.) in this software are not intended to maliciously infringe trademark rights. All other trademarks and assets are property of their respective owners. This is a community project and this is available for others to use according to the GPL-3.0 license, without charge.

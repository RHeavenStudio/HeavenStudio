# Build instructions

## Prerequisites:
- git (see below to know how to install git)
- A Unity 2020.3.xx LTS version
- A Machine running Windows, Linux or MacOS

## Installing git:
#### Windows
1. Grab the installer from [here](https://git-scm.com/download/win).
2. Follow the usual installation process.
3. When you reach the step where it asks about PATH enviornment in the installer, choose "Use Git from the Windows Command Prompt".
4. Now open CMD/Command Prompt/Windows Terminal to use git.

#### Mac
1. Open the terminal.
2. Install brew using
```
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```
3. After it finishes installing, install git using
```
sudo brew install git
```

#### Linux
1. Look up instructions for your distro online.


## Building Heaven Studio:
1. Clone the repository to your machine
```
git clone https://github.com/megaminerjenny/HeavenStudio.git
```
It should clone to the home directory on your machine by default (on Windows that's your main user's folder, on MacOS that's the folder you access by pressing Shift + Command + H in Finder)

2. Open Unity 2020.3.xx LTS.
3. Load the HeavenStudio repository you just cloned to Unity.
5. After Unity loads, Build AssetBundles by going to Assets -> Build AssetBundles
6. After Building AssetBundles is done, build the game itself by going to File -> Build Settings -> Build
7. And done, you now have built the game for your current platform.


### Platform-specific notes:
- If you get "empty errors" on Linux, run Unity Hub using the following command and load the Unity project through it. This is a general problem with Unity for some users on some distros such as Arch Linux.
```
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 unityhub
```
- If your MacOS build does not have libraries, make sure that all libraries are set to Any OS and Any CPU in their properties.

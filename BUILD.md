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

## Installing Unity:
1. If you are on Windows or MacOS, download [Unity Hub](https://unity.com/download), on Linux just follow instructions on how to install Unity Hub [here](https://docs.unity3d.com/hub/manual/InstallHub.html#install-hub-linux) (or look up instructions online if your distro isn't mentioned in that page).
2. Login with your Unity account (or create one if you haven't yet).
3. Choose a Personal license.
4. From the menu on left, go to "Installs".
5. Press "Install Editor".
6. Install the 2020.3.xx "Long Time Support (LTS)" version. 

## Building Heaven Studio:
1. Clone the repository to your machine
```
git clone https://github.com/megaminerjenny/HeavenStudio.git
```
Note: It should clone to the home directory on your machine by default (on Windows that's your main user's folder, on MacOS that's the folder you access by pressing Shift + Command + H in Finder).

2. Open Unity Hub.
3. Go to "Projects" from the menu on left.
4. Press "Open" in the top right.
5. Select the Heaven Studio repository you just cloned in step 1, if you do not know where it is, see the note in step 1.
6. After Unity loads, Build AssetBundles by going to Assets -> Build AssetBundles.
7. After Building AssetBundles is done, build the game itself by going to File -> Build Settings -> Build.
8. And done, you now have built the game for your current platform.


### Platform-specific notes:
- If you get "empty errors" on Linux, run Unity Hub using the following command and load the Unity project through it. This is a general problem with Unity for some users on some distros such as Arch Linux.
```
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 unityhub
```
- If your MacOS build does not have libraries, make sure that all libraries are set to Any OS and Any CPU in their properties.

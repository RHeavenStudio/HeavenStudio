# Build instructions

### Prerequisites:
- git
- A Unity 2020.3.xx LTS version
- A Machine running Windows, Linux or MacOS

### Instructions:
1. Clone the repository to your machine
```
git clone https://github.com/megaminerjenny/HeavenStudio.git
```
2. Open Unity 2020.3.xx LTS
3. Load the HeavenStudio repository you just cloned to Unity
4. After Unity loads, Build AssetBundles by going to Assets -> Build AssetBundles
5. After Building AssetBundles is done, build the game itself by going to File -> Build Settings -> Build
6. And done, you now have built the game for your current platform.

### Platform-specific notes:
- If you get "empty errors" on Linux, run Unity Hub using the following command and load the Unity project through it. This is a general problem with Unity for some users on some distros such as Arch Linux.
```
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 unityhub
```
- If your MacOS build does not have libraries, make sure that all libraries are set to Any OS and Any CPU in their properties.

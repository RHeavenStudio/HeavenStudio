# bread2unity

bread2unity is a tool that allows you to convert Rhythm Heaven animation files (BCCAD) to Unity animation files.

## Preparations

1. Download [Bread](https://github.com/rhmodding/bread)
2. Obtain Rhythm Heaven Megamix RomFS and extract the BCCAD and sprite sheet files. 


## Usage

1. Open your BCCAD file in Bread
2. Go to the toolbar, select "Tools", and then choose "bread2unity"
3. Select the prefab of your game
4. View the animations in Bread and decide how to split them into game objects. For each game object you need to:
   1. Give the game object a name in the name field
   2. Use Bread to find the animation indexes of the animations the game object should have. Write them in the animation field separated by a comma (e.g. 0,1,2,3,4)
   3. Decide which sprite should be the default sprite and copy the sprite index from Bread and put it in the sprite index field
5. If more than one game object needs to be imported from the BCCAD file, use the plus button to add another row of fields. You can delete a row with the minus button.
6. If the sprite sheet you want to use is rotated to the right, check the "Rotate Spritesheet" checkbox.
7. Click on "Generate Assets", select the BCCAD and PNG files and your files will be generated.

## Notes

Not all of the features the BCCAD file can contain were implemented in this tool:
1. Interpolation was not implemented due to a lack of documentation about how it works.
2. Screen Color was not implemented due to the fact that it requires a shader to be implemented and only a small amount of games use this feature.
# DeadRisingArcTool
Modding tools for Dead Rising 1. This tool will allow you to edit the game files and provides specialized editors for certain file types. The tool is primarily designed for the PC verison of the game, but some features are supported for the Xbox 360 version as well.

## Current Features
- Extraction/Injection of files
- Textures: Extract/Inject/Preview, all texture formats for PC fully supported
- 3D Models: Can view models and level geometry for PC

## Upcoming Features
- Primitive model extraction
- Better rendering support: alpha transparency, bump/light/depth maps
- Add/Delete/Duplication/Rename files
- Support for additional file types

# How to use
DeadRisingArcTool (hereby short handed to just ArcTool) works by opening all of the arc files the game uses and building a list of all files inside the archives. After opening ArcTool you can select File->Open to select the game's arc folder, for PC this is Dead Rising\nativeWin64, for Xbox 360 this is just the game's root folder.

![](/Images/open_folder.png)

After you open your game files for the first time ArcTool will remember this location and it will be auto selected the next time you click File->Open.

## Basic Navigation
After opening the arc files ArcTool will create a list of files that is initially sorted by their internal file name. You can change the sort order by right clicking the file list and selecting one of the "Sort By" options. Currently you can sort by file name, arc file, and file type.

Any files displayed in red mean that ArcTool does not have a specialized editor for. You can extract and inject these files but you will not have any further editing ability past that. Any files displayed not in red means AcrTool has a specialized editor for this file type, e.x.: texture viewer, model viewer, xml text editor, etc.
![](/Images/file_view.png)

Right clicking on a file entry will allow you to extract and inject the file. This will read/write the file using the game's proprietary format e.x.: rtexture, rmodel, etc. If the file type has a specialized editor you may have additional extraction/injection options that are more robust. For example, the texture editor will let you extract and inject textures in .dds format. 

If you want to extract and inject files in the game's proprietary format use the Extract/Inject options from the file list. If you want to extract and inject files in a more usable format (.dds, .txt, .obj, etc.) use the Extract/Inject options that are part of the specialized editors. You cannot mix the two together.
- File list -> .rtexture, .rmodel, etc.
- Specialized editor -> .dds, .txt, .obj, etc.

## Backup Files
Any time you modify a file ArcTool will backup the arc file being modified with the new file extension ".arc_bak". If you need to restore the file delete the .arc file and rename the .arc_bak to .arc. The next time you go to modify this file ArcTool will make a new backup.

## Duplicate Files
Throughout the game's many arc files there are a lot of textures and such that are duplicated, and the same texture will be found in multiple arc files. When you edit a file in ArcTool it will by default update all duplicate copies of the file for you. Support is in the works to let the user pick exactly which arc files to update when saving a file that is duplicated.

## Texture Editor
If you click on any rtexture files the texture editor will display a preview of the image with some additional info, and let you view the different mip map levels for that texture. Right clicking the preview will let you extract/inject the texture in .dds format. If the image has multiple mip maps they will all be extracted into one file. Additionally you can drag-n-drop .dds files onto the preview image and it will automatically inject the file for you.
![](/Images/texture_editor.png)

Currently all PC texture formats are supported. However, due to discrepancies in how different image editors handle dds files, you may have issues opening dds images saved from ArcTool, as well as issues injecting dds images saved with certain image editors. Here is a compatibility list:
- DirectX Texture Tool - Full support
- Paint .Net - Full viewing support, cannot save U8V8 bumpmaps
- Photoshop - Should work fine, need to confirm U8V8 support

If you have issues opening or injecting files with a certain image editor, open a github issue with the name of the game file you were trying to modify and the image editor you were using, and I will try to add support for it.

When injecting a a texture ArcTool will update the game file to match the new width, height, format, type, and mip map count. Though unless you know what you are doing you should match the format the texture was originally in.

## Text Editor
The following file types can be modified in the text editor:
- rCameraListXml
- rClothXml
- rEnemyLayoutXml
- rFSMBrainXml
- rMarkerLayoutXml
- rModelInfoXml
- rModelLayoutXml
- rNMMachineXml
- rRouteNodeXml
- rSchedulerXml
- rSoundSegXml
- rUBCellXml
- rEventTimeSchedule
- rHavokVehicleData
- rAreaHitLayout
- rHavokConstraintLayout
- rHavokLinkCollisionLayout
- rHavokVertexLayout
- rItemLayout
- rMobLayout
- rSprLayout
- rSMAdd
- rMapLink

If you make changes to the file the lines changed will have a yellow indicator by the line number. The text editor supports a bunch of hotkeys, the most notable ones are:
- Ctrl+F: Find dialog
- Ctrl+G: Goto line number
- Ctrl+Z: Undo
- Ctrl+Y: Redo
- Ctrl+S: Save changes

If you make changes to a file and then try to select a new file without saving, you will be prompted to save or discard your changes.
![](/Images/text_editor.png)

## Model Editor
The model editor can view any game model and most of the level geometry for the different levels. After clicking on a rmodel file, the model editor will display some information about the model: textures, joints, materials, primitives, etc. Clicking the Render button will open the render view and display the model with a flycam.

Rendering level geometry is done by selecting the folder for the level models you want to render, right clicking, and selecting the "Render" option in the file list. This will search the current folder and all subfolders for rmodel files and render all of them in one window. 

This is typically used for the \scroll\stageN\sXXX folders, you can also use it for the higher up \scroll\stageN folders as well. The more models it tries to render the longer the render view will take to load. Some level sections will have missing bits of geometry here and there because the render viewer is still a WIP and is missing functionality for more advanced features. This will be worked out in a future releases.
![](/Images/bulk_render.png)

When the render viewer opens it will try to position you near the model with it in sight, and adjust the movement speed of the camera to suit the size of the model. This is all guess work though, so sometimes it will be completely off and you will spawn without the model in front of you. You may need to pan around until you find it.

### Flycam Controls
- W/S/A/D - Camera movement
- Z/X - Move camera up or down
- +/- - Change camera movement speed
- Left mouse hold and click - Aim the camera

![](/Images/model_editor.png)

# Debug Builds
Any builds posted on github are release builds and considered stable. As I add features and make changes they will be pushed to the dev branch before they get merged into master. If you want to test features as I code them you will need to checkout the dev branch and build it. This branch is not stable and at times may not work or compile correctly.

All debug builds have a "DEBUG" menu on the main window. This is where I put various options for testing purposes:
- Batch Extract -> Textures - Extracts every texture in the game to a folder, takes ~15min and saves ~15k textures
- Restore Backups - Restores all backups made for modified arc files

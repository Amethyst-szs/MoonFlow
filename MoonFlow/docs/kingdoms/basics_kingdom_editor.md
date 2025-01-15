---
tags:
  - world
  - starter_guide
---
# Kingdom Editor - Basics
MoonFlow's Kingdom Editor is a streamlined process for working with Super Mario Odyssey's `SystemData` files pertaining to the `WorldList`.

## Managing Kingdoms
The kingdom list can be found as a tab on your project's homepage. This tab contains a recreation of the map screen found in-game, and is fully dynamic to any changes you make to the list. Opening a kingdom is as simple as clicking on your desired destination!

One key feature you might notice is missing is the ability to re-arrange, delete, or add brand new kingdoms. This is very intentionally ***not*** included, and very likely never will be. Adding new kingdoms to the game requires modifying a large amount of system files, 2D layouts, 3D models, and special assembly code patches added to an exefs project. To learn more about what is required to add new kingdoms, view [this guide by Octember](https://github.com/octember8/SMO-Kingdom-18/blob/main/Kingdom%2018%20Implementation%20Guide.md).

The theme of the previous note extends to the editor itself. Unlike most other tools in MoonFlow, the kingdom editor is *not* designed to be comprehensive. There are dozens upon dozens of niche technicalities for each kingdom, and as such, not everything will be supported by MoonFlow. The goal of this editor is to do a handful of annoying tasks very well, rather than be a catch-all for kingdom list management.

## Editing a Kingdom
The kingdom editor is divided into two halves, the info sidebar and the content editor. The sidebar contains general information about the kingdom including home stage, power moon and purple coin type, purple coin count, scenarios, and more. More kingdom configuration can be done manually through `SystemData`, though make sure to reload your MoonFlow project after making edits to ensure nothing is overridden!

The content editor is split into different categories, each specializing in a task. The following is a brief summary and link to additional info for each category:

### Stage List
The stage list is vital for creating custom sub areas and other tasks involving adding custom stages. Aside from some rare exceptions (inside of the odyssey, some cutscenes, the world map, and other oddballs) every single stage is assigned to one kingdom and one kingdom *only*.

Adding new stages to the stage list is also vital to add new stages to the [StageMessage archive](../text/file_management.md#stagemessage). Once a stage is assigned to a specific kingdom a text file can be added for it in the text tab.

For more info on the stage list, take a look [here](stage_list.md).

### Power Moon List
The Power Moon List is designed after the in-game moon list, however with lots of additional information displayed and modifiable. Each entry displays the name in the project's default language, the object ID, the stage which contains the moon, and some icons indicating additional flags.

Three tools are placed on the right edge, including an add display name / rename tool (which will open the corresponding entry in the msbt text editor), a delete button, and the index. You can type an index, press the arrows, or drag the arrows to pull the moon into the index you want.

Most importantly, there is a dropdown menu allowing full editing of every property. More information on this tool [here](moon_list.md).
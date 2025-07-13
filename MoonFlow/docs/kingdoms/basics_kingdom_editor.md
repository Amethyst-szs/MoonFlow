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
The kingdom editor is divided into two halves, the map sidebar and the content editor. The sidebar contains an overview of the kingdom, and the content editor includes Power Moons, the home stage, other linked stages, scenarios, and more. More kingdom configuration can be done manually through `SystemData`, though make sure to reload your MoonFlow project after making edits to ensure nothing is overridden!

## See more
- [Power Moon List](kingdoms/moon_list.md)
- [Stage List](kingdoms/stage_list.md)
- [Maps](kingdoms/maps/basics_map.md)
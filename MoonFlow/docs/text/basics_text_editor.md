---
tags:
  - msbt
  - starter_guide
---

# Text Editor - Basics
MoonFlow's text editor is an [LMS](https://nintendo-formats.com/libs/lms/overview.html) [MSBT](https://nintendo-formats.com/libs/lms/msbt.html) file editor designed specifically for Super Mario Odyssey. This set of tools is inspired by and is meant to fully replace [Msbt Editor Reloaded](https://gbatemp.net/threads/release-msbt-editor-reloaded.406208/) for the purposes of SMO mod development.

## Opening a File
Text files are split into three folders, called archives or `.szs` files.

- **System** stores files related to game information like kingdom names, tutorial messages, cutscene text, complex or recurring NPCs, and more. 
- **Stage** contains a file for every stage in the game. MoonFlow sorts the files in this folder into kingdoms and by stage type (sub area, shop, ect.)
- **Layout** is used for text meant to be displayed on the HUD. Most menus, popups, and other related text is stored here.

Text files can be opened by double clicking or pressing open file. The file list can also be modified with a palate of common operations, but that will be covered [here](file_management.md).

## Editor Layout
The text editor is divided into three sections; The entry/label list, the page editor, and the header.

### Entry List
Every MSBT text file is made up of entries (also called labels). Each entry can contains pages of text, along with some additional metadata. The entry list sidebar shows all entries in the file, and if you open a stage's text file, it will have additional organization to sort Power Moons, Checkpoints, Quests, and more into their own categories.

The footer of the entry list shows how many entries are in the file, plus add new entries, search for entries by name, delete the currently selected entry, or open the wiki help page *(you are here!)*

### Page Editor
The page editor is the primary window of the text editor. This is what contains the actual text for each label, and where you can write your text!

Most text entries will only contain one page, and all displayed text is contained within that page. For character dialogue though, it is very common for there to be multiple pages which will require the player to press A to advance to the next dialogue box. You can add pages with the ➕ separators, as well as re-arranging or deleting pages on the right side of each page.

When writing text you'll frequently run into *tags*, one of the main features that MoonFlow provides compatibility for. To edit a tag, use **Ctrl + Left-Click** or **Ctrl + Shift + T** *(default keybind)*. This menu will look different for each type of tag. Read more about tag editing [here](tags.md#editing).

In order to add new tags to your text you can **Right-Click** or press **Ctrl + T** *(default keybind)*. This will pull up a tag insertion wheel, which can be rotated to additional pages with the spin button in the center. Read more about tag insertion [here](tags.md#wheel).

### Header
The header displays your selected entry and file, as well as offering a language selector. By default all edits you make in your [default language](../introduction.md#language) will copied to all other languages to preserve mod functionality for all players. If you want to provide manual translations, switch to that language and input them there. Read more about MoonFlow translations [here](translations.md).

## Beginner Tips
- When working with stage text files, it's very helpful to have a level editor or actor inspector open to view object IDs. Being able to search for entries by ID is very helpful when attempting to add/modify text from a specific actor.
- Before adding a new text file for a new stage, make sure to register that stage with a kingdom first! This is required to update the msbp, a file that contains a database of all text files and is required for the game to recognize your new text file.
- You do not need to manually manage the msbp with MoonFlow, that is automatically handled for you.

## Additional Resources
- [File Management](file_management.md)
- [Text Tags](tags.md)
- [Translations](translations.md)
- [Technical Details & Design Philosophy](technical.md)
---
tags:
  - msbt
---
# Text Editor - File Management
Managing your text files is designed to feel intuitive, just like working with your preferred file manager. However, under the hood there's a lot going on for even the most simple operations! To get a better idea of what's happening to your project and learn how to fix any potential issues, read on dear traveler!

## Managing your Files
One of the core components of the text editor's design is that there is never an "Export" or "Compile Project" button. At any given moment, your projects are always ready to execute on real hardware or emulator. This means that the text editor, despite all of it's abstraction and simplification of the under-the-hood work, does still need to manage all the [messy details](technical.md).

When navigating the Text tab of the Home Menu, files can be freely opened, searched, created, copied, pasted, duplicated, renamed, and deleted. You can access these tools with the toolbar on the bottom of the file list, or with the selection of hotkeys. These operations can be performed just like any other file manager, except for the Stage Message archive.

## Stage Message
The Stage Message archive / folder is special, as seen from the fact that each file shares a name with a stage and the app sorts each text file into kingdoms! This isn't just a quality-of-life feature either, the game assigns each text file in this folder to a specific kingdom through the [MSBP Database](technical.md#CTI1). Because of this, you are *not* allowed to freely create files here.

In order to create a new **MSBT** text file in this archive, you'll need to first assign your desired stage name to a kingdom over in the [Kingdom Editor](../kingdoms/basics_kingdoms.md). Once your stage name is assigned to *one* kingdom, you can then create the corresponding text file in the Stage Message archive if desired.

## Further Reading
For the average user, this is the extent of the required knowledge on the file management system. However, under the hood there is a lot going on including managing over a dozen different languages and the project database. Read more about the details of what MoonFlow is actually doing in the [technical breakdown](technical.md).
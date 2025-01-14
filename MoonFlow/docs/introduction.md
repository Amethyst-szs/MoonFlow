---
tags:
  - starter_guide
---

# Introduction to MoonFlow
Welcome, welcome! This page will help you figure out if MoonFlow is the right tool for what you're trying to make, how to get it installed, and getting your first project created.

## What is and isn't MoonFlow?
MoonFlow is a specialized Super Mario Odyssey modding tool. Unlike lots of similar mod development tools (e.g. [Switch Toolbox](https://github.com/KillzXGaming/Switch-Toolbox)) this tool is built specifically and *only* for the Nintendo Switch's Mario Odyssey, released 2017. If you're looking to use this tool for other games, this tool is not what you're looking for. [^1]

[^1]: If you're a developer wanting to use this project as a starting point for research other Action Library & LMS games, there is a fair bit of modularity built into the low-level Nindot library. It is not built with the intention of supporting other games however, so keep this in mind

This tool is a GUI and Project Manager for RomFS development. It features no built-in support for reading, editing, or creating ExeFS patching. This means that out-of-the-box there is no way to integrate your custom game code into the tool if you drastically change the game's behavior.

The mission of this project is *not* to be an "everything tool". It is not a replacement for any pre-existing level editors, model editors, texture editors, or any other tools. This is an expansion and improvement to your existing toolkit, not a complete replacement and likely never will be.

MoonFlow can be used on both new and old romfs projects. The project creation system is designed to work from scratch or integrate into pre-existing projects of any complexity.[^2]

[^2]: If you encounter errors creating a new MoonFlow project integrated into a pre-existing project, it is likely due to corrupted MSBT archives created by [Msbt Editor Reloaded](https://gbatemp.net/threads/release-msbt-editor-reloaded.406208/). File an issue or DM me directly for support repairing your archives.

MoonFlow is **NOT** a piracy tool. It does not distribute or encourage pirated game content. You must provide your own game files on startup, which can obtained off a modified Nintendo Switch or emulators. Modding a switch and dumping your game files to your computer is outside the scope of this guide.

## Getting MoonFlow Installed
MoonFlow is available on both Windows and Linux, visit the [GitHub releases](https://github.com/Amethyst-szs/MoonFlow/releases) to download the latest version of the application. The app features built-in update notifications on the boot screen, so you'll always know if a newer version is available.

This application is built using [C# .NET](https://dotnet.microsoft.com/en-us/languages/csharp) and the [Godot Engine](https://godotengine.org/). It is designed to support most systems, however there are some lightweight GPU shaders, which can cause issues on particularly underpowered machines. At least 1GB of free RAM is highly recommended. The most important specification is disk read/writes to manage your projects and read game files. A local (not cloud based) SSD storage device is highly recommended, but not required.

Unless you want to compile the engine from source, downloading .[NET](https://dotnet.microsoft.com/en-us/languages/csharp) or the [Godot Engine Editor](https://godotengine.org/) is **NOT** required. All the required libraries are included in the download, just launch the executable to get started.

On startup you'll need to provide *at least* 1 unmodified clean RomFS dump. The tool will automatically detect the validity and version of your provided game dumps, and you'll only be able to create or open projects that use game versions you provide paths for.[^3]

[^3]: If your game dump gets rejected by the application, make sure you selected the right folder and your dump is completely unmodified. If it is modified, create a new dump for your desired version to use with MoonFlow.

## Creating your First Project
After providing your game file path/paths, you'll be greeted by the welcome screen! If you have any MoonFlow projects ready you can open them with the bar on the left. If not, you can create your first project!

Creating a project requires 3 things, a path, a game version, and a default language.

#### Path
The path of your project is where all your modified files will be stored. This can be an empty folder, or a folder with modded Super Mario Odyssey files already present. If you pick a folder already containing modded files, make sure the files are from the same game version you select below, and that they aren't corrupted.

> Pro Tip: Pick a central place to store all you projects, like an `smo_projects` folder! Make sure to avoid spaces in your folder names, MoonFlow supports them but not everything does.

#### Game Version
This is where you select which version of the game you want to base your project on top of. It is *strongly recommended* to choose Version 1.0.0. This is the primary modding version for all developers, and is what 99% of mod projects are created on. However, for specific use cases you can pick any other version from 1.0.1 to 1.3.0.[^4]

**This cannot be changed later!**

[^4]: If more updates are released in the future, MoonFlow plans to support them (within reason). Kiosk Demo and Tencent builds are not supported at this time, but may be in the future.

#### Language
The default language is the language your project will be developed on. Changing this later can be difficult, so make sure to pick the language that fits your project the best! All text changes made to this language will be copied to all other languages during development (until a manual translation is provided).

## Conclusion
Once everything is set up, you can hit the go-ahead! Your project will be created and you can start using MoonFlow on a brand new blank project or modifying pre-existing modded files in the new tool!

The project creation process will create `.mfproj`, `.mfmeta`, and `.mfgraph` files in your project. These are not directly read by Super Mario Odyssey at any point, so they can safely be removed from published builds. However, they are designed to stay quite small and out of the way. It is highly recommended to include these in shipped releases. These files are used by MoonFlow to keep track of lots of important extra information about your project, so keep them close and make sure your source control backs them up!

And with that, you've got MoonFlow set up and ready to roll! Check out the intro guides to whichever editor catches your eye below.

* [Text Editor](text/basics_text_editor.md)
* [Event Graph Editor](events/basics_event_graph.md)
* [Kingdom Editor](kingdoms/basics_kingdoms.md)
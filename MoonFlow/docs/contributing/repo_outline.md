---
tags:
  - backend
---
# Repository Outline
Before working on contributing to the repository, it's important to know what each project's purpose and tasks are.

## MoonFlow
Likely the most important project of the entire repository, the `MoonFlow` directory. This contains the [Godot Engine](https://godotengine.org/) project and all of the user interface, along with lots of other vital utilities and functionalities. This is also the project that contains the documentation, as to make it easily accessible from the in-app documentation tool.

For most simple or high-level pull requests and changes, this directory is your target!

## MoonFlow.Project
`MoonFlow.Project` is a required dependency of `MoonFlow` and stores all of the components that make up a user-created project. Note that the actual project manager and project state are stored in the main `MoonFlow` directory, only the individual components and utilities are stored here. This primarily serves as a mid-level interfacing between `MoonFlow` and the lower level `Nindot` projects.

## Nindot
`Nindot` is the birthplace of MoonFlow, the foundation that the rest of the application is built up upon. This project serves as a low-level interface with the raw Nintendo file formats and exposing them for using in Godot C# projects like MoonFlow. Unlike the `MoonFlow` project, `Nindot` is not designed to be exclusively for Super Mario Odyssey, though is heavily specialized for Odyssey. Most systems are built upon a factory system, which would allow the code to be used for other games if a new factory was developed for that specific game.

Changes to this library are far more technical and precise, featuring zero UI and very little high-level API access.

## Nindot.Tests
A test library for the `Nindot` project. These tests can be run locally (with additional tests if a Super Mario Odyssey v1.0.0-v1.3.0 romfs path is provided) and are also run by GitHub actions to ensure stability in `Nindot`.

## Godot.Extension
A side-library built as a dependency for `MoonFlow`, featuring little more than extension methods for the `GodotSharp` classes and nodes.

## Modules
The modules directory contains any extra projects pulled from other open source projects that cannot be acquired through a nuget package, like [RiiStudio](https://github.com/riidefi/RiiStudio)'s [yaz0](http://www.amnoid.de/gc/yaz0.txt) library.
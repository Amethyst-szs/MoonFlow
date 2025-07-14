---
tags:
  - tools
---
# MoonFlow Command Line Arguments

## How to use
Any [Godot Engine command line](https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html) arguments can be provided to the executable when run from source or a compiled release. To use MoonFlow-specific arguments documented below, make sure to include the argument `"--"` to mark the start of the user arguments.

## `--project`
Provide a path for a MoonFlow project to open. The home screen will be skipped and it will load directly into the project. If path does not contain a valid MoonFlow project, an error will be printed and the home screen will load as normal.

## `--ignore_update_timestamp`
Prevent the automatic updater from checking for a new release

## `--launchmode`
Determines the mode that MoonFlow boots in. Valid options:

- `--launchmode=appless`: Launches editor without any app content. Used for debugging.

- `--launchmode=update_debug`: Interface to test automatic update functionality
- `--launchmode=update_cleanup`: Makes editor clean-up automatic updater remnants before finishing boot.
- `--launchmode=update_replace`: Internal use only, used for automatic updater
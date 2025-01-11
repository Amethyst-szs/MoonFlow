---
tags:
  - msbt
---
# Text Editor - Technical Details

## What are MSBT and MSBP files?
**MSBT** `(Message Studio Binary Text)` and **MSBP** `(Message Studio Binary Project)` are proprietary file formats created by Nintendo, used in a library called **LMS** `(Light Message Studio)`. These file formats are used in a wide variety of Nintendo games and software, however MoonFlow is designed specifically for Super Mario Odyssey's **LMS** files.

### Design Philosophy & Tags
Why just support Mario Odyssey and not other games? You might have noticed the "Binary" part of **MSBT** and **MSBP**, which means that these files are not stored in plain text, even when removed from their compressed archives. Many previous tools have been created to handle these file formats by converting them to plain-text to be edited in a standard or special text editor. MoonFlow does *not* work in this same way.

The big disadvantage of working with plain-text is *Binary Tags*, a common find in **MSBT** files. Take a look at this example of a plain-text string, taken from `SystemMessage.szs/CapMessage.byml/AlreadyVisitStage_ExistCollectCoin`:
```
\0CapTalkInfo018I remember this place!\0\0\0$"CapTalkInfo018_02I bet there are still \0 coins
in here somewhere...
```

See all the unintelligible gunk mixed in with the text? Those are tags being interpreted as text! So then you might raise the question, why not convert these tags to text to make them editable? This is absolutely possible, and could use a format like HTML to embed tags (ex. `<color:yellow>`).

This is also *not* the approach that MoonFlow uses. Instead, tags are stored as special objects in the middle of your string.[^1] This approach has pros and cons, but the massive advantage is speed of development, safety, and user-friendliness. Instead of needing to write arbitrary tags by hand and needing to deal with compiler errors, each tag is able to get it's own specialized UI!

[^1]: Internally, each MSBT entry is split up into pages, and each page contains a list of elements. An element can either contain plain-text or tags. This is how so much additional information can be crammed into a single text character, it's not actually a character at all but instead a special object that can store any extra data and be rendered to the screen with any image!

### CTI1
Prior to MoonFlow, adding new **MSBT** text files required updating the `CTI1` / `Project Sources` in the **MSBP**. Instead of looking up files by name, the **MSBP** is responsible for providing a database of sources in the `CTI1` data block. This was an annoying manual process, which required handling lots of delicate archives.

However, MoonFlow automatically handles updating your **MSBP** with all created text files.

## Archives & Languages
MoonFlow is designed to abstract the handling of individual [SZS](https://nintendo-formats.com/libs/sead/sarc.html) archives, however all of these need to be carefully managed on the backend. This is done through language syncing.

Each supported language by the game is assigned a folder code (ex. USen for US English, JPja for Japan Japanese, etc.) and will contain their own copies of all the text archives. This means that any mods modifying text will break if playing on an unsupported language, and manually managing every single language and ensuring synchrony is unreasonable.

This is one of the key problems that MoonFlow aims to solve. However, another goal was to avoid a compile / export process, so how do you manage to keep the project in an always-compiled state while having to manage so many separate languages and archives? Simple, *just do everything at once!*

When editing a text file in MoonFlow, you aren't actually editing *a* text file, you're editing every single language's version of that file at once! This is also true for the home page's file management system, creating a new file is actually creating over a dozen new files inside every single language.

The complexity of this task is what influenced the project structure of MoonFlow. In order to accurately store the translation data across all these languages, projects allow creating metadata files to track additional information.

## Metadata
Each language gets a `.mfmeta` file added to its message data. These files are automatically generated and read by MoonFlow and store additional information. The primary (but not only) purpose of these files is to track the [language syncing toggle](translations.md), controlling how data is copied between each language's instance the requested file.

Metadata can be safely excluded from release builds, but *not* source control. This data is not meant to be temporary, and should be saved with all other mod files. Despite not being required for release builds, it is strongly recommended to include them to ensure that users can have an easier experience working with your released project in the MoonFlow editor.
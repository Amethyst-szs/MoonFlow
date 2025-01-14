---
tags:
  - msbt
---

# Text Editor - Tags
MoonFlow introduces support for creating and modifying text tags. Behind the scenes, these tags are completely different from the rest of the text, so how do you work with them and what tags are available?

## What are Tags?
Tags, in the broad sense, are a way to format text in some specific way. For some simple examples, take markdown tags, like *\*italics\** by surrounding your text in asterisks, a common feature in many applications like Discord. This concept extends to more complex tagging systems like HTML: `<script> console.log("Hello World!); </script>`

The MSBT tag system works quite differently than these prior examples. For more information on the technical specifics on what these tags are, [look here](technical.md). Super Mario Odyssey is even more unique in this regard, implementing lots of custom tags and also not implementing quite a few common standards for the format.

Super Mario Odyssey's tags do *not* require closing markers. Each tag is entirely self-contained, and can contain any data. Most tags do some form of text formatting, but there are lots of special tags for controlling dialogue printing, playing sounds, and more.

In the context of MoonFlow, tags can mostly be handled the same way you do text. They can be copied, pasted, undone, redone, and more. One thing you *cannot* do is copy a tag and then paste it into a different text editor. These tags aren't text, after all!

So, what tags are there? And once we know what tags there are, how do we use them?

## All tags
- **Text Formatting:**
	- Set Color
	- Change Font
	- Change Font Size
	- Text Alignment
	- Japanese Furigana[^1]
	- Grammar Cap & Decap[^2]
- **Print Control:**
	- Text Speed
	- Text Delay
- **Icons:**
	- Picture Icons *(Mario, Cappy, Peach, etc.)*
	- Static Device/Controller Icons
	- Dynamic Device/Controller Icons[^3]
- Text Animation
- Sound Effect Playback
- And various replacement formats for numbers, strings, and time[^4]

[^1]: [Furigana](https://en.wikipedia.org/wiki/Furigana) is a Japanese reading aid which adds additional kana to indicate pronunciation
[^2]: The grammar tag is not fully understood, mostly but not exclusively appears in Korean
[^3]: Dynamic controller icons will change and adapt based on what controller the player is using (Joy-Con, Pro-Con, Single Joy-Con, etc..)
[^4]: Replacement format tags require additional exefs code

## Wheel
In order to add new tags to your custom text, you'll need to bring up the Tag Wheel. This can be done by **Right-Clicking** where you want to add a tag, or pressing **Ctrl + T** *(default keybind)*.

The wheel contains every single type of tag, the first page containing all the most common and important tags. The wheel can be navigated with the mouse or with the arrow keys & tab. The center button allows access to additional page(s) which contain more complicated or niche tags.

## Editing
To edit tags already in text (including tags you add with the wheel) you'll need to bring up the Tag Editor. This is done by **Ctrl + Left-Clicking** on the tag, or pressing **Ctrl + Shift + T** *(default keybind)* with your text cursor positioned on the tag.

Each tag contains completely different data, and as such, each tag has its own specially designed interface to edit its contents and properties. Unsure of what a tag does? Open up the Tag Editor and have a look!
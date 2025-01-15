---
tags:
  - world
---
# Kingdom Editor - Stage List
The stage list is vital for creating custom sub areas and other tasks involving adding custom stages. Aside from some rare exceptions (inside of the odyssey, some cutscenes, the world map, and other oddballs) every single stage is assigned to one kingdom and one kingdom *only*.

## Restrictions
One of the main restrictions of the stage list tab is the inability to add one stage to multiple kingdoms.  This is fully by design, the game also never has a stage co-exist between kingdoms. Any rare examples like the inside of the odyssey are not included in any kingdom's stage list. This means that the inside of the Odyssey cannot have a [StageMessage](../text/file_management.md#stagemessage) text file under normal circumstances.

If you encounter conflicts where the stage you want to add to a kingdom is already in another kingdom, MoonFlow will prevent you from doing this until you remove the stage from the other kingdom.

You also cannot add Power Moons to the [moon list](moon_list.md) until they stage with their display name is added to the kingdom.

## Stage Types
The stage list allows you to add and remove stages from your kingdom, as well as change their type. Each stage type has lots of small effects on the game's behavior, so making sure to select the right type can be important.

- `Main Stage:` The home stage of the kingdom where the Odyssey will land.
- `Main Route Stage:` A vital sub-stage to the story. (ex. Sand Kingdom underground)
- `Pathway Stage:` Very similar to main route, usually less vital / mandatory areas. (ex. Deep Woods)
- `Ex Stage:` Sub-area containing two power moons, where Cappy will notify you of your collectable status upon entry.
- `Moon Ex Stage:` Same as Ex Stage, but for moon pipes unlocked by opening the moon rocks.
- `Moon Far Side Ex Stage:` Same as Moon Ex Stage, but only for Dark Side.
- `Small Stage:` A small stage similar to a sub-area, but with any number of moons and no cappy notification, often confused for sub-areas by casual players. (ex. Vibration room in Sand & Seaside)
- `Boss Revenge:` Boss rematches, exclusively found in Mushroom Kingdom.
- `Mini Game:` Bonus games like Snow's Koopa Tracewalking or Metro's RC track.
- `Shop Stage:` Crazy Cap Shops located indoors
- `Zone:` Sub-stages loaded into other stages. Includes things like Sand Kingdom town, every island in Seaside Kingdom, each segment of moon cave, and many more.
- `Demo:` Cutscenes
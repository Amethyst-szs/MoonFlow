---
tags:
  - world
---
# Kingdom Editor - Power Moon List
The Power Moon List is designed after the in-game moon list, however with lots of additional information displayed and modifiable. For the very basics on viewing the list, look [here](basics_kingdom_editor.md).

## Properties
Each moon has a large assortment of properties, accessible by opening their dropdown menu.

### Stage Name
The stage name can be any stage file within the current kingdom's [stage list](stage_list.md), selected by a dropdown menu. Note that this is *NOT* the same as where the moon will be collected. The vast majority of the time it will be, but in some rare cases like hint arts, the moon will be collected in a different stage than the stage name. In these cases, the stage name is only for display name lookup.

### Object ID
The objID is the identifier of the `Shine` object in the level data. This is not the same as the Unique ID which is used for distinguishing each moon from each other, and is mostly used for display name lookup along with the stage name.

### Unique ID
The UID is a unique number for each moon, used to track what you have collected. If two moons share the same UID, they will be considered the same moon and both be collected if either is collected. MoonFlow features an automatic button to find an unused value, and is recommended to use this in most cases.

### Hint ID
Similar to the Unique ID but only unique within the kingdom. Used for tracking unlocked hints, and is also recommended to use the automatic ID button unless absolutely necessary.

### Power Moon Type
The moon type switches determine a few basic behaviors.

The Multi-Moon toggle makes the moon worth three in your total, has no effect on the Shine object placed in the world.

The Moon Rock toggle is deceiving, because it doesn't exactly reflect what you see in the in-game moon list. This toggle is mainly only used for Moon Rock Moons located in the kingdom's home stage. The in-game moon list determines the icon's visibility based on if the only [collectable scenario](#scenarios) is equal to the Post-Moon Rock Scenario. (viewable and editable in the info sidebar)

The Toadette / Achievement toggle is unused. The achievement system is almost completely separate from the Power Moon system, and they are stored in a completely separate list.

### Quest
The quest field allows you to select which Quest ID this moon is connected to. Every story moon that progresses your next objective is connected to a quest. The base game kingdom's only implement up to 6 quests, however MoonFlow offers support for up to 15 quest slots to ensure future compatibility with large modding projects.

### Scenarios
This bitfield allows you to set which scenario IDs the Power Moon is collectable in. Note that this is based on the home stage's current scenario progression, *not* the current stage and scenario you happen to be in (ex. Koopa Freerunning). Any moons only collectable in higher scenarios than your progression will be hidden from the moon list. Highly recommended to make sure this matches your stage files.

### Translation
Position of the Shine object in 3D space. Position can be copied from EditorCore, in-game, or any other interface that uses real coordinates. This does *not* include Moonlight/Spotlight due to using fake simplified coordinates.

This position is only used for rendering the icon on the kingdom's map screen, in respect to the world's projection matrix.
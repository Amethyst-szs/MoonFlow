# Exefs Extensions - Color Palette Editor

## Information
This code patch adds support for the Color Palette Editor extension provided by MoonFlow. It functions by adding an additional BYML file to the `LocalizedData/Common/ProjectData` archive which gets read during game initialization to define the text color palette size and values.

It requires an `eui` namespace header which, at the time of this extension's publishing, is not included in [OdysseyHeaders](https://github.com/MonsterDruide1/OdysseyHeaders). You can find the header provided below the code implementation.

## Built-In Support Info
The following template repositories have support for this extension built-in. *(If you want your repository added to this list, create an issue/PR!)*
- QuickMoon

## Code Implementation
Select the implementation for the framework you're using and copy-paste into a source file in your repository. You may need to add additional header includes as-needed.
- [ExLaunch](#ExLaunch)
- *[Hakkun](#Hakkun) (Currently unavailable, open to PR!)*

### ExLaunch
```cpp
void MoonFlowExtensionPatch_MessageMgrColorGradiation(eui::MessageMgr* thiz, sead::Heap* heap, uint maxColor)
{
    static constexpr const char* cArcPath = "LocalizedData/Common/ProjectData";
    static constexpr const char* cByamlPath = "ColorTagExtension";
  
    al::Resource* dataRes = al::findOrCreateResource(cArcPath, nullptr);
    if (!dataRes->isExistByml(cByamlPath)) {
        thiz->initialize(heap, maxColor);
        return;
    }

    al::ByamlIter data = dataRes->tryGetByml(cByamlPath);
    const uint dataListSize = data.getSize();
    if (dataListSize == 0) {
        thiz->initialize(heap, maxColor);
        return;
    }

    // Now that we've passed the safety checks, setup the message manager
    // and patch out the hardcoded color definitions that will run
    // after this function returns to (al::LayoutSystem::initEui())
    thiz->initialize(heap, dataListSize);

    #define INSTALL_EXL_NOP(Address) p.Seek(Address); p.WriteInst(exl::armv8::inst::Nop());
    exl::patch::CodePatcher p(0xFFFFFF);
    INSTALL_EXL_NOP(0x8ba93c);
    INSTALL_EXL_NOP(0x8ba954);
    INSTALL_EXL_NOP(0x8ba970);
    INSTALL_EXL_NOP(0x8ba990);
    INSTALL_EXL_NOP(0x8ba9ac);
    INSTALL_EXL_NOP(0x8ba9c8);
    INSTALL_EXL_NOP(0x8ba9e4);
    INSTALL_EXL_NOP(0x8ba9fc);

    // Load the color data from the archive
    for (uint i = 0; i < dataListSize; i++) {
        al::ByamlIter colorInfo = data.getIterByIndex(i);
  
        sead::Color4f top = sead::Color4f::cWhite;
        sead::Color4f bottom = sead::Color4f::cWhite;
        if (al::tryGetByamlColor(&top, colorInfo, "Top")) {
            if (colorInfo.isExistKey("Bottom"))
                al::tryGetByamlColor(&bottom, colorInfo, "Bottom");
            else
                bottom = top;
        }

        sead::Color4u8 topU8(u8(top.a * 255.f), u8(top.b * 255.f), u8(top.g * 255.f), u8(top.r * 255.f));
        sead::Color4u8 bottomU8(u8(bottom.a * 255.f), u8(bottom.b * 255.f), u8(bottom.g * 255.f), u8(bottom.r * 255.f));
        thiz->setGradationColor(i, topU8, bottomU8);

    }

}

void InstallHook_MoonFlowColorPaletteEditorExtension()
{
    exl::patch::CodePatcher p(0xFFFFFF);

    /* - MSBP Color Gradiation Patch - */
    p.Seek(0x8ba890);
    p.BranchLinkInst((void*)MoonFlowExtensionPatch_MessageMgrColorGradiation);
}
```

### *Hakkun*
*Currently unavailable, open to PR!*

## Relevant Headers
The headers provided below are provided as an additional support due to them not being included in [OdysseyHeaders](https://github.com/MonsterDruide1/OdysseyHeaders) at the time of this extension's publishing. Common headers that are well established in repositories won't be provided, please reference [OdysseyHeaders](https://github.com/MonsterDruide1/OdysseyHeaders) if you are missing something required.

### euiMessageMgr.h
```cpp
#pragma once

#include <sead/gfx/seadColor.h>
#include <sead/heap/seadDisposer.h>
#include <sead/prim/seadRuntimeTypeInfo.h>
#include <sead/prim/seadSafeString.h>

namespace eui {

class MessageSet;

class MessageMgr {
    SEAD_RTTI_BASE(MessageMgr);
    SEAD_SINGLETON_DISPOSER(MessageMgr);
    MessageMgr();
    virtual ~MessageMgr();

public:
    class Archive;

public:
    virtual void loadArchive(sead::Heap*, void*, unsigned int);
    virtual void unloadArchive(void*);

    void initialize(sead::Heap*, unsigned int maxColors);
    void finalize();
    void archiveDisposeCallback_(Archive*);
    void dumpLastGotMessageSetInfo(); // Implementation requires some debug compiler flag

    void setGradationColor(unsigned int idx, sead::Color4u8 top, sead::Color4u8 bottom);
    void setTextBoxWidthSizeOverColor(sead::Color4u8);
    MessageSet* getLayoutMessageSet(sead::SafeString const&) const;
    MessageSet* getMessageSet(sead::SafeString const&) const;

private:
    char unk[0x30];
};

static_assert(sizeof(MessageMgr) == 0x58);

} // namespace eui
```
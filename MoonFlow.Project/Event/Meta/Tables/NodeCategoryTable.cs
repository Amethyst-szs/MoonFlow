using System.Collections.Generic;

namespace MoonFlow.Project;

public static class MetaCategoryTable
{
    public enum Categories
    {
        DEFAULT,
        ENTRY_POINT,
        CUSTOM_TYPE,

        FLOW,
        EVENT,
        ACTOR,
        PLAYER,
        DIALOUGE,
        ITEM,
        CAMERA,
        STAGE,
        QUERY,
        DEMO,
        WIPE,
        FLAGS,
        AMIIBO,
        AUDIO,
    }

    public static readonly Dictionary<string, Categories> Table = new(){
        { "EntryPoint", Categories.ENTRY_POINT },

        { "ActionLoop", Categories.ACTOR },
        { "ActionOneTime", Categories.ACTOR },
        { "ActorBaseMovementEnd", Categories.ACTOR },
        { "ActorBaseMovementStart", Categories.ACTOR },
        { "ActorKill", Categories.ACTOR },
        { "NpcMoveToLink", Categories.ACTOR },
        { "TurnToPlayer", Categories.ACTOR },
        { "TurnToPlayerActionOneTime", Categories.ACTOR },
        { "TurnToPreDir", Categories.ACTOR },
        { "HitReaction", Categories.ACTOR },
        { "KakkuTurn", Categories.ACTOR },

        { "AmiiboTouchLayout", Categories.AMIIBO },
        { "AppearMapAmiiboHint", Categories.AMIIBO },
        { "GetAmiiboCostume", Categories.AMIIBO },
        { "GetAmiiboNotSearchHintNum", Categories.AMIIBO },
        { "GetSearchAmiibo", Categories.AMIIBO },
        { "IsCostumeAmiibo", Categories.AMIIBO },
        { "IsEnableSearchAmiibo", Categories.AMIIBO },
        { "IsTalkAmiiboHelp", Categories.AMIIBO },

        { "AnimCameraStart", Categories.CAMERA },
        { "CameraEnd", Categories.CAMERA },
        { "CameraStart", Categories.CAMERA },
        { "VrGyroReset", Categories.CAMERA },

        { "CapMessage", Categories.DIALOUGE },
        { "CloseTalkMessage", Categories.DIALOUGE },
        { "CloseTalkMessageNoSe", Categories.DIALOUGE },
        { "MessageBalloon", Categories.DIALOUGE },
        { "MessageTalk", Categories.DIALOUGE },
        { "MessageTalkSpecialPurpose", Categories.DIALOUGE },
        { "NextTalkMessage", Categories.DIALOUGE },
        { "SelectChoice", Categories.DIALOUGE },
        { "SelectYesNo", Categories.DIALOUGE },

        { "DemoAction", Categories.DEMO },
        { "DemoCamera", Categories.DEMO },
        { "DemoPlayerAction", Categories.DEMO },
        { "DemoPlayerHide", Categories.DEMO },
        { "DemoPlayerShow", Categories.DEMO },
        { "BindKeepDemoStart", Categories.DEMO },
        { "CapManHeroTalkSetDemoStartPose", Categories.DEMO },
        { "CapManHeroTalkAppear", Categories.DEMO },
        { "CapManHeroTalkFocus", Categories.DEMO },
        { "CapManHeroTalkPlayerTurn", Categories.DEMO },
        { "CapManHeroTalkReturn", Categories.DEMO },
        { "CapManHeroTalkSetDemoEndPose", Categories.DEMO },
        { "CutSceneDemoStart", Categories.DEMO },
        { "DemoEnd", Categories.DEMO },
        { "DemoForceStartOnGround", Categories.DEMO },
        { "DemoForceStart", Categories.DEMO },
        { "DemoResetPlayerDynamics", Categories.DEMO },
        { "DemoStart", Categories.DEMO },
        { "ForcePutOnDemoCap", Categories.DEMO },
        { "ChangeWorldDemoMessage", Categories.DEMO },
        { "NormalDemoTryStart", Categories.DEMO },
        { "SetDemoInfoDemoName", Categories.DEMO },

        { "Event", Categories.EVENT },
        { "EventQuery", Categories.EVENT },

        { "FirstTalkEndCollectBgmNpc", Categories.FLAGS },
        { "GetCollectBgmBonus01", Categories.FLAGS },
        { "GetCollectBgmBonus02", Categories.FLAGS },
        { "OpenBgmList", Categories.FLAGS },
        { "EnableHint", Categories.FLAGS },
        { "UnlockHint", Categories.FLAGS },

        { "Fork", Categories.FLOW },
        { "Join", Categories.FLOW },
        { "JumpEntry", Categories.FLOW },
        { "WaitSimple", Categories.FLOW },

        { "CoinPayment", Categories.ITEM },
        { "PopItem", Categories.ITEM },
        { "DirectGetLinkShine", Categories.ITEM },
        { "PopLinkShine", Categories.ITEM },

        { "CheckPlayerOnGround", Categories.PLAYER },
        { "PlayerAction", Categories.PLAYER },
        { "PlayerTurn", Categories.PLAYER },
        { "ReplacePlayer", Categories.PLAYER },

        { "QueryJudge", Categories.QUERY },
        { "CheckClear3CollectBgm", Categories.QUERY },
        { "CheckCompleteCollectBgm", Categories.QUERY },
        { "CheckCostume", Categories.QUERY },
        { "CheckCostumeInvisible", Categories.QUERY },
        { "CheckCostumeMissMatchPart", Categories.QUERY },
        { "CheckCostumePair", Categories.QUERY },
        { "CheckCount", Categories.QUERY },
        { "CheckFirstTalkCollectBgm", Categories.QUERY },
        { "CheckFlag", Categories.QUERY },
        { "CheckGetLinkShine", Categories.QUERY },
        { "CheckLifeUpItem", Categories.QUERY },
        { "CheckLink", Categories.QUERY },
        { "CheckMoonLockOpened", Categories.QUERY },
        { "CheckOpenDoorSnow", Categories.QUERY },
        { "CheckPlayingCollectBgm", Categories.QUERY },
        { "CheckYukimaruRaceResult", Categories.QUERY },
        { "SessionWaitMusician", Categories.QUERY },

        { "BgmCtrl", Categories.STAGE },
        { "ChangeStage", Categories.STAGE },
        { "CheckSwitch", Categories.STAGE },
        { "CheckWaitSwitch", Categories.STAGE },
        { "SwitchOff", Categories.STAGE },
        { "SwitchOn", Categories.STAGE },

        { "WaitWipeOpenEnd", Categories.WIPE },
        { "WipeFadeBlackClose", Categories.WIPE },
        { "WipeFadeBlackOpen", Categories.WIPE },
        { "SceneWipeClose", Categories.WIPE },
    };

    public static Categories Lookup(string type)
    {
        if (Table.TryGetValue(type, out Categories value))
            return value;

        return Categories.CUSTOM_TYPE;
    }
}
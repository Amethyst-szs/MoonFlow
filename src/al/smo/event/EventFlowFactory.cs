using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public static class EventFlowFactory
{
    private static readonly Dictionary<string, Type> FactoryEntries = new(){
        // EventFlowNodeFactory definitions (al namespace)
        { "ActionLoop", typeof(NodeBase) },
        { "ActionOneTime", typeof(NodeBase) },
        { "ActorBaseMovementEnd", typeof(NodeBase) },
        { "ActorBaseMovementStart", typeof(NodeBase) },
        { "ActorKill", typeof(NodeBase) },
        { "AnimCameraStart", typeof(NodeBase) },
        { "CameraEnd", typeof(NodeBase) },
        { "CameraStart", typeof(NodeBase) },
        { "CheckSwitch", typeof(NodeBase) },
        { "CheckWaitSwitch", typeof(NodeBase) },
        { "DemoAction", typeof(NodeBase) },
        { "DemoCamera", typeof(NodeBase) },
        { "DemoPlayerAction", typeof(NodeBase) },
        { "DemoPlayerHide", typeof(NodeBase) },
        { "DemoPlayerShow", typeof(NodeBase) },
        { "Event", typeof(NodeBase) },
        { "EventQuery", typeof(NodeBase) },
        { "Fork", typeof(NodeBase) },
        { "HitReaction", typeof(NodeBase) },
        { "Join", typeof(NodeBase) },
        { "JumpEntry", typeof(NodeBase) },
        { "QueryJudge", typeof(NodeBase) },
        { "SwitchOff", typeof(NodeBase) },
        { "SwitchOn", typeof(NodeBase) },
        { "TurnToPlayer", typeof(NodeBase) },
        { "TurnToPlayerActionOneTime", typeof(NodeBase) },
        { "TurnToPreDir", typeof(NodeBase) },

        // ProjectEventFlowNodeFactory (not al namespace)
        { "AmiiboTouchLayout", typeof(NodeBase) },
        { "AppearMapAmiiboHint", typeof(NodeBase) },
        { "BgmCtrl", typeof(NodeBase) },
        { "BindKeepDemoStart", typeof(NodeBase) },
        { "CapMessage", typeof(NodeBase) },
        { "CapManHeroTalkSetDemoStartPose", typeof(NodeBase) },
        { "CapManHeroTalkAppear", typeof(NodeBase) },
        { "CapManHeroTalkFocus", typeof(NodeBase) },
        { "CapManHeroTalkPlayerTurn", typeof(NodeBase) },
        { "CapManHeroTalkReturn", typeof(NodeBase) },
        { "CapManHeroTalkSetDemoEndPose", typeof(NodeBase) },
        { "ChangeStage", typeof(NodeBase) },
        { "ChangeWorldDemoMessage", typeof(NodeBase) },
        { "CheckClear3CollectBgm", typeof(NodeBase) },
        { "CheckCompleteCollectBgm", typeof(NodeBase) },
        { "CheckCostume", typeof(NodeBase) },
        { "CheckCostumeInvisible", typeof(NodeBase) },
        { "CheckCostumeMissMatchPart", typeof(NodeBase) },
        { "CheckCostumePair", typeof(NodeBase) },
        { "CheckCount", typeof(NodeBase) },
        { "CheckFirstTalkCollectBgm", typeof(NodeBase) },
        { "CheckFlag", typeof(NodeBase) },
        { "CheckGetLinkShine", typeof(NodeBase) },
        { "CheckLifeUpItem", typeof(NodeBase) },
        { "CheckLink", typeof(NodeBase) },
        { "CheckMoonLockOpened", typeof(NodeBase) },
        { "CheckOpenDoorSnow", typeof(NodeBase) },
        { "CheckPlayingCollectBgm", typeof(NodeBase) },
        { "CheckPlayerOnGround", typeof(NodeBase) },
        { "CheckYukimaruRaceResult", typeof(NodeBase) },
        { "CloseTalkMessage", typeof(NodeBase) },
        { "CloseTalkMessageNoSe", typeof(NodeBase) }, // v1.2.0+
        { "CoinPayment", typeof(NodeBase) },
        { "CutSceneDemoStart", typeof(NodeBase) },
        { "DemoEnd", typeof(NodeBase) },
        { "DemoForceStartOnGround", typeof(NodeBase) },
        { "DemoForceStart", typeof(NodeBase) },
        { "DemoResetPlayerDynamics", typeof(NodeBase) },
        { "DemoStart", typeof(NodeBase) },
        { "DirectGetLinkShine", typeof(NodeBase) },
        { "EnableHint", typeof(NodeBase) },
        { "FirstTalkEndCollectBgmNpc", typeof(NodeBase) },
        { "ForcePutOnDemoCap", typeof(NodeBase) },
        { "GetAmiiboCostume", typeof(NodeBase) },
        { "GetAmiiboNotSearchHintNum", typeof(NodeBase) },
        { "GetCollectBgmBonus01", typeof(NodeBase) },
        { "GetCollectBgmBonus02", typeof(NodeBase) },
        { "GetSearchAmiibo", typeof(NodeBase) },
        { "IsCostumeAmiibo", typeof(NodeBase) },
        { "IsEnableSearchAmiibo", typeof(NodeBase) },
        { "IsTalkAmiiboHelp", typeof(NodeBase) },
        { "KakkuTurn", typeof(NodeBase) },
        { "MessageBalloon", typeof(NodeBase) },
        { "MessageTalk", typeof(NodeBase) },
        { "MessageTalkSpecialPurpose", typeof(NodeBase) },
        { "NextTalkMessage", typeof(NodeBase) },
        { "NormalDemoTryStart", typeof(NodeBase) },
        { "NpcMoveToLink", typeof(NodeBase) },
        { "OpenBgmList", typeof(NodeBase) },
        { "PlayerAction", typeof(NodeBase) },
        { "PlayerTurn", typeof(NodeBase) },
        { "PopItem", typeof(NodeBase) },
        { "PopLinkShine", typeof(NodeBase) },
        { "ReplacePlayer", typeof(NodeBase) },
        { "SceneWipeClose", typeof(NodeBase) },
        { "SelectChoice", typeof(NodeBase) },
        { "SelectYesNo", typeof(NodeBase) },
        { "SessionWaitMusician", typeof(NodeBase) },
        { "SetDemoInfoDemoName", typeof(NodeBase) },
        { "UnlockHint", typeof(NodeBase) },
        { "VrGyroReset", typeof(NodeBase) }, // v1.3.0+
        { "WaitWipeOpenEnd", typeof(NodeBase) },
        { "WaitSimple", typeof(NodeBase) },
        { "WipeFadeBlackClose", typeof(NodeBase) },
        { "WipeFadeBlackOpen", typeof(NodeBase) },
    };

    public static NodeBase CreateNode(Dictionary<object, object> dict)
    {
        // Setup a string to access into the factory table
        string nType = GetNodeType(dict);
        nType = nType.Replace("EventFlowNode", "");

        // Ensure this string exists in the factory table
        if (!FactoryEntries.TryGetValue(nType, out Type factoryType))
            return new NodeBase(dict);
        
        object n = factoryType.GetConstructor([typeof(Dictionary<object, object>)]).Invoke([dict]);
        return (NodeBase)n;
    }

    public static string GetNodeType(Dictionary<object, object> dict)
    {
        string type = "";

        if (dict.ContainsKey("Base")) type = (string)dict["Base"];
        else if (dict.ContainsKey("Type")) type = (string)dict["Type"];

        return type;
    }
}
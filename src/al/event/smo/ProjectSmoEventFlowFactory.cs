using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class ProjectSmoEventFlowFactory : EventFlowFactoryBase
{
    private static readonly Dictionary<string, Type> FactoryEntries = new(){
        // EventFlowNodeFactory definitions (al namespace)
        { "ActionLoop", typeof(NodeActionLoop) },
        { "ActionOneTime", typeof(EventFlowNodeGeneric) },
        { "ActorBaseMovementEnd", typeof(EventFlowNodeGeneric) },
        { "ActorBaseMovementStart", typeof(EventFlowNodeGeneric) },
        { "ActorKill", typeof(EventFlowNodeGeneric) },
        { "AnimCameraStart", typeof(EventFlowNodeGeneric) },
        { "CameraEnd", typeof(EventFlowNodeGeneric) },
        { "CameraStart", typeof(EventFlowNodeGeneric) },
        { "CheckSwitch", typeof(EventFlowNodeGeneric) },
        { "CheckWaitSwitch", typeof(EventFlowNodeGeneric) },
        { "DemoAction", typeof(EventFlowNodeGeneric) },
        { "DemoCamera", typeof(EventFlowNodeGeneric) },
        { "DemoPlayerAction", typeof(EventFlowNodeGeneric) },
        { "DemoPlayerHide", typeof(EventFlowNodeGeneric) },
        { "DemoPlayerShow", typeof(EventFlowNodeGeneric) },
        { "Event", typeof(EventFlowNodeGeneric) },
        { "EventQuery", typeof(EventFlowNodeGeneric) },
        { "Fork", typeof(EventFlowNodeGeneric) },
        { "HitReaction", typeof(EventFlowNodeGeneric) },
        { "Join", typeof(EventFlowNodeGeneric) },
        { "JumpEntry", typeof(EventFlowNodeGeneric) },
        { "QueryJudge", typeof(EventFlowNodeGeneric) },
        { "SwitchOff", typeof(EventFlowNodeGeneric) },
        { "SwitchOn", typeof(EventFlowNodeGeneric) },
        { "TurnToPlayer", typeof(EventFlowNodeGeneric) },
        { "TurnToPlayerActionOneTime", typeof(EventFlowNodeGeneric) },
        { "TurnToPreDir", typeof(EventFlowNodeGeneric) },

        // ProjectEventFlowNodeFactory (not al namespace)
        { "AmiiboTouchLayout", typeof(EventFlowNodeGeneric) },
        { "AppearMapAmiiboHint", typeof(EventFlowNodeGeneric) },
        { "BgmCtrl", typeof(EventFlowNodeGeneric) },
        { "BindKeepDemoStart", typeof(EventFlowNodeGeneric) },
        { "CapMessage", typeof(EventFlowNodeGeneric) },
        { "CapManHeroTalkSetDemoStartPose", typeof(EventFlowNodeGeneric) },
        { "CapManHeroTalkAppear", typeof(EventFlowNodeGeneric) },
        { "CapManHeroTalkFocus", typeof(EventFlowNodeGeneric) },
        { "CapManHeroTalkPlayerTurn", typeof(EventFlowNodeGeneric) },
        { "CapManHeroTalkReturn", typeof(EventFlowNodeGeneric) },
        { "CapManHeroTalkSetDemoEndPose", typeof(EventFlowNodeGeneric) },
        { "ChangeStage", typeof(EventFlowNodeGeneric) },
        { "ChangeWorldDemoMessage", typeof(EventFlowNodeGeneric) },
        { "CheckClear3CollectBgm", typeof(EventFlowNodeGeneric) },
        { "CheckCompleteCollectBgm", typeof(EventFlowNodeGeneric) },
        { "CheckCostume", typeof(EventFlowNodeGeneric) },
        { "CheckCostumeInvisible", typeof(EventFlowNodeGeneric) },
        { "CheckCostumeMissMatchPart", typeof(EventFlowNodeGeneric) },
        { "CheckCostumePair", typeof(EventFlowNodeGeneric) },
        { "CheckCount", typeof(EventFlowNodeGeneric) },
        { "CheckFirstTalkCollectBgm", typeof(EventFlowNodeGeneric) },
        { "CheckFlag", typeof(EventFlowNodeGeneric) },
        { "CheckGetLinkShine", typeof(EventFlowNodeGeneric) },
        { "CheckLifeUpItem", typeof(EventFlowNodeGeneric) },
        { "CheckLink", typeof(EventFlowNodeGeneric) },
        { "CheckMoonLockOpened", typeof(EventFlowNodeGeneric) },
        { "CheckOpenDoorSnow", typeof(EventFlowNodeGeneric) },
        { "CheckPlayingCollectBgm", typeof(EventFlowNodeGeneric) },
        { "CheckPlayerOnGround", typeof(EventFlowNodeGeneric) },
        { "CheckYukimaruRaceResult", typeof(EventFlowNodeGeneric) },
        { "CloseTalkMessage", typeof(EventFlowNodeGeneric) },
        { "CloseTalkMessageNoSe", typeof(EventFlowNodeGeneric) }, // v1.2.0+
        { "CoinPayment", typeof(EventFlowNodeGeneric) },
        { "CutSceneDemoStart", typeof(EventFlowNodeGeneric) },
        { "DemoEnd", typeof(EventFlowNodeGeneric) },
        { "DemoForceStartOnGround", typeof(EventFlowNodeGeneric) },
        { "DemoForceStart", typeof(EventFlowNodeGeneric) },
        { "DemoResetPlayerDynamics", typeof(EventFlowNodeGeneric) },
        { "DemoStart", typeof(EventFlowNodeGeneric) },
        { "DirectGetLinkShine", typeof(EventFlowNodeGeneric) },
        { "EnableHint", typeof(EventFlowNodeGeneric) },
        { "FirstTalkEndCollectBgmNpc", typeof(EventFlowNodeGeneric) },
        { "ForcePutOnDemoCap", typeof(EventFlowNodeGeneric) },
        { "GetAmiiboCostume", typeof(EventFlowNodeGeneric) },
        { "GetAmiiboNotSearchHintNum", typeof(EventFlowNodeGeneric) },
        { "GetCollectBgmBonus01", typeof(EventFlowNodeGeneric) },
        { "GetCollectBgmBonus02", typeof(EventFlowNodeGeneric) },
        { "GetSearchAmiibo", typeof(EventFlowNodeGeneric) },
        { "IsCostumeAmiibo", typeof(EventFlowNodeGeneric) },
        { "IsEnableSearchAmiibo", typeof(EventFlowNodeGeneric) },
        { "IsTalkAmiiboHelp", typeof(EventFlowNodeGeneric) },
        { "KakkuTurn", typeof(EventFlowNodeGeneric) },
        { "MessageBalloon", typeof(EventFlowNodeGeneric) },
        { "MessageTalk", typeof(EventFlowNodeGeneric) },
        { "MessageTalkSpecialPurpose", typeof(EventFlowNodeGeneric) },
        { "NextTalkMessage", typeof(EventFlowNodeGeneric) },
        { "NormalDemoTryStart", typeof(EventFlowNodeGeneric) },
        { "NpcMoveToLink", typeof(EventFlowNodeGeneric) },
        { "OpenBgmList", typeof(EventFlowNodeGeneric) },
        { "PlayerAction", typeof(EventFlowNodeGeneric) },
        { "PlayerTurn", typeof(EventFlowNodeGeneric) },
        { "PopItem", typeof(EventFlowNodeGeneric) },
        { "PopLinkShine", typeof(EventFlowNodeGeneric) },
        { "ReplacePlayer", typeof(EventFlowNodeGeneric) },
        { "SceneWipeClose", typeof(EventFlowNodeGeneric) },
        { "SelectChoice", typeof(EventFlowNodeGeneric) },
        { "SelectYesNo", typeof(EventFlowNodeGeneric) },
        { "SessionWaitMusician", typeof(EventFlowNodeGeneric) },
        { "SetDemoInfoDemoName", typeof(EventFlowNodeGeneric) },
        { "UnlockHint", typeof(EventFlowNodeGeneric) },
        { "VrGyroReset", typeof(EventFlowNodeGeneric) }, // v1.3.0+
        { "WaitWipeOpenEnd", typeof(EventFlowNodeGeneric) },
        { "WaitSimple", typeof(EventFlowNodeGeneric) },
        { "WipeFadeBlackClose", typeof(EventFlowNodeGeneric) },
        { "WipeFadeBlackOpen", typeof(EventFlowNodeGeneric) },
    };

    public override NodeBase CreateNode(Dictionary<object, object> dict)
    {
        // Setup a string to access into the factory table
        string nType = GetNodeType(dict);
        nType = nType.Replace("EventFlowNode", "");

        // Ensure this string exists in the factory table
        if (!FactoryEntries.TryGetValue(nType, out Type factoryType))
            return new EventFlowNodeGeneric(dict);

        object n = factoryType.GetConstructor([typeof(Dictionary<object, object>)]).Invoke([dict]);
        return (NodeBase)n;
    }
}
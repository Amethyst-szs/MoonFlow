using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class ProjectSmoEventFlowFactory : EventFlowFactoryBase
{
    private static readonly Dictionary<string, Type> FactoryEntries = new(){
        // EventFlowNodeFactory definitions (al namespace)
        { "ActionLoop", typeof(NodeActionLoop) }, // Actor plays action repeatedly
        { "ActionOneTime", typeof(NodeActionOneTime) }, // Actor plays action once
        { "ActorBaseMovementEnd", typeof(NodeGeneric) }, // End event's control over actor movement
        { "ActorBaseMovementStart", typeof(NodeGeneric) }, // Start event's control over actor movement
        { "ActorKill", typeof(NodeGeneric) }, // Kill actor
        { "AnimCameraStart", typeof(NodeAnimCameraStart) }, // Register a camera in EventFlowDataHolder by name and animation
        { "CameraEnd", typeof(NodeCameraEnd) }, // End camera by name
        { "CameraStart", typeof(NodeCameraStart) }, // Start camera by name
        { "CheckSwitch", typeof(NodeCheckSwitch) }, // Check if switch at Node.Name is on or off
        { "CheckWaitSwitch", typeof(NodeCheckWaitSwitch) }, // Await switch on before continuing event flow
        { "DemoAction", typeof(NodeDemoAction) }, // Play a demo action on this actor
        { "DemoCamera", typeof(NodeDemoCamera) }, // Play a demo camera
        { "DemoPlayerAction", typeof(NodeDemoPlayerAction) }, // Play a demo action on the demo player
        { "DemoPlayerHide", typeof(NodeGeneric) }, // Hide demo player
        { "DemoPlayerShow", typeof(NodeGeneric) }, // Show demo player
        { "Event", typeof(NodeEvent) }, // Customizable callusing IEventFlowEventReciever->recieveEvent()
        { "EventQuery", typeof(NodeEventQuery) }, // Customizable bool CaseEventList using IEventFlowEventReciever->recieveEvent()
        { "Fork", typeof(NodeFork) }, // Split execution in two? Not sure
        { "HitReaction", typeof(NodeGeneric) },
        { "Join", typeof(NodeJoin) }, // Merge a branch back together? Not sure
        { "JumpEntry", typeof(NodeJumpEntry) }, // End current event and change next entry point
        { "QueryJudge", typeof(NodeGeneric) },
        { "SwitchOff", typeof(NodeGeneric) },
        { "SwitchOn", typeof(NodeGeneric) },
        { "TurnToPlayer", typeof(NodeGeneric) },
        { "TurnToPlayerActionOneTime", typeof(NodeGeneric) },
        { "TurnToPreDir", typeof(NodeGeneric) },

        // ProjectEventFlowNodeFactory (not al namespace)
        { "AmiiboTouchLayout", typeof(NodeGeneric) },
        { "AppearMapAmiiboHint", typeof(NodeGeneric) },
        { "BgmCtrl", typeof(NodeGeneric) },
        { "BindKeepDemoStart", typeof(NodeGeneric) },
        { "CapMessage", typeof(NodeGeneric) },
        { "CapManHeroTalkSetDemoStartPose", typeof(NodeGeneric) },
        { "CapManHeroTalkAppear", typeof(NodeGeneric) },
        { "CapManHeroTalkFocus", typeof(NodeGeneric) },
        { "CapManHeroTalkPlayerTurn", typeof(NodeGeneric) },
        { "CapManHeroTalkReturn", typeof(NodeGeneric) },
        { "CapManHeroTalkSetDemoEndPose", typeof(NodeGeneric) },
        { "ChangeStage", typeof(NodeGeneric) },
        { "ChangeWorldDemoMessage", typeof(NodeGeneric) },
        { "CheckClear3CollectBgm", typeof(NodeGeneric) },
        { "CheckCompleteCollectBgm", typeof(NodeGeneric) },
        { "CheckCostume", typeof(NodeGeneric) },
        { "CheckCostumeInvisible", typeof(NodeGeneric) },
        { "CheckCostumeMissMatchPart", typeof(NodeGeneric) },
        { "CheckCostumePair", typeof(NodeGeneric) },
        { "CheckCount", typeof(NodeGeneric) },
        { "CheckFirstTalkCollectBgm", typeof(NodeGeneric) },
        { "CheckFlag", typeof(NodeGeneric) },
        { "CheckGetLinkShine", typeof(NodeGeneric) },
        { "CheckLifeUpItem", typeof(NodeGeneric) },
        { "CheckLink", typeof(NodeGeneric) },
        { "CheckMoonLockOpened", typeof(NodeGeneric) },
        { "CheckOpenDoorSnow", typeof(NodeGeneric) },
        { "CheckPlayingCollectBgm", typeof(NodeGeneric) },
        { "CheckPlayerOnGround", typeof(NodeGeneric) },
        { "CheckYukimaruRaceResult", typeof(NodeGeneric) },
        { "CloseTalkMessage", typeof(NodeGeneric) },
        { "CloseTalkMessageNoSe", typeof(NodeGeneric) }, // v1.2.0+
        { "CoinPayment", typeof(NodeGeneric) },
        { "CutSceneDemoStart", typeof(NodeGeneric) },
        { "DemoEnd", typeof(NodeGeneric) },
        { "DemoForceStartOnGround", typeof(NodeGeneric) },
        { "DemoForceStart", typeof(NodeGeneric) },
        { "DemoResetPlayerDynamics", typeof(NodeGeneric) },
        { "DemoStart", typeof(NodeGeneric) },
        { "DirectGetLinkShine", typeof(NodeGeneric) },
        { "EnableHint", typeof(NodeGeneric) },
        { "FirstTalkEndCollectBgmNpc", typeof(NodeGeneric) },
        { "ForcePutOnDemoCap", typeof(NodeGeneric) },
        { "GetAmiiboCostume", typeof(NodeGeneric) },
        { "GetAmiiboNotSearchHintNum", typeof(NodeGeneric) },
        { "GetCollectBgmBonus01", typeof(NodeGeneric) },
        { "GetCollectBgmBonus02", typeof(NodeGeneric) },
        { "GetSearchAmiibo", typeof(NodeGeneric) },
        { "IsCostumeAmiibo", typeof(NodeGeneric) },
        { "IsEnableSearchAmiibo", typeof(NodeGeneric) },
        { "IsTalkAmiiboHelp", typeof(NodeGeneric) },
        { "KakkuTurn", typeof(NodeGeneric) },
        { "MessageBalloon", typeof(NodeGeneric) },
        { "MessageTalk", typeof(NodeGeneric) },
        { "MessageTalkSpecialPurpose", typeof(NodeGeneric) },
        { "NextTalkMessage", typeof(NodeGeneric) },
        { "NormalDemoTryStart", typeof(NodeGeneric) },
        { "NpcMoveToLink", typeof(NodeGeneric) },
        { "OpenBgmList", typeof(NodeGeneric) },
        { "PlayerAction", typeof(NodeGeneric) },
        { "PlayerTurn", typeof(NodeGeneric) },
        { "PopItem", typeof(NodeGeneric) },
        { "PopLinkShine", typeof(NodeGeneric) },
        { "ReplacePlayer", typeof(NodeGeneric) },
        { "SceneWipeClose", typeof(NodeGeneric) },
        { "SelectChoice", typeof(NodeGeneric) },
        { "SelectYesNo", typeof(NodeGeneric) },
        { "SessionWaitMusician", typeof(NodeGeneric) },
        { "SetDemoInfoDemoName", typeof(NodeGeneric) },
        { "UnlockHint", typeof(NodeGeneric) },
        { "VrGyroReset", typeof(NodeGeneric) }, // v1.3.0+
        { "WaitWipeOpenEnd", typeof(NodeGeneric) },
        { "WaitSimple", typeof(NodeGeneric) },
        { "WipeFadeBlackClose", typeof(NodeGeneric) },
        { "WipeFadeBlackOpen", typeof(NodeGeneric) },
    };

    public override Node CreateNode(Dictionary<object, object> dict)
    {
        // Setup a string to access into the factory table
        string nType = GetNodeType(dict);
        nType = nType.Replace("EventFlowNode", "");

        // Ensure this string exists in the factory table
        if (!FactoryEntries.TryGetValue(nType, out Type factoryType))
            return new NodeGeneric(dict);

        object n = factoryType.GetConstructor([typeof(Dictionary<object, object>)]).Invoke([dict]);
        return (Node)n;
    }
}
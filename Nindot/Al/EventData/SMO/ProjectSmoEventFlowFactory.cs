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
        { "HitReaction", typeof(NodeHitReaction) }, // Play a hit reaction
        { "Join", typeof(NodeJoin) }, // Merge a branch back together? Not sure
        { "JumpEntry", typeof(NodeJumpEntry) }, // End current event and change next entry point
        { "QueryJudge", typeof(NodeJudgeQuery) }, // Branch based on actor judge function
        { "SwitchOff", typeof(NodeSwitchOff) }, // Disable a StageSwitch
        { "SwitchOn", typeof(NodeSwitchOn) }, // Enable a StageSwitch
        { "TurnToPlayer", typeof(NodeGeneric) }, // Rotate actor to face towards player
        { "TurnToPlayerActionOneTime", typeof(NodeTurnToPlayerActionOneTime) }, // Rotate actor to face towards player with action
        { "TurnToPreDir", typeof(NodeGeneric) }, // Rotate actor?

        // ProjectEventFlowNodeFactory (not al namespace)
        { "AmiiboTouchLayout", typeof(NodeAmiiboTouchLayout) }, // 5-way branch for scanning amiibo
        { "AppearMapAmiiboHint", typeof(NodeGeneric) }, // Display map screen to showcase new amiibo hints
        { "BgmCtrl", typeof(NodeBgmCtrl) }, // Control music playback
        { "BindKeepDemoStart", typeof(NodeGenericQuery) }, // Something with demos and binds, not fully understood
        { "CapMessage", typeof(NodeCapMessage) }, // Display a cappy message from SystemMessage/CapMessage.msbt
        { "CapManHeroTalkSetDemoStartPose", typeof(NodeGeneric) }, // Setup demo cappy's start pose
        { "CapManHeroTalkAppear", typeof(NodeCapManHeroTalkAppear) }, // Play appear animation for demo cappy
        { "CapManHeroTalkFocus", typeof(NodeCapManHeroTalkFocus) }, // Define focus point by link from demo cappy
        { "CapManHeroTalkPlayerTurn", typeof(NodeCapManHeroTalkPlayerTurn) }, // Turn player towards focus point link
        { "CapManHeroTalkReturn", typeof(NodeGeneric) }, // Return demo cappy to Mario's head with no parameters
        { "CapManHeroTalkSetDemoEndPose", typeof(NodeCapManHeroTalkSetDemoEndPose) }, // Set demo cappy to his ending pose
        { "ChangeStage", typeof(NodeChangeStage) }, // Load new stage
        { "ChangeWorldDemoMessage", typeof(NodeGeneric) }, // Display cappy's tips and dialouge for transitioning kingdoms
        { "CheckClear3CollectBgm", typeof(NodeGenericQuery) }, // Check if BGM "EndRockSpecial" is collected
        { "CheckCompleteCollectBgm", typeof(NodeGenericQuery) }, // Check if all unlockable BGMs are collected
        { "CheckCostume", typeof(NodeCheckCostume) }, // Check if player is using specific costume or costume pattern
        { "CheckCostumeInvisible", typeof(NodeGenericQuery) }, // Is the player using the invisible cap?
        { "CheckCostumeMissMatchPart", typeof(NodeCheckCostumeMissMatchPart) }, // Is the player using miss-matching outfit parts?
        { "CheckCostumePair", typeof(NodeCheckCostumePair) }, // Compare costume to pattern
        { "CheckCount", typeof(NodeCheckCount) }, // Check "Shine", "Coin", and "BossDeathCount" against some target value
        { "CheckFirstTalkCollectBgm", typeof(NodeGenericQuery) }, // Has the player spoken to the collect BGM npc before
        { "CheckFlag", typeof(NodeCheckFlag) }, // Check if one of five specific save data flags have been set
        { "CheckGetLinkShine", typeof(NodeCheckGetLinkShine) }, // Check if the shine on the end of a link has been collected
        { "CheckLifeUpItem", typeof(NodeGenericQuery) }, // Check if the player has a life-up heart
        { "CheckLink", typeof(NodeCheckLink) }, // Check if a "ShineActor" link exists
        { "CheckMoonLockOpened", typeof(NodeGenericQuery) }, // Is the moon rock opened for the current world?
        { "CheckOpenDoorSnow", typeof(NodeCheckOpenDoorSnow) }, // Five-way branch depending on number of open doors
        { "CheckPlayingCollectBgm", typeof(NodeCheckPlayingCollectBgm) }, // Three-way branch if playing requested bgm
        { "CheckPlayerOnGround", typeof(NodeGenericQuery) }, // Is player grounded?
        { "CheckYukimaruRaceResult", typeof(NodeGenericQuery) }, // Check bound-bowl result
        { "CloseTalkMessage", typeof(NodeGeneric) }, // Close dialouge balloon
        /* ! v1.2.0+ ! */ { "CloseTalkMessageNoSe", typeof(NodeGeneric) }, // Close dialouge balloon without sound effects
        { "CoinPayment", typeof(NodeCoinPayment) }, // Subtract CoinNum amount of coins
        { "CutSceneDemoStart", typeof(NodeGeneric) }, // Start demo
        { "DemoEnd", typeof(NodeGeneric) }, // End demo
        { "DemoForceStartOnGround", typeof(NodeDemoForceStart) }, // Force start demo on ground
        { "DemoForceStart", typeof(NodeDemoForceStart) }, // Force start demo
        { "DemoResetPlayerDynamics", typeof(NodeDemoResetPlayerDynamics) }, // Reset player IK and dynamics after DelayStep 
        { "DemoStart", typeof(NodeDemoStart) }, // Start a demo
        { "DirectGetLinkShine", typeof(NodeDirectGetLinkShine) }, // Get directly handed a shine from LinkName
        { "EnableHint", typeof(NodeGeneric) }, // Enable hint for link named "ShineActor", "ShineDummy", or "NoDelete_Shine"
        { "FirstTalkEndCollectBgmNpc", typeof(NodeGeneric) }, // Set GameDataFile flag for talking to collect bgm npc
        { "ForcePutOnDemoCap", typeof(NodeGeneric) }, // Force the demo cappy into put on state
        { "GetAmiiboCostume", typeof(NodeGetAmiiboCostume) }, // Accquire a costume by amiibo, and optionally equip it
        /* Incorrect type! */ { "GetAmiiboNotSearchHintNum", typeof(NodeGenericQuery) }, // ???
        { "GetCollectBgmBonus01", typeof(NodeGeneric) }, // Unlock CollectBgm 01
        { "GetCollectBgmBonus02", typeof(NodeGeneric) }, // Unlock CollectBgm 02
        { "GetSearchAmiibo", typeof(NodeGetSearchAmiibo) }, // Four-way branch based on searching amiibo
        { "IsCostumeAmiibo", typeof(NodeGenericQuery) }, // Boolean query for amiibo costume
        { "IsEnableSearchAmiibo", typeof(NodeGenericQuery) }, // Can amiibo search?
        { "IsTalkAmiiboHelp", typeof(NodeGenericQuery) }, // Is talking to amiibo help?
        { "KakkuTurn", typeof(NodeGeneric) }, // Turn glydon
        { "MessageBalloon", typeof(NodeMessageBalloon) }, // Display a balloon over an actor
        { "MessageTalk", typeof(NodeMessageTalk) }, // Enter a talking dialogue balloon
        { "MessageTalkSpecialPurpose", typeof(NodeMessageTalkSpecialPurpose) }, // Special kind of MessageTalk used for peach
        { "NextTalkMessage", typeof(NodeGeneric) }, // Advance to next talk message
        { "NormalDemoTryStart", typeof(NodeGeneric) }, // Attempt to start a "normal demo"?
        { "NpcMoveToLink", typeof(NodeNpcMoveToLink) }, // Move an NPC to a location at LinkName
        { "OpenBgmList", typeof(NodeGeneric) }, // Open BGM menu
        { "PlayerAction", typeof(NodePlayerAction) }, // Play an action on the player
        { "PlayerTurn", typeof(NodePlayerTurn) }, // Turn the player to face the EventFlow's actor
        { "PopItem", typeof(NodePopItem) }, // Pop out an item using ItemTiming
        { "PopLinkShine", typeof(NodePopLinkShine) }, // Pop out a linked shine using LinkName
        { "ReplacePlayer", typeof(NodeGeneric) }, // Replace normal version of player with demo player
        { "SceneWipeClose", typeof(NodeSceneWipeClose) }, // Close a scene with a wipe by SituationName
        { "SelectChoice", typeof(NodeSelectChoice) }, // Dialouge balloon selection between a variable amount of options
        { "SelectYesNo", typeof(NodeSelectYesNo) }, // Two-way SelectChoice, strings must still be set in CaseEventItems
        { "SessionWaitMusician", typeof(NodeSessionWaitMusician) }, // Check if Count amount of musicians are playing
        { "SetDemoInfoDemoName", typeof(NodeSetDemoInfoDemoName) }, // Set demo name
        { "UnlockHint", typeof(NodeGeneric) }, // Unlock a random(?) hint for the current world
        /* ! v1.3.0+ ! */ { "VrGyroReset", typeof(NodeGeneric) }, // ???
        { "WaitWipeOpenEnd", typeof(NodeGeneric) }, // Wait for the wipe to finish opening
        { "WaitSimple", typeof(NodeWaitSimple) }, // Pause the event flow for some number of frames
        { "WipeFadeBlackClose", typeof(NodeWipeFadeBlack) }, // Start the basic fading wipe
        { "WipeFadeBlackOpen", typeof(NodeWipeFadeBlack) }, // End the basic fading wipe
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
    public static Node CreateNode(Graph graph, string name)
    {
        // Setup a string to access into the factory table
        var nType = name.Replace("EventFlowNode", "");

        // Ensure this string exists in the factory table
        if (!FactoryEntries.TryGetValue(nType, out Type factoryType))
            return new NodeGeneric(graph, name);

        object n = factoryType.GetConstructor([typeof(Graph), typeof(string)]).Invoke([graph, name]);
        return (Node)n;
    }
}
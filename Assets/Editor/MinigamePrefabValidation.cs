using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MinigamePrefabValidation
{
    [MenuItem("OldTech/Validate Minigame Prefabs")]
    public static void Validate()
    {
        string[] types = { "Fan", "Wire", "Tape", "Lamp" };
        int[] counts = { 8, 8, 0, 6 };
        for (int i = 0; i < types.Length; i++)
        {
            string path = $"Assets/Prefabs/Minigames/{types[i]}Minigame.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var logic = root.GetComponent<MinigameBase>();
                var view = root.GetComponent<MinigameView>();
                Require(logic && view && view.closeButton && view.progressLabel, "Missing minigame components: " + path);
                Require(view.actionButtons.Length == counts[i], "Wrong button count: " + path);
                if (logic is WireMinigame)
                {
                    var board = root.GetComponentInChildren<WireDragBoard>(true);
                    Require(board && !board.raycastTarget && board.contacts.Length == 8, "Wire drawing layer is invalid.");
                    for (int contactIndex = 0; contactIndex < 8; contactIndex++)
                    {
                        var contact = board.contacts[contactIndex] ? board.contacts[contactIndex].GetComponent<WireContactDrag>() : null;
                        Require(contact && contact.board == board && contact.index == contactIndex, "Wire contact drag reference is invalid.");
                    }
                }
                if (logic is TapeMinigame)
                {
                    var reel = root.GetComponentInChildren<TapeReelDrag>(true);
                    Require(reel && reel.rotor, "Tape drag control or rotor is missing.");
                    Require(reel.GetComponent<UnityEngine.UI.Image>().raycastTarget, "Tape drag surface must receive input.");
                }
                foreach (var button in view.actionButtons) Require(button && button.targetGraphic, "Missing action control: " + path);
                Require(root.GetComponent<Image>().raycastTarget, "Modal must block clicks: " + path);
                for (int round = 0; round < 64; round++) ValidateRound(logic);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            var zone = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/BreakdownZones/{types[i]}Zone.prefab");
            var point = zone ? zone.GetComponent<BreakdownPoint>() : null;
            Require(point && point.minigamePrefab && point.stateImage && point.stateLabel, "Zone has missing references.");
            Require(point.Type == (BreakdownType)i, "Zone points to wrong repair.");
        }
        var a = PrefabUtility.LoadPrefabContents("Assets/Prefabs/BreakdownZones/FanZone.prefab");
        var b = PrefabUtility.LoadPrefabContents("Assets/Prefabs/BreakdownZones/FanZone.prefab");
        var holder = new GameObject("Validation only");
        try
        {
            var manager = holder.AddComponent<BreakdownManager>();
            var first = a.GetComponent<BreakdownPoint>(); var second = b.GetComponent<BreakdownPoint>();
            manager.Register(first); manager.Register(second);
            first.SetBroken(true); second.SetBroken(true);
            Require(manager.ActiveCount == 2, "Same-type zones must count independently.");
            manager.Repair(first);
            Require(!first.IsBroken && second.IsBroken && manager.ActiveCount == 1, "Repair affected another zone.");
            manager.ResetBreakdowns(); Require(!second.IsBroken && manager.ActiveCount == 0, "Restart failed to reset zones.");
            manager.Unregister(first); manager.Unregister(second);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(holder);
            PrefabUtility.UnloadPrefabContents(a); PrefabUtility.UnloadPrefabContents(b);
        }
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/MainGame.unity");
        try
        {
            int zoneCount = 0; bool controllerLinked = false; bool roomCanvas = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                zoneCount += root.GetComponentsInChildren<BreakdownPoint>(true).Length;
                var controller = root.GetComponentInChildren<MinigameController>(true);
                if (controller) controllerLinked = controller.minigameLayer;
                if (root.name == "Room") roomCanvas = root.GetComponent<Canvas>();
            }
            Require(zoneCount > 0 && controllerLinked && roomCanvas, "MainGame scene references are invalid.");
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
        Debug.Log("Minigame prefab validation passed.");
    }
    static void ValidateRound(MinigameBase logic)
    {
        logic.Begin();
        Require(logic.Completed == 0 && logic.Target > 0, "Invalid new round.");
        Require(!logic.Act(-1), "Invalid action was accepted.");
        if (logic is WireMinigame wires)
        {
            var left = new System.Collections.Generic.HashSet<int>();
            var right = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < 4; i++) { left.Add(wires.WireAt(i)); right.Add(wires.WireAt(i + 4)); }
            Require(left.SetEquals(new[] { 0, 1, 2, 3 }) && left.SetEquals(right), "Wire permutation is invalid.");
            int wrong = 4;
            while (wires.WireAt(wrong) == wires.WireAt(0)) wrong++;
            wires.Connect(0, wrong);
            Require(wires.Completed == 0, "Wrong wire pair was accepted.");
            Require(!wires.Connect(0, 0) && !wires.Connect(0, 1) && !wires.Connect(0, -1), "Invalid drop was accepted.");
            for (int i = 0; i < 4; i++)
            {
                int match = 4;
                while (wires.WireAt(match) != wires.WireAt(i)) match++;
                Require(!wires.Act(i) && !wires.Act(match), "Clicking must not connect wires.");
                Require(wires.Connect(i, match), "Matching wire drag failed.");
                Require(wires.Connected(i) && wires.Connected(match), "Connected contacts are inconsistent.");
                Require(!wires.Connect(i, match), "Connected pair was counted twice.");
            }
        }
        else if (logic is FanMinigame fan)
        {
            int count = 0;
            for (int i = 0; i < 8; i++)
                if (fan.HasBug(i)) { count++; Require(fan.Act(i) && !fan.Act(i), "Bug action failed."); }
                else Require(!fan.Act(i), "Absent bug was accepted.");
            Require(count == fan.Target, "Bug count differs from target.");
        }
        else if (logic is LampMinigame lamps)
        {
            int count = 0;
            for (int i = 0; i < 6; i++)
                if (lamps.NeedsRepair(i)) { count++; Require(lamps.Act(i) && !lamps.Act(i), "Lamp action failed."); }
                else Require(!lamps.Act(i), "Healthy lamp was accepted.");
            Require(count == lamps.Target, "Broken lamp count differs from target.");
        }
        else if (logic is TapeMinigame tape)
        {
            Require(!tape.Act(0), "Clicking must not wind tape.");
            tape.Rotate(-360); Require(tape.RemainingTurns == tape.Target + 1, "Unwinding below zero failed.");
            tape.Rotate(360); Require(tape.Completed == 0, "One turn must cancel one unwound turn.");
            tape.Rotate(180); tape.Rotate(-90);
            Require(Mathf.Abs(tape.WoundTurns - .25f) < .0001f, "Fractional rotation failed.");
            tape.Rotate(-90);
            for (int i = 0; i < tape.Target; i++) Require(tape.Rotate(360), "Tape turn failed.");
            Require(!tape.Rotate(360), "Solved tape accepts more rotation.");
        }
        Require(logic.Solved && !logic.Act(0), "Repair completion failed.");
        logic.Begin(); Require(!logic.Solved && logic.Completed == 0, "Replay did not reset.");
    }
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

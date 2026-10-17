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
        int[] counts = { 8, 8, 1, 6 };
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
                foreach (var button in view.actionButtons) Require(button && button.targetGraphic, "Missing action control: " + path);
                Require(root.GetComponent<Image>().raycastTarget, "Modal must block clicks: " + path);
                logic.Begin();
                Require(logic.Completed == 0, "Begin must reset progress.");
                if (logic is WireMinigame wires)
                {
                    wires.Act(0); wires.Act(5);
                    Require(wires.Completed == 0 && wires.Selected == -1, "Wrong wire pair was accepted.");
                    for (int j = 0; j < 4; j++) { wires.Act(j); Require(wires.Act(j + 4), "Wire pair failed."); }
                }
                else for (int j = 0; j < logic.Target; j++) Require(logic.Act(j), "Action failed: " + path);
                Require(logic.Solved, "Minigame cannot be completed: " + path);
                Require(!logic.Act(0), "Completed repair accepts duplicate action.");
                logic.Begin(); Require(!logic.Solved && logic.Completed == 0, "Replay does not reset.");
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
            Require(zoneCount == 4 && controllerLinked && roomCanvas, "MainGame scene references are invalid.");
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
        Debug.Log("Minigame prefab validation passed.");
    }
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

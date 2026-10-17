using UnityEngine;
using UnityEngine.UI;

// Only wires up existing prefab controls. Never generates or repositions artwork.
public class MinigameView : MonoBehaviour
{
    public Button closeButton;
    [Tooltip("Action order: bug/lamp slots; wires left 0-3 and right 4-7 (identities shuffle); tape uses index 0.")]
    public Button[] actionButtons;
    public Text progressLabel;
    MinigameController controller;
    MinigameBase minigame;
    readonly Color[] wireColors = {
        new Color(.8f, .3f, .2f), new Color(.2f, .5f, .8f),
        new Color(.7f, .6f, .2f), new Color(.4f, .65f, .35f)
    };
    public void Bind(MinigameController owner)
    {
        controller = owner;
        minigame = GetComponent<MinigameBase>();
        if (closeButton) { closeButton.onClick.RemoveListener(Close); closeButton.onClick.AddListener(Close); }
        for (int i = 0; i < actionButtons.Length; i++)
        {
            int index = i;
            if (actionButtons[i] && !(minigame is WireMinigame)) actionButtons[i].onClick.AddListener(() => Act(index));
        }
        if (minigame is WireMinigame)
        {
            var board = GetComponentInChildren<WireDragBoard>(true);
            if (board) board.Bind(owner, this);
        }
        if (minigame is TapeMinigame)
        {
            var reel = GetComponentInChildren<TapeReelDrag>(true);
            if (reel) reel.Bind(owner, this);
        }
        Refresh();
    }
    void Close() { controller.Close(); }
    void Act(int index)
    {
        if (controller.Current != minigame) return;
        controller.Act(index);
        if (controller.Current == minigame) Refresh();
    }
    public void Refresh()
    {
        if (progressLabel) progressLabel.text = $"ГОТОВО: {minigame.Completed} / {minigame.Target}";
        if (progressLabel && minigame is TapeMinigame tape)
            progressLabel.text = $"НАМОТАНО: {tape.WoundTurns:0.0} об. / {tape.Target}\nЗАЛИШИЛОСЯ: {tape.RemainingTurns:0.0} об.";
        for (int i = 0; i < actionButtons.Length; i++)
        {
            var button = actionButtons[i]; if (!button) continue;
            var label = button.GetComponentInChildren<Text>(true);
            if (minigame is WireMinigame wires)
            {
                button.interactable = !wires.Connected(i);
                int wire = wires.WireAt(i);
                if (wire < 0) { button.interactable = false; continue; }
                if (button.image) button.image.color = wireColors[wire];
                if (label) label.text = wires.Connected(i) ? "З'ЄДНАНО" :
                    (i < 4 ? "ЛІВИЙ " : "ПРАВИЙ ") + (wire + 1);
            }
            else if (minigame is FanMinigame fan)
            {
                button.gameObject.SetActive(fan.HasBug(i));
                button.interactable = !fan.Squashed(i);
                if (label) label.text = fan.Squashed(i) ? "ГОТОВО" : "ЖУЧОК\n> : <";
            }
            else if (minigame is LampMinigame lamps)
            {
                button.interactable = lamps.NeedsRepair(i) && !lamps.Replaced(i);
                if (label) label.text = !lamps.NeedsRepair(i) ? "ЛАМПА\n( O )" :
                    lamps.Replaced(i) ? "ЗАМІНЕНО\n( O )" : "ПЕРЕГОРІЛА\n( X )";
            }
        }
    }
    public Color WireColor(int identity) => wireColors[Mathf.Clamp(identity, 0, wireColors.Length - 1)];
}

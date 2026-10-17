using UnityEngine;
using UnityEngine.UI;

// Only wires up existing prefab controls. Never generates or repositions artwork.
public class MinigameView : MonoBehaviour
{
    public Button closeButton;
    [Tooltip("Action order: bugs/lamps by index; wires left 0-3, right 4-7; tape uses index 0.")]
    public Button[] actionButtons;
    public Text progressLabel;
    MinigameController controller;
    MinigameBase minigame;
    readonly System.Collections.Generic.HashSet<int> completed = new System.Collections.Generic.HashSet<int>();
    public void Bind(MinigameController owner)
    {
        controller = owner;
        minigame = GetComponent<MinigameBase>();
        completed.Clear();
        if (closeButton) { closeButton.onClick.RemoveListener(Close); closeButton.onClick.AddListener(Close); }
        for (int i = 0; i < actionButtons.Length; i++)
        {
            int index = i;
            if (actionButtons[i]) actionButtons[i].onClick.AddListener(() => Act(index));
        }
        Refresh();
    }
    void Close() { controller.Close(); }
    void Act(int index)
    {
        if (controller.Current != minigame) return;
        if (controller.Act(index)) completed.Add(index);
        if (controller.Current == minigame) Refresh();
    }
    void Refresh()
    {
        if (progressLabel) progressLabel.text = $"ГОТОВО: {minigame.Completed} / {minigame.Target}";
        if (progressLabel && minigame is WireMinigame selection && selection.Selected >= 0)
            progressLabel.text += $" / ЛІВИЙ {selection.Selected + 1}";
        for (int i = 0; i < actionButtons.Length; i++)
        {
            var button = actionButtons[i]; if (!button) continue;
            if (minigame is WireMinigame wires)
            {
                button.interactable = !wires.Connected(i % 4);
                // Selection marker is separate from the artist's button label.
                var outline = button.GetComponent<Outline>();
                if (outline) outline.enabled = i < 4 && wires.Selected == i;
            }
            else if (!(minigame is TapeMinigame)) button.interactable = !completed.Contains(i);
        }
    }
}

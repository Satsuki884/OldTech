using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Each placed instance owns its damage independently, even when types match.
public class BreakdownPoint : MonoBehaviour, IPointerClickHandler
{
    public MinigameBase minigamePrefab;
    public BreakdownType Type => minigamePrefab ? minigamePrefab.Type : BreakdownType.Fan;
    [Tooltip("Optional prototype visuals. Replace sprites/text or clear these references for custom art.")]
    public Image stateImage;
    public Text stateLabel;
    public bool IsBroken { get; private set; }
    BreakdownManager manager;
    void OnEnable()
    {
        manager = FindFirstObjectByType<BreakdownManager>();
        if (manager) manager.Register(this);
        RefreshVisual();
    }
    void OnDisable()
    {
        if (manager) manager.Unregister(this);
        var controller = FindFirstObjectByType<MinigameController>();
        if (controller && controller.Source == this) controller.Close();
        SetBroken(false);
    }
    public void SetBroken(bool broken) { IsBroken = broken; RefreshVisual(); }
    [ContextMenu("Break Zone (Play Mode)")]
    void BreakForTesting() { if (Application.isPlaying) SetBroken(true); }
    void RefreshVisual()
    {
        if (stateImage) stateImage.color = IsBroken ? new Color(.45f, .15f, .12f) : new Color(.12f, .19f, .20f);
        if (stateLabel) stateLabel.text = IsBroken ? "ПОЛОМКА\n[ РЕМОНТ ]" : "ПРАЦЮЄ";
    }
    public void OnPointerClick(PointerEventData data)
    { if (data.button == PointerEventData.InputButton.Left) Interact(); }
    public void Interact()
    {
        var game = FindFirstObjectByType<GameManager>();
        if (game && !game.Finished && IsBroken) game.Minigames.Open(this);
    }
}

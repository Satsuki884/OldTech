using UnityEngine;
public class MinigameController : MonoBehaviour
{
    [Tooltip("Parent for the instantiated repair window, under UI_Canvas.")]
    public RectTransform minigameLayer;
    public MinigameBase Current { get; private set; }
    public BreakdownPoint Source { get; private set; }
    public void Open(BreakdownPoint point)
    {
        var game = FindFirstObjectByType<GameManager>();
        if (!game || game.Finished || !point || !point.IsBroken || !point.minigamePrefab || !minigameLayer) return;
        Close();
        Source = point;
        Current = Instantiate(point.minigamePrefab, minigameLayer, false);
        Current.Begin();
        Current.gameObject.SetActive(true);
        var view = Current.GetComponent<MinigameView>();
        if (view) view.Bind(this);
    }
    public void Close()
    {
        if (Current) { Current.gameObject.SetActive(false); Destroy(Current.gameObject); }
        Current = null;
        Source = null;
    }
    public bool Act(int index)
    {
        if (!Current || !Source || !Source.isActiveAndEnabled) { Close(); return false; }
        bool changed = Current.Act(index);
        if (changed) FindFirstObjectByType<AudioManager>()?.PlayClick();
        if (Current.Solved)
        {
            FindFirstObjectByType<BreakdownManager>().Repair(Source);
            Close();
        }
        return changed;
    }
}

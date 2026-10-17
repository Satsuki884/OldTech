using UnityEngine;
public abstract class MinigameBase : MonoBehaviour
{
    public abstract BreakdownType Type { get; }
    public int Completed { get; protected set; }
    public abstract int Target { get; }
    public bool Solved => Completed >= Target;
    public virtual void Begin() { Completed = 0; }
    protected static int RandomSubset(bool[] selected, int minimum, int maximum)
    {
        System.Array.Clear(selected, 0, selected.Length);
        int lower = Mathf.Clamp(Mathf.Min(minimum, maximum), 1, selected.Length);
        int upper = Mathf.Clamp(Mathf.Max(minimum, maximum), lower, selected.Length);
        int count = Random.Range(lower, upper + 1);
        int remaining = count;
        // Choose a uniform subset without creating or moving any prefab controls.
        for (int i = 0; i < selected.Length; i++)
            if (Random.Range(0, selected.Length - i) < remaining)
            { selected[i] = true; remaining--; }
        return count;
    }
    public abstract bool Act(int index);
}

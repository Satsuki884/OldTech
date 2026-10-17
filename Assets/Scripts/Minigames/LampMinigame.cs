using UnityEngine;
public class LampMinigame : MinigameBase
{
    [Range(1, 6)] public int minBroken = 1;
    [Range(1, 6)] public int maxBroken = 5;
    readonly bool[] broken = new bool[6];
    readonly bool[] replaced = new bool[6];
    int target;
    public override BreakdownType Type => BreakdownType.Lamp;
    public override int Target => target;
    public bool NeedsRepair(int index) => index >= 0 && index < broken.Length && broken[index];
    public bool Replaced(int index) => NeedsRepair(index) && replaced[index];
    public override void Begin()
    {
        base.Begin();
        target = RandomSubset(broken, minBroken, maxBroken);
        System.Array.Clear(replaced, 0, replaced.Length);
    }
    public override bool Act(int index)
    {
        if (!NeedsRepair(index) || replaced[index]) return false;
        replaced[index] = true; Completed++; return true;
    }
}

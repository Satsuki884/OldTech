using UnityEngine;
public class FanMinigame : MinigameBase
{
    [Range(1, 8)] public int minBugs = 3;
    [Range(1, 8)] public int maxBugs = 8;
    readonly bool[] present = new bool[8];
    readonly bool[] squashed = new bool[8];
    int target;
    public override BreakdownType Type => BreakdownType.Fan;
    public override int Target => target;
    public bool HasBug(int index) => index >= 0 && index < present.Length && present[index];
    public bool Squashed(int index) => HasBug(index) && squashed[index];
    public override void Begin()
    {
        base.Begin();
        target = RandomSubset(present, minBugs, maxBugs);
        System.Array.Clear(squashed, 0, squashed.Length);
    }
    public override bool Act(int index)
    {
        if (!HasBug(index) || squashed[index]) return false;
        squashed[index] = true; Completed++; return true;
    }
}

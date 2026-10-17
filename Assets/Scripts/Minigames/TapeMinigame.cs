using UnityEngine;
public class TapeMinigame : MinigameBase
{
    [Min(1)] public int minTurns = 12;
    [Min(1)] public int maxTurns = 24;
    int target;
    public override BreakdownType Type => BreakdownType.Tape;
    public override int Target => target;
    public override void Begin()
    {
        base.Begin();
        int lower = Mathf.Clamp(Mathf.Min(minTurns, maxTurns), 1, 1000);
        int upper = Mathf.Clamp(Mathf.Max(minTurns, maxTurns), lower, 1000);
        target = Random.Range(lower, upper + 1);
    }
    public override bool Act(int index)
    {
        if (index != 0 || Solved) return false;
        Completed++; return true;
    }
}

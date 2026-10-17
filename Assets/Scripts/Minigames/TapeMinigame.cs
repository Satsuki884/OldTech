using UnityEngine;
public class TapeMinigame : MinigameBase
{
    [Min(1)] public int minTurns = 12;
    [Min(1)] public int maxTurns = 24;
    int target;
    double woundDegrees;
    public float WoundTurns => (float)(woundDegrees / 360.0);
    public float RemainingTurns => Mathf.Max(0, target - WoundTurns);
    public override BreakdownType Type => BreakdownType.Tape;
    public override int Target => target;
    public override void Begin()
    {
        base.Begin();
        int lower = Mathf.Clamp(Mathf.Min(minTurns, maxTurns), 1, 1000);
        int upper = Mathf.Clamp(Mathf.Max(minTurns, maxTurns), lower, 1000);
        target = Random.Range(lower, upper + 1);
        woundDegrees = 0;
    }
    // Clicking cannot wind tape. Signed angular movement is supplied by TapeReelDrag.
    public override bool Act(int index) => false;
    public bool Rotate(float clockwiseDegrees)
    {
        if (target <= 0 || Solved || float.IsNaN(clockwiseDegrees) || float.IsInfinity(clockwiseDegrees) || clockwiseDegrees == 0) return false;
        woundDegrees = System.Math.Min(target * 360.0, woundDegrees + clockwiseDegrees);
        // Counterclockwise movement can go below zero: more tape must then be wound.
        Completed = Mathf.Max(0, (int)System.Math.Floor(woundDegrees / 360.0 + .000001));
        return true;
    }
}

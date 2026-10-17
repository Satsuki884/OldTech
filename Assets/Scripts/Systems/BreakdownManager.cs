using System.Collections.Generic;
using UnityEngine;
public class BreakdownManager : MonoBehaviour
{
    readonly List<BreakdownPoint> points = new List<BreakdownPoint>();
    readonly List<BreakdownPoint> candidates = new List<BreakdownPoint>();
    float elapsed;
    bool first = true;
    public int ActiveCount
    { get { int count = 0; foreach (var point in points) if (point && point.IsBroken) count++; return count; } }
    public void Register(BreakdownPoint point) { if (!points.Contains(point)) points.Add(point); }
    public void Unregister(BreakdownPoint point) { points.Remove(point); }
    public void Tick(float delta, GameBalanceConfig config)
    {
        elapsed += delta;
        if (elapsed < (first ? config.firstBreakdownSeconds : config.breakdownIntervalSeconds)) return;
        elapsed = 0; first = false;
        candidates.Clear();
        foreach (var point in points)
            if (point && point.isActiveAndEnabled && !point.IsBroken && point.minigamePrefab)
                candidates.Add(point);
        if (candidates.Count > 0) candidates[Random.Range(0, candidates.Count)].SetBroken(true);
    }
    public void Repair(BreakdownPoint point) { if (point) point.SetBroken(false); }
    public void ResetBreakdowns()
    {
        foreach (var point in points) if (point) point.SetBroken(false);
        elapsed = 0; first = true;
    }
}

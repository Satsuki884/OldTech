using UnityEngine;
public class EfficiencyManager : MonoBehaviour
{
    public float Value { get; private set; } = 1;
    public void Recalculate(int count, GameBalanceConfig config)
    { Value = Mathf.Clamp01(1 - count * config.efficiencyLossPerBreakdown); }
}

using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "OldTech/Game Balance Config")]
public class GameBalanceConfig : ScriptableObject
{
    [Min(30)] public float deadlineSeconds = 180;
    [Min(10)] public float projectWorkSeconds = 100;
    [Range(0, 1)] public float efficiencyLossPerBreakdown = .28f;
    [Range(0, 1)] public float stopThreshold = .25f;
    [Min(1)] public float firstBreakdownSeconds = 4;
    [Min(1)] public float breakdownIntervalSeconds = 12;
}

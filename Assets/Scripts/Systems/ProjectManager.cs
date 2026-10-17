using UnityEngine;
public class ProjectManager : MonoBehaviour
{
    public float Progress { get; private set; }
    public void Tick(float delta, float efficiency, GameBalanceConfig config)
    {
        if (efficiency >= config.stopThreshold)
            Progress = Mathf.Clamp01(Progress + delta * efficiency / Mathf.Max(1, config.projectWorkSeconds));
    }
    public void ResetProgress() { Progress = 0; }
}

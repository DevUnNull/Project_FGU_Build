using UnityEngine;

public class ScenarioImpactHandler : MonoBehaviour
{
    public ScenarioPlayer scenarioPlayer;

    void OnEnable()
    {
        if (scenarioPlayer != null)
        {
            scenarioPlayer.onChoiceImpact += HandleImpact;
        }
    }

    void OnDisable()
    {
        if (scenarioPlayer != null)
        {
            scenarioPlayer.onChoiceImpact -= HandleImpact;
        }
    }

    private void HandleImpact(StatImpactType stat, float value)
    {
        // Avoid double-counting since ScenarioPlayer handles direct updates
    }
}

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
        if (StomachDayData.Instance == null) return;

        switch (stat)
        {
            case StatImpactType.AtpRecovery:
                StomachDayData.Instance.atpRecoveryMultiplier += value;
                break;
            case StatImpactType.MucosaHp:
                StomachDayData.Instance.mucosaHpMultiplier += value;
                break;
            case StatImpactType.CellDamage:
                StomachDayData.Instance.cellDamageMultiplier += value;
                break;
            case StatImpactType.CellAttackSpeed:
                StomachDayData.Instance.cellAttackSpeedMultiplier += value;
                break;
            case StatImpactType.GastricAcid:
                StomachDayData.Instance.gastricAcidLevel += value;
                break;
            case StatImpactType.ToxinObstacles:
                StomachDayData.Instance.toxinObstaclesCount += (int)value;
                break;
        }
    }
}

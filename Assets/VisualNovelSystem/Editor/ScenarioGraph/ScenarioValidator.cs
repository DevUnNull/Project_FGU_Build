using System.Collections.Generic;
using UnityEngine;

public static class ScenarioValidator
{
    public static void Validate(ScenarioData data)
    {
        if (data == null) return;
        
        Debug.Log("--- Scenario Validation Started ---");
        
        bool hasErrors = false;
        
        var guidSet = new HashSet<string>();
        var idSet = new HashSet<string>();

        // Check for duplicate GUIDs and Situation IDs
        foreach (var sit in data.situations)
        {
            if (string.IsNullOrEmpty(sit.guid))
            {
                Debug.LogError($"[Validation] Situation '{sit.situationId}' is missing a GUID.");
                hasErrors = true;
            }
            else
            {
                if (guidSet.Contains(sit.guid))
                {
                    Debug.LogError($"[Validation] Duplicate GUID found: {sit.guid}");
                    hasErrors = true;
                }
                guidSet.Add(sit.guid);
            }

            if (string.IsNullOrEmpty(sit.situationId))
            {
                Debug.LogWarning($"[Validation] A Situation is missing a human-readable Situation ID.");
            }
            else
            {
                if (idSet.Contains(sit.situationId))
                {
                    Debug.LogWarning($"[Validation] Duplicate Situation ID found: {sit.situationId}");
                }
                idSet.Add(sit.situationId);
            }
        }
        
        // Check for missing entry
        if (string.IsNullOrEmpty(data.entryGuid))
        {
            Debug.LogError("[Validation] Scenario has no Entry GUID set.");
            hasErrors = true;
        }
        else if (!guidSet.Contains(data.entryGuid))
        {
            Debug.LogError($"[Validation] Entry GUID '{data.entryGuid}' points to a missing/invalid Situation.");
            hasErrors = true;
        }

        // Check for invalid targets and orphans
        var reachableGuids = new HashSet<string>();
        if (!string.IsNullOrEmpty(data.entryGuid))
        {
            reachableGuids.Add(data.entryGuid);
        }

        foreach (var sit in data.situations)
        {
            foreach (var choice in sit.choices)
            {
                if (string.IsNullOrEmpty(choice.targetGuid))
                {
                    Debug.LogWarning($"[Validation] Situation '{sit.situationId}' has a Choice ('{choice.choiceText}') with no target GUID (Linear fallback or End).");
                }
                else if (!guidSet.Contains(choice.targetGuid))
                {
                    Debug.LogError($"[Validation] Situation '{sit.situationId}', Choice '{choice.choiceText}' points to an INVALID target GUID: {choice.targetGuid}");
                    hasErrors = true;
                }
                else
                {
                    reachableGuids.Add(choice.targetGuid);
                }
            }
        }

        foreach (var sit in data.situations)
        {
            if (!reachableGuids.Contains(sit.guid))
            {
                Debug.LogWarning($"[Validation] Orphan Node Detected: Situation '{sit.situationId}' is unreachable from the Entry node and other choices.");
            }
        }

        if (!hasErrors)
        {
            Debug.Log("--- Scenario Validation Passed Successfully ---");
        }
        else
        {
            Debug.LogError("--- Scenario Validation Failed with Errors ---");
        }
    }
}

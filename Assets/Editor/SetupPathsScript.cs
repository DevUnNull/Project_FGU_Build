using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SetupPathsScript
{
    [MenuItem("Tools/Setup 5 PvZ Paths")]
    public static void Setup()
    {
        string configDir = "Assets/_Scrip/Enemies/Config/Resources/";
        
        for (int i = 3; i <= 5; i++)
        {
            string wavePath = $"{configDir}WaveConfig{i}_1.asset";
            if (!AssetDatabase.LoadAssetAtPath<WaveConfig>(wavePath))
            {
                AssetDatabase.CopyAsset($"{configDir}WaveConfig2_1.asset", wavePath);
            }
            WaveConfig wc = AssetDatabase.LoadAssetAtPath<WaveConfig>(wavePath);
            wc.pathID = (PathID)(i - 1);
            EditorUtility.SetDirty(wc);

            string tunePath = $"{configDir}TuneConfigMap1_{i}.asset";
            if (!AssetDatabase.LoadAssetAtPath<TuneConfig>(tunePath))
            {
                AssetDatabase.CopyAsset($"{configDir}TuneConfigMap1_2.asset", tunePath);
            }
            TuneConfig tc = AssetDatabase.LoadAssetAtPath<TuneConfig>(tunePath);
            tc.waves = new List<WaveConfig> { wc };
            EditorUtility.SetDirty(tc);
        }
        AssetDatabase.SaveAssets();

        GameObject path2 = GameObject.Find("Path2");
        GameObject waySpawn2 = GameObject.Find("waySpawn2");
        
        if (path2 == null || waySpawn2 == null)
        {
            Debug.LogError("Path2 or waySpawn2 not found!");
            return;
        }

        for (int i = 3; i <= 5; i++)
        {
            GameObject p = GameObject.Find("Path" + i);
            if (p != null) GameObject.DestroyImmediate(p);
            GameObject ws = GameObject.Find("waySpawn" + i);
            if (ws != null) GameObject.DestroyImmediate(ws);
        }

        for (int i = 3; i <= 5; i++)
        {
            float yOffset = -1.55f * (i - 2);

            GameObject newPath = GameObject.Instantiate(path2, path2.transform.parent);
            newPath.name = "Path" + i;
            newPath.transform.position = path2.transform.position + new Vector3(0, yOffset, 0);

            GameObject newWaySpawn = GameObject.Instantiate(waySpawn2, waySpawn2.transform.parent);
            newWaySpawn.name = "waySpawn" + i;
            newWaySpawn.transform.position = waySpawn2.transform.position + new Vector3(0, yOffset, 0);

            WaveSpawner wsComp = newWaySpawn.GetComponent<WaveSpawner>();
            wsComp.spawnPoint = newWaySpawn.transform;
            wsComp.assignedPath = newPath.GetComponent<WaypointPath>();
            wsComp.myPathID = (PathID)(i - 1);
            EditorUtility.SetDirty(wsComp);
        }

        WaveManager wm = GameObject.FindObjectOfType<WaveManager>();
        if (wm != null)
        {
            SerializedObject so = new SerializedObject(wm);
            SerializedProperty tunesProp = so.FindProperty("tunes");
            tunesProp.arraySize = 5;
            for (int i = 0; i < 5; i++)
            {
                TuneConfig tc = AssetDatabase.LoadAssetAtPath<TuneConfig>($"{configDir}TuneConfigMap1_{i+1}.asset");
                tunesProp.GetArrayElementAtIndex(i).objectReferenceValue = tc;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(wm);
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("5 Paths Setup Completed!");
    }
}

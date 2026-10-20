using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class ScenarioGraphWindow : EditorWindow
{
    private ScenarioGraphView _graphView;
    private ScenarioData _currentScenario;

    public static void OpenGraphWindow(ScenarioData data)
    {
        var window = GetWindow<ScenarioGraphWindow>("Scenario Graph");
        window.Initialize(data);
    }

    private void Initialize(ScenarioData data)
    {
        _currentScenario = data;
        ConstructGraphView();
        GenerateToolbar();
    }

    private void ConstructGraphView()
    {
        if (_graphView != null)
        {
            rootVisualElement.Remove(_graphView);
        }

        _graphView = new ScenarioGraphView
        {
            name = "Scenario Graph"
        };
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);

        if (_currentScenario != null)
        {
            _graphView.PopulateView(_currentScenario);
        }
    }

    private void GenerateToolbar()
    {
        var toolbar = new UnityEditor.UIElements.Toolbar();

        var saveButton = new Button(() => 
        {
            if (_currentScenario != null)
            {
                EditorUtility.SetDirty(_currentScenario);
                AssetDatabase.SaveAssets();
                Debug.Log("Scenario Graph Saved.");
            }
        })
        {
            text = "Save Data"
        };

        var validateButton = new Button(() =>
        {
            if (_currentScenario != null)
            {
                ScenarioValidator.Validate(_currentScenario);
            }
        })
        {
            text = "Validate Graph"
        };

        toolbar.Add(saveButton);
        toolbar.Add(validateButton);

        rootVisualElement.Add(toolbar);
    }

    private void OnDisable()
    {
        if (_graphView != null)
        {
            rootVisualElement.Remove(_graphView);
        }
    }
}

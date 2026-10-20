using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class ScenarioGraphView : GraphView
{
    private ScenarioData _currentScenario;

    public ScenarioGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }

    public void PopulateView(ScenarioData data)
    {
        _currentScenario = data;
        
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if (data.situations == null) return;

        // Create Nodes
        var nodeMap = new Dictionary<string, SituationNode>();
        foreach (var sit in data.situations)
        {
            var node = CreateSituationNode(sit);
            nodeMap[sit.guid] = node;
            AddElement(node);
        }

        // Create START Node
        var startNode = CreateStartNode();
        nodeMap["START"] = startNode;
        AddElement(startNode);

        // Create END Node
        var endNode = CreateEndNode();
        nodeMap["END"] = endNode;
        AddElement(endNode);

        // Create Edges
        foreach (var sit in data.situations)
        {
            if (!nodeMap.ContainsKey(sit.guid)) continue;
            var parentNode = nodeMap[sit.guid];
            var outputs = parentNode.outputContainer.Query<Port>().ToList();
            
            if (sit.choices.Count > 0)
            {
                for (int i = 0; i < sit.choices.Count; i++)
                {
                    var choice = sit.choices[i];
                    string target = choice.targetType == TargetType.EndScene ? "END" : choice.targetGuid;
                    if (string.IsNullOrEmpty(target) || !nodeMap.ContainsKey(target)) continue;

                    var childNode = nodeMap[target];
                    var childInput = childNode.inputContainer.Query<Port>().First();
                    if (i < outputs.Count)
                    {
                        var edge = outputs[i].ConnectTo(childInput);
                        AddElement(edge);
                    }
                }
            }
            else
            {
                // Auto Transition
                string target = sit.autoTargetType == TargetType.EndScene ? "END" : sit.autoTargetGuid;
                if (!string.IsNullOrEmpty(target) && nodeMap.ContainsKey(target) && outputs.Count > 0)
                {
                    var childNode = nodeMap[target];
                    var childInput = childNode.inputContainer.Query<Port>().First();
                    var edge = outputs[0].ConnectTo(childInput);
                    AddElement(edge);
                }
            }
        }

        // Create Edge from START to entryGuid
        if (!string.IsNullOrEmpty(data.entryGuid) && nodeMap.ContainsKey(data.entryGuid))
        {
            var targetNode = nodeMap[data.entryGuid];
            var startOutput = startNode.outputContainer.Query<Port>().First();
            var targetInput = targetNode.inputContainer.Query<Port>().First();
            var edge = startOutput.ConnectTo(targetInput);
            AddElement(edge);
        }
    }

    private SituationNode CreateSituationNode(SituationData sit)
    {
        var node = new SituationNode
        {
            title = sit.situationId + "\n(" + sit.timeText + ")",
            GUID = sit.guid,
            SituationId = sit.situationId,
            IsEntryNode = _currentScenario.entryGuid == sit.guid
        };

        node.SetPosition(new Rect(sit.editorPosition, new Vector2(200, 150)));

        // Input port
        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        // Output ports (one for each choice)
        if (sit.choices.Count > 0)
        {
            for (int i = 0; i < sit.choices.Count; i++)
            {
                var choice = sit.choices[i];
                var outputPort = GeneratePort(node, Direction.Output, Port.Capacity.Single);
                outputPort.portName = choice.choiceText;
                node.outputContainer.Add(outputPort);
            }
        }
        else
        {
            // Auto transition port
            var outputPort = GeneratePort(node, Direction.Output, Port.Capacity.Single);
            outputPort.portName = $"Auto ({sit.autoTransitionDelay}s)";
            node.outputContainer.Add(outputPort);
        }

        node.RefreshExpandedState();
        node.RefreshPorts();

        return node;
    }

    private SituationNode CreateStartNode()
    {
        var node = new SituationNode
        {
            title = "ENTRY / START",
            GUID = "START",
            SituationId = "START"
        };

        float minX = -300;
        float minY = 200;
        if (_currentScenario.situations.Count > 0)
        {
            minX = _currentScenario.situations.Min(s => s.editorPosition.x) - 300;
            minY = _currentScenario.situations.Min(s => s.editorPosition.y);
        }
        node.SetPosition(new Rect(new Vector2(minX, minY), new Vector2(150, 100)));

        node.titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));

        var outputPort = GeneratePort(node, Direction.Output, Port.Capacity.Single);
        outputPort.portName = "Start Day";
        node.outputContainer.Add(outputPort);
        
        node.capabilities &= ~Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        return node;
    }

    private SituationNode CreateEndNode()
    {
        var node = new SituationNode
        {
            title = "EXIT / END DAY",
            GUID = "END",
            SituationId = "END"
        };

        // Tìm vị trí hợp lý cho End Node (Bên phải cùng)
        float maxX = 500;
        float maxY = 200;
        if (_currentScenario.situations.Count > 0)
        {
            maxX = _currentScenario.situations.Max(s => s.editorPosition.x) + 300;
            maxY = _currentScenario.situations.Max(s => s.editorPosition.y);
        }
        node.SetPosition(new Rect(new Vector2(maxX, maxY), new Vector2(150, 100)));

        node.titleContainer.style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f));

        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "End Day";
        node.inputContainer.Add(inputPort);
        
        node.capabilities &= ~Capabilities.Deletable; // Không cho phép xoá node này

        node.RefreshExpandedState();
        node.RefreshPorts();

        return node;
    }

    private Port GeneratePort(SituationNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); // Type doesn't matter for logic flow
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        bool isDirty = false;

        // Elements moved
        if (graphViewChange.movedElements != null)
        {
            Undo.RecordObject(_currentScenario, "Move Node");
            foreach (var element in graphViewChange.movedElements)
            {
                if (element is SituationNode node)
                {
                    var sit = _currentScenario.situations.FirstOrDefault(s => s.guid == node.GUID);
                    if (sit != null)
                    {
                        sit.editorPosition = node.GetPosition().position;
                        isDirty = true;
                    }
                }
            }
        }

        // Elements deleted
        if (graphViewChange.elementsToRemove != null)
        {
            Undo.RecordObject(_currentScenario, "Delete Edge");
            foreach (var element in graphViewChange.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    var parentNode = edge.output.node as SituationNode;
                    var childNode = edge.input.node as SituationNode;
                    if (parentNode != null && childNode != null)
                    {
                        if (parentNode.GUID == "START")
                        {
                            _currentScenario.entryGuid = "";
                            isDirty = true;
                        }
                        else
                        {
                            int choiceIndex = parentNode.outputContainer.IndexOf(edge.output);
                            var sit = _currentScenario.situations.FirstOrDefault(s => s.guid == parentNode.GUID);
                            if (sit != null)
                            {
                                if (sit.choices.Count > 0 && choiceIndex >= 0 && choiceIndex < sit.choices.Count)
                                {
                                    sit.choices[choiceIndex].targetGuid = "";
                                    sit.choices[choiceIndex].targetType = TargetType.Situation;
                                    isDirty = true;
                                }
                                else if (sit.choices.Count == 0 && choiceIndex == 0)
                                {
                                    sit.autoTargetGuid = "";
                                    sit.autoTargetType = TargetType.Situation;
                                    isDirty = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Elements added (Edges)
        if (graphViewChange.edgesToCreate != null)
        {
            Undo.RecordObject(_currentScenario, "Create Edge");
            foreach (var edge in graphViewChange.edgesToCreate)
            {
                var parentNode = edge.output.node as SituationNode;
                var childNode = edge.input.node as SituationNode;
                if (parentNode != null && childNode != null)
                {
                    if (parentNode.GUID == "START")
                    {
                        _currentScenario.entryGuid = childNode.GUID;
                        isDirty = true;
                    }
                    else
                    {
                        TargetType tType = childNode.GUID == "END" ? TargetType.EndScene : TargetType.Situation;
                        string tGuid = childNode.GUID == "END" ? "" : childNode.GUID;

                        int choiceIndex = parentNode.outputContainer.IndexOf(edge.output);
                        var sit = _currentScenario.situations.FirstOrDefault(s => s.guid == parentNode.GUID);
                        if (sit != null)
                        {
                            if (sit.choices.Count > 0 && choiceIndex >= 0 && choiceIndex < sit.choices.Count)
                            {
                                sit.choices[choiceIndex].targetGuid = tGuid;
                                sit.choices[choiceIndex].targetType = tType;
                                isDirty = true;
                            }
                            else if (sit.choices.Count == 0 && choiceIndex == 0)
                            {
                                sit.autoTargetGuid = tGuid;
                                sit.autoTargetType = tType;
                                isDirty = true;
                            }
                        }
                    }
                }
            }
        }

        if (isDirty)
        {
            EditorUtility.SetDirty(_currentScenario);
        }

        return graphViewChange;
    }
}

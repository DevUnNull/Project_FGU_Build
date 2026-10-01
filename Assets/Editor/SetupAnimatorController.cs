using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Editor script tự động nối dây (Transitions) và tạo Parameter "IsAttack" cho Animator Controller của CellB.
/// </summary>
public class SetupAnimatorController
{
    [MenuItem("Tools/Setup CellB Animator Transitions")]
    [InitializeOnLoadMethod]
    public static void SetupCellBAnimator()
    {
        string[] controllerPaths = new string[]
        {
            "Assets/ImageCallOurMyCell/animation/CellB/CellB_.controller",
            "Assets/ImageCallOurMyCell/animation/CellB/CellB.controller"
        };

        foreach (var path in controllerPaths)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) continue;

            // 1. Đảm bảo Parameter "IsAttack" tồn tại trong Animator
            bool hasIsAttackParam = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == "IsAttack")
                {
                    hasIsAttackParam = true;
                    break;
                }
            }

            if (!hasIsAttackParam)
            {
                controller.AddParameter("IsAttack", AnimatorControllerParameterType.Bool);
            }

            // 2. Lấy StateMachine cơ sở của Controller
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            AnimatorState idleState = null;
            AnimatorState attackState = null;

            foreach (var childState in stateMachine.states)
            {
                string stateName = childState.state.name.ToLower();
                if (stateName.Contains("cellb") || stateName.Contains("idle") || stateName.Contains("idel"))
                {
                    idleState = childState.state;
                }
                else if (stateName.Contains("attack") || stateName.Contains("atcak"))
                {
                    attackState = childState.state;
                }
            }

            if (idleState == null && stateMachine.states.Length > 0)
            {
                idleState = stateMachine.defaultState;
            }

            if (idleState == null || attackState == null)
            {
                Debug.LogWarning($"⚠️ Không tìm thấy 2 state idle và attack trong Animator Controller: {path}");
                continue;
            }

            // 3. Đặt state Idle làm Default State (màu cam)
            stateMachine.defaultState = idleState;

            // 4. Tạo transition từ Idle -> Attack khi IsAttack == true
            bool hasIdleToAttack = false;
            foreach (var t in idleState.transitions)
            {
                if (t.destinationState == attackState)
                {
                    hasIdleToAttack = true;
                    break;
                }
            }

            if (!hasIdleToAttack)
            {
                AnimatorStateTransition t = idleState.AddTransition(attackState);
                t.hasExitTime = false;
                t.duration = 0.05f;
                t.AddCondition(AnimatorConditionMode.If, 0, "IsAttack");
            }

            // 5. Tạo transition từ Attack -> Idle khi IsAttack == false
            bool hasAttackToIdle = false;
            foreach (var t in attackState.transitions)
            {
                if (t.destinationState == idleState)
                {
                    hasAttackToIdle = true;
                    break;
                }
            }

            if (!hasAttackToIdle)
            {
                AnimatorStateTransition t = attackState.AddTransition(idleState);
                t.hasExitTime = false;
                t.duration = 0.05f;
                t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttack");
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log($"✅ [Tools] Đã tự động tạo Parameter 'IsAttack' & Nối dây Transitions cho Animator: {path}");
        }
    }
}

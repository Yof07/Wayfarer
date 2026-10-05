using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// 自定义 Inspector 编辑器，用于 UIAnimator 组件
/// 提供“自动生成动画控制器”的功能
/// </summary>
[CustomEditor(typeof(UIAnimator))]
public class UIAnimatorInspector : Editor
{
    // 当前编辑器操作的目标对象
    protected UIAnimator m_target;

    // 当前目标对象的 Animator 组件
    protected Animator m_animator;

    /// <summary>
    /// 将 AnimationClip 添加到 AnimatorController 中，并确保导入
    /// </summary>
    /// <param name="controller">AnimatorController 对象</param>
    /// <param name="clip">要添加的 AnimationClip</param>
    protected virtual void AddClipToController(AnimatorController controller, AnimationClip clip)
    {
        AssetDatabase.AddObjectToAsset(clip, controller); // 将 clip 作为子资源添加到 controller
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip)); // 导入资源，确保 Unity 识别
    }

    /// <summary>
    /// 编辑器启用时调用
    /// 初始化目标对象和 Animator 组件
    /// </summary>
    protected virtual void OnEnable()
    {
        m_target = (UIAnimator)target;           // 转换为 UIAnimator 类型
        m_animator = m_target.GetComponent<Animator>(); // 获取 Animator 组件
    }

    /// <summary>
    /// 自定义 Inspector 绘制逻辑
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 绘制默认的 Inspector 界面

        // 如果 Animator 没有绑定 Runtime Animator Controller
        if (!m_animator.runtimeAnimatorController)
        {
            GUILayout.Space(10); // 在界面增加一些空白

            // 显示按钮“Auto Generate Animation”
            if (GUILayout.Button("Auto Generate Animation"))
            {
                // 弹出保存对话框，让用户选择 AnimatorController 保存路径
                var path = EditorUtility.SaveFilePanelInProject(
                    "New Animation Controller",
                    m_target.gameObject.name,
                    "controller",
                    ""
                );

                if (path.Length == 0) return; // 用户取消保存，直接返回

                // 创建三个 AnimationClip，名称对应 UIAnimator 的触发器
                var normalClip = new AnimationClip() { name = m_target.normalTrigger };
                var showClip = new AnimationClip() { name = m_target.showTrigger };
                var hideClip = new AnimationClip() { name = m_target.hideTrigger };

                // 创建 AnimatorController
                var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

                // 将三个 Clip 添加到 Controller
                AddClipToController(controller, normalClip);
                AddClipToController(controller, showClip);
                AddClipToController(controller, hideClip);

                // 在 Controller 中添加三个触发器参数
                controller.AddParameter(m_target.normalTrigger, AnimatorControllerParameterType.Trigger);
                controller.AddParameter(m_target.showTrigger, AnimatorControllerParameterType.Trigger);
                controller.AddParameter(m_target.hideTrigger, AnimatorControllerParameterType.Trigger);

                // 获取默认层的 StateMachine
                var rootStateMachine = controller.layers[0].stateMachine;

                // 在 StateMachine 中添加三个状态，并绑定对应的 AnimationClip
                var normal = rootStateMachine.AddState(m_target.normalTrigger);
                var show = rootStateMachine.AddState(m_target.showTrigger);
                var hide = rootStateMachine.AddState(m_target.hideTrigger);

                normal.motion = normalClip;
                show.motion = showClip;
                hide.motion = hideClip;

                // 添加 AnyState 到各状态的转换条件（触发器）
                var anyToNormal = rootStateMachine.AddAnyStateTransition(normal);
                anyToNormal.AddCondition(AnimatorConditionMode.If, 0, m_target.normalTrigger);

                var anyToShow = rootStateMachine.AddAnyStateTransition(show);
                anyToShow.AddCondition(AnimatorConditionMode.If, 0, m_target.showTrigger);

                var anyToHide = rootStateMachine.AddAnyStateTransition(hide);
                anyToHide.AddCondition(AnimatorConditionMode.If, 0, m_target.hideTrigger);

                // 设置 Show 状态可以过渡到 Normal 状态（带退出时间）
                var showToNormal = show.AddTransition(normal);
                showToNormal.hasExitTime = true;

                // 将生成的 AnimatorController 绑定到目标 Animator
                m_animator.runtimeAnimatorController = controller;
            }
        }
    }
}

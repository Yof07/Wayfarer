using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 关卡列表组件，用于动态生成关卡卡片并管理焦点选择
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Level List")] // 编辑器菜单路径
public class UILevelList : MonoBehaviour
{
    [Header("设置")]
    public bool focusFirstElement = true; // 是否在列表初始化后自动聚焦第一个元素
    public UILevelCard card;              // UI 关卡卡片预制体
    public RectTransform container;       // 容器，用于存放生成的关卡卡片

    // 内部存储生成的卡片列表
    protected List<UILevelCard> m_cardList = new List<UILevelCard>();

    /// <summary>
    /// 初始化方法
    /// </summary>
    protected virtual void Awake()
    {
        // 获取游戏中的所有关卡数据
        var levels = Game.instance.levels;

        // 遍历关卡数据，生成对应的 UI 卡片并填充数据
        for (int i = 0; i < levels.Count; i++)
        {
            // 实例化卡片并设置为容器的子物体
            m_cardList.Add(Instantiate(card, container));
            // 使用关卡数据填充卡片 UI
            m_cardList[i].Fill(levels[i]);
        }

        // 如果启用自动聚焦，设置第一个卡片的 play 按钮为当前选中对象
        if (focusFirstElement && m_cardList.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(m_cardList[0].play.gameObject);
        }
    }
}

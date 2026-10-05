using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI 存档卡片组件，用于显示和操作单个存档槽的数据
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Save Card")] // 编辑器菜单路径
public class UISaveCard : MonoBehaviour
{
    [Header("下一关卡场景")]
    public string nextScene; // 加载存档后要进入的场景名称

    [Header("文本格式")]
    public string retriesFormat = "00";      // 重试次数文本格式
    public string starsFormat = "00";        // 星星数量文本格式
    public string coinsFormat = "000";       // 金币数量文本格式
    public string dateFormat = "MM/dd/y hh:mm"; // 日期文本格式

    [Header("容器对象")]
    public GameObject dataContainer;  // 存档存在时显示的数据 UI 容器
    public GameObject emptyContainer; // 存档为空时显示的 UI 容器

    [Header("UI 元素")]
    public Text retries;       // 重试次数显示
    public Text stars;         // 星星数量显示
    public Text coins;         // 金币数量显示
    public Text createdAt;     // 存档创建时间显示
    public Text updatedAt;     // 存档最后更新时间显示
    public Button loadButton;  // 加载按钮
    public Button deleteButton;// 删除按钮
    public Button newGameButton; // 新建存档按钮

    // 内部变量
    protected int m_index;       // 存档槽索引
    protected GameData m_data;   // 当前存档数据

    /// <summary>
    /// 存档是否已填充数据
    /// </summary>
    public bool isFilled { get; protected set; }

    /// <summary>
    /// 加载当前存档
    /// </summary>
    public virtual void Load()
    {
        Game.instance.LoadState(m_index, m_data); // 加载存档数据到游戏
        GameLoader.instance.Load(nextScene);      // 加载下一场景
    }

    /// <summary>
    /// 删除当前存档
    /// </summary>
    public virtual void Delete()
    {
        GameSaver.instance.Delete(m_index);       // 删除存档
        Fill(m_index, null);                       // 更新 UI 显示为空存档
        EventSystem.current.SetSelectedGameObject(newGameButton.gameObject); // 将焦点设置到新建按钮
    }

    /// <summary>
    /// 创建一个新存档
    /// </summary>
    public virtual void Create()
    {
        var data = GameData.Create();              // 创建新存档数据
        GameSaver.instance.Save(data, m_index);   // 保存到对应槽位
        Fill(m_index, data);                       // 更新 UI 显示存档信息
        EventSystem.current.SetSelectedGameObject(loadButton.gameObject); // 将焦点设置到加载按钮
    }

    /// <summary>
    /// 根据存档数据填充 UI
    /// </summary>
    /// <param name="index">存档槽索引</param>
    /// <param name="data">存档数据，若为 null 则表示该槽为空</param>
    public virtual void Fill(int index, GameData data)
    {
        m_index = index;
        isFilled = data != null;

        // 控制容器显示
        dataContainer.SetActive(isFilled);
        emptyContainer.SetActive(!isFilled);

        // 控制按钮可用状态
        loadButton.interactable = isFilled;
        deleteButton.interactable = isFilled;
        newGameButton.interactable = !isFilled;

        // 填充具体 UI 数据
        if (data != null)
        {
            m_data = data;
            retries.text = data.retries.ToString(retriesFormat);             // 显示重试次数
            stars.text = data.TotalStars().ToString(starsFormat);            // 显示总星星数
            coins.text = data.TotalCoins().ToString(coinsFormat);            // 显示总金币数
            createdAt.text = DateTime.Parse(data.createdAt).ToLocalTime().ToString(dateFormat); // 创建时间
            updatedAt.text = DateTime.Parse(data.updatedAt).ToLocalTime().ToString(dateFormat); // 更新时间
        }
    }

    /// <summary>
    /// 初始化事件绑定
    /// </summary>
    protected virtual void Start()
    {
        loadButton.onClick.AddListener(Load);           // 绑定加载事件
        deleteButton.onClick.AddListener(Delete);       // 绑定删除事件
        newGameButton.onClick.AddListener(Create);     // 绑定新建事件
    }
}

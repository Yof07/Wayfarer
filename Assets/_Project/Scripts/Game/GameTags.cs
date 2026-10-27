/// <summary>
/// 游戏物体标签常量类。
/// 用于统一管理 Unity 中 GameObject 的 Tag 字符串，
/// 避免在代码中直接写死字符串（减少拼写错误风险）。
/// 这些标签需要在 Unity 编辑器的 Tag 管理中预先定义。
/// </summary>
public class GameTags
{
    /// <summary>
    /// 玩家角色的标签。
    /// 用于检测玩家对象（如碰撞检测、触发事件等）。
    /// </summary>
    public static string Player = "Player";

    /// <summary>
    /// 敌人的标签。
    /// 用于识别敌人对象（如攻击判定、AI 逻辑等）。
    /// </summary>
    public static string Enemy = "Enemy";

    /// <summary>
    /// 危险区域或陷阱的标签。
    /// 用于判定伤害区域（如尖刺、火焰等）。
    /// </summary>
    public static string Hazard = "Hazard";

    /// <summary>
    /// 平台的标签。
    /// 用于识别可站立的平台对象（可能是移动平台）。
    /// </summary>
    public static string Platform = "Platform";

    /// <summary>
    /// 旗杆或杆子的标签。
    /// 用于特殊交互（如滑杆、攀爬等）。
    /// </summary>
    public static string Pole = "Pole";

    /// <summary>
    /// 面板的标签。
    /// 可能是按钮、开关或 UI 交互面板。
    /// </summary>
    public static string Panel = "Panel";

    /// <summary>
    /// 弹簧的标签。
    /// 用于跳跃辅助物（如弹跳板）。
    /// </summary>
    public static string Spring = "Spring";

    /// <summary>
    /// 水体区域的标签。
    /// "Volume/Water" 表示这是一个带有体积检测的水域。
    /// </summary>
    public static string VolumeWater = "Volume/Water";

    /// <summary>
    /// 可交互的轨道的标签。
    /// "Interactive/Rail" 可能用于滑行轨道或移动路径。
    /// </summary>
    public static string InteractiveRail = "Interactive/Rail";
}

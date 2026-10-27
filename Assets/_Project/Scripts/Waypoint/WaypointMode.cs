/// <summary>
/// 路点切换模式枚举，用于WaypointManager控制路点移动方式
/// </summary>
public enum WaypointMode
{
    Loop,       // 循环模式：到达最后一个路点后从头开始循环
    PingPong,   // 往返模式：到达末路点后反向移动，像乒乓球来回
    Once        // 单次模式：到达最后一个路点后停止
}

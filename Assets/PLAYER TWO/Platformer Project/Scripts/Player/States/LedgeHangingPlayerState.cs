
using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家抓悬挂状态
/// - 当玩家挂在悬崖边缘时进入该状态
/// - 处理悬挂位置、左右移动、跳跃、释放悬挂、爬升等逻辑
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Ledge Hanging Player State")]
public class LedgeHangingPlayerState : PlayerState
{
    // 是否保持父对象（用于爬升时防止重置父对象）
    protected bool m_keepParent;

    // 延迟清理父对象的协程引用
    protected Coroutine m_clearParentRoutine;

    // 清理父对象的延迟时间
    protected const float k_clearParentDelay = 0.25f;

    /// <summary>
    /// 进入抓悬挂状态
    /// - 调整玩家皮肤偏移
    /// - 重置跳跃、空中旋转和空中冲刺次数
    /// </summary>
    protected override void OnEnter(Player player)
    {
        if (m_clearParentRoutine != null)
            player.StopCoroutine(m_clearParentRoutine); // 停止可能存在的清理协程

        m_keepParent = false;
        player.skin.position += player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
        player.ResetJumps();       // 重置跳跃次数
        player.ResetAirSpins();    // 重置空中旋转次数
        player.ResetAirDash();     // 重置空中冲刺次数
    }

    /// <summary>
    /// 离开抓悬挂状态
    /// - 启动延迟协程清理父对象
    /// - 还原皮肤偏移
    /// </summary>
    protected override void OnExit(Player player)
    {
        m_clearParentRoutine = player.StartCoroutine(ClearParentRoutine(player));
        player.skin.position -= player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
    }

    /// <summary>
    /// Ledge 悬挂状态下每帧的步进逻辑：
    /// - 同时做“侧面”与“顶端”两类检测，确认玩家正贴着一面墙且墙的顶部有可落脚的平台；
    /// - 若满足悬挂条件：约束玩家位置与朝向，并处理左右移动、释放、跳跃与翻越；
    /// - 若条件不满足：切换为下落状态。
    /// </summary>
    protected override void OnStep(Player player)
    {

        // ---------- 顶端（平台面）检测参数 ----------
        // 以玩家半径 + 可向前探测的最大距离作为“顶端检测”的前向探测范围
        var ledgeTopMaxDistance = player.radius + player.stats.current.ledgeMaxForwardDistance;

        // 顶端检测的竖直起点高度：玩家半高 + 允许向下探测的最大深度
        // 这样从较高处向下打射线，能扫到平台的上表面（ledge 的“台面”）
        var ledgeTopHeightOffset = player.height * 0.5f + player.stats.current.ledgeMaxDownwardDistance;

        // 顶端检测的射线起点：玩家位置 + 向上偏移 + 向前偏移
        // 向上：把起点抬到“头顶更高一些”的位置；向前：把起点移到平台外缘上方
        var topOrigin = player.position
                        + Vector3.up * ledgeTopHeightOffset
                        + player.transform.forward * ledgeTopMaxDistance;


        // ---------- 侧面（墙面）检测参数 ----------
        // 侧面检测起点：玩家中心高度（height * 0.5）再向下一个可配置的偏移量
        // 目的：在胸口/腹部附近用球体扫描前方墙面，获得贴墙的命中点与法线
        var sideOrigin = player.position
                         + Vector3.up * player.height * 0.5f
                         + Vector3.down * player.stats.current.ledgeSideHeightOffset;

        // 侧面扫描距离：玩家半径 + 可向前探测的最大距离
        var rayDistance = player.radius + player.stats.current.ledgeSideMaxDistance;

        // 侧面碰撞的球体半径：球形扫描比射线更稳健，能处理些许凹凸
        var rayRadius = player.stats.current.ledgeSideCollisionRadius;

        // ---------- 侧面 + 顶端双重命中判定 ----------
        // 条件1：前方“球形扫描”命中可悬挂层（说明前方确实有墙/边）
        // 条件2：在“平台上方”自上而下打射线命中可悬挂层（说明墙上方确实有台面）
        if (Physics.SphereCast(
                sideOrigin, // 扫描起点（胸腹高度附近）
                rayRadius, // 球体半径，容错更好
                player.transform.forward, // 向前
                out var sideHit, // 命中信息（墙面）
                rayDistance, // 扫描距离
                player.stats.current.ledgeHangingLayers, // 允许悬挂的层
                QueryTriggerInteraction.Ignore) // 忽略触发器
            &&
            Physics.Raycast(
                topOrigin, // 顶端扫描起点（平台外缘上方）
                Vector3.down, // 向下
                out var topHit, // 命中信息（平台台面）
                player.height, // 向下长度（足够覆盖玩家身高）
                player.stats.current.ledgeHangingLayers, // 同层筛选
                QueryTriggerInteraction.Ignore))
        {
            // 读取玩家输入方向（x：左右，z：前后）
            var inputDirection = player.inputs.GetMovementDirection();

            // 计算用于“左右移动检测”的起点：
            // 以侧面射线起点为基准，沿左右方向偏移一个“半径”，偏移方向取决于输入 x 的正负
            var ledgeSideOrigin =
                sideOrigin + player.transform.right * Mathf.Sign(inputDirection.x) * player.radius;

            // 计算玩家悬挂时应该放置的“竖直高度”：
            // 取平台命中点的 y，再减去玩家半高（让玩家胸口附近对齐到平台边缘下方）
            var ledgeHeight = topHit.point.y - player.height * 0.5f;

            // 根据墙面法线计算“面向墙的前向方向”：
            // sideHit.normal 是墙面法线（朝外），取其水平分量并反向，即指向墙的方向
            var sideForward = -new Vector3(sideHit.normal.x, 0, sideHit.normal.z).normalized;

            // 计算翻越后“需要的落点高度” = 玩家半高 + 物理接触偏移
            // 用于判断“玩家整体体积”能否放置到平台上（FitsIntoPosition）
            var destinationHeight = player.height * 0.5f + Physics.defaultContactOffset;

            // 计算翻越时的目标落点位置：
            // 平台命中点 + 上抬到能容纳玩家胶囊的高度 + 向前挪一个半径（避免卡台边）
            var climbDestination =
                topHit.point + Vector3.up * destinationHeight + player.transform.forward * player.radius;

            // ---------- 对齐玩家朝向 ----------
            // 令玩家“面向墙面”（避免出现背对墙还在挂的违和感）
            player.FaceDirection(sideForward);


            // ---------- 左右沿边移动（横移）逻辑 ----------
            // 从左右偏移后的起点向“贴墙方向”再打射线，若仍有墙，允许横向移动；
            // 若打空，说明边缘到了（旁边没墙可挂），不允许继续横移，置零速度。
            if (Physics.Raycast(
                    ledgeSideOrigin,         // 左右偏移后的起点
                    sideForward,             // 仍然朝墙方向检测
                    rayDistance,
                    player.stats.current.ledgeHangingLayers,
                    QueryTriggerInteraction.Ignore))
            {
                // 允许横移：按输入 x 方向设置横向速度（世界空间的左右 = transform.right）
                player.lateralVelocity =
                    player.transform.right * inputDirection.x * player.stats.current.ledgeMovementSpeed;
            }
            else
            {
                // 不允许横移：速度清零
                player.lateralVelocity = Vector3.zero;
            }

            // ---------- 约束玩家的悬挂位置 ----------
            // 把玩家位置锁定在：墙面命中点的水平位置 + 目标悬挂高度
            // 再沿“面朝墙的方向”后退一个半径（让玩家胶囊体刚好贴在墙外缘）
            // 最后减去 player.center（若角色模型中心不在胶囊中心，需要修正）
            player.transform.position =
                new Vector3(sideHit.point.x, ledgeHeight, sideHit.point.z)
                - sideForward * player.radius
                - player.center;


            // ---------- 输入处理：释放 / 跳跃 / 翻越 ----------

            // 1) 释放悬挂（向下）：玩家主动松手，切回下落
            if (player.inputs.GetReleaseLedgeDown())
            {
                // 松手时把朝向反过来（面向离开方向，可选的表现细节）
                player.FaceDirection(-sideForward);
                player.states.Change<FallPlayerState>();
            }
            // 2) 跳跃：从悬挂状态直接起跳，然后切回下落（等待落地）
            else if (player.inputs.GetJumpDown())
            {
                player.Jump(player.stats.current.maxJumpHeight);
                player.states.Change<FallPlayerState>();
            }
            // 3) 翻越（向前爬上平台）：
            // 条件：
            //   - 玩家有前推输入（z > 0）
            //   - 配置允许翻越
            //   - 平台所属层在“可翻越层”集合中
            //   - 目标落点 climbDestination 能容纳玩家体积（不穿墙/不顶棚）
            else if (inputDirection.z > 0
                     && player.stats.current.canClimbLedges
                     && (((1 << topHit.collider.gameObject.layer)
                          & player.stats.current.ledgeClimbingLayers) != 0)
                     && player.FitsIntoPosition(climbDestination))
            {
                // 标记：切换状态期间保持父子关系（若动画/根节点需要）
                m_keepParent = true;

                // 切换到“翻越”状态，并触发事件回调（用于动画、音效等）
                player.states.Change<LedgeClimbingPlayerState>();
                player.playerEvents.OnLedgeClimbing?.Invoke();
            }
        }
        else
        {
            // 若“侧面命中 + 顶端命中”任一失败，说明无法保持悬挂 → 切到下落
            player.states.Change<FallPlayerState>();
        }
    }


    /// <summary>
    /// 碰撞处理逻辑
    /// - 抓悬挂状态通常不处理碰撞
    /// </summary>
    public override void OnContact(Player player, Collider other) { }

    /// <summary>
    /// 延迟清理父对象协程
    /// - 如果爬升保持父对象则跳过清理
    /// </summary>
    protected virtual IEnumerator ClearParentRoutine(Player player)
    {
        if (m_keepParent) yield break;

        yield return new WaitForSeconds(k_clearParentDelay);

        player.transform.parent = null;
    }
}

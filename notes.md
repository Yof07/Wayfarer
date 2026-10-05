## 全量对比 Odyssey 完整版：注释类差异清零（2026-10-06）
**方法**：三轮扫描（① 本地"注释行" vs Odyssey"启用行"逐行严格匹配；② 去空白宽松匹配；③ 把 Odyssey 全部代码去空白拼接后做跨行子串匹配，防止多行写法漏网）。范围：本地 116 个脚本（唯一路径差异：`Entity/Interfaccs/IEntityContact.cs` 文件夹拼写不同，类本身一致，未动；本地无块注释 `/*`）。
**结果**：
- **命中并修复 1 处**：`Player.cs` 的 `EnterWater` 里被注释的 `Throw();`（依赖已存在）→ 解开（Odyssey 为启用状态）。加上本轮早前修的死亡链（`HurtPlayerState` 的 `DiePlayerState` 切换 + `Player.ApplyDamage` 的 OnDie 触发），**注释类差异已清零**。
- 其余差异全部是**版本级**（Odyssey 较新、本地＝镜像较旧），**非注释问题，本轮未动**（改动风险高、需配套回归测试）：
  - `Entity.cs`：Odyssey 多约 85 行（`HandlePenetration`/`HandleHighLedge`/斜坡限制 `m_penetratorCollider` 等新系统）
  - `Player/Player.cs`：Odyssey 多约 60 行（另一发行版：`RegularSlopeFactor`/`InitializeSkin`/`FaceDirection` 等）
  - `LevelStarter.cs`：Odyssey 版多了"冻结玩家输入→开始后恢复、开始计时"的启动流程（本地版没有；**关卡计时器不启动可能与此有关**）
  - `AirDivePlayerState`/`LedgeHangingPlayerState`/`FallPlayerState`/`RailGrindPlayerState`/`SpinPlayerState`/`PlayerStats` 等：Odyssey 有斜坡系数等新逻辑
  - 大量状态文件的 `{ }` vs 换行花括号 = 纯格式差异
  - `Collectable.cs`：Odyssey 多一个 `OnDrawGizmos`（编辑器可视化）；本地用 `display?.SetActive`（更安全）
  - `GameSaver.cs`：本地用 `+` 拼接，与 Odyssey 的 `$""` 等价
**若要完全收敛到 Odyssey 版本**：建议单独开一轮，从 `Entity`/`Player` 基类开始成套替换并回归测试（Player 系列与本地 prefab 的状态列表强耦合，单点替换容易失配）。

## 死亡不触发 / 一直往后倒退的根因：两处死亡代码缺失（2026-10-06）
**现象**：血量归零不进入死亡状态，而是被击退后**一直往后滑**（无限倒退）。
**根因**：
1. `Player/States/HurtPlayerState.cs:47` 的 `player.states.Change<DiePlayerState>();` 被注释——血空时唯一的出口被堵死 → 玩家永远停在 `HurtPlayerState`；而该状态每帧只调 `Gravity()`（不摩擦、不处理横向速度）→ 受伤时给的击退速度 `lateralVelocity = -transform.forward * hurtBackwardsForce` 永不衰减 → 无限后退。
2. `Player.ApplyDamage` 缺少 Odyssey 的死亡触发段 `if (health.isEmpty) { Throw(); playerEvents.OnDie?.Invoke(); }` → 即使进了死亡状态也永远不会通知 `LevelRespawner` → 永远不复活。
**来源**：`DiePlayerState`/`LevelRespawner` 是 Odyssey 独有（镜像没有），本地 `HurtPlayerState` 从镜像搬来时依赖缺失、按规则注释；后来依赖补齐但没回头解注释（镜像 `Player.cs` 里对应死亡行同样是注释）。
**修复（已做，按 Odyssey）**：① 解开 HurtPlayerState 的 DiePlayerState 切换；② `Player.ApplyDamage` 末尾补 `if (health.isEmpty) { Throw(); playerEvents?.OnDie?.Invoke(); }`。
**链路核对（全部就位）**：`DiePlayerState` ✓；`LevelRespawner`+`Level`+`LevelPauser`+`LevelStarter` 都挂在 `__LEVEL__` 根上 ✓；`Fader` 已挂（淡出/淡入可用）✓；`Level.player` 是 `FindObjectOfType` 自动查找 ✓。**预期**：挨打击退 → 落地 → 死亡状态（摩擦滑停）→ OnDie → 扣一次命 + 淡出 → 回重生点 + 回满血 + 淡入；命扣到 0 时走 GameOver 重载场景。

## Star.cs 补全：本地/镜像是 10 行空壳，Odyssey 才是完整版（2026-10-06）
**对比**：本地 `Misc/Star.cs` 与镜像逐字相同（`class Star : Collectable { }` 空壳，无任何成员）；Odyssey 完整版有 `index` 字段、`Collect(Player)` 覆盖（调 `LevelScore.CollectStar(index)` 计数）、`Disable()`、`Awake` 里 `OnScoreLoaded` 监听（存档已收集 → 自动隐藏）。**三个工程的 Star.prefab 的 `onCollect` 都是空的（`m_Calls: []`）→ 星星计数完全靠代码**，所以空壳期间吃星星不计数（HUD 星星不亮、不进存档、结算缺星）。
**补全（已做）**：把本地 `Star.cs` 换成 Odyssey 完整版（剥命名空间；保留本地文件与 .meta guid `17341162baec48a4b2f8d232137070e2`，Star.prefab 的组件引用不动）。TestLevelScene 里 3 颗星星实例本来就带着沉睡的 `index` 覆盖（值 2 / 1 / 默认 0——与 Odyssey 场景同款），字段恢复后自动苏醒，**场景无需手动改动**。
**验证方法**：Play → 吃一颗星 → HUD 对应星星图标亮起；读档/重进关后，已收集的星应自动隐藏。

## 吃爱心报 ArgumentException 的根因：Heart.prefab 的 onCollect 被改成固定参数模式（2026-10-05）
**现象**：吃爱心 → `ArgumentException: Object of type 'UnityEngine.Object' cannot be converted to type 'Player'`，堆栈 `Collectable.CollectRoutine → onCollect.Invoke`（Collectable.cs:159）。
**根因**：本地 `Collectables/Heart.prefab` 的 `onCollect`（`PlayerEvent` = `UnityEvent<Player>`）里，`AddHealth` 那条调用的参数模式被改成了 `m_Mode: 2`（固定对象参数）且对象槽为空（`m_ObjectArgument: {fileID: 0}`）→ 运行时 Unity 要把这个空参数转换成 `Player` 就抛异常。**镜像和 Odyssey 原版都是 `m_Mode: 0`（Dynamic/EventDefined：把事件自带的 player 动态传进去）**——玩家是场景对象，prefab 里根本引用不到，只能用动态参数。
**修复（已做）**：`Heart.prefab` 两行改回原版：`m_Mode: 2 → 0`、`m_ObjectArgumentAssemblyTypeName: Player, Assembly-CSharp → UnityEngine.Object, UnityEngine`。Bouncy Heart Variant 是其变体自动继承；其余收藏品（Coin/Life/Bouncy Life）都是 Int 模式（`m_Mode: 3`，值 1），本来就没问题。

## HUD 命/金币恒为 0 的根因：__LEVEL__.prefab 里 HUD 组件被实例级移除（2026-10-05）
**现象**：关卡里 HUD 的命、金币一直显示 0，控制台**无任何异常**。
**根因**：HUD 界面来自 `Examples/Prefabs/UI/HUD.prefab`，但它只被 `Global/__LEVEL__.prefab` 引用（关卡场景经 __LEVEL__ 实例带上 HUD）。本地 __LEVEL__.prefab 的 HUD 实例上有一行 `m_RemovedComponents: [7717047409434490784]`——7717047409434490784 正是 HUD.prefab 里的 **HUD 脚本组件**，即"在这个实例上把 HUD 组件移除"。**Odyssey 原版这里是 `[]`**。这是"缺脚本时代"的遗留：本地最初没有 HUD.cs（Missing Script）→ 组件被清掉；后来 HUD.cs 搬进来了，但这行删除记录还压着组件 → HUD 的 Awake/事件绑定从不执行 → Text 停在初始 0，且零报错。同类遗留还有 Virtual Gamepad 实例上被移除的 **MobileRig**（7624686433000663197，MobileRig.cs 后来也搬进来了）。
**修复（已做）**：把 __LEVEL__.prefab 里这两处 `m_RemovedComponents` 条目清成 `[]`（各删 1 行；验证过文件里再无这两个 id、场景无其他引用，无需动场景）。Pause Screen 实例的两条删除记录（1699…/1249…）在本地对不上任何组件（悬空、无害，未动）；TestLevelScene 里 __LEVEL__ 实例的 9 条删除记录同样全部悬空（未动）。
**预期**：Play 后 HUD 显示 命=03（initialRetries=3）、金币=000，吃金币立即 +1；HUD.prefab 自带的 Text 接线（retries/coins/health/timer/starsImages）本来就是填齐的，组件恢复后直接用。

## FileSelect 报 SerializationException（"点 Start 像没进入场景"）排障（2026-10-05）
**现象**：标题界面点 Start 后看起来没切到 FileSelect；Console 报 `SerializationException: The ObjectManager found an invalid number of fixups`，堆栈 `UISaveList.Awake → GameSaver.LoadList → GameSaver.Load → GameSaver.LoadBinary`（GameSaver.cs:150）。
**真相**：场景切换其实成功了（Editor.log 里有 3 次 `Loaded scene FileSelect.unity`）。崩的是 FileSelect 里的**存档列表**：`UISaveList.Awake` 第一行 `GameSaver.instance.LoadList()` 读旧存档时抛异常 → 5 张存档卡片一张都没生成（初始焦点也没设置）→ 界面空着，看起来像"没进去"。
**原因**：`C:\Users\YoF\AppData\LocalLow\DefaultCompany\Wayfarer\save_0.data`（457 字节，2026/5/27 的旧存档）里存的是**带命名空间的老类名** `PLAYERTWO.PlatformerProject.GameData` / `.LevelData[]`；而现在项目里的类叫 `GameData`/`LevelData`（无命名空间）。BinaryFormatter 按类型全名找类 → 找不到 → 反序列化失败。**结论：剥命名空间会让旧二进制存档全部失效。**
**处理**：已把 `save_0.data` 改名成 `save_0.data.bak`（留在原地，可反悔）。之后 FileSelect 会正常显示 5 个空槽（`UISaveCard.Fill(i, null)` 对空存档安全），点"新建"会用当前类写新存档，以后读写正常。不改任何代码——这是数据文件不兼容，不是代码 bug。

## 2026-10-05 收尾：最后 4 个脚本 + 全部 GUID 冲突修复
**新搬入（Odyssey 独有）**：
| 文件 | GUID | 作用 |
|---|---|---|
| `Player/States/DiePlayerState.cs` | `f2e750e2…` | 玩家死亡状态：`OnStep` 里照样跑 `Gravity()`+`Friction()`+`SnapToGround()`，所以"尸体"会自然下落、贴地滑停；`OnEnter/OnExit/OnContact` 都留空 |
| `Player/PlayerLean.cs` | `90983a3b…` | **跑动时模型左右倾斜**：`LateUpdate` 里用"输入方向 vs 实际移动方向"的带符号夹角 `Vector3.SignedAngle`，只有 Walk/Swim/Gliding 三种状态才倾斜（`CanLean()`），用 `Mathf.SmoothDampAngle` 平滑过渡到 `target` 的 z 旋转 |
| `Player/PlayerSpinTrail.cs` | `e0ac71a7…` | 旋转攻击的拖尾：挂在带 `TrailRenderer` 的物体上，`Start` 里把它挂到玩家手部 `hand` 下并默认关闭；监听 `states.events.onChange`，当前状态是 `SpinPlayerState` 才 `enabled = true` |
| `Tools/Editor/UIAnimatorInspector.cs` | `80ee0dc6…` | UIAnimator 的自定义 Inspector：没绑定 Controller 时给个 **"Auto Generate Animation"** 按钮，一键生成含 Normal/Show/Hide 三个触发器与状态的 AnimatorController |

**GUID 冲突修复（把资产里"找不到脚本"的旧 guid 换成本地同名脚本的 guid）**——组件块和里面的字段数据本来就在，只改 `m_Script` 一行即可原地复活，**不需要手动重挂**：

| 脚本 | 旧 guid（Odyssey）→ 新 guid（本地） | 修复的资产 |
|---|---|---|
| UIAnimator | `00907b95…` → `6f515295…` | `Loading Screen.prefab` |
| Rotator | `0e4206c7…` → `5ccbc92f…` | `End Level Panel.prefab`、`Dice.prefab`、`Rotating Platform.prefab` |
| Panel | `42a38493…` → `3d84606d…` | `End Level Panel.prefab` |
| LevelController | `4ba78aa9…` → `b6b77b6e…` | `End Level Panel.prefab` |
| ItemBox | `52f2272b…` → `2777ba99…` | `Item Box (Empty).prefab` |
| Toggle | `7204037e…` → `3959f6f3…` | `TestLevelScene.unity` |
| GameController | `d8f83f29…` → `d1154b45…` | `LevelSelect.unity`、`TitleScreen.unity` |

（换之前确认过：每个旧 guid 在这些文件里都只出现 1 次、且都在 `m_Script:` 行上，所以是纯机械替换，不会误伤别的引用；换之后抽查 `End Level Panel` 的 Panel/Rotator 字段值都原样还在。）
**结果：全项目 Platformer 相关脚本的 Missing Script 归零**（扫描里剩下的 34 个"未解析 guid"都是 Unity 包/内置脚本：Image/Text/PostProcessing/InputSystem 等）。

## UI 辅助脚本一批（2026-10-05，全部 Odyssey 独有、镜像里没有，用 Odyssey GUID）
| 文件 | GUID | 作用 |
|---|---|---|
| `UI/UILevelCard.cs` | `ad62664f…` | 选关界面的一张"关卡卡片"：`Fill(GameLevel)` 填标题/描述/金币/时间/图片/星星，`locked` 控制 play 按钮能否点；`Play()` → `GameLoader.Load(scene)` |
| `UI/UILevelList.cs` | `3f843423…` | `Awake` 里按 `Game.instance.levels` 逐个 `Instantiate(card, container)` 生成卡片并 `Fill`，可选把第一张卡片的 play 设为 EventSystem 的当前选中项 |
| `UI/UISaveCard.cs` | `15ed8acc…` | 存档卡片：`Create()` 新建（`GameData.Create()` + `GameSaver.Save`）、`Load()`（`Game.LoadState` + `GameLoader.Load(nextScene)`）、`Delete()`，`Fill(index, data)` 切换"有档/空档"两套 UI 并填数值 |
| `UI/UISaveList.cs` | `3911be38…` | `Awake` 里 `GameSaver.instance.LoadList()` 生成 5 张存档卡片，自动选中第一张 |
| `UI/UIFocusKeeper.cs` | `21df07c5…` | 挂在 EventSystem 上：每帧检查"当前有没有被选中的 UI 元素"，没有就把上一次选中的（且仍激活、可交互的）重新选上——**手柄/键盘导航防丢焦点** |
| `UI/UIHorizontalAutoScroll.cs` | `05478505…` | 挂在 ScrollRect 上：读手柄/键盘水平输入 → 换子项索引 → 协程 `Mathf.Lerp` 平滑滚动，带 0.1 秒重复间隔（用 `UnityEngine.InputSystem.UI.InputSystemUIInputModule`，项目已装 Input System 1.7.0） |
| `Misc/MobileRig.cs` | `1f83caf8…` | **移动端虚拟摇杆**：`OnEnable` 检查平台，iOS/Android 才把子物体（虚拟按键）SetActive(true)，其它平台全隐藏；带 `#if UNITY_EDITOR [ExecuteInEditMode]`，编辑器里不运行也能看到效果。挂在 `Virtual Gamepad.prefab` 上 |

搬进来后这几个资产上原本的 Missing Script 自动恢复：`Level Card.prefab`、`Save Card.prefab`、`Virtual Gamepad.prefab`、`Scenes/LevelSelect.unity`、`Scenes/FileSelect.unity`、`Scenes/TitleScreen.unity`。

## HUD 抬头显示 (UI/HUD.cs，Odyssey guid `6fb0f003…`)
- **镜像里没有这个文件**，是 Odyssey 独有；本地 `Examples/Prefabs/UI/HUD.prefab` 一直挂着它（所以之前是 Missing Script，搬进来后自动解析，且**引用已经全填好**：`retries`/`coins`/`health`/`timer` 四个 Text + `starsImages` 三张星图标）
- 显示四项：重试次数、金币、生命值、关卡计时（+ 星星）
- 需要手动配置：三个格式化格式串 `retriesFormat = "00"` / `coinsFormat = "000"` / `healthFormat = "0"`（`ToString(format)` 用）
- 具体逻辑：
  - `Awake` 拿 `Game`/`LevelScore`/`Player`，然后**等 `m_score.OnScoreLoaded` 触发后才去订阅**：`OnCoinsSet → UpdateCoins`、`OnStarsSet → UpdateStars`、`m_game.OnRetriesSet → UpdateRetries`、`m_player.health.onChange → UpdateHealth`，最后 `Refresh()` 全刷一遍
  - `Update` 只干计时器：累加 `timerStep`，满 `timerRefreshRate`（0.1 秒）才用 `GameLevel.FormattedTime(m_score.time)` 写一次文本——**限流，不是每帧刷 UI**
  - ⚠️ 依赖链：`Awake` 里直接 `m_score.OnScoreLoaded`，所以**场景里必须有 `LevelScore`**（已挂在 `__LEVEL__` 上），否则 HUD 自己就会空指针

## Game/Level 管理层（2026-10-05 从 Odyssey 搬入第一批）
搬入的 7 个新文件 + 2 个改写，全部是「关卡流程 / 存档」这一路的依赖（剥掉 namespace，其余逐字照搬）：

| 文件 | GUID | 作用 |
|---|---|---|
| `Game/GameData.cs` | `781ab288…` | `[Serializable]` 存档数据：`retries` / `LevelData[] levels` / 创建与更新时间；`Create()` 用 `Game.instance.initialRetries` 和 `Game.instance.levels` 生成默认存档；`ToJson/FromJson` 走 `JsonUtility` |
| `Game/GameLevel.cs` | `d6708414…` | 单个关卡的静态数据（`locked` / `scene` / `name` / `description` / `image`）＋ 进度（`coins` / `time` / `stars`）；`StarsPerLevel = 3`；`LoadState/ToData` 与 LevelData 互转；`FormattedTime()` 格式化 `02'34"56` |
| `Game/GameLoader.cs` | `0b938ab5…` | 单例，异步切场景（`SceneManager.LoadSceneAsync`），带 `loadingScreen`（UIAnimator）、`startDelay/finishDelay`、`isLoading`/`loadingProgress`、`currentScene`、`OnLoadStart/OnLoadFinish`。**`currentScene` 是整个"当前是第几关"判断的依据** |
| `Game/GameSaver.cs` | `0fcbb56f…` | 单例，三种存档方式（Binary / JSON / PlayerPrefs），`Save/Load/Delete/LoadList`，路径在 `Application.persistentDataPath` |
| `Level/LevelData.cs` | `d38ee35d…` | 单关存档：`locked`/`coins`/`time`/`stars` + `CollectedStars()` |
| `Level/Level.cs` | `9d4543d6…` | 单例，只干一件事：缓存当前场景的 `Player`（`FindObjectOfType` 一次） |
| `Level/LevelScore.cs` | `4b03f4ce…` | 单例计分：`coins`（set 时触发 `OnCoinsSet`）、`stars[]`、`time`（`stopTime` 为 false 才累加）、`OnScoreLoaded`；`Consolidate()` 把本关成绩并回 `GameLevel` 并 `Game.RequestSaving()` |
| `Game/Game.cs`（**改写**） | 保留本地 guid `82a761d6…`（`__GAME__.prefab` 用它） | 原来只有 51 行（`retries` + `LockCursor`），现在补成完整版：`initialRetries`/`levels` 列表/`OnSavingRequested`、`LoadState`、`LevelsData`、`GetCurrentLevel`（靠 `GameLoader.instance.currentScene` 匹配 `level.scene`）、`GetCurrentLevelIndex`、`RequestSaving`、`UnlockNextLevel`、`ToData`，`Awake` 里设初始重试次数并 `DontDestroyOnLoad` |
| `Game/GameController.cs`（**加方法**） | 保留本地 guid | 新增 `LoadScene(string)` → `GameLoader.instance.Load(scene)`，这样标题页/选关页的按钮才有目标方法 |

## Fader 画面淡入淡出 (Misc/Fader.cs，Odyssey guid `9b061817…`)
- `Singleton<Fader>` + `[RequireComponent(typeof(Image))]`——挂在全屏 UI Image 上（镜像里没有这个文件，是 Odyssey 独有；Odyssey 把它挂在 `UI/Fader.prefab` 上）
- `FadeOut(callback)`：把 Image 的 alpha **加**到 1（画面变黑/被盖住）；`FadeIn(callback)`：alpha **减**到 0（画面露出来）；两者都先 `StopAllCoroutines()` 再起协程，速度由 `speed`（默认 1/秒）决定；`SetAlpha(a)` 是立即设置
- 注意命名反直觉：**`FadeOut` = 变不透明（遮住画面）**，`FadeIn` = 变透明（露出画面）

## LevelRespawner 复活/重开 (Level/LevelRespawner.cs，Odyssey guid `55882192…`)
- `Singleton<LevelRespawner>`，挂在 `__LEVEL__` 上（本地还没有，需要手动加）；它把「玩家死亡 → 复活动画 → 扣命 → 没命了就 Game Over」这条链一次做完
- 依赖（本地都已具备）：`Level.instance`、`LevelScore`、`LevelPauser`、`Game`、`GameLoader`、**`Fader`**、`Player.Respawn()`、`PlayerCamera.Reset()`、`playerEvents.OnDie`
- 具体逻辑：
  - `Start`：抓全场景的 `PlayerCamera` 存进列表；给 `player.playerEvents.OnDie` 挂 `Respawn(true)`（**玩家一死自动触发复活流程**）
  - `Respawn(bool consumeRetries)` → `Routine()`：先 `Pause(false)`、禁止暂停、禁玩家输入；若 `consumeRetries` 且 `Game.retries == 0` → 走 `GameOverRoutine()`（停表 → 等 `gameOverFadeOutDelay` → `GameLoader.Reload()` 重载场景）；否则等 `respawnFadeOutDelay` → `Fader.FadeOut(...)` → `RespawnRoutine()`
  - `RespawnRoutine()`：扣一次命（如果 consumeRetries）→ `Level.player.Respawn()`（玩家回重生点、血回满、切 Idle）→ `LevelScore.coins = 0` → 复位所有摄像机 → 播 `OnRespawn` → 等 `respawnFadeInDelay` → `Fader.FadeIn(恢复输入 + 允许暂停)`
  - `Restart()` → `RestartRoutine()`：`Pause(false)`、禁止暂停、禁输入 → 等 `restartFadeOutDelay` → `GameLoader.Reload()`（重开当前场景）
  - 四个延迟参数都可在 Inspector 调：`respawnFadeOutDelay` 1 / `respawnFadeInDelay` 0.5 / `gameOverFadeOutDelay` 5 / `restartFadeOutDelay` 0.5

## LevelFinisher 通关/退出 (Level/LevelFinisher.cs，Odyssey guid `2076fe28…`)
- `Singleton<LevelFinisher>`，挂在 `__LEVEL__` 上（本地还没有，需要手动加）；管"通关"和"退出关卡"两条流程
- 需要手动配置：`unlockNextLevel`（通关是否解锁下一关）、`nextScene`（通关后去哪）、`exitScene`（退出后去哪）、`loadingDelay`（切场景前的延迟，默认 1 秒）；还有两个 `OnFinish` / `OnExit` 事件可挂回调
- 具体逻辑：
  - `Finish()` → `FinishRoutine()`：`Pause(false)` + **`canPause = false`**（结束后不许再暂停）→ `m_score.stopTime = true`（停表）→ 禁玩家输入 → 等 `loadingDelay` → `Game.UnlockNextLevel()`（可选）→ `Game.LockCursor(false)`（放鼠标）→ **`m_score.Consolidate()`（把本关成绩写回 `GameLevel` 并存档）** → `GameLoader.Load(nextScene)` → 播 `OnFinish`
  - `Exit()` → `ExitRoutine()`：同上但**不保存分数**，只是延迟后 `GameLoader.Load(exitScene)` → 播 `OnExit`
  - 注意它声明了 `m_fader` 但两条流程里都没用到（照抄上游，没删）

## LevelController 的接口现状 (Level/LevelController.cs)
- **六个方法全部可用**（与 Odyssey 版逐字一致）：`Finish()` / `Exit()`（→ `LevelFinisher`）、`Respawn(bool)` / `Restart()`（→ `LevelRespawner`）、`AddCoins(int)`（→ `LevelScore`）、`Pause(bool)`（→ `LevelPauser`）
- 四个单例都是 `xxx.instance` 的惰性访问——**如果场景里没有对应的组件，点按钮时会空指针**，所以下面那些手动挂载不能省
- **GUID 冲突**：本地 `LevelController.cs` 的 guid 是 `b6b77b6e…`（`Coin.prefab` 和 `Pause Screen.prefab` 用它），而 `End Level Panel.prefab` 里那个组件用的是 **Odyssey 的 `4ba78aa9…`** —— 只能二选一。我保留了本地 guid，所以 **End Level Panel 上的 LevelController 组件需要手动重挂**（或之后单独处理）

## 跑起来还需要手动做的配置
> 2026-10-05 进展：`__GAME__` 上的 `GameLoader`(id `539602378330051259`) + `GameSaver`(id `2298921648025393660`) 和 `__LEVEL__` 上的 `LevelScore`(id `4496529499839231001`) 已由我直接写进 prefab（fileID 沿用 Odyssey 同名组件，无冲突）；其余仍需手动。

1. ~~`__GAME__` 加 `GameLoader` + `GameSaver`~~ ✅ 已挂；~~`__LEVEL__` 加 `LevelScore`~~ ✅ 已挂。**但 `GameLoader.loadingScreen` 还是空的**（本地 `Loading Screen.prefab` 里的 `UIAnimator` 是 Missing——它引用的是 Odyssey 的 `00907b95…`，而本地 `UIAnimator.cs` 是另一个 guid），所以点"进游戏/退出"走到 `GameLoader.Load()` 时会在加载界面上空指针。要么在 `Loading Screen.prefab` 上重挂 `UIAnimator`（然后拖进 `loadingScreen`），要么先不用加载界面
3. **`Game` 组件的 `levels` 列表要填**：每项填 `scene`（**必须和场景文件名完全一致**）、`name`、可选 `description`/`image`、`locked`。`GetCurrentLevel()` 就是拿 `GameLoader.currentScene` 去匹配 `level.scene`，匹配不到 `LevelScore` 的 `m_level` 就是 null（金币照加，但 `Consolidate()` 不会写回进度）
4. **`Pause Screen` 的按钮**：`Exit` / `Restart` / `Respawn` 三条的 `m_Target` 现在是 `{fileID: 0}`（**目标对象根本没指定**），需要在 Inspector 里把带 `LevelController` 的物体拖进去
5. **`__LEVEL__` 上再加 `Level` + `LevelRespawner` + `LevelFinisher` 三个组件**（Odyssey 的 `__LEVEL__` 上都有）：`Level` 是"当前玩家"的入口，`LevelRespawner` 是"死亡→复活/重开"的执行者，`LevelFinisher` 是"通关/退出"的执行者。注意 `LevelRespawner.Start` 会找 `Level.player` 和挂玩家死亡事件，所以**场景里必须先有玩家**，否则它的 `Start` 会空指针。另外 `LevelFinisher` 的两个场景名（`nextScene` / `exitScene`）要填
6. **`UI/Fader.prefab` 上加 `Fader` 组件**（它已经有 Image，满足 `[RequireComponent]`）；`LevelRespawner` 复活时要调它做黑屏过渡，没有 Fader 会空指针

## StompPlayerState 重击下砸状态
- 下砸状态
- 通过`Player.StompAttack()`来切换到这个状态
- `Player.StompAttack()`被后空翻状态和下落状态的更新步进函数中被调用
- 具体逻辑：
  - 进入状态的时候速度为0，然后事件开始广播
  - 在状态更新函数中，首先它会用一个计时器来记录它在空中停留的时间，时间符合之后，就会开始下砸，更改角色的下落速度；当角色落地之后(角色的isGrounded变量为true时)，开始广播事件，然后会有另一个计时器来让它在地面上硬直一段时间，结束之后会让角色有一个向上的小反弹，最后让它进入下落状态

## SpinPlayerState 旋转状态
- 旋转状态
- 通过`Player.Spin()`来切换到这个状态
- `Player.Spin()`会播放旋转动画，然后进入旋转状态
- 具体逻辑：
  - 进入状态时如果不在地上，则会向上跃升一小段距离，如果计时器超过时间，则退出，在此期间也可以进行下砸攻击

## Portal 传送门 (Misc/Portal.cs)
- 挂在传送门上，要求物体上有 `Collider` 和 `AudioSource`；`Start` 里把碰撞体强制设成触发器（`isTrigger = true`），并把场景里的 `PlayerCamera` 找出来缓存
- 需要手动配置：`exit`（指向出口传送门，不填则不响应）、`exitOffset`（出口方向的落点偏移，默认 1，避免卡在门口）、`teleportClip`（传送音效）、`useFlash`（是否触发闪光特效）
- 具体逻辑：
  - 玩家进入触发器时（`OnTriggerEnter`），先确认 `exit` 存在且进入的是 `Player`
  - 用「玩家中心点到传送门的高度差」`yOffset` 把玩家放到出口的同一相对高度上，然后让玩家朝向出口方向、重置相机
  - 再取一次玩家输入的相机方向 `GetMovementCameraDirection()`：如果这个方向和出口朝向相反（点积 < 0），说明玩家是往回走的，就让玩家转 180°，避免一传送就被拽回原位
  - 最后沿出口朝向前推 `exit.exitOffset` 并保留水平速度大小、方向改成出口朝向；`useFlash` 为真时触发闪光，播放 `teleportClip`
- 当前有一处依赖已补齐：`Flash.cs` 于 2026-10-05 搬入，`Flash.instance?.Trigger()` 已解开注释

## Flash 闪屏特效 (Misc/Flash.cs)
- 单例（继承 `Singleton<Flash>`）+ `[RequireComponent(typeof(Image))]`，所以挂在 UI 上、物体上要有 `Image` 遮罩（默认 alpha 0 看不见）
- 需要手动配置：`duration`（全亮保持时间，默认 0.1）、`fadeDuration`（渐隐时间，默认 0.5）
- 具体逻辑：
  - `Trigger()` 用默认两个时间调 `Trigger(duration, fadeDuration)`；后者先 `StopAllCoroutines()`（避免多次触发叠加）再起协程
  - 协程里把 Image 的 alpha 直接设成 1（瞬间全亮）→ `WaitForSeconds(duration)` → 在 `fadeDuration` 内用 `Mathf.Lerp(1, 0, elapsed/fadeDuration)` 每帧降 alpha → 结束后确保 alpha = 0
  - 唯一调用者是 `Portal`（`useFlash` 为真时传送瞬间白闪一下）
- 镜像里这个组件挂在 `Examples/Prefabs/Global/__GAME__.prefab` 的 `Flash`（UI 物体，Layer 5）上

## GravityField 重力场/上升气流 (Misc/GravityField.cs)
- 挂在带 `Collider` 的物体上（`[RequireComponent(typeof(Collider))]`），`Start` 里把 Collider 强制设成触发器——所以它只做检测、不阻挡玩家
- 需要手动配置：`force`（施加的力，脚本默认 75；镜像的 `Misc/Gravity Field.prefab` 上填的是 70）
- 具体逻辑：
  - `OnTriggerStay`（玩家停留在区域内时每帧调用）：先确认带 `Player` 标签、再取 `Player` 组件
  - 如果玩家此刻 `isGrounded`（踩在地面/场子底部），先把 `verticalVelocity` 清 0 —— 否则地面检测把角色"吸附"住，力抬不起来
  - 然后 `player.velocity += transform.up * force * Time.deltaTime`，即沿着**该物体自身的 up 方向**（所以把场子旋转一下就能做斜向气流），按帧时间累加到速度上，玩家会被持续往上推
- 与 `Buoyancy`（浮力，作用在刚体上）不同：这个是直接改玩家速度，不需要 Rigidbody

## EntityVolumeEffector 实体区域影响器 (Entity/EntityVolumeEffector.cs)
- 挂在带 `Collider` 的物体上，`Start` 里把 Collider 设成触发器（只检测进出、不阻挡），用来做「泥地/浅水/缓速区」这类区域效果
- 需要手动配置六个倍率（都默认 1，即无效果）：`velocityConversion`（进入瞬间直接乘当前速度）、`accelerationMultiplier`、`topSpeedMultiplier`、`decelerationMultiplier`、`turningDragMultiplier`、`gravityMultiplier`
- 具体逻辑：
  - `OnTriggerEnter`：取碰撞体上的 `EntityBase`（玩家、敌人、可推动物体都算），先把 `entity.velocity *= velocityConversion`（例如 0.25 = 进区瞬间速度掉到 1/4），再把五个倍率写到实体身上——这些倍率是 `EntityBase` 里本来就有的字段，会乘在加速度/最高速/减速度/转向阻力/重力上，所以改变的是**后续运动行为**而不只是当前速度
  - `OnTriggerExit`：把这五个倍率全部还原成 1（注意 `velocityConversion` 不还原，因为它是一次性的速度缩放）
- 镜像里挂在 `Examples/Prefabs/Misc/Mud.prefab`（泥地），参数：`velocityConversion: 0.25`、`accelerationMultiplier: 0.5`、`topSpeedMultiplier: 0.5`、`decelerationMultiplier: 1`、`turningDragMultiplier: 0.8`、`gravityMultiplier: 3`（重力 ×3 = 在泥里往下沉得更快）

## WaypointMode 路点模式枚举 (Waypoint/WaypointMode.cs)
- 只有三个值，决定 `WaypointManager` 走到头的表现：`Loop`（走完回到第一个，循环）、`PingPong`（走完倒着走回来，乒乓球）、`Once`（走到最后一个就停）
- 注意枚举顺序是 `Loop, PingPong, Once`，所以序列化里 `mode: 0` 就是 Loop（镜像的 Moving Platform.prefab 正是 0）

## WaypointManager 路点管理器 (Waypoint/WaypointManager.cs)
- 通用「沿一串路点走」的组件，敌人巡逻（`Enemy`）和移动平台（`MovingPlatform`）都用它
- 需要手动配置：`mode`（上面那个枚举）、`waitTime`（每个路点停留时间）、`waypoints`（`Transform` 列表，按顺序填路点物体）
- 具体逻辑：
  - `current`：当前路点。**惰性初始化**——第一次读的时候如果为空就取 `waypoints[0]`（所以列表必须至少有一个元素，否则会越界）；`index` 就是 `waypoints.IndexOf(current)`
  - `Next()`：按模式算出下一个索引，然后 `StartCoroutine(Change(next))`；`m_changing` 为真时直接返回（切换动画期间不响应重复调用）
    - `PingPong`：用 `m_pong` 记方向，到头就翻转（`m_pong` 的写法是「下一格是不是末端/起点」）
    - `Loop`：`index + 1 < Count` 就前进，否则回 0
    - `Once`：只在还没到最后一个时前进
  - `Change(to)` 协程：等 `waitTime` 秒之后才真正把 `current` 换成新路点（停留效果），再把 `m_changing` 复位

## MovingPlatform 移动平台 (Misc/MovingPlatform.cs)
- 挂在带 `Collider` 的物体上，且 `[RequireComponent(typeof(WaypointManager))]`——平台位置由路点驱动，`Awake` 里还会把 tag 设成 `GameTags.Platform`（玩家/敌人靠这个 tag 判断"站在平台上"）
- 需要手动配置：`speed`（移动速度，镜像 prefab 是 3）、以及同级 `WaypointManager` 的 `mode`/`waitTime`/`waypoints`
- 具体逻辑：
  - `Update` 里每帧 `Vector3.MoveTowards(当前位置, waypoints.current.position, speed * deltaTime)` 朝当前路点滑过去
  - 到达后（距离 == 0）调一次 `waypoints.Next()` 切下一个路点；**注意它只改 position，不改旋转**，所以平台是"平移"而不是"沿轨道转向"
- 镜像里挂在 `Examples/Prefabs/Prototype/Moving Platform.prefab`（`round_block` → `Moving Platform`），`WaypointManager` 的 `waypoints` 列表在没有实例化的 prefab 里是空的，需要在场景里把路点物体拖进去

## Spring 弹簧 (Misc/Spring.cs)
- 挂在带 `Collider` 的物体上，实现 `IEntityContact` 接口——**不是靠自己的 OnTrigger，而是靠实体的命中检测回调**（`Entity.cs` 里遍历 `IEntityContact` 调 `OnEntityContact`），所以不需要把 Collider 设成触发器
- 需要手动配置：`force`（弹力，脚本默认 25，镜像 prefab 是 **28**）、`clip`（弹跳音效，镜像 prefab 用 `bd164b0b…`）
- 具体逻辑：
  - `Start`：把 tag 设成 `GameTags.Spring`，拿 Collider；**没有 AudioSource 就自动 AddComponent 一个**
  - `OnEntityContact(entity)`：先用 `entity.IsPointUnderStep(m_collider.bounds.max)` 判断接触点是不是在弹簧顶部下方（也就是"从上面踩下来"），再确认是 `Player` 且 `player.isAlive`，然后：
    1. `ApplyForce(player)`——只有当 `player.verticalVelocity.y <= 0`（正在下落/静止）时才生效：播音效 + `verticalVelocity = Vector3.up * force`
    2. `SetJumps(1)` / `ResetAirSpins()` / `ResetAirDash()`：把空中技能次数补回来
    3. `player.states.Change<FallPlayerState>()`：强制切到下落状态，避免停留在旧状态
- 依赖补全：镜像 `Player.cs:64` 的 `public virtual bool isAlive => !health.isEmpty;` 本地原本没有（本地 Player.cs 与镜像在 water/respawn 那块是不同版本），2026-10-05 按镜像原样补进了 `Player.cs`，位置接在 `pole` 属性后面

## FallingPlatform 掉落平台 (Misc/FallingPlatform.cs)
- 同样实现 `IEntityContact`（靠实体命中回调触发，不是自己的 OnTrigger），挂在带 `Collider` 的平台上；`Start` 里记录初始位置并把 tag 设成 `GameTags.Platform`
- 需要手动配置：`autoReset`（掉落后是否自动复位）、`fallDelay`（踩上后延迟多久掉，默认 2）、`resetDelay`（掉落后多久复位，默认 5）、`fallGravity`（下落速度，默认 40——**不是真的物理重力，是直接位移**）、`shake`/`speed`/`height`（掉落前的抖动开关、频率、幅度）
- 具体逻辑：
  - `OnEntityContact`：确认是 `Player` 且 `IsPointUnderStep(bounds.max)`（从上面踩）→ 若还没 `activated` 就置位并启动 `Routine()`（防止重复触发）
  - `Routine()` 协程：用 `timer = fallDelay` 倒计时；**倒计时过半之后**开始抖（`transform.position = 初始位置 + Vector3.up * Mathf.Sin(Time.time * speed) * height`）→ 计时归零调 `Fall()` → 若 `autoReset` 则再等 `resetDelay` 秒调 `Restart()`
  - `Fall()`：置 `falling = true` 并把 Collider 变触发器（这样平台下落时不再挡住玩家）
  - `Update()`：只要 `falling` 就每帧 `transform.position += fallGravity * Vector3.down * deltaTime`（所以是匀速直线下坠，等玩家掉出去以后自然就看不见了）
  - `Restart()`：复位 `activated`/`falling`、位置回到初始值、Collider 复原成实体，然后 `OffsetPlayer()`
  - `OffsetPlayer()`：用 `Physics.OverlapBoxNonAlloc` 抓平台范围内的碰撞体，只挑带 `Player` 标签的，把它们往上抬 `(平台顶部 y - 玩家 y) + 玩家高度/2`——**防止平台复位时把玩家卡在平台内部**
- 镜像里挂在 `Examples/Prefabs/Platforms/Falling Platform.prefab`（`round_block` → `Falling Platform`），参数就是脚本默认值（`fallDelay: 2`/`resetDelay: 5`/`fallGravity: 40`/`speed: 45`/`height: 0.1`）

## Checkpoint 检查点/存档点 (Misc/Checkpoint.cs)
- 挂在带 `Collider` 的物体上（`Awake` 里把 Collider 设成触发器），靠自己的 `OnTriggerEnter` 触发；没有 AudioSource 时自动 AddComponent 一个
- 需要手动配置：`respawn`（重生点的 Transform，取它的 position + rotation）、`clip`（激活音效，镜像用 `949d6f89…`）、`OnActivate`（UnityEvent，镜像里挂的是 `GameObject.SetActive(true)`——激活后把原本隐藏的模型/旗帜显示出来）
- 具体逻辑：
  - `OnTriggerEnter`：`!activated` 且带 `Player` 标签 → 取 `Player` → `Activate(player)`
  - `Activate`：只生效一次（`activated` 守卫）→ 播音效 → **`player.SetRespawn(respawn.position, respawn.rotation)`**（把玩家身上的重生点改到这里）→ `OnActivate?.Invoke()` 播事件
  - 注意它**只记录重生点**，真正把玩家送回去的是 `Player.Respawn()`
- 依赖补全：本地 `Player.cs` **原本完全没有重生逻辑**（镜像有 `m_respawnPosition`/`m_respawnRotation` 字段 + `InitializeRespawn()`/`Respawn()`/`SetRespawn()`），2026-10-05 按镜像原样补入：字段加在 `m_skinInitialRotation` 之后、`InitializeRespawn()` 跟在 `InitializeTag()` 之后并在 `Awake` 里调用、`Respawn()`/`SetRespawn()` 放在 `RemovePickable()` 前面。`Respawn()` 现在本地还没有调用者（镜像里由 LevelRespawner 之类调用，本项目未搬），属于 API 预留

## EnemyState 敌人状态基类 (Enemy/EnemyState.cs)
- 全文只有一行：`public abstract class EnemyState : EntityState<Enemy> { }`
- 作用就是把 `EntityState<T>` 这个泛型状态基类**绑死成敌人版**：`EntityState<T> where T : Entity<T>` 要求 T 是具体实体类型（`Enemy : Entity<Enemy>`），所以有了它就写成 `EnemyState` 而不用到处写 `EntityState<Enemy>`
- 是 `Enemy/States/` 下三个具体状态（Idle / Follow / Waypoint）的父类，也是 `EnemyStateManager`（`EntityStateManager<Enemy>` + `EnemyState` 版）的类型基础
- 无外部依赖（`EntityState`、`Enemy` 本地都有），搬入后不影响现有代码；真正生效要等 EnemyStateManager 和三个状态类补齐

## FollowEnemyState 敌人跟随状态 (Enemy/States/FollowEnemyState.cs)
- 敌人 AI 的"追玩家"状态，父类 `EnemyState`；**内容与镜像版逐字相同**，按用户 2026-10-05 的决定改用 **Odyssey 的 .meta GUID**（`3257cc648cf18b94fae1da4eb8b7ffaf`）
- 具体逻辑（`OnStep` 每步执行）：
  - `enemy.Gravity()`：按 `stats.gravity` 施加重力
  - `enemy.SnapToGround()`：按 `stats.snapForce` 往下吸附，防止走下坡时弹跳/悬空
  - 取目标方向：`enemy.player.position - enemy.position`，**把 y 清零再单位化**（只追水平方向，不会往天上冲）
  - `enemy.Accelerate(direction, stats.followAcceleration, stats.followTopSpeed)`：按跟随参数加速
  - `enemy.FaceDirectionSmooth(direction)`：按 `stats.rotationSpeed` 平滑转身
  - `OnEnter`/`OnExit`/`OnContact` 都是空实现
- 它依赖 `Enemy.stats`（EnemyStatsManager）和 `Enemy` 上那五个取参快捷方法（`Gravity`/`SnapToGround`/`Accelerate`/`FaceDirectionSmooth`/`Friction`/`Decelerate`）——这些原先因 `EnemyStatsManager` 未搬而注释，2026-10-05 已一并解开

## IdleEnemyState 敌人待机状态 (Enemy/States/IdleEnemyState.cs)
- 敌人 AI 的"站着不动"状态（也是 `EnemyStateManager.states` 列表里没写别的状态时的兜底/冷却状态），父类 `EnemyState`；内容取自 Odyssey，剥掉命名空间，GUID 用 Odyssey 的 `75f91a607d1eaa041bf0581c8e95f345`（**比镜像版多了 `[AddComponentMenu]` 特性**，镜像里那个没有）
- `OnStep` 三行，正好是"接地静止"的标准组合：
  - `enemy.Gravity()`：空中时按 `stats.gravity` 下落
  - `enemy.SnapToGround()`：接地时按 `stats.snapForce` 往下粘（防止斜坡/台阶上悬空，见笔记上文）
  - `enemy.Friction()`：按 `stats.friction` 把水平速度磨到 0 —— 所以它是"从别的状态切回来后自己滑停"的状态，不需要额外逻辑
- `OnEnter`/`OnExit`/`OnContact` 都是空实现

## EnemyAnimator 敌人动画驱动 (Enemy/EnemyAnimator.cs)
- 挂在敌人上（`[RequireComponent(typeof(Enemy))]`），把敌人的状态和速度**喂给 Animator**；内容取自 Odyssey，剥命名空间，GUID `692b52228a09bf44f9ea74018ef96b36`
- 需要手动配置：`animator`（绑定的 Animator）、以及 7 个参数名（`stateName`/`lastStateName`/`lateralSpeedName`/`verticalSpeedName`/`healthName`/`isGroundedName`/`onStateChangedName`）——这些名字要和 Animator Controller 里的参数名对得上
- 具体逻辑：
  - `Start`：`InitializeParametersHash()` 把 7 个名字预先转成 `Animator.StringToHash`（避免每帧用字符串查参数，慢）；`InitializeAnimatorTriggers()` 给 `m_enemy.states.events.onChange` 挂一个回调 → 状态一变就 `animator.SetTrigger("On State Changed")`
  - `LateUpdate`（注意是 LateUpdate，等这一帧所有移动/状态结算完再喂参数）：写 `State`/`Last State`（当前/上一个状态的**索引**，对应 `states.index`/`lastIndex`）、`Lateral Speed`（水平速度大小）、`Vertical Speed`（`verticalVelocity.y`）、`Health`（`health.current`）、`Is Grounded`
- **注意**：状态传的是**索引**不是名字，所以 Animator Controller 里的 `State` 整数分支要按 `EnemyStateManager.states` 列表的顺序来配

## EnemyAudio 敌人音效 (Enemy/EnemyAudio.cs)
- 挂在敌人上，目前只有一条逻辑：监听 `enemyEvents.OnDie` → 播 `death` 音效；没有 AudioSource 时 `Start` 里自动 AddComponent 一个
- 需要手动配置：`death`（死亡音效，Slime 上用的是 `squish.wav`）
- 内容取自 Odyssey，剥命名空间，GUID `db03ed0643c1f814c92cb80ffe878d09`

## HitFlash 受击闪红 (Misc/HitFlash.cs)
- **镜像里没有这个文件**，是 Odyssey 独有的，本地 `Slime.prefab` 一直挂着它（所以之前显示 Missing）；GUID 用 Odyssey 的 `24cfe5e2abc106644befa09d6308afd5`
- 挂在带 `Health` 的物体上；需要手动配置：`renderers`（要闪的 `SkinnedMeshRenderer[]`）、`flashColor`（默认红）、`flashDuration`（默认 0.5）
- 具体逻辑：
  - `Start`：拿 `Health`，给 `m_health.onDamage` 挂 `Flash`（**也依赖本地 `Health` 有 `onDamage` 这个 UnityEvent**，✅ 本地有）
  - `Flash()`：先 `StopAllCoroutines()`（连打不叠加），然后对每个 renderer 的 `renderer.material` 起一个协程
  - `FlashRoutine(material)`：在 `flashDuration` 内把 `material.color` 从 `flashColor` `Color.Lerp` 回原始颜色
  - 注意它改的是 `renderer.material`（**实例化材质**，不是 `sharedMaterial`），所以每个渲染器会各复制一份材质；好处是只影响自己，代价是多了材质实例

## EnemyEvents 敌人事件集 (Enemy/EnemyEvents.cs)
- `[Serializable]` 的纯数据类（不是 MonoBehaviour），里面 5 个 `UnityEvent`：`OnPlayerSpotted`（发现玩家）、`OnPlayerScaped`（玩家逃脱/丢失目标）、`OnPlayerContact`（与玩家接触）、`OnDamage`（受伤）、`OnDie`（死亡）
- `Enemy` 上有一个 `public EnemyEvents enemyEvents` 字段（Inspector 里直接展开勾回调），和 `EntityEvents`/`PlayerEvents` 是一个套路；内容取自 Odyssey，剥命名空间，GUID `610fac7748de5df47abc04dfc398db31`

## EnemyStateManager 敌人状态机 (Enemy/EnemyStateManager.cs)
- `EntityStateManager<Enemy>` 的薄壳，比 Player 那套多一件事：`[ClassTypeName(typeof(EnemyState))] public string[] states;` —— **状态不是拖组件，而是填类名字符串**，`GetStateList()` 交给 `EnemyState.CreateListFromStringArray(states)` 反射创建（`EntityState<T>.CreateFromString` → `System.Type.GetType(名字)` + `Activator.CreateInstance`）
- 所以名字必须是**完全限定名或当前程序集里的类名**：本项目剥了命名空间，就得写 `WaypointEnemyState` 这种裸类名（带 `PLAYERTWO.PlatformerProject.` 前缀会拿到 null → `CreateInstance(null)` 抛异常）
- 还带 `[RequireComponent(typeof(Enemy))]`；内容取自 Odyssey，剥命名空间，GUID `58e3b8364f3c7414793f114cc6c9f441`

## Enemy.cs 的 AI 段落已全部解开（2026-10-05）
- `[RequireComponent(typeof(EnemyStatsManager/EnemyStateManager/WaypointManager/Health))]` 四条特性、`enemyEvents` 字段、`OnUpdate` 里的 `HandleSight()`/`ContactAttack()` 调用、`ContactAttack()` 与 `HandleSight()` 整段方法体、`ApplyDamage()` 里的 `OnDamage`/`OnDie` 事件，都已按规则解开（依赖全部补齐：EnemyStatsManager、EnemyStateManager、EnemyEvents、WaypointManager）
- `HandleSight()`：没锁定玩家时用 `Physics.OverlapSphereNonAlloc(position, stats.spotRange, ...)` 找带 `Player` 标签的碰撞体，找到就记进 `this.player` 并播 `OnPlayerSpotted`；已锁定时若**玩家血量归零**或距离超过 `stats.viewRange` 就解除锁定并播 `OnPlayerScaped`
- `ContactAttack()`：`stats.canAttackOnContact` 为真时用 `OverlapEntity(..., stats.contactOffset)` 找玩家，**用"敌人头顶往下 contactSteppingTolerance"的位置做容差**，判定玩家不是从上方踩下来（`!player.IsPointUnderStep(stepping)`）才生效——击退（`contactPushback` 时给敌人自己一个反方向横向速度）+ 对玩家 `ApplyDamage(contactDamage)` + 播 `OnPlayerContact`

## WaypointEnemyState 敌人巡逻状态 (Enemy/States/WaypointEnemyState.cs)
- 敌人 AI 的"沿路点巡逻"状态，父类 `EnemyState`；内容 Odyssey 与镜像逐字相同，GUID 用 Odyssey 的 `a91fcc327e265d7468b1739a40a746f8`
- `OnStep` 逻辑：
  - `enemy.Gravity()` + `enemy.SnapToGround()`：照旧管下落与贴地
  - 取当前路点 `enemy.waypoints.current.position`，**把 y 换成敌人自己的 y**（只做水平移动，不会被路点的高低差带着走）
  - 算出方向与距离；`distance <= stats.waypointMinDistance`（默认 0.5）时 → `Decelerate()` 减速 + `waypoints.Next()` 切下一个路点；否则 → 按 `waypointAcceleration`/`waypointTopSpeed` 加速，且若 `stats.faceWaypoint` 为真则 `FaceDirectionSmooth()` 转向
  - （小细节：`var direction = head / distance` 在 distance 为 0 时会得到 NaN，但那种情况下会直接走"到达"分支，NaN 不会被用到）
- **注意**：`enemy.waypoints`（`WaypointManager`）的 `waypoints` 列表**不能为空**——`WaypointManager.current` 第一次读取时是 `waypoints[0]`，空列表会 `IndexOutOfRangeException`。本地 `Slime.prefab` 上这个列表现在是空的（`waypoints: []`），而它的 `states` 恰好只用 WaypointEnemyState，所以一运行就会每帧报错，需要在场景实例里把路点物体拖进去
- 依赖补全：本地 `Enemy.cs` 里 `waypoints` 属性 / `InitializeWaypointsManager()` / `Awake` 里的调用（原来因为 `WaypointManager` 未搬而注释）已在 2026-10-05 一并解开；`[RequireComponent(typeof(WaypointManager))]` 那行仍是注释（镜像里本来就是注释状态，未动）

## EnemyStats / EnemyStatsManager 敌人属性 (Enemy/EnemyStats.cs, Enemy/EnemyStatsManager.cs)
- `EnemyStats : EntityStats<EnemyStats>`（ScriptableObject，带 `[CreateAssetMenu]`，菜单 "PLAYER TWO/Platformer Project/Enemy/New Enemy Stats"），`EnemyStatsManager : EntityStatsManager<EnemyStats>`（挂组件、持有一组 EnemyStats 并能 `Change(index)`)
- 三组参数：General（`gravity` 35 / `snapForce` 15 / `rotationSpeed` 970 / `deceleration` 28 / `friction` 16 / `turningDrag` 28）、Contact Attack（`canAttackOnContact`/`contactPushback`/`contactOffset` 0.15/`contactDamage` 1/`contactPushBackForce` 18/`contactSteppingTolerance` 0.1）、View（`spotRange` 5 / `viewRange` 8）、Follow（`followAcceleration` 10 / `followTopSpeed` 4）、Waypoint（`faceWaypoint`/`waypointMinDistance` 0.5/`waypointAcceleration` 10/`waypointTopSpeed` 2）
- **内容取自 Odyssey 版**（比镜像多 `[CreateAssetMenu]` 特性），GUID 也用 Odyssey 的：EnemyStats `2924e3087121cc04881d0a0e1a2bc7a1`、EnemyStatsManager `4a2d66322a450104eb6f3fec868e114d`——这样 `Examples/Stats/RegularEnemyStats.asset` 和 `Slime.prefab` 上的槽位自动对上，不用手动重挂
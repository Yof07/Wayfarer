# AGENTS.md

## 项目概览

这是一个名为 `Wayfarer` 的 Unity 项目。

- Unity 版本：`2023.2.20f1c1`
- 解决方案文件：`Wayfarer.sln`
- 主要可编辑项目内容位于 `Assets/`
- 包依赖通过 `Packages/manifest.json` 管理
- 当前大部分玩法脚本使用 Unity 默认生成的 `Assembly-CSharp` 程序集

## 重要目录

- `Assets/PLAYER TWO/Platformer Project/Scripts/` - 当前主要编辑的玩法脚本目录。
- `Assets/GameMain/` - 项目自定义游戏内容目录。
- `Assets/ithappy/`、`Assets/characters_5_02/` - 导入的资源包以及演示/示例内容。
- `Packages/` - Unity 包清单和锁定数据。
- `ProjectSettings/` - Unity 项目配置。

除非明确要求，否则不要手动编辑生成目录或缓存目录：

- `Library/`
- `Temp/`
- `Logs/`
- `obj/`
- 生成的 `*.csproj`
- 生成的 `*.sln`，除非任务明确和 IDE/项目文件生成有关

## 验证方式

修改代码后：

- 在 Rider 或 Unity Console 中检查 C# 编译错误。
- 如果命令行可以访问 Unity，可以使用下面的批处理模式进行编译/测试检查：

```powershell
Unity.exe -batchmode -quit -projectPath "E:\Unity\Projects\Wayfarer" -runTests
```

如果 `Unity.exe` 不在 `PATH` 中，请使用已安装的 Unity `2023.2.20f1c1` 编辑器可执行文件。

## 协作注意事项

- 修改架构或命名前，先阅读附近相关文件。
- 不要格式化无关的导入资源或演示包脚本。
- 改动范围保持在用户请求的功能或解释内。
- 当当前项目缺少符号或需要从完整包补全代码时，对比镜像目录 `E:\Unity\Projects\OdysseyDemo第五章完整包\Assets\PLAYER TWO\Platformer Project\Scripts`，只添加当前缺失且被请求所需的对应文件或代码，不添加多余内容。
- 如果修改了序列化字段、场景、Prefab 或资源，需要说明 Unity 可能会更新对应的 `.meta`、`.unity` 或 `.prefab` 文件。

# Unity FPS 脚本说明（按当前代码同步）

> 本文档只描述 `Assets/Scripts` 目录里当前可见代码，避免和历史版本不一致。

## 当前脚本结构

```text
Assets/Scripts/
├─ Player/
│  ├─ GameManager.cs
│  ├─ PlayerInput.cs
│  ├─ PlayerSetup.cs
│  ├─ PlayerShooting.cs
│  └─ PlayerWeapon.cs
└─ Network/
   ├─ ClientNetworkTransform.cs
   └─ NetworkManagerUI.cs
```

## 功能概览（对应现有实现）

- 玩家输入：`PlayerInput` 读取 `Horizontal/Vertical`、`Mouse X/Y`、`Jump`，并调用控制器接口执行移动、旋转和推力。
- 联机角色初始化：`PlayerSetup` 依据 `IsLocalPlayer` 启停组件，关闭/恢复场景主相机，并按 `NetworkObjectId` 设置玩家名。
- 射击：`PlayerShooting` 使用摄像机前向做 `Physics.Raycast`（带 `LayerMask`），命中后通过 `ServerRpc` 上报。
- 命中信息显示：`GameManager` 维护静态字符串，并在 `OnGUI` 中显示最近一次命中信息。
- 联机入口 UI：`NetworkManagerUI` 绑定三个按钮，分别启动 `Host/Server/Client`。
- 位姿同步策略：`ClientNetworkTransform` 继承 `NetworkTransform`，返回非服务端权威（Client Authority）。

## 关键脚本说明

### `Player/PlayerInput.cs`

- 持有 `speed`、`lookSensitivity`、`thrusterForce` 参数。
- 每帧采集输入，计算速度向量和视角旋转量。
- 通过 `controller.Move()`、`controller.Rotate()`、`controller.Thrust()` 下发动作。
- 在 `Jump` 按住时调整 `ConfigurableJoint.yDrive`，模拟离地/贴地的不同弹簧行为。

### `Player/PlayerShooting.cs`

- 开火按键：`Fire1`。
- 命中检测：`Physics.Raycast(cam.position, cam.forward, ..., weapon.range, mask)`。
- 网络上报：`ShootServerRpc(string hittedName)`，并调用 `GameManager.UpdateInfo(...)`。

### `Player/PlayerSetup.cs`

- 非本地玩家：关闭 `componentsToDisable` 列表中的组件。
- 本地玩家：关闭场景主相机，避免和玩家相机冲突。
- 对象禁用时会恢复场景主相机。

### `Player/PlayerWeapon.cs`

- 轻量武器数据类（`name`、`damage`、`range`）。
- 当前默认值：`M4A1`、`10`、`100`。

### `Player/GameManager.cs`

- 静态方法 `UpdateInfo` 用于更新信息文本。
- `OnGUI` 绘制调试文本，位置固定在屏幕区域 `(200, 200, 200, 400)`。

### `Network/NetworkManagerUI.cs`

- 在 `Start()` 中给三个 `Button` 注册点击事件。
- 分别调用 `NetworkManager.Singleton.StartHost/StartServer/StartClient`。

### `Network/ClientNetworkTransform.cs`

- 重写 `OnIsServerAuthoritative()` 返回 `false`。
- 表示该同步组件采用客户端权威。

## 当前代码边界（重要）

- `PlayerInput` 依赖 `PlayerController` 类型，但该脚本 **不在当前目录可见文件中**。  
  如果项目里确实缺失该文件，会导致编译报错；如果它在其他目录，请以实际工程为准。
- 目前未看到完整战斗闭环（血量、死亡、重生、击杀统计等），命中后主要是信息显示。
- UI 仍是调试导向（`OnGUI` + 简单按钮），适合学习验证，不是成品 HUD。

## 运行与联机测试（当前可验证）

1. 用 Unity Hub 打开项目根目录（包含 `Assets/`、`Packages/`、`ProjectSettings/`）。
2. 在场景中确认有 `NetworkManagerUI` 组件，且 `hostBtn/serverBtn/clientBtn` 已绑定。
3. 在玩家对象上确认有 `PlayerSetup`、`PlayerInput`、`PlayerShooting` 及其依赖组件（如 Camera）。
4. Play 后点击 `Host/Server/Client`，开火命中后查看 `GameManager` 的屏幕文本输出。

> 说明：当前工作区只包含 `Assets/Scripts`，未包含 `.unity/.prefab` 文件，因此无法在文档中写死具体场景名和对象层级。
> 若你希望 README 包含精确路径，可补充以下信息后直接替换：
> - 入口场景：`Assets/Scenes/xxx.unity`
> - 玩家预制体：`Assets/Prefabs/xxx.prefab`
> - UI 根对象路径：`Canvas/.../NetworkManagerUI`

## 后续建议

- 补齐/确认 `PlayerController` 脚本来源，确保输入链路完整。
- 将命中显示升级为完整伤害流程（血量同步、死亡与重生）。
- 将 `OnGUI` 迁移到 UGUI/TMP，增加准星与基础 HUD。
- 增加射击反馈（开火间隔、后坐力、命中反馈）与网络一致性校验。

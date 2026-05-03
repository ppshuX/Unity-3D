# Unity FPS 脚本说明（按当前代码同步）

> 本文档描述 `Assets/Scripts` 当前可见脚本，重点覆盖 Lesson 6 的射击特效接入。

## 当前脚本结构

```text
Assets/Scripts/
├─ GameManager/
│  ├─ GameManager.cs
│  └─ MatchingSettings.cs
├─ Network/
│  ├─ ClientNetworkTransform.cs
│  └─ NetworkManagerUI.cs
└─ Player/
   ├─ Player.cs
   ├─ PlayerController.cs
   ├─ PlayerInput.cs
   ├─ PlayerSetup.cs
   ├─ PlayerShooting.cs
   ├─ PlayerWeapon.cs
   ├─ WeaponGraphics.cs
   └─ WeaponManager.cs
```

## 功能概览（对应现有实现）

- 移动输入：`PlayerInput` 负责移动、视角与跳跃推力输入。
- 联机初始化：`PlayerSetup` 在本地/远端玩家上执行差异化组件开关与注册。
- 武器切换：`WeaponManager` 管理主副武器实例化，并通过 `Q` + `ServerRpc/ClientRpc` 同步切枪。
- 射击判定：`PlayerShooting` 根据当前武器参数（射速、伤害、射程）执行射线检测并上报伤害。
- 射击特效（Lesson 6）：`PlayerShooting` 通过 `WeaponGraphics` 触发枪口火焰与命中材质特效。
- 生命与重生：`Player` 在服务端权威扣血，死亡后禁用组件并延时重生。
- 对局状态显示：`GameManager` 维护玩家字典并在 `OnGUI` 绘制实时血量。

## Lesson 6：射击特效链路

### 1) 武器特效容器：`Player/WeaponGraphics.cs`

- `muzzleFlash`：开枪瞬间播放的枪口火焰粒子。
- `metalHitEffectPrefab`：命中金属目标时实例化的粒子预制体。
- `stoneHitEffectPrefab`：命中石头/默认目标时实例化的粒子预制体。

### 2) 特效触发：`Player/PlayerShooting.cs`

- 每次 `Shoot()` 先调用 `OnShootServerRpc()`，再广播 `OnShootClientRpc()`，统一播放枪口火焰。
- 射线命中后调用 `OnHitServerRpc(hit.point, hit.normal, material)`，由各端执行 `OnHit(...)`。
- `OnHit(...)` 使用 `Quaternion.LookRotation(normal)` 让命中特效朝向法线，并在 1 秒后销毁。
- 当前材质枚举为 `Metal/Stone`，玩家命中走 `Metal`，其余命中走 `Stone`。

### 3) 武器模型与特效绑定：`Player/WeaponManager.cs`

- `EquipWeapon(...)` 实例化武器模型后缓存 `currentGraphics`。
- `GetCurrentGraphics()` 供 `PlayerShooting` 读取当前武器特效引用。
- 切枪后会自动切换到新武器的特效配置。

## Inspector 配置检查

1. 每把武器预制体根节点挂载 `WeaponGraphics`。
2. `WeaponGraphics.muzzleFlash` 已绑定粒子系统。
3. `WeaponGraphics.metalHitEffectPrefab` / `stoneHitEffectPrefab` 已绑定特效预制体。
4. 玩家物体同时挂载 `PlayerShooting` 与 `WeaponManager`。
5. `PlayerShooting.mask` 能命中目标层（Player 与场景碰撞体）。

## 当前代码边界

- 命中材质判定暂为简化版（玩家=Metal，其他=Stone），未按 Tag/Material 自动识别真实材质。
- 命中特效仅做本地播放与 1 秒销毁，未做对象池优化。
- HUD 仍是 `OnGUI` 调试样式，未接入正式 UI。

## 后续建议

- 将命中材质改为按 `Tag` 或 `PhysicMaterial` 自动判定。
- 对 `muzzleFlash` 与命中特效使用对象池，降低频繁 `Instantiate/Destroy` 开销。
- 增加音效、后坐力、换弹与击中标记，完善射击反馈闭环。

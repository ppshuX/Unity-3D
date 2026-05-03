# Lesson 6 打卡日志：射击特效（Muzzle Flash + Hit Effect）

> 主题：完成开枪与命中粒子特效的网络同步播放，并接入当前武器配置。

## 本节核心知识点

### 1) 武器特效数据抽象：`WeaponGraphics`

- 给武器模型新增统一特效入口：
  - `muzzleFlash`
  - `metalHitEffectPrefab`
  - `stoneHitEffectPrefab`
- 不同武器可配置不同特效，切枪后自动切换对应视觉反馈。

### 2) 开枪特效同步：`OnShootServerRpc + OnShootClientRpc`

- 本地调用 `OnShootServerRpc()` 上报开火事件。
- 服务器转发 `OnShootClientRpc()` 给所有客户端。
- 客户端执行 `OnShoot()`，播放 `weaponManager.GetCurrentGraphics().muzzleFlash`。

### 3) 命中特效同步：`OnHitServerRpc + OnHitClientRpc`

- 命中后上报命中点、法线与材质类型。
- 各端执行 `OnHit(pos, normal, material)`：
  - 根据材质选择金属或石头特效预制体；
  - 用 `Quaternion.LookRotation(normal)` 对齐命中面朝向；
  - `ParticleSystem.Emit(1)` + `Play()`；
  - `Destroy(..., 1f)` 自动回收临时对象。

### 4) 射击输入与切枪冲突处理

- 单发：`Input.GetButtonDown("Fire1")`。
- 连发：`InvokeRepeating("Shoot", 0f, 1f / currentWeapon.shootRate)`。
- 停止条件：`Input.GetButtonUp("Fire1") || Input.GetKeyDown(KeyCode.Q)`，避免切枪时持续连发。

---

## 本节实现结果（当前项目同步版）

### 关键流程

1. 本地玩家按下 `Fire1`。
2. `PlayerShooting.Shoot()` 先触发 `OnShootServerRpc()`。
3. 服务器广播 `OnShootClientRpc()`，全端播放枪口火焰。
4. 射线命中后：
   - 命中玩家：调用 `ShootServerRpc(name, damage)` 扣血，并触发 `Metal` 命中特效；
   - 命中其他目标：触发 `Stone` 命中特效。
5. 所有客户端在相同命中点播放对应粒子，视觉保持一致。

---

## 本节关键脚本

### `Player/WeaponGraphics.cs`

```csharp
public class WeaponGraphics : MonoBehaviour
{
    public ParticleSystem muzzleFlash;
    public GameObject metalHitEffectPrefab;
    public GameObject stoneHitEffectPrefab;
}
```

### `Player/WeaponManager.cs`（新增特效访问）

```csharp
private WeaponGraphics currentGraphics;

public WeaponGraphics GetCurrentGraphics()
{
    return currentGraphics;
}
```

### `Player/PlayerShooting.cs`（特效触发）

```csharp
private void OnShoot()
{
    weaponManager.GetCurrentGraphics().muzzleFlash.Play();
}

private void OnHit(Vector3 pos, Vector3 normal, HitEffectMaterial material)
{
    GameObject prefab = material == HitEffectMaterial.Metal
        ? weaponManager.GetCurrentGraphics().metalHitEffectPrefab
        : weaponManager.GetCurrentGraphics().stoneHitEffectPrefab;

    GameObject obj = Instantiate(prefab, pos, Quaternion.LookRotation(normal));
    ParticleSystem ps = obj.GetComponent<ParticleSystem>();
    ps.Emit(1);
    ps.Play();
    Destroy(obj, 1f);
}
```

---

## Inspector 配置检查（本节重点）

- 武器预制体根节点挂载 `WeaponGraphics`。
- `muzzleFlash` 绑定枪口火焰粒子系统。
- `metalHitEffectPrefab`、`stoneHitEffectPrefab` 绑定命中特效预制体。
- `PlayerShooting` 与 `WeaponManager` 在同一玩家对象。
- `PlayerShooting.mask` 可命中玩家与场景层。

---

## 本节复盘

- 已完成“开枪瞬时反馈 + 命中反馈”的联机同步链路。
- 特效配置与武器解耦，切枪后可自动切换特效资源。
- 为后续扩展（按 Tag 判定材质、对象池、音效、后坐力）打下基础。

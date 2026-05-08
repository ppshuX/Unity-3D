# Lesson 9.2 打卡日志：HTTP 房间列表、建房与退房

> 主题：用 `UnityWebRequest` 访问房间列表 Web 服务，动态生成房间按钮；建房时向服务端申请端口并用 NGO `StartClient` 进入；房主退出游戏时通知服务端移除房间。

## 本节核心知识点

### 1) HTTP 与 `JsonUtility`

- **`UnityWebRequest.Get(uri)`** 拉取 JSON，通过 **`JsonUtility.FromJson<T>(text)`** 反序列化。
- DTO 类型需带 **`[System.Serializable]`**，字段名与 JSON 键一致（区分大小写）。
- 本工程 DTO：`Room`、`GetRoomListResponse`、`BuildRoomResponse`、`RemoveRoomResponse`（均在 `Assets/Scripts/Network/response/`）。

### 2) 三个接口（本仓库基址）

- 基址常量：`NetworkManagerUI.ApiBase` = **`http://49.232.65.186:8000`**（与课件 AcWing 地址不同，可按需改一处常量）。
- **`GET /fps/get_room_list/`** → `GetRoomListResponse`，含 `rooms` 数组（`name`、`port`）。
- **`GET /fps/build_room/`** → `BuildRoomResponse`，`error_message == "success"` 时取 `port`（及 `name`）。
- **`GET /fps/remove_room/?port=<端口>`** → `RemoveRoomResponse`，退房上报。

### 3) UI：刷新、建房与动态房间按钮

- **Refresh**：重新请求列表，销毁旧动态按钮，按课件布局 `localPosition`（`-21`, `92 - k*60`, `0`）实例化 **`roomButtonPrefab`** 到 **`menuUI`** 下。
- 预制体需含 **`Button`**，子物体上有 **`TextMeshProUGUI`** 显示 `room.name`。
- **Build**：请求成功后设置 `UNetTransport` 端口、记录 **`buildRoomPort`**、**`StartClient()`** 并收起菜单（与课件一致）。
- 点击某一房间按钮：把传输层端口设为该房间 **`port`**，再 **`StartClient()`**。

### 4) 房主退房：`OnApplicationQuit`

- 若 **`buildRoomPort != -1`**（本局曾成功建房），退出时 **`RemoveRoom()`** 请求服务端删房；协程在退出阶段可能无法跑完，属 Unity 限制，课件同样思路。

### 5) 与 9.1 共用：命令行 `-port` / `-lauch-as-server`

- **`ApplyCommandLineConfig()`** 优先处理 `-port`、**`-lauch-as-server`**（课件拼写）。
- 专服模式下 **`StartServer()`** 后 **`DestroyAllButtons()`** 并 **return**，不再执行 **`InitButtons` / `RefreshRoomList`**，避免 UI 已销毁仍访问引用。

### 6) 本仓库相对课表的实现细节（便于维护）

- **`foreach` + 按钮 `onClick`**：使用循环内局部变量 **`roomPort` / `roomName`** 绑定，避免闭包全部指向最后一个房间。
- **`using (UnityWebRequest ...)`** 释放原生请求。
- 传输层、`NetworkManager.Singleton`、预制体/`menuUI` 等 **判空与日志**，减少打包场景漏绑时的静默失败。

---

## 相关脚本路径

| 文件 | 作用 |
|------|------|
| `Network/NetworkManagerUI.cs` | 菜单逻辑、HTTP、专服入口 |
| `Network/response/Room.cs` | 单房间数据结构 |
| `Network/response/GetRoomListResponse.cs` | 列表接口响应 |
| `Network/response/BuildRoomResponse.cs` | 建房接口响应 |
| `Network/response/RemoveRoomResponse.cs` | 退房接口响应 |

---

## Inspector / 场景检查清单

1. `NetworkManager` 物体：`NetworkManager`、`UNetTransport`、`NetworkManagerUI`（同物体以便 `GetComponent<UNetTransport>()`）。
2. `NetworkManagerUI`：**Refresh**、**Build** 按钮；**菜单 Canvas**（`menuUI`）；**房间按钮预制体**（含 Button + TMP 文本）。
3. 工程已引用 **TextMesh Pro**。
4. 若使用 **Android** 等非编辑器平台，明文 **HTTP** 可能需在 Player 设置中允许 **Cleartext**（视系统策略而定）。

## 版本说明

- 与 Lesson 9.1 相同：**Unity 2021.3** 下可使用 **`UNetTransport`**；更高版本 Unity 若关闭 UNet，需评估 **Unity Transport (UTP)**。

---

*参考：AcWing 作者 yxc — [课上代码](https://www.acwing.com/activity/content/code/content/6331705/)*

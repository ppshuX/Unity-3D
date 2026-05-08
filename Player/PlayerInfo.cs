using TMPro;
using UnityEngine;

/// <summary>
/// 头顶血条 billboard。与扣血无关：Player 根节点请保持 Tag「Player」；
/// 朝向谁由「当前在渲染的相机」决定——优先 Camera.main（FPS 子相机请打 MainCamera），
/// 未配对时再用场景里最合适的一台启用相机兜底。
/// </summary>
public class PlayerInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI playerName;
    [SerializeField]
    private Transform playerHealth;
    [SerializeField]
    private Transform infoUI;

    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        playerName.text = transform.name;
        playerHealth.localScale = new Vector3(player.GetHealth() / 100f, 1f, 1f);

        var cam = GetViewCamera();
        if (cam == null) return;

        // 仅用水平朝向（绕 Y），避免课件里全角度 LookAt + Rotate(180,Y,0) 在第三人称下令 TMP 左右镜像
        Vector3 toCam = cam.transform.position - infoUI.position;
        toCam.y = 0f;
        if (toCam.sqrMagnitude < 1e-5f) return;

        infoUI.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up);
    }

    private static Camera GetViewCamera()
    {
        var main = Camera.main;
        if (main != null && main.isActiveAndEnabled && main.gameObject.activeInHierarchy)
            return main;

        Camera best = null;
        foreach (var c in Object.FindObjectsOfType<Camera>())
        {
            if (c == null || !c.enabled || !c.gameObject.activeInHierarchy) continue;
            if (best == null || c.depth > best.depth)
                best = c;
        }

        return best;
    }
}

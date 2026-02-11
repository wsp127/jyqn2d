using UnityEngine;

public class TimeShift : MonoBehaviour
{
    [Header("设置")]
    public float worldOffset = 100f; // 两个世界的垂直距离 (Y轴)

    private bool isModern = true; // 当前是否在现代

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchWorld();
        }
    }

    void SwitchWorld()
    {
        // 只在 Y 轴移动
        Vector3 moveDistance = new Vector3(0, worldOffset, 0);

        if (isModern)
        {
            // --- 穿越到古代 (+100) ---
            transform.position += moveDistance;
            
            // 同步移动摄像机
            // 注意：摄像机必须是主摄像机 (Main Camera)
            Camera.main.transform.position += moveDistance;
        }
        else
        {
            // --- 回到现代 (-100) ---
            transform.position -= moveDistance;
            Camera.main.transform.position -= moveDistance;
        }

        // 切换状态
        isModern = !isModern;
        Debug.Log(isModern ? "回到现代" : "穿越到古代");
    }
}
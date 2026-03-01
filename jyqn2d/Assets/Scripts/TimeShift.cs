using UnityEngine;
using System.Collections; // 必须引入这个才能使用协程

public class TimeShift : MonoBehaviour
{
    [Header("世界设置")]
    public float worldOffset = 100f; // 两个世界的垂直距离 (Y轴)

    [Header("过渡动画设置")]
    public float transitionDuration = 0.5f; // 整个黑屏过渡的时间
    public CanvasGroup fadeCanvasGroup;     // 拖入挂有 CanvasGroup 的全屏黑图

    private bool isModern = true;
    private bool isTransitioning = false; // 状态锁：防止在过渡期间玩家狂按 E 键导致坐标错乱

    void Update()
    {
        // 只有当按下了 E 键，且当前【不在】过渡中时，才允许穿梭
        if (Input.GetKeyDown(KeyCode.E) && !isTransitioning)
        {
            StartCoroutine(TransitionRoutine());
        }
    }

    // 这是一个协程 (Coroutine)，可以用来编写随时间推移执行的动画代码
    IEnumerator TransitionRoutine()
    {
        // 1. 上锁
        isTransitioning = true;

        // 2. 屏幕逐渐变黑 (Fade Out)
        float timer = 0f;
        float halfDuration = transitionDuration / 2f;
        
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            // Mathf.Lerp 是平滑插值，让 Alpha 从 0 匀速变到 1
            if (fadeCanvasGroup != null) 
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / halfDuration);
            }
            yield return null; // 等待下一帧再继续循环
        }

        // 3. 在屏幕完全黑掉的瞬间，执行位置切换！(玩家看不见突变)
        SwitchWorldPosition();

        // 4. 屏幕逐渐变亮 (Fade In)
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            if (fadeCanvasGroup != null) 
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / halfDuration);
            }
            yield return null;
        }

        // 5. 解锁，允许下一次穿梭
        isTransitioning = false;
    }

    // 将原本的移动逻辑单独抽离出来
    void SwitchWorldPosition()
    {
        Vector3 moveDistance = new Vector3(0, worldOffset, 0);

        if (isModern)
        {
            transform.position += moveDistance;
            Camera.main.transform.position += moveDistance;
        }
        else
        {
            transform.position -= moveDistance;
            Camera.main.transform.position -= moveDistance;
        }

        isModern = !isModern;
        Debug.Log(isModern ? "回到现代" : "穿越到古代");
    }
}
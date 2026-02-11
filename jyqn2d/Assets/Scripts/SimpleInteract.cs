using UnityEngine;

public class SimpleInteract : MonoBehaviour
{
    private bool canInteract = false;
    private SpriteRenderer myRenderer; // 2D 用 SpriteRenderer，不是 Renderer

    void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.F))
        {
            // 随机变色
            myRenderer.color = new Color(Random.value, Random.value, Random.value);
            Debug.Log("交互成功！");
        }
    }

    // 2D 物理必须用 OnTriggerEnter2D (带 2D 后缀)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Player") // 确保主角物体名字叫 Player
        {
            canInteract = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "Player")
        {
            canInteract = false;
        }
    }
}
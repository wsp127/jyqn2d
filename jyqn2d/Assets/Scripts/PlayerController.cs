using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 6f; // 2D 可以在这里调快一点
    public float jumpForce = 12f; // 2D 跳跃力度通常需要大一点

    [Header("地面检测")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.3f; // 检测圈大小
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 左右移动
        float xInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);

        // 2. 转向 (翻转 X 轴缩放)
        if (xInput != 0)
        {
            transform.localScale = new Vector3(xInput > 0 ? 1 : -1, 1, 1);
        }

        // 3. 跳跃
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        // 4. 地面检测 (物理检测建议在 FixedUpdate)
        // 使用 OverlapCircle 画一个圆来检测脚底有没有碰到 Ground 层
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    // 辅助线：在编辑器里画出脚底的检测圈，方便调试
    void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
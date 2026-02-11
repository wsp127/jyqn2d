using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("基础属性")]
    public float moveSpeed = 9f;      // 稍微调大一点，配合加速度
    public float jumpForce = 16f;     // 跳跃力度
    public float climbSpeed = 5f;     // 爬墙速度 (预留)

    [Header("相关参数")]
    public float acceleration = 60f;      // 地面加速度 (越大起步越快)
    public float deceleration = 50f;      // 地面减速度 (越大刹车越快)
    public float airAcceleration = 30f;   // 空中加速度 (空中控制力)
    public float airDeceleration = 10f;   // 空中阻力
    public float fallGravityMult = 2.5f;  // 下落时的重力倍数 (让下落更干脆)
    public float jumpCutGravityMult = 4f; // 小跳时的重力倍数 (松开空格快速下落)

    [Header("容错机制")]
    public float coyoteTime = 0.1f;       // 土狼时间 (离开地面后多久内还能跳)
    public float jumpBufferTime = 0.15f;  // 跳跃缓存 (落地前多久按跳也能触发)

    [Header("检测设置")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    // --- 内部变量 ---
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canDoubleJump;
    
    // 计时器
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // 输入缓存
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 获取输入
        moveInput = Input.GetAxisRaw("Horizontal");

        // 2. 转向逻辑 (翻转模型)
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(moveInput > 0 ? 1 : -1, 1, 1);
        }

        // 3. 土狼时间计时 (Coyote Time)
        // 如果在地面，计时器补满；如果离地，开始倒计时
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true; // 落地刷新二段跳
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 4. 跳跃缓存计时 (Jump Buffer)
        // 如果按了跳跃键，给缓存计时器充能
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) // 兼容空格键
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 5. 执行跳跃逻辑
        // 条件：【想跳(缓存>0)】 且 【能跳(土狼>0 或 有二段跳)】
        if (jumpBufferCounter > 0f)
        {
            if (coyoteTimeCounter > 0f)
            {
                // --- 一段跳 (消耗土狼时间) ---
                PerformJump();
                jumpBufferCounter = 0f; // 清空想跳的意图
            }
            else if (canDoubleJump)
            {
                // --- 二段跳 (消耗次数) ---
                PerformJump();
                canDoubleJump = false;
                jumpBufferCounter = 0f;
            }
        }

        // 6. 可变跳跃高度 (松开按键，重力瞬间变大，实现小跳)
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.velocity.y > 0)
            {
                // 瞬间削减向上的速度，实现“松手即停”
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }
    }

    void FixedUpdate()
    {
        // 1. 地面检测
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // 2. 施加更真实的移动力 (Run)
        Run();

        // 3. 施加更好的重力 (Better Gravity)
        ApplyGravityScale();
    }

    // --- 核心功能函数 ---

    void Run()
    {
        // 计算目标速度
        float targetSpeed = moveInput * moveSpeed;
        
        // 计算当前速度差
        float speedDif = targetSpeed - rb.velocity.x;
        
        // 根据是否在地面，选择不同的加减速度 (手感调优的关键)
        float accelRate;
        if (isGrounded)
        {
            // 在地面：如果你想动用加速度，如果你想停(Input=0)用减速度
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        }
        else
        {
            // 在空中：阻力小一点，控制力弱一点
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? airAcceleration : airDeceleration;
        }

        // 施加力 F = m * a
        // ForceMode2D.Force 使得移动有“重量感”
        float movement = speedDif * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    void PerformJump()
    {
        // 重置 Y 轴速度（确保二段跳力度一致，不会被下落速度抵消）
        rb.velocity = new Vector2(rb.velocity.x, 0);
        
        // 施加跳跃力 (Impulse 是瞬间爆发力)
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        // 跳跃瞬间清空土狼时间（防止连跳）
        coyoteTimeCounter = 0f;
    }

    void ApplyGravityScale()
    {
        // 如果正在下落 (vy < 0)，重力加倍 -> 掉得快
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallGravityMult;
        }
        // 如果正在上升但没按住跳跃键，重力超级加倍 -> 小跳
        else if (rb.velocity.y > 0 && !(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)))
        {
            rb.gravityScale = jumpCutGravityMult;
        }
        // 其他情况恢复默认重力 (1)
        else
        {
            rb.gravityScale = 1f;
        }
    }

    // 辅助线
    void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
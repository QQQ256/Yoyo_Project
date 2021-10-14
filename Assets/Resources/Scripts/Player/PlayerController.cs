using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    [Header("Player in the room")] 
    public GameObject Bag;
    public bool hasBag;
    public bool hasYoyo;
    public static PlayerController Instance;
    public float speed, jumpForce;
    [Header("Player State")] 
    public float health;
    public bool isDead;
    [Header("States Check")] 
    public bool isGround;
    public bool isJump;
    public bool canJump2;
    public bool jumpTwice;
    public bool isIdle;
    public bool isPlatform;
    [Header("Ground Check")] 
    public Transform groundCheck,platformCheck;
    public LayerMask groundLayer, platformLayer;
    public float downTime;
    public float checkRadius;
    [Header("FX Check")] 
    public GameObject jumpFX;
    public GameObject fallFX;
    public GameObject leftRunFX;
    [Header("Wall Run")] 
    public float playerHeight;

    private BoxCollider2D _collider2D;
    #region private members
    
    public Rigidbody2D rb;
    private Animator _animator;
    private float horizontalInput;
    private float verticalInput;
    
    private int animationCount;
    #endregion

    //买了几个技能的判断
    // private int _purchasedSkill;
    private void Awake()
    {
        Instance = this;
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            isIdle = true;
        }
    }

    void Start()
    {
        Bag.SetActive(false);
        _collider2D = GetComponent<BoxCollider2D>();
        // GameManager.Instance.IsPlayer(this);
        if(isIdle) return;
        //获取面板中的image
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isIdle)
            return;
        if (isDead)
        {
            // _animator.SetBool("dead",isDead);
            _animator.SetTrigger("Dead");
            return;
        }
        InputCheck();
    }

    private void InputCheck()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGround)
            {
                Jump();
                canJump2 = true;
            }
            // else if (canJump2)
            // {
            //     JumpTwice();
            //     canJump2 = false;
            //     jumpFX.SetActive(false);
            // }
            
        }
        
        if (Input.GetButtonDown("Vertical"))
        {
            IsPlatform();
        }

      
    }

    private void FixedUpdate()
    {
        if(isIdle)
            return;
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        PhysicsCheck();
        Movement();
        
    }

    private void Movement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
        if (horizontalInput == 0)
        {
            leftRunFX.SetActive(false);
        }
        
        if (horizontalInput != 0 && isGround)
        {
            leftRunFX.SetActive(true);
            transform.localScale = new Vector3(horizontalInput, 1, 1);
        }
        else if (horizontalInput != 0 && isJump)
        {
            transform.localScale = new Vector3(horizontalInput, 1, 1);
            leftRunFX.SetActive(false);
        }
    }

    void Jump()
    {
        isJump = true;
        jumpFX.SetActive(true);
        jumpFX.transform.position = transform.position + new Vector3(0, -0.45f, 0);
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        rb.gravityScale = 4;
        if (isJump && gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }

    void JumpTwice()
    {
        jumpTwice = true;
        jumpFX.SetActive(true);
        jumpFX.transform.position = transform.position + new Vector3(0, -0.45f, 0);
        rb.velocity = new Vector2(rb.velocity.x, 12);
        rb.gravityScale = 4;
    }

    void WallRunCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(2, 0), 2f,groundLayer);
        if (!hit.collider.CompareTag("Wall"))
            return;
        Debug.Log("hello"); 
        _animator.SetTrigger("wallJump");

        // RaycastHit2D[] hit2D = Physics2D.RaycastAll(transform.position, new Vector2(0, -1f), groundLayer);
        // foreach (var items in hit2D)
        // {
        //     if (items.collider.CompareTag("Platforms"))
        //     {
        //         if (Mathf.Abs(transform.position.y - items.collider.transform.position.y) > 0)
        //         {
        //             Debug.Log(transform.position.y-items.collider.transform.position.y);
        //             items.collider.enabled = false;
        //             Debug.Log("player"+transform.position);
        //             Debug.Log("obj"+items.collider.transform.position);
        //         }
        //         items.collider.enabled = true;
        //         //     isPlatform = true;
        //       
        //     }
        // }
    }

    public void FallFXFinish()//animation event
    {
        fallFX.SetActive(true);
        fallFX.transform.position = transform.position + new Vector3(0, -0.75f, 0);
    }

    private void PhysicsCheck()
    {
        var position = groundCheck.position;
        var position1 = platformCheck.transform.position;
        isGround = Physics2D.OverlapCircle(position, checkRadius, groundLayer)
                   || Physics2D.OverlapCircle(position1, checkRadius, platformLayer);
        isPlatform = Physics2D.OverlapCircle(position1, 0.02f, platformLayer);
        if (isGround)
        {
            rb.gravityScale = 1;
            isJump = false;
            jumpTwice = false;
        }
        else if (!isJump && !isGround)//上平台之后修复重力变成1
        {
            rb.gravityScale = 4;
        }

        var direction = transform.localScale.x;
        Vector2 grabDir = new Vector2(direction, 0f);
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(direction, playerHeight), grabDir, 0.4f,groundLayer);
        // if (hit.collider.CompareTag("Wall"))
        // {
        //     Debug.DrawRay(transform.position,grabDir);
        // }
         
    }
    

    #region 下落检测方法

    private void IsPlatform()
    {
        verticalInput = Input.GetAxis("Vertical");
        if (isPlatform && verticalInput == 0f)//按下了下落键
        {
            // _animator.SetTrigger("fall");
            gameObject.layer = LayerMask.NameToLayer("OneWayPlatform");
            Invoke(nameof(RestoreLayer),downTime);
        }
        
    }

    public void RestoreLayer()
    {
        isGround = false;
        if (!isGround && gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }

    #endregion
    
    

    public void GetHit(float damage)
    {
        //玩家收到伤害之后播放完受伤动画之后再少血
        //算是可以模拟一个受伤之后无敌的效果
        if (!_animator.GetCurrentAnimatorStateInfo(1).IsName("player_hit"))
        {
            health -= damage;
            if (health < 1)
            {
                health = 0;
                isDead = true;
            }
            _animator.SetTrigger("hit");
        }
    }

    #region Animation events
    //animation event
   
    #endregion
    
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.gameObject.CompareTag("Diamond"))//这么写是为了防止吃宝石数量错误 通过下面的调用来增加宝石数量。
    //     {
    //         other.GetComponent<Animator>().Play("DiamondCollected");//必须要这么写 否则会在吃宝石1的时候 触发宝石2的动画 然后报错宝石1的animator空引用
    //         
    //     }
    // }
    
    // public void CollectCoin()//在CollectCoin中使用
    // {
    //     CoinUI.instance.currentCoinNumber += 1;
    // }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using DG.Tweening;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class Mechanics : MonoBehaviour
{
    
    public GameObject yoyo;
    public GameObject tempYoyo;
    public int yoyoRopeLength;
    public float direction;//the direction the player stands
    public bool isInstantiated;//whether the yoyo is instantiated or not
    public bool isUseSkills;//when use one skill, other skill cannot be used
    public Vector2 destinationPoint;


    [Header("Mechanic1&2")]
    public bool isMechanic_1,isMechanic_2;//control whether use the mechanic-1 or not

    [Header("Dynamic1")]
    public bool isDynamic_1;

    public bool isJumped;

    public Transform dynamic1_InstantiatePoint;


    private Rigidbody2D rd;
    private Animator _animator;
    
    private Vector2 walkDogPoint;//use for mechanic_2, for walk dog destination point
    private Vector2 playerPoint;


    private bool isReturn;
    private bool grabItems;
    private bool yoyoMaxDistance;

    private WallCheck WallCheck;
    
    private static Mechanics _instance;

    public static Mechanics Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<Mechanics>();
            }

            return _instance;
        }
        
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        rd = GetComponent<Rigidbody2D>();
        WallCheck = FindObjectOfType<WallCheck>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        playerPoint = transform.position;
        direction = transform.localScale.x;

        if (!PlayerController.Instance.hasYoyo)
            return;

        #region mechanic_1

        if (Input.GetMouseButtonDown(0) && !isUseSkills)
        {
            isMechanic_1 = true;
            isUseSkills = true;
            InstantiateYoyo(direction);
        }
        
        if (isInstantiated && isMechanic_1)
        {
            if (!tempYoyo)
            {
                return;
            }
            tempYoyo.transform.position = Vector2.MoveTowards(tempYoyo.transform.position, destinationPoint, 3 * Time.deltaTime);
            YoyoReturnToPlayer();
            tempYoyo.transform.position = Vector2.MoveTowards(tempYoyo.transform.position, destinationPoint, 3 * Time.deltaTime);
        }

        #endregion

        #region mechanic_2
        if (Input.GetMouseButtonDown(1) && !isUseSkills  && PlayerController.Instance.isGround && !WallCheck.isWall)//player cannot use walk dog when he jumps
        {
            if (isUseSkills)
            {
                return;
            }
            walkDogPoint = new Vector3(playerPoint.x + direction * 20f, playerPoint.y);
            isMechanic_2 = true;
            isUseSkills = true;
            InstantiateYoyo(direction);
        }
        
        if (isInstantiated && isMechanic_2)
        {
            if (!tempYoyo)
            {
                return;
            }
            tempYoyo.transform.position = Vector2.MoveTowards(tempYoyo.transform.position, walkDogPoint, 6 * Time.deltaTime);
            WalkDog();
            if (yoyoMaxDistance)//max rope length of yoyo, player move with yoyo together
            {
                if (!WallCheck.isWall)
                {
                    //when the player in the gap, player should not chase the yoyo, instead, it will stay in that place
                    transform.position = Vector2.MoveTowards(transform.position, walkDogPoint, 6 * Time.deltaTime);
                    _animator.SetFloat("speed",1f);//TODO
                    if (Vector2.Distance(transform.position,walkDogPoint) <= 0.5f)
                    {
                        transform.position = walkDogPoint;
                    } 
                }
                else
                {
                    destinationPoint = playerPoint;
                    walkDogPoint = playerPoint;
                    
                }
                
            }
            // tempYoyo.transform.position = Vector2.MoveTowards(tempYoyo.transform.position, destinationPoint, 3 * Time.deltaTime);
           
        }
        

        #endregion

        #region dynamic_1

        if (Input.GetMouseButtonDown(2) && !isUseSkills)
        {
            isDynamic_1 = true;
            isUseSkills = true;
            InstantiateYoyoInYAxis();
        }

        if (isDynamic_1 && isJumped && rd.velocity.y < 0.1f)
        {
            YoyoReturnToPlayer();
            if (rd.velocity.y < 0.1f)
            {
                destinationPoint = playerPoint;
                tempYoyo.transform.position = Vector2.MoveTowards(tempYoyo.transform.position, destinationPoint, 7 * Time.deltaTime);
            }
            // tempYoyo.transform.position = Vector2.MoveTowards(tempYoyo.transform.position, destinationPoint, 7 * Time.deltaTime);
        }
        #endregion
    }

    private void FixedUpdate()
    {
        // isWall = Physics2D.OverlapBox(transform.position,)
    }


    void InstantiateYoyo(float direction)
    {
        switch (direction)
        {
            case 1:
                tempYoyo = Instantiate(yoyo, playerPoint, Quaternion.identity);
                tempYoyo.name = "YOYO";
                isInstantiated = true;
                if (isMechanic_2)
                {
                    destinationPoint = walkDogPoint;
                    return;
                }
                destinationPoint = new Vector2(playerPoint.x + yoyoRopeLength, playerPoint.y);
                break;
            case -1:
                tempYoyo = Instantiate(yoyo, playerPoint, Quaternion.identity);
                tempYoyo.name = "YOYO";
                isInstantiated = true;
                if (isMechanic_2)
                {
                    destinationPoint = walkDogPoint;
                    return;
                }
                destinationPoint = new Vector2(playerPoint.x - yoyoRopeLength, playerPoint.y);
                // Destroy(tempYoyo,5f);
                break;
            default:
                break;
        }
    }

    void InstantiateYoyoInYAxis()
    {
        //the size of the yoyo should be pay more attention to!!!
        //based on the size of yoyo to set the instantiate point
        tempYoyo = Instantiate(yoyo, dynamic1_InstantiatePoint.transform.position, Quaternion.identity);
        rd.velocity = new Vector2(playerPoint.x, 15f);
        tempYoyo.name = "YOYO";
        isInstantiated = true;
        tempYoyo.layer = LayerMask.NameToLayer("Items");
        destinationPoint = playerPoint;
    }

    #region mechanic_1

    void YoyoReturnToPlayer()
    {
        // has good effect!
        // if (Vector2.Distance(tempYoyo.transform.position,destinationPoint) < 0.2f)
        // {
        //     destinationPoint = playerPoint;
        //     isReturn = true;
        // }
        //
        // if (Vector2.Distance(tempYoyo.transform.position,destinationPoint) < 0.2f && isReturn)
        // {
        //     destinationPoint = new Vector2(0, 0);
        //     isReturn = false;
        // }
        if (Vector2.Distance(tempYoyo.transform.position,destinationPoint) < 0.4f)//0.4 the yoyo is in max distance
        {
            destinationPoint = playerPoint;
            if (Vector2.Distance(tempYoyo.transform.position,destinationPoint) < 1f)//0.3
            {
                if (grabItems && isMechanic_1)
                {
                    Transform childGameObject = tempYoyo.transform.GetChild(0);//make grab items not the yoyo's child object
                    childGameObject.transform.parent = null;
                    // if (Vector2.Distance(childGameObject.transform.position,playerPoint) < 0.5f)
                    // {
                    //     Destroy(childGameObject.gameObject);
                    // }
                    grabItems = false;
                    // Destroy(childGameObject.gameObject);
                    // isMechanic_1 = false;
                }
                
                // isMechanic_2 = false;
                if (tempYoyo.GetComponentsInChildren<Transform>(true).Length <= 1 && Vector2.Distance(tempYoyo.transform.position,destinationPoint) < 0.2f)//拽着走的时候，超过5f，就直接子母分开，然后等到yoyo返回pkayer之后再删掉
                {
                    destinationPoint = new Vector2(0, 0);//set destination point to a new point
                    isInstantiated = false;
                    grabItems = false;
                    DestroyYoyo();
                }
                
                // Invoke("DestroyYoyoLate",0.02f);//sometimes the item is still the child object of the yoyo, it will be destroyed at the same time
            }
            else if (Vector2.Distance(tempYoyo.transform.position,destinationPoint) > 5f && grabItems && Vector2.Distance(tempYoyo.transform.position,destinationPoint) < 0.2f)
            {
                if (isMechanic_1 && grabItems)
                {
                    Transform childGameObject = tempYoyo.transform.GetChild(0);//make grab items not the yoyo's child object
                    childGameObject.transform.parent = null;
                }
                
                if (tempYoyo.GetComponentsInChildren<Transform>(true).Length <= 1)
                {
                    destinationPoint = new Vector2(0, 0);//set destination point to a new point
                    isInstantiated = false;
                    grabItems = false;
                    DestroyYoyo();
                }
            }
            //fix when player use walk dog, and fall into the gap, the yoyo should be back
        }
        else if (WallCheck.isWall && Vector2.Distance(tempYoyo.transform.position,destinationPoint) >= 1f)
        {
            tempYoyo.transform.position = Vector2.MoveTowards(tempYoyo.transform.position, destinationPoint, 3 * Time.deltaTime);
            // Debug.Log("FUCK");
        }
    }

    void DestroyYoyo()
    {
        isUseSkills = false;
        isMechanic_1 = false;
        isMechanic_2 = false;
        yoyoMaxDistance = false;
        isJumped = false;
        Destroy(tempYoyo);
    }

    #endregion

    #region mechanic_2

    void WalkDog()
    {
        bool yoyoIsHere = false;
        if (Vector2.Distance(playerPoint,tempYoyo.transform.position) >= 5f)
        {
            yoyoMaxDistance = true;//let the player follow the max length of the yoyo
        }

        if (Vector2.Distance(transform.position,walkDogPoint) <= 0.5f)
        { 
            yoyoIsHere = true;//player is close to the destination point
            transform.position = walkDogPoint;
        }
        
        if (Vector2.Distance(tempYoyo.transform.position,playerPoint) <= 1f && yoyoIsHere)
        {
            YoyoReturnToPlayer();
        }
    }
    

    #endregion

    // bool YoyoHitWall()
    // {
    //     
    // }
    

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Breakfast")  && isMechanic_1)
        {
            grabItems = true;
        }

        if (other.collider.CompareTag("Yoyo") && isDynamic_1)
        {
            tempYoyo.layer = LayerMask.NameToLayer("Yoyo");
            isJumped = true;
        }
    }
}

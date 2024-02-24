using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 控制玩家移動
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    public float walkSpeed = 10f; // 移動速度
    public float runSpeed = 15f; // 奔跑速度
    public float jumpForce = 3f; // 跳躍力度
    private Vector3 velocity; // 設置玩家Y軸的一個衝量變化
    private Vector3 moveDirection; // 設置移動方向
    public float gravity = -9f; // 設置重力

    public Transform groundCheck; // 地面檢測物件
    private float groundDistance = 0.4f; // 與地面的距離
    public LayerMask groundMask;
    private bool isJump; // 判斷是否在跳躍
    private bool isGround; // 判斷是否在地面上
    public bool isWalk; // 判斷是否在行走
    public bool isRun; // 判斷是否在奔跑
    private bool isCrouch; // 判斷是否蹲下

    [SerializeField] private float slopeForce = 6.0f; // 走斜坡施加的力(是一個乘量)
    [SerializeField] private float slopeForceRayLength = 2.0f; // 斜坡射線長度（自定義量）

    [Header("鍵位設置")]
    [SerializeField] [Tooltip("跳躍按鍵")] private string jumpInputName = "Jump";
    [SerializeField] [Tooltip("奔跑按鍵")] private KeyCode runInputName;
    [SerializeField] [Tooltip("下蹲按鍵")] private KeyCode crouchInputName;

    private AudioSource audioSource;
    public AudioClip walkingSound;
    public AudioClip runningSound;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = walkingSound;
        audioSource.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        Move();
        Crouch();
    }

    /// <summary>
    /// 判斷是否在斜坡上
    /// </summary>
    /// <returns></returns>
    public bool OnSlope()
    {
        if (isJump)
            return false;

        RaycastHit hit;
        // 向下打出射線(檢查是否在斜坡上)
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength))
        {
            // 如果觸碰到的點的法線，不是在（0，1，0）這個方向上的，那麼就人物處在斜坡上
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 地面檢測
    /// </summary>
    public void CheckGround()
    {
        // 在 groundCheck 位置上做一個球體檢測判斷是否處在地面上
        isGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        // 如果處在地面上，重力設置成一個固定值
        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    /// <summary>
    /// 跳躍
    /// </summary>
    public void Jump()
    {
        isJump = Input.GetButtonDown(jumpInputName);
        // 施加跳躍的力 
        if (isJump && isGround)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            //velocity.y = 20f;
        }
    }

    /// <summary>
    /// 下蹲
    /// </summary>
    public void Crouch()
    {
        isCrouch = Input.GetKey(crouchInputName);
        if (isCrouch)
        {
            characterController.height = 1f;
        }
        else
        {
            characterController.height = 1.8f;
        }
    }
}
/// <summary>
/// 移動
/// </summary>
public void Moveing()
{
    float speed;
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");

    isRun = Input.GetKey(runInputName);

    isWalk = (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0) ? true : false;
    speed = isRun ? runSpeed : walkSpeed; // 設置行走或奔跑的速度

    moveDirction = (transform.right * h + transform.forward * v).normalized; // 設置玩家移動方向(將移動速度進行規範化，防止斜向走速度變大)

    characterController.Move(moveDirction * speed * Time.deltaTime); // 移動

    velocity.y += gravity * Time.deltaTime; // 不在地面上（空中，累加重力值）
    characterController.Move(velocity * Time.deltaTime); // 施加重力
    Jump();
    // 如果處在斜面上移動
    if (OnSlope())
    {
        // 向下增加力量
        characterController.Move(Vector3.down * characterController.height / 2 * slopeForce * Time.deltaTime);
    }
    PlayFootStepSound();
}

/// 播放移動的音效
public void PlayFootStepSound()
{
    if (isGround && moveDirction.sqrMagnitude > 0.9f)
    {
        audioSource.clip = isRun ? runingSound : walkingSound; // 設置行走或者奔跑的音效
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    else
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
}


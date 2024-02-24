using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 手槍(G18)武器射擊
/// </summary>
public class HandGunController : MonoBehaviour
{
    public PlayerMovement PM;
    public Transform shooterPoint;//射擊的位置（槍口）

    [Tooltip("武器射程")] public int range = 100;//武器射程
    [Tooltip("一個彈匣數量")] public int bulletsMag = 31;//一個彈匣數量
    [Tooltip("當前子彈數")] public int currentBullets;//當前子彈數
    [Tooltip("備彈")] public int bulletLeft = 300;//備彈
    [Header("槍口特效")]
    public ParticleSystem muzzleFlash;//槍口火焰特效
    public GameObject hitParticles;//子彈擊中粒子特效
    public GameObject bulletHole;//彈孔
    public Light muzzleFlashLight;//槍口火焰燈光
    [Header("武器射速")]
    public float fireRate = 0.1f;//射速
    private float fireTimer;//計時器
    private float SpreadFactor; //射擊的一點偏移量

    [Header("鍵位設置")]
    [SerializeField] [Tooltip("填裝子彈按鍵")] private KeyCode reloadInputName;
    [SerializeField] [Tooltip("查看武器按鍵")] private KeyCode inspectInputName;
    [SerializeField] [Tooltip("主武器")] private KeyCode AutoRifleKey;
    [SerializeField] [Tooltip("副武器")] private KeyCode HandGunKey;
    [SerializeField] [Tooltip("自動半自動切換")] private KeyCode GunShootModelInputName;


    private Animator anim;
    private AudioSource audioSource;
    [Header("音效片段")]
    public AudioClip HandGunSound;/*槍聲音效片段*/
    public AudioClip reloadAmmoLeftClip;//換子彈1音效片段
    public AudioClip reloadOutOFAmmoClip;//換子彈2音效片段（拉槍栓）

    private bool isReloading;//判斷是否在裝彈
    private bool isAiming;//判斷是否在瞄準

    public Transform casingSpawnPoint;//子彈殼拋出的位置
    public Transform casingPrefab; //子彈殼預製體

    private Camera mainCamera;
    public ChooseGunController CGC; //聲明切換武器類的實例

    /*使用枚舉區分全自動和半自動模式*/
    public enum ShootMode { AutoRifle, SemiGun };
    public ShootMode shootingMode;
    private bool GunShootInput; //根據全自動和半自動 射擊的鍵位輸入發生改變
    private int modeNum = 0; //模式切換的一個中間參數（1：全自動模式，2：半自動模式）
    private string shootModelName;

    [Header("UI的設置")]
    public Image crossHairUI;
    public Text ammoTextUI;
    public Text ShootModelTextUI;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        currentBullets = bulletsMag;
        mainCamera = Camera.main;
        shootingMode = ShootMode.SemiGun; //G18手槍默認是半自動模式
        shootModelName = "半自動";
        UpdateAmmoUI();
    }

    private void Update()
    {
        //切換模式(全自動和半自動)
        if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 1)
        {
            modeNum = 1;
            shootModelName = "全自動";
            shootingMode = ShootMode.AutoRifle;
            ShootModelTextUI.text = shootModelName;

        }
        else if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 0)
        {
            modeNum = 0;
            shootModelName = "半自動";
            shootingMode = ShootMode.SemiGun;
            ShootModelTextUI.text = shootModelName;

        }

        /*控制射擊模式的轉換  後面就要用代碼去動態控制了*/
        switch (shootingMode)
        {
            case ShootMode.AutoRifle:
                GunShootInput = Input.GetMouseButton(0);
                fireRate = 0.08f;
                break;
            case ShootMode.SemiGun:
                GunShootInput = Input.GetMouseButtonDown(0);
                fireRate = 0.2f;
                break;
        }



        if (GunShootInput && currentBullets > 0)
        {
            GunFire();
        }
        else
        {
            muzzleFlashLight.enabled = false;
        }

        //計時器加時間
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        anim.SetBool("Run", PM.isRun);//播放跑步動畫
        anim.SetBool("Walk", PM.isWalk);
        //獲取動畫狀態機第一層動畫的狀態
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        //兩種換子彈的動畫
        if (info.IsName("reload_ammo_left") || info.IsName("reload_out_of_ammo"))
        {
            isReloading = true;
        }
        else
        {
            isReloading = false;
        }

        if (Input.GetKeyDown(reloadInputName) && currentBullets < bulletsMag && bulletLeft > 0)
        {
            Reload();
        }
        SpreadFactor = (isAiming) ? 0f : 0.01f;
        DoingAim();


        if (Input.GetKeyDown(inspectInputName))
        {
            anim.SetTrigger("Inspect");
        }

        //切換主武器（自動步槍）
        if (Input.GetKeyDown(AutoRifleKey))
        {
            CGC.ChangeWeapon(0);
        }
        //切換副武器（手槍） 
        if (Input.GetKeyDown(HandGunKey))
        {
            CGC.ChangeWeapon(1);
        }
    }
    //更新UI
    public void UpdateAmmoUI()
    {
        ammoTextUI.text = currentBullets + " / " + bulletLeft;
        ShootModelTextUI.text = shootModelName;
    }

    /// <summary>
    /// 瞄準的邏輯
    /// </summary>
    public void DoingAim()
    {
        if (Input.GetMouseButton(1) && !isReloading && !PM.isRun)
        {
            isAiming = true;
            anim.SetBool("Aim", isAiming);
            crossHairUI.gameObject.SetActive(false);
            mainCamera.fieldOfView = 40;//瞄準的時候攝像機視野變小
        }
        else
        {
            isAiming = false;
            anim.SetBool("Aim", isAiming);
            crossHairUI.gameObject.SetActive(true);
            mainCamera.fieldOfView = 60;//瞄準的時候攝像機視野恢復
        }
    }

    /// <summary>
    /// 射擊邏輯
    /// </summary>
    public void GunFire()
    {
        //控制射速， 當前彈夾打光了,正在裝子彈,正在奔跑  就不可以發射了
        if (fireTimer < fireRate || currentBullets <= 0 || isReloading || PM.isRun) return;


        RaycastHit hit;
        Vector3 shootDirection = shooterPoint.forward;
        //改成這個，shootDirection shooterPoint這個遊戲物體進行小的偏移(TransformDirection 將local坐標轉換為世界坐標)
        shootDirection = shootDirection + shooterPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));

        if (Physics.Raycast(shooterPoint.position, shootDirection, out hit, range))
        {
            Debug.Log(hit.transform.name + "打到了");

            GameObject hitParticleEffect = Instantiate(hitParticles, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//實例出子彈擊中的火光特效
            GameObject bulletHoleEffect = Instantiate(bulletHole, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));//實例出彈孔特效
                                                                                                                                //回收特效
            Destroy(hitParticleEffect, 1f);
            Destroy(bulletHoleEffect, 3f);
        }

        if (!isAiming)
        {
            anim.CrossFadeInFixedTime("fire", 0.1f); //播放普通開火動畫（使用動畫的淡出淡入效果）
        }
        else
        {//瞄準狀態下，播放瞄準的開火動畫
            anim.Play("aim_fire", 0, 0f);
        }
        muzzleFlash.Play(); //播放火光特效
        muzzleFlashLight.enabled = true;
        PlayerShootSound();//播放射擊音效
                           //實例拋彈殼
        Instantiate(casingPrefab, casingSpawnPoint.transform.position, casingSpawnPoint.transform.rotation);
        currentBullets--;
        UpdateAmmoUI();
        fireTimer = 0f;//重置計時器
    }


    public void PlayerShootSound()
    {
        audioSource.clip = HandGunSound;
        audioSource.Play();
    }
    /// <summary>
    /// 裝填彈藥邏輯
    /// </summary>
    public void Reload()
    {
        if (bulletLeft <= 0) return;
        DoReloadAnimation();

        //計算需要填充的子彈
        int bulletToLoad = bulletsMag - currentBullets;
        //計算備彈扣除的子彈
        int bulletToReduce = (bulletLeft >= bulletToLoad) ? bulletToLoad : bulletLeft;
        bulletLeft -= bulletToReduce; //備彈減少
        currentBullets += bulletToReduce;//當前子彈增加
        UpdateAmmoUI();
    }

    //播放裝彈動畫
    public void DoReloadAnimation()
    {
        if (currentBullets > 0)
        {

            anim.Play("reload_ammo_left", 0, 0);
            audioSource.clip = reloadAmmoLeftClip;
            audioSource.Play();
        }

        if (currentBullets == 0)
        {
            anim.Play("reload_out_of_ammo", 0, 0);
            audioSource.clip = reloadOutOFAmmoClip;
            audioSource.Play();
        }
    }


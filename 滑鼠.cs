using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 鼠標觀察
/// 控制玩家左右旋轉以及視角左右移動
/// 控制攝像機上下旋轉以及視角上下移動
/// </summary>
public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f; // 鼠標靈敏度

    public Transform playerBody; // 玩家的位置
    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // 將光標鎖定在遊戲窗口中心，並且隱藏光標
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY; // 累計上下旋轉的軸值
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 限制軸值的範圍在-90到90度之間（這裡上下90度角正好對應了90的軸值）
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // 設置攝像機的局部旋轉
        playerBody.Rotate(Vector3.up * mouseX); // 控制玩家左右旋轉
    }
}


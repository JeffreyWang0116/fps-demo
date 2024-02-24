using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器搖擺
/// </summary>
public class WeaponSway : MonoBehaviour
{
    /*搖擺的參數*/
    public float amout; //搖擺幅度
    public float smoothAmout;//一個平滑值
    public float maxAmout;//最大搖擺幅度

    private Vector3 originPostion; //初始位置

    // Start is called before the first frame update
    void Start()
    {
        //自身位置（相對於父級物體變換得位置）
        originPostion = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        //設置武器手臂模型位置得值，
        float movementX = -Input.GetAxis("Mouse X") * amout;
        float movementY = -Input.GetAxis("Mouse Y") * amout;
        //限制
        movementX = Mathf.Clamp(movementX, -maxAmout, maxAmout);
        movementY = Mathf.Clamp(movementY, -maxAmout, maxAmout);

        Vector3 finallyPostion = new Vector3(movementX, movementY, 0);
        //手柄位置變換
        transform.localPosition = Vector3.Lerp(transform.localPosition, finallyPostion + originPostion, Time.deltaTime * smoothAmout);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器切換
/// </summary>
public class ChooseGunController : MonoBehaviour
{
    // 武器設置為集合，存放多個武器
    public List<GameObject> Weapons = new List<GameObject>();
    private int index;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
    }

    // 切換武器方法（全部武器掛載於人物手上，默認隱藏，需要時顯示出來）
    public void ChangeWeapon(int WeaponIndex)
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (WeaponIndex == i)
            {
                Weapons[i].gameObject.SetActive(true);
                if (WeaponIndex == 0)
                {
                    Weapons[i].gameObject.GetComponentInChildren<WeaponController>().UpdateAmmoUI();
                    // print(Weapons[i].gameObject.name);
                }
                else if (WeaponIndex == 1)
                {
                    Weapons[i].gameObject.GetComponentInChildren<HandGunController>().UpdateAmmoUI();
                    //print(Weapons[i].gameObject.name);
                }
            }
            else
            {
                Weapons[i].gameObject.SetActive(false);
            }
        }
    }
}


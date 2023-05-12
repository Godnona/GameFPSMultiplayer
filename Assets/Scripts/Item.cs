using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemInfo itemInfo;

    public GameObject itemGameObject;

    public abstract void Use();

}
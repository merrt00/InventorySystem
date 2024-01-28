using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftTable : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventory;


    public void Start()
    {
        inventory.SetActive(false);
    }


}

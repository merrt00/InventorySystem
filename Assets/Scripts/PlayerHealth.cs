using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int health = 0;
    [SerializeField] private int food = 0;
    [SerializeField] private int water = 0;


    public void addHealt(int addedHealth)
    {
        health += addedHealth;
    }
    public void addFood(int addedFood)
    {
        food += addedFood;
    }
    public void addWater(int addedWater)
    {
        water += addedWater;
    }
}

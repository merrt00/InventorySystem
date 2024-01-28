using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeHealth : MonoBehaviour
{
    [SerializeField] private int currentHealth = 10;
    [SerializeField] private List<ItemDrop> ItemDrops = new List<ItemDrop>();
    public string description = "New Description";

    public void takeDamage(int damage , GameObject player)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);    
            foreach (ItemDrop item in ItemDrops)
            {
                int quantityToDrop = Random.Range(item.minQuantityToDrop, item.maxQuantityToDrop);

                if (quantityToDrop == 0)
                {
                    return;
                }
                
                Item droppedItem = Instantiate(item.ItemToDrop, transform.position, Quaternion.identity).GetComponent<Item>();
                droppedItem.currentQuantity = quantityToDrop;

                player.GetComponent<Inventory>().addItemToInventory(droppedItem);

            }
            
        }
    }
}

[System.Serializable]
public class ItemDrop
{
    public GameObject ItemToDrop;
    public int minQuantityToDrop = 1;
    public int maxQuantityToDrop = 5;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class Inventory : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventory;
    public GameObject craft;
    private List<Slot> allInventorySlots = new List<Slot>();
    public List<Slot> inventorySlots = new List<Slot>();
    public List<Slot> hotbarSlots = new List<Slot>();
    public Image crosshair;
    public TMP_Text itemHoverText;
    public TMP_Text itemDescription;
    public TMP_Text treeText;
    public GameObject itemDescriptionImage;


    [Header("Raycast")]
    public float raycastDistance= 5f;
    public LayerMask itemLayer;
    public Transform dropLocation; // The location items will be dropped from.

    [Header("Drag and Drop")]
    public Image dragIconImage;
    private Item currentDraggedItem;
    private int currentDragSlotIndex = -1;

    [Header("Equippable Items")]
    public List<GameObject> equippableItems = new List<GameObject>();
    public Transform selectedItemImage;
    private int curHotbarIndex = -1;

    [Header("Crafting")]
    public List<Recipe> itemRecipes = new List<Recipe>();



    [Header("Save/Load")]
    public List<GameObject> allItemPrefabs = new List<GameObject>();
    private string saveFileName = "inventorySave.json";



    public void Start()
    {
        toggleInventory(false);

        allInventorySlots.AddRange(hotbarSlots);
        allInventorySlots.AddRange(inventorySlots);

        foreach(Slot uiSlot in allInventorySlots)
        {
            uiSlot.inistializeSlot();
        }

        loadInventory();

    }
    public void OnApplicationQuit()
    {
        saveInventory();
    }

    public void Update()
    {
        itemRaycast(Input.GetMouseButtonDown(0));

        if (Input.GetKeyDown(KeyCode.E) && !craft.activeInHierarchy)
        {
            toggleInventory(!inventory.activeInHierarchy);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            inventory.SetActive(false);
            craft.SetActive(false);
            toggleInventory(false);
            toggleCraft(false);
        }
        if (inventory.activeInHierarchy && Input.GetMouseButtonDown(0))
        {
            dragInventoryIcon();
        }
        else if (currentDragSlotIndex != -1 && Input.GetMouseButtonUp(0) || currentDragSlotIndex != -1 && !inventory.activeInHierarchy) //
        {
            dropInventoryIcon();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            dropItem();
        }

        for (int i = 0; i < hotbarSlots.Count +1 ; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                enableHotbarItem(i-1);

                curHotbarIndex = i - 1;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            attemptToUseItem();
        }

        dragIconImage.transform.position = Input.mousePosition;

    }

    private void itemRaycast(bool hasClicked = false)
    {
        itemHoverText.text = "";
        treeText.text = "";
        Ray ray = Camera.main.ScreenPointToRay(crosshair.transform.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit,raycastDistance,itemLayer))
        {
            if (hit.collider != null)
            {
                if (hasClicked)// Pick Up Object
                {
                    Item newItem = hit.collider.GetComponent<Item>();
                    if (newItem)
                    {
                        addItemToInventory(newItem);
                        itemDescriptionImage.SetActive(false);
                    }
                }
                else // Get the Name of Object
                {
                    Item newItem = hit.collider.GetComponent<Item>();
                    TreeHealth newTree = hit.collider.GetComponent<TreeHealth>();
                    if (newItem)
                    {
                        itemHoverText.text = newItem.name;
                        itemDescriptionImage.SetActive(true);
                        itemDescription.text = newItem.description;
                    }
                    if (newTree)
                    {
                        treeText.text = newTree.description;

                    }

                }
            }   
        }
        else
        {
            itemDescriptionImage.SetActive(false);
        }
    }

    public void addItemToInventory(Item itemToAdd, int overrideIndex = -1)
    {
        if (overrideIndex != -1)
        {
            allInventorySlots[overrideIndex].setItem(itemToAdd);
            itemToAdd.gameObject.SetActive(false);
            allInventorySlots[overrideIndex].updateData();
            return;
        }

        int leftoverQuantity = itemToAdd.currentQuantity;
        Slot openSlot = null;
        for (int i = 0; i < allInventorySlots.Count; i++)
        {
            Item heldItem = allInventorySlots[i].getItem();

            if (heldItem != null && itemToAdd.name == heldItem.name)
            {
                int freeSpaceInSlot = heldItem.maxQuantity - heldItem.currentQuantity;

                if (freeSpaceInSlot >= leftoverQuantity)
                {
                    heldItem.currentQuantity += leftoverQuantity;
                    Destroy(itemToAdd.gameObject);
                    allInventorySlots[i].updateData();
                    return;
                }
                else
                {
                    heldItem.currentQuantity = heldItem.maxQuantity;
                    leftoverQuantity -= freeSpaceInSlot;
                }
            }
            else if (heldItem == null)
            {
                if (!openSlot)
                {
                    openSlot = allInventorySlots[i];
                }
            }
            allInventorySlots[i].updateData();

        }
        if (leftoverQuantity > 0 && openSlot)
        {
            openSlot.setItem(itemToAdd);
            itemToAdd.currentQuantity = leftoverQuantity;
            itemToAdd.gameObject.SetActive(false);
        }
        else
        {
            itemToAdd.currentQuantity = leftoverQuantity;
        }
    }
    private void dropItem()
    {
        for(int i = 0; i < allInventorySlots.Count; i++)
        {
            Slot curSlot = allInventorySlots[i];
            if (curSlot.hovered && curSlot.hasItem())
            {
                curSlot.getItem().gameObject.SetActive(true);
                curSlot.getItem().transform.position = dropLocation.position;
                curSlot.setItem(null);
                break;
            }
        }
    }

    private void toggleInventory(bool enable)
    {
        inventory.SetActive(enable);

        Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = enable;

        //Disable the rotation of the camera
        Camera.main.GetComponent<FirstPersonLook>().sensitivity = enable ? 0 : 2;

        if (!enable) //Bug Fix
        {
            foreach(Slot curSlot in allInventorySlots)
            {
                curSlot.hovered = false;
            }
        }
    }
    private void toggleCraft(bool enable)
    {
        craft.SetActive(enable);

        Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = enable;

        //Disable the rotation of the camera
        Camera.main.GetComponent<FirstPersonLook>().sensitivity = enable ? 0 : 2;

        if (!enable) //Bug Fix
        {
            foreach (Slot curSlot in allInventorySlots)
            {
                curSlot.hovered = false;
            }
        }
    }

    public void dragInventoryIcon()
    {
        for (int i = 0; i < allInventorySlots.Count; i++)
        {
            Slot curSlot = allInventorySlots[i];
            if (curSlot.hovered && curSlot.hasItem())
            {
                currentDragSlotIndex = i; // Update the current drag slot index variable.

                currentDraggedItem = curSlot.getItem();
                dragIconImage.sprite = currentDraggedItem.icon;
                dragIconImage.color = new Color(1, 1, 1, 1); // Make the follow mouse icon Opaque(visible).

                curSlot.setItem(null); // Remove the item from the slot we just picked up the item from.

            }
        }
    }
    public void dropInventoryIcon()
    {
        dragIconImage.sprite = null;
        dragIconImage.color = new Color(1, 1, 1, 0); // Make unvisible.
        for (int i = 0; i < allInventorySlots.Count; i++)
        {
            Slot curSlot = allInventorySlots[i];
            if (curSlot.hovered)
            {
                if (curSlot.hasItem()) // Swap the items.
                {
                    Item itemToSwap = curSlot.getItem();

                    curSlot.setItem(currentDraggedItem);

                    allInventorySlots[currentDragSlotIndex].setItem(itemToSwap);

                    resetDragVariables();
                    return;
                }
                else // Place the item with no swap. 
                {
                    curSlot.setItem(currentDraggedItem);
                    resetDragVariables();
                    return;
                }
            }
        }

        // If we get to this point we dropped the item in an invalid location (or closed the inventory).
        allInventorySlots[currentDragSlotIndex].setItem(currentDraggedItem);
        resetDragVariables();  
    }

    private void resetDragVariables()
    {
        currentDraggedItem = null; 
        currentDragSlotIndex = -1;
    }

    private void enableHotbarItem(int hotbarIndex)
    {
        foreach(GameObject a in equippableItems)
        {
            a.SetActive(false);
        }
        Slot hotbarSlot = hotbarSlots[hotbarIndex];
        selectedItemImage.transform.position = hotbarSlots[hotbarIndex].transform.position;

        if (hotbarSlot.hasItem())
        {
            if (hotbarSlot.getItem().equippableItemIndex != -1)
            {
                equippableItems[hotbarSlot.getItem().equippableItemIndex].SetActive(true);
            }
        }
    }

    public void craftItem(string itemName)
    {
        foreach (Recipe recipe in itemRecipes)
        {
            if (recipe.createdItemPrefab.GetComponent<Item>().name == itemName)
            {
                bool haveAllIngredients = true;
                for (int i = 0; i < recipe.requiredIngredients.Count; i++)
                {
                    if (haveAllIngredients)
                    {
                        haveAllIngredients = haveIngredients(recipe.requiredIngredients[i].itemName, recipe.requiredIngredients[i].requiredQuantity);
                    }
                }
                if (haveAllIngredients)
                {
                    for(int i = 0; i < recipe.requiredIngredients.Count; i++)
                    {
                        removeIngredient(recipe.requiredIngredients[i].itemName , recipe.requiredIngredients[i].requiredQuantity);
                    }

                    GameObject craftedItem = Instantiate(recipe.createdItemPrefab, dropLocation.position, Quaternion.identity);
                    craftedItem.GetComponent<Item>().currentQuantity = recipe.quantityProduced;

                    addItemToInventory(craftedItem.GetComponent<Item>());
                }
                break;
            }
        }
    }

    private void removeIngredient(string itemName, int quantity)
    {
        if (!haveIngredients(itemName,quantity))
        {
            return;
        }
        int remainingQuantity = quantity;

        foreach(Slot curSlot in allInventorySlots)
        {
            Item item = curSlot.getItem();
            if (item != null && item.name == itemName) 
            {
                if (item.currentQuantity >= remainingQuantity)
                {
                    item.currentQuantity -= remainingQuantity;
                    if (item.currentQuantity == 0)
                    {
                        curSlot.setItem(null);
                        curSlot.updateData();
                    }
                    return;
                }
                else
                {
                    remainingQuantity -= item.currentQuantity;
                    curSlot.setItem(null);
                }
            }
        }
    }


    private bool haveIngredients(string itemName, int quantity)
    {
        int foundQuantity = 0;
        foreach (Slot curSlot in allInventorySlots)
        {
            if (curSlot.hasItem() && curSlot.getItem().name == itemName)
            {
                foundQuantity += curSlot.getItem().currentQuantity;

                if (foundQuantity >= quantity)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void saveInventory()
    {
        InventoryData data = new InventoryData();

        foreach (Slot slot in allInventorySlots)
        {
            Item item = slot.getItem();
            if (item != null)
            {
                ItemData itemData = new ItemData(item.name,item.currentQuantity,allInventorySlots.IndexOf(slot));
                data.slotData.Add(itemData);
            }
        }
        string jsonData = JsonUtility.ToJson(data);

        File.WriteAllText(saveFileName, jsonData);
    }
    private void loadInventory()
    {
        if (File.Exists(saveFileName))
        {
            string jsonData = File.ReadAllText(saveFileName);

            InventoryData data = JsonUtility.FromJson<InventoryData>(jsonData);

            clearInventory();

            foreach(ItemData itemData in data.slotData)
            {
                GameObject itemPrefab = allItemPrefabs.Find(prefab => prefab.GetComponent<Item>().name == itemData.itemName);

                if (itemPrefab != null)
                {
                    GameObject createdItem = Instantiate(itemPrefab, dropLocation.position, Quaternion.identity);
                    Item item = createdItem.GetComponent<Item>();

                    item.currentQuantity = itemData.quantity;

                    addItemToInventory(item,itemData.slotIndex);
                }
            }
        }
        foreach(Slot slot in allInventorySlots)
        {
            slot.updateData();
        }
    }
    private void clearInventory()
    {
        foreach (Slot slot in allInventorySlots)
        {
            slot.setItem(null);
        }
    }

    private void attemptToUseItem()
    {
        if (curHotbarIndex == -1)
        {
            return;
        }
        Item curItem = hotbarSlots[curHotbarIndex].getItem();

        if (curItem)
        {
            curItem.UseItem();
            if (curItem.currentQuantity != 0)
            {
                hotbarSlots[curHotbarIndex].updateData();
            }
            else
            {
                hotbarSlots[curHotbarIndex].setItem(null);
            }
            enableHotbarItem(curHotbarIndex);
        }
    }
}

[System.Serializable]
public class ItemData
{
    public string itemName;
    public int quantity;
    public int slotIndex;

    public ItemData(string itemName, int quantity, int slotIndex)
    {
        this.itemName = itemName;
        this.quantity = quantity;
        this.slotIndex = slotIndex;
    }
}

[System.Serializable]
public class InventoryData
{
    public List<ItemData> slotData = new List<ItemData>();
}

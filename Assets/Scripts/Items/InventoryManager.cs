using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject itemCursor;
    [SerializeField] private GameObject inventoryPanel;


    [SerializeField] private GameObject slotHolder;
    [SerializeField] private GameObject hotbarSlotHolder;
    [SerializeField] private ItemClass itemToAdd;
    [SerializeField] private ItemClass itemToRemove;

    [SerializeField] private SlotClass[] startingItems;

    private SlotClass[] items;

    private GameObject[] slots;
    private GameObject[] hotbarSlots;

    private SlotClass movingSlot;
    private SlotClass tempSlot;
    private SlotClass ogSlot;

    public bool isMovingItem;

    [SerializeField] private GameObject hotbarSelector;
    [SerializeField] private int selectedSlotIndex = 0;
    public ItemClass selectedItem;

    public static bool isInventoryOpened;


    private void Start()
    {

        isInventoryOpened = false;
        slots = new GameObject[slotHolder.transform.childCount];
        items = new SlotClass[slots.Length];

        hotbarSlots = new GameObject[hotbarSlotHolder.transform.childCount];
        for(int i = 0;i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i] = hotbarSlotHolder.transform.GetChild(i).gameObject;
        }

        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new SlotClass();
        }
        for (int i = 0; i < startingItems.Length; i++)
        {
            items[i] = startingItems[i];
        }

        for (int i = 0; i < slotHolder.transform.childCount; i++)
            slots[i] = slotHolder.transform.GetChild(i).gameObject;
        
        RefreshUI();

        Add(itemToAdd, 1);
        Remove(itemToRemove);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryPanel.activeSelf)
            {
                inventoryPanel.SetActive(false);
                isInventoryOpened = false;
            }
            else
            {
                inventoryPanel.SetActive(true);
                isInventoryOpened = true;
            }
        }
        if (inventoryPanel.activeSelf)
        {
            itemCursor.SetActive(isMovingItem);
            itemCursor.transform.position = Input.mousePosition;
            if (isMovingItem)
                itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;
            if (Input.GetMouseButtonDown(0))
            {
                if (isMovingItem)
                {
                    EndItemMove();
                }
                else
                {
                    BeginItemMove();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (isMovingItem)
                {
                    EndItemMove_Single();
                }
                else
                {
                    BeginItemMove_Half();
                }
            }
        }

        if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex+1, 0, hotbarSlots.Length-1);
        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex - 1, 0, hotbarSlots.Length-1);
        }

        hotbarSelector.transform.position = hotbarSlots[selectedSlotIndex].transform.position;
        selectedItem = items[selectedSlotIndex + (hotbarSlots.Length*2)].GetItem(); // add a var for hotbar rows + correct index
    }
    #region inv utility
    public void RefreshUI()
    {
        for(int i = 0; i < slots.Length;i++)
        {
            try
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].GetItem().itemIcon;
                if (items[i].GetItem().isStackable)
                    slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = items[i].GetQuantity().ToString();
                else
                    slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";


                
            }
            catch
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }

        RefreshHotbar();
    }
    public void RefreshHotbar()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            try
            {
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i + (hotbarSlots.Length*2)].GetItem().itemIcon;
                if (items[i + (hotbarSlots.Length * 2)].GetItem().isStackable)
                    hotbarSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = items[i + (hotbarSlots.Length * 2)].GetQuantity().ToString();
                else
                    hotbarSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";



            }
            catch
            {
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                hotbarSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }
    public bool Add(ItemClass item, int quantity)
    {
        SlotClass slot = Contains(item);
        if (slot != null && slot.GetItem().isStackable)
            slot.AddQuantity(quantity);
        else
        {
            for(int i = 0; i < items.Length; i++)
            {
                if(items[i].GetItem() == null)
                {
                    items[i].AddItem(item, quantity);
                    break;  
                }
            }
        }
        RefreshUI();
        return true;
    }
   
    public bool Remove(ItemClass item)
    {
        //items.Remove(item);
        SlotClass temp = Contains(item);
        if (temp != null)
            if(temp.GetQuantity() > 1)
                temp.SubQuantity(1);
            else
            {
                int slotToRemoveIndex = 0;

                for(int i = 0;i < items.Length;i++)
                {
                    if (items[i].GetItem() == item)
                    {
                        slotToRemoveIndex = i;
                        break;
                    }
                }
                items[slotToRemoveIndex].Clear();
            }
        else
        {
            return false;
        }

        
        RefreshUI();
        return true;
    }

    public void UsedSelected()
    {
        items[selectedSlotIndex + (hotbarSlots.Length * 2)].SubQuantity(1);
        RefreshUI();
    }
    public SlotClass Contains(ItemClass item)
    {
        for(int i = 0; i < items.Length; i++)
        {
            if(items[i].GetItem() == item)
            {
                return items[i];
            }
        }
        return null;
    }
    #endregion inv utility 

    #region inv move
    private bool BeginItemMove()
    {
        ogSlot = (GetClosestSlot());
        if(ogSlot == null || ogSlot.GetItem() == null)
            return false;

        movingSlot = new SlotClass(ogSlot);
        ogSlot.Clear();
        isMovingItem = true;
        RefreshUI();
        return true;
    }
    private bool BeginItemMove_Half()
    {
        ogSlot = (GetClosestSlot());
        if (ogSlot == null)
            return false;

        movingSlot = new SlotClass(ogSlot.GetItem(),Mathf.CeilToInt(ogSlot.GetQuantity()/2f));
        ogSlot.SubQuantity(Mathf.CeilToInt(ogSlot.GetQuantity() / 2f));
        if (ogSlot.GetQuantity() == 0)
            ogSlot.Clear();
        isMovingItem = true;
        RefreshUI();
        return true;
    }
    private bool EndItemMove()
    {
        ogSlot = (GetClosestSlot());
        if (ogSlot == null)
        {
            //Debug.Log(movingSlot.GetQuantity());
            Add(movingSlot.GetItem(), movingSlot.GetQuantity());
            movingSlot.Clear();
        }
        else
        {

            if (ogSlot.GetItem() != null)
            {
                if (ogSlot.GetItem() == movingSlot.GetItem())
                {
                    if (ogSlot.GetItem().isStackable)
                    {
                        ogSlot.AddQuantity(movingSlot.GetQuantity());
                        movingSlot.Clear();
                    }
                    else
                        return false;
                }
                else
                {
                    tempSlot = new SlotClass(ogSlot);
                    ogSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                    movingSlot.AddItem(tempSlot.GetItem(), tempSlot.GetQuantity());
                    RefreshUI();
                    return true;
                }
            }
            else
            {
                ogSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                movingSlot.Clear();
            }
        }
        isMovingItem = false;
        RefreshUI();
        return true;
    }
    private bool EndItemMove_Single()
    {
        ogSlot = (GetClosestSlot());

        if (ogSlot == null)
            return false;

        if (movingSlot.GetQuantity() == 1)
            return false;
        
        else if(ogSlot.GetItem() != null && ogSlot.GetItem()!= movingSlot.GetItem())
            return false;
        else
            movingSlot.SubQuantity(1);

        if (ogSlot.GetItem() != null && ogSlot.GetItem() == movingSlot.GetItem())
        {
            ogSlot.AddQuantity(1);
        }
        else if (ogSlot.GetItem() == null)
        {
            ogSlot.AddItem(movingSlot.GetItem(), 1);
        }

        if(movingSlot.GetQuantity() < 1)
        {
            isMovingItem = false;
            movingSlot.Clear();
        }
        else
        {
            isMovingItem = true;
        }
        isMovingItem = true;
        RefreshUI();
        return true;
    }
    private SlotClass GetClosestSlot()
    {

        for (int i = 0; i < slots.Length; i++)
        {
            if (Vector2.Distance(slots[i].transform.position, Input.mousePosition) <= 32f)
            {
                return items[i];
            }
        }
        return null;
    }
    #endregion  inv move
}


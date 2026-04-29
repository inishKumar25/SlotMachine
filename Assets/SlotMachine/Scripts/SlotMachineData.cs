using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SlotItem
{
    public Sprite icon;
    public int id;
}


[System.Serializable]
public class SlotColumn
{
    public List<SlotItem> slotItems = new List<SlotItem>(new SlotItem[5]);

    public List<SlotItem> OffsetSlots(int by)
    {
        int length = slotItems.Count;
        by = ((by % length) + length) % length; // normalize to handle negatives/overflow

        List<SlotItem> offsetList = new List<SlotItem>(new SlotItem[length]);

        for (int i = 0; i < length; i++)
        {
            offsetList[(i + by) % length] = slotItems[i];
        }

        slotItems = offsetList;
        return slotItems;
    }
}


public class SlotMachineData : MonoBehaviour
{
    public static SlotMachineData Instance;

    public SlotColumn sampleColumnData;


    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

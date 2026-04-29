/*
 
SLOT MACHINE DATA CODE 
  
LOGIC:

1. SlotItem
   → Represents ONE symbol (sprite + ID)

2. SlotColumn
   → Holds a list of SlotItems (one reel)
   → Can rotate items using OffsetSlots()

3. SlotMachineData (Singleton)
   → Stores a SAMPLE column layout
   → Other scripts (like SlotMachineController) clone this data

FLOW:
Controller → asks for sampleColumnData
→ clones it into each column
→ spins using OffsetSlots()
→ renders sprites


*/

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
        by = ((by % length) + length) % length;

        List<SlotItem> offsetList = new List<SlotItem>(new SlotItem[length]);
        for (int i = 0; i < length; i++)
            offsetList[(i + by) % length] = slotItems[i];

        slotItems = offsetList;
        return slotItems;
    }
}

public class SlotMachineData : MonoBehaviour
{
    public static SlotMachineData Instance;
    public SlotColumn sampleColumnData;

    private void Awake() { Instance = this; }
}
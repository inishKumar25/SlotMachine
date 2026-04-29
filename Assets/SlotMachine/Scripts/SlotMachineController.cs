/*
 
SLOT MACHINE CONTROLLER CODE 
  
LOGIC FLOW:

1. Spin() is called → starts spinning all 3 columns.

2. Each column spins independently using a coroutine (SpinColumn).

3. When a column finishes:
   → it increments `columnsFinished`.

4. SpinAllColumns() waits until:
   → all 3 columns are done (columnsFinished >= 3)

5. ONLY THEN:
   → we check the PAYLINE IDs of all 3 columns.

6. If all IDs match:
   → it's a REAL WIN
   → play particle effect instantly (no delay)

7. Reset columnsFinished for next spin.


*/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotMachineController : MonoBehaviour
{
    [System.Serializable]
    public class SlotColumnRenderer
    {
        public List<SpriteRenderer> slotRenderers;
        private SlotColumn slotColumnData;

        public void SetSampleData(SlotColumn sampleData)
        {
            slotColumnData = new SlotColumn();
            slotColumnData.slotItems = new List<SlotItem>();

            foreach (var item in sampleData.slotItems)
            {
                slotColumnData.slotItems.Add(new SlotItem
                {
                    icon = item.icon,
                    id = item.id
                });
            }
        }

        public void ApplyRandomOffset()
        {
            int randomOffset = Random.Range(0, slotColumnData.slotItems.Count);
            slotColumnData.OffsetSlots(randomOffset);
        }

        public void StepOffset(int by = 1) => slotColumnData.OffsetSlots(by);

        public void UpdateRenderers()
        {
            for (int i = 0; i < slotRenderers.Count; i++)
                if (i < slotColumnData.slotItems.Count)
                    slotRenderers[i].sprite = slotColumnData.slotItems[i]?.icon;
        }

        public int GetItemCount() => slotColumnData?.slotItems?.Count ?? 0;

        public int GetPaylineItemId()
        {
            int paylineIndex = slotRenderers.Count / 2;
            if (slotColumnData?.slotItems != null && paylineIndex < slotColumnData.slotItems.Count)
                return slotColumnData.slotItems[paylineIndex]?.id ?? -1;
            return -1;
        }

        public int GetNextPaylineItemId()
        {
            int paylineIndex = slotRenderers.Count / 2;
            int length = slotColumnData.slotItems.Count;
            int sourceIndex = (paylineIndex - 1 + length) % length;
            return slotColumnData.slotItems[sourceIndex]?.id ?? -1;
        }
    }

    public SlotColumnRenderer slotColumnRenderer1;
    public SlotColumnRenderer slotColumnRenderer2;
    public SlotColumnRenderer slotColumnRenderer3;

    [Header("Spin Settings")]
    public float minStepInterval = 0.05f;
    public float maxStepInterval = 0.3f;
    public float spinDuration = 2f;
    public float decelerationDuration = 1f;
    public float columnStopDelay = 0.4f;

    [Header("Fake Win Settings")]
    public float fakeWinPauseDuration = 0.8f;

    [Header("Jackpot Effects")]
    public ParticleSystem jackpotParticlesPrefab;

    [Header("Currency")]
    public int spinCost = 10;
    public int winReward = 30;

    [Header("Bet System")]
    public int currentBet = 20;   

    private int columnsFinished = 0;

    void Start() => InitialiseSlotRenderers();

    void InitialiseSlotRenderers()
    {
        slotColumnRenderer1.SetSampleData(SlotMachineData.Instance.sampleColumnData);
        slotColumnRenderer1.ApplyRandomOffset();
        slotColumnRenderer1.UpdateRenderers();

        slotColumnRenderer2.SetSampleData(SlotMachineData.Instance.sampleColumnData);
        slotColumnRenderer2.ApplyRandomOffset();
        slotColumnRenderer2.UpdateRenderers();

        slotColumnRenderer3.SetSampleData(SlotMachineData.Instance.sampleColumnData);
        slotColumnRenderer3.ApplyRandomOffset();
        slotColumnRenderer3.UpdateRenderers();
    }

    public void Spin(LeverController lever = null)
    {
        if (!CurrencyManager.Instance.CanAfford(currentBet))
        {
            Debug.Log("Not enough coins!");
            lever?.UnlockLever();
            return;
        }

        CurrencyManager.Instance.Spend(currentBet);

        columnsFinished = 0;

        slotColumnRenderer1.ApplyRandomOffset();
        slotColumnRenderer2.ApplyRandomOffset();
        slotColumnRenderer3.ApplyRandomOffset();

        StartCoroutine(SpinAllColumns(lever));
    }

    private IEnumerator SpinAllColumns(LeverController lever = null)
    {
        StartCoroutine(SpinColumn(slotColumnRenderer1, 0f));
        StartCoroutine(SpinColumn(slotColumnRenderer2, columnStopDelay));
        StartCoroutine(SpinColumn(slotColumnRenderer3, columnStopDelay * 2));

       
        yield return new WaitUntil(() => columnsFinished >= 3);

        int id1 = slotColumnRenderer1.GetPaylineItemId();
        int id2 = slotColumnRenderer2.GetPaylineItemId();
        int id3 = slotColumnRenderer3.GetPaylineItemId();

        Debug.Log($"Payline IDs: {id1}, {id2}, {id3}");

        bool isWin = (id1 == id2) && (id2 == id3);

        if (isWin)
        {
            Debug.Log("WINNER!");

            int reward = currentBet * 2;
            CurrencyManager.Instance.Add(reward);

            if (jackpotParticlesPrefab != null)
            {
                ParticleSystem ps = Instantiate(
                    jackpotParticlesPrefab,
                    transform.position,
                    Quaternion.identity
                );

                ps.Play();
                Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            }
        }

        lever?.UnlockLever();
    }

    private IEnumerator SpinColumn(SlotColumnRenderer column, float stopDelay)
    {
        float elapsed = 0f;
        float accelerationDuration = 0.3f;

        // Acceleration
        while (elapsed < accelerationDuration)
        {
            float t = elapsed / accelerationDuration;
            float interval = Mathf.Lerp(maxStepInterval, minStepInterval, t);
            column.StepOffset(Random.Range(1, 3));
            column.UpdateRenderers();
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // Full speed
        float fullSpeedEnd = spinDuration + stopDelay;
        while (elapsed < fullSpeedEnd)
        {
            column.StepOffset(1);
            column.UpdateRenderers();
            yield return new WaitForSeconds(minStepInterval);
            elapsed += minStepInterval;
        }

        // Deceleration
        float decelStart = elapsed;
        float decelEnd = decelStart + decelerationDuration;

        while (elapsed < decelEnd)
        {
            float t = (elapsed - decelStart) / decelerationDuration;
            float interval = Mathf.Lerp(minStepInterval, maxStepInterval, t);
            column.StepOffset(1);
            column.UpdateRenderers();
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        
        columnsFinished++;
    }
}
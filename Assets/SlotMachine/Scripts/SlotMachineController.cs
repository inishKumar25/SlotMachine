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
            slotColumnData.slotItems = new List<SlotItem>(sampleData.slotItems);
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
    [Tooltip("How long Col3 hovers on the near-miss position before slipping away")]
    public float fakeWinPauseDuration = 0.8f;

    [Header("Win Condition")]
    [Tooltip("Triggers a near-miss after this many normal spins")]
    public int spinsUntilFakeWin = 8;

    [Tooltip("Triggers a real win after this many spins (must be >= spinsUntilFakeWin)")]
    public int spinsUntilRealWin = 10;

    [Header("Jackpot Effects")]
    [Tooltip("Assign a ParticleSystem in the Inspector to play on a real win")]
    public ParticleSystem jackpotParticles;

    [Header("Debug — Read Only")]
    [SerializeField] private int spinCount = 0;
    [SerializeField] private bool isFakeWin = false;
    [SerializeField] private bool isRealWin = false;



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
        spinCount++;

        isRealWin = spinCount >= spinsUntilRealWin;
        isFakeWin = isRealWin || spinCount >= spinsUntilFakeWin;

        if (isRealWin)
        {
            spinCount = 0;
            Debug.Log("[RealWin] Forced win triggered!");
        }
        else if (isFakeWin)
        {
            Debug.Log($"[FakeWin] Near-miss triggered on spin {spinCount}.");
        }
        else
        {
            Debug.Log($"[NormalSpin] Spin {spinCount}/{spinsUntilFakeWin} until near-miss.");
        }

        StartCoroutine(SpinAllColumns(lever));
    }

    private IEnumerator SpinAllColumns(LeverController lever = null)
    {
        int targetId = -1;

        if (isFakeWin)
        {
            var items = SlotMachineData.Instance.sampleColumnData.slotItems;
            targetId = items[Random.Range(0, items.Count)]?.id ?? 0;
            Debug.Log($"[Alignment] Target symbol id: {targetId}");
        }

        StartCoroutine(SpinColumn(slotColumnRenderer1, stopDelay: 0f, extraSteps: Random.Range(0, 10), fakeWin: isFakeWin, targetId: targetId, slip: false, realWin: isRealWin));
        StartCoroutine(SpinColumn(slotColumnRenderer2, stopDelay: columnStopDelay, extraSteps: Random.Range(0, 10), fakeWin: isFakeWin, targetId: targetId, slip: false, realWin: isRealWin));
        StartCoroutine(SpinColumn(slotColumnRenderer3, stopDelay: columnStopDelay * 2, extraSteps: Random.Range(0, 10), fakeWin: isFakeWin, targetId: targetId, slip: true, realWin: isRealWin));

        float totalDuration = spinDuration + decelerationDuration + (columnStopDelay * 2) + 1f;
        if (isFakeWin)
            totalDuration += fakeWinPauseDuration + (maxStepInterval * 10);

        yield return new WaitForSeconds(totalDuration);

        if (isRealWin) 
        {

            if (jackpotParticles != null)
                jackpotParticles.Play();
            else
                Debug.LogWarning("No jackpot ParticleSystem assigned!");
            Debug.Log("WINNER!");

        }

       
        Debug.Log("All columns stopped.");

        lever?.UnlockLever();
    }

    private IEnumerator SpinColumn(
        SlotColumnRenderer column,
        float stopDelay,
        int extraSteps = 0,
        bool fakeWin = false,
        int targetId = -1,
        bool slip = false,
        bool realWin = false)
    {
        float elapsed = 0f;
        float accelerationDuration = 0.3f;

        // Acceleration
        while (elapsed < accelerationDuration)
        {
            float t = elapsed / accelerationDuration;
            float interval = Mathf.Lerp(maxStepInterval, minStepInterval, t);
            column.StepOffset(1);
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

        // Extra random steps (guarantees unique stop position per column)
        for (int i = 0; i < extraSteps; i++)
        {
            column.StepOffset(1);
            column.UpdateRenderers();
            yield return new WaitForSeconds(minStepInterval);
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

        // Alignment phase
        if (!fakeWin || targetId < 0) yield break;

        int itemCount = column.GetItemCount();

        if (realWin || !slip)
        {
            
            for (int i = 0; i < itemCount; i++)
            {
                if (column.GetPaylineItemId() == targetId) break;
                column.StepOffset(1);
                column.UpdateRenderers();
                yield return new WaitForSeconds(maxStepInterval);
            }
        }
        else
        {
            
            for (int i = 0; i < itemCount; i++)
            {
                if (column.GetNextPaylineItemId() == targetId) break;
                column.StepOffset(1);
                column.UpdateRenderers();
                yield return new WaitForSeconds(maxStepInterval);
            }

            yield return new WaitForSeconds(fakeWinPauseDuration);

            column.StepOffset(1);
            column.UpdateRenderers();
        }
    }
}
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

        public void StepOffset(int by = 1)
        {
            slotColumnData.OffsetSlots(by);
        }

        public void UpdateRenderers()
        {
            for (int i = 0; i < slotRenderers.Count; i++)
            {
                if (i < slotColumnData.slotItems.Count)
                    slotRenderers[i].sprite = slotColumnData.slotItems[i]?.icon;
            }
        }

        public int GetItemCount() => slotColumnData?.slotItems?.Count ?? 0;

        /// <summary>Returns the id of the item currently sitting on the payline (center slot).</summary>
        public int GetPaylineItemId()
        {
            int paylineIndex = slotRenderers.Count / 2;
            if (slotColumnData?.slotItems != null && paylineIndex < slotColumnData.slotItems.Count)
                return slotColumnData.slotItems[paylineIndex]?.id ?? -1;
            return -1;
        }

        /// <summary>
        /// Returns the id of the item that WILL be on the payline after one StepOffset(1).
        /// Used to detect the near-miss position for the slip column.
        /// 
        /// OffsetSlots(1) moves item[i] → index (i+1)%length,
        /// so the item arriving at paylineIndex came from paylineIndex-1.
        /// </summary>
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
    [Tooltip("When enabled, col1+col2 land on the same symbol while col3 tantalizingly slips past it")]
    public bool fakeWinSpin = false;
    [Tooltip("How long column 3 hovers on the near-miss position before slipping away")]
    public float fakeWinPauseDuration = 0.8f;

    void Start()
    {
        InitialiseSlotRenderers();
    }

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

    [ContextMenu("Spin")]
    public void Spin()
    {
        StartCoroutine(SpinAllColumns());
    }

    private IEnumerator SpinAllColumns()
    {
        int targetId = -1;

        if (fakeWinSpin)
        {
            // Pick a random symbol from the data to be the "almost winning" symbol
            var items = SlotMachineData.Instance.sampleColumnData.slotItems;
            targetId = items[Random.Range(0, items.Count)]?.id ?? 0;
            Debug.Log($"[FakeWin] Near-miss target symbol id: {targetId}");
        }

        StartCoroutine(SpinColumn(slotColumnRenderer1, stopDelay: 0f, extraSteps: Random.Range(0, 10), fakeWin: fakeWinSpin, targetId: targetId, slip: false));
        StartCoroutine(SpinColumn(slotColumnRenderer2, stopDelay: columnStopDelay, extraSteps: Random.Range(0, 10), fakeWin: fakeWinSpin, targetId: targetId, slip: false));
        StartCoroutine(SpinColumn(slotColumnRenderer3, stopDelay: columnStopDelay * 2, extraSteps: Random.Range(0, 10), fakeWin: fakeWinSpin, targetId: targetId, slip: true));

        // Base wait + extra buffer for fake-win alignment steps and pause
        float totalDuration = spinDuration + decelerationDuration + (columnStopDelay * 2) + 1f;
        if (fakeWinSpin)
            totalDuration += fakeWinPauseDuration + (maxStepInterval * 10);

        yield return new WaitForSeconds(totalDuration);
        Debug.Log("All columns stopped.");
    }

    private IEnumerator SpinColumn(
        SlotColumnRenderer column,
        float stopDelay,
        int extraSteps = 0,
        bool fakeWin = false,
        int targetId = -1,
        bool slip = false)
    {
        float elapsed = 0f;
        float accelerationDuration = 0.3f;

        // ── Acceleration ──────────────────────────────────────────────────
        while (elapsed < accelerationDuration)
        {
            float t = elapsed / accelerationDuration;
            float interval = Mathf.Lerp(maxStepInterval, minStepInterval, t);
            column.StepOffset(1);
            column.UpdateRenderers();
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // ── Full speed ────────────────────────────────────────────────────
        float fullSpeedEnd = spinDuration + stopDelay;
        while (elapsed < fullSpeedEnd)
        {
            column.StepOffset(1);
            column.UpdateRenderers();
            yield return new WaitForSeconds(minStepInterval);
            elapsed += minStepInterval;
        }

        // ── Extra random steps (prevents cols landing on same position) ───
        for (int i = 0; i < extraSteps; i++)
        {
            column.StepOffset(1);
            column.UpdateRenderers();
            yield return new WaitForSeconds(minStepInterval);
        }

        // ── Deceleration ──────────────────────────────────────────────────
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

        // ── Fake Win Alignment ────────────────────────────────────────────
        if (!fakeWin || targetId < 0) yield break;

        int itemCount = column.GetItemCount();

        if (!slip)
        {
            // Columns 1 & 2: creep forward until the target symbol is on the payline
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
            // Column 3: creep forward until ONE more step would land target on payline
            for (int i = 0; i < itemCount; i++)
            {
                if (column.GetNextPaylineItemId() == targetId) break;
                column.StepOffset(1);
                column.UpdateRenderers();
                yield return new WaitForSeconds(maxStepInterval);
            }

            // Dramatic near-miss pause — the player thinks they've won!
            yield return new WaitForSeconds(fakeWinPauseDuration);

            // Cruel slip — one step past the winning symbol
            column.StepOffset(1);
            column.UpdateRenderers();
        }
    }
}
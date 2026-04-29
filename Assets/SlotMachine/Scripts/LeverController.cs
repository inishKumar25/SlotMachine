/*
 
LEVER CONTROLLER CODE
 
LEVER FLOW:

1. Player clicks on the lever (mouse click).

2. We raycast using Physics2D to check:
   → Did the player actually click THIS lever?

3. If yes and NOT already spinning:
   → Trigger animation
   → Call SlotMachineController.Spin()

4. While spinning:
   → isSpinning = true (prevents spam clicks)

5. When SlotMachineController finishes:
   → it calls UnlockLever()

6. Lever becomes usable again.

*/


using UnityEngine;

public class LeverController : MonoBehaviour
{
    [Tooltip("Animator for the lever pull animation")]
    public Animator leverAnim;

    [Tooltip("Reference to the SlotMachineController to trigger Spin()")]
    public SlotMachineController SMC;

    [SerializeField] private bool isSpinning = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void HandleClick()
    {
        if (isSpinning) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCol = Physics2D.OverlapPoint(mousePos);

        if (hitCol != null && hitCol.gameObject == gameObject)
            PullLever();
    }

    void PullLever()
    {
        if (isSpinning) return;

        isSpinning = true;
        leverAnim.SetTrigger("IsClicked");
        SMC.Spin(this);
    }

    /// Called by SlotMachineController when all columns have stopped
    public void UnlockLever()
    {
        isSpinning = false;
        Debug.Log("Lever unlocked.");
    }
}
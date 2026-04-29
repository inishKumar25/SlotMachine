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

    /// <summary>Called by SlotMachineController when all columns have stopped.</summary>
    public void UnlockLever()
    {
        isSpinning = false;
        Debug.Log("Lever unlocked.");
    }
}
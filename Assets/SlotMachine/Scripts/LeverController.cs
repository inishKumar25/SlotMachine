using UnityEngine;

public class LeverController : MonoBehaviour
{
    public Animator leverAnim; //Animator for Lever

    [SerializeField] private bool isSpinning = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    /*

    HandleClick() uses to 


    */
    void HandleClick()
    {
        if (isSpinning) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCol = Physics2D.OverlapPoint(mousePos);

        if(hitCol != null && hitCol.gameObject == gameObject)
        {
            PullLever();
        }
    }

    void PullLever()
    {
        if (isSpinning) return;

        isSpinning = true;

        leverAnim.SetTrigger("IsClicked");


    }

    public void UnlockLever()
    {
        isSpinning = false;

    }
}

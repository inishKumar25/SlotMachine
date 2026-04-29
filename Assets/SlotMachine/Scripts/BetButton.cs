using UnityEngine;

public class BetButton : MonoBehaviour
{
    public int betAmount;
    public SlotMachineController slotMachine;

    public void SetBet()
    {
        slotMachine.currentBet = betAmount;
        Debug.Log("Bet set to: " + betAmount);
    }
}
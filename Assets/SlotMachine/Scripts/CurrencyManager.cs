using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Currency")]
    public int startingCoins = 100;
    public int currentCoins;

    [Header("UI")]
    public TextMeshProUGUI coinText;

    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        currentCoins = startingCoins;
        UpdateUI();
    }

    public bool CanAfford(int amount)
    {
        return currentCoins >= amount;
    }

    public void Spend(int amount)
    {
        currentCoins -= amount;
        UpdateUI();
        
    }

    public void Add(int amount)
    {
        currentCoins += amount;
        UpdateUI();
        
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + currentCoins;
    }


    
}
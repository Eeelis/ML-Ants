using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TMP_Text foodCollectedText;

    private int foodCollected;

    private void Awake()
    {
        Instance = this;
    }

    public void AddFood()
    {
        foodCollected += 1;
        foodCollectedText.text = "Food Collected: " + foodCollected;
    }

    public void ResetFood()
    {
        foodCollected = 0;
        foodCollectedText.text = "Food Collected: " + foodCollected;
    }
}

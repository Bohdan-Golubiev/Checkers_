using UnityEngine;
using TMPro;

public class TextSwap : MonoBehaviour
{
    public static TextSwap Instance;
    public TextMeshProUGUI turnText; 
    private bool isWhiteTurn = true; 

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void EndTurn()
    {
        isWhiteTurn = !isWhiteTurn;
        UpdateTurnText();
    }

    void UpdateTurnText()
    {
        turnText.text = isWhiteTurn ? "White checkers move" : "Black checkers move";
    }
}

using UnityEngine;
using TMPro;

public class TextSwap : MonoBehaviour
{
    public static TextSwap Instance;  // Глобальная ссылка
    public TextMeshProUGUI turnText;     // Ссылка на текст хода
    private bool isWhiteTurn = true;     // Очередность ходов

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void EndTurn()
    {
        isWhiteTurn = !isWhiteTurn; // Меняем ход
        UpdateTurnText();
    }

    void UpdateTurnText()
    {
        turnText.text = isWhiteTurn ? "White checkers move" : "Black checkers move";
    }
}

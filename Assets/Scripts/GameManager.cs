using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int whiteCheckers = 12;
    public int blackCheckers = 12;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void RemoveChecker(string team)
    {
        if (team == "WhitePawn"|| team == "WhiteCrown")
            whiteCheckers--;
        else if (team == "BlackPawn" || team == "BlackCrown")
            blackCheckers--;

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (whiteCheckers == 0)
        {
            PlayerPrefs.SetString("Winner", $"Congrats!\nBlack team win!");
            SceneManager.LoadScene("EndScene");
        }
        else if (blackCheckers == 0)
        {
            PlayerPrefs.SetString("Winner", $"Congrats!\nWhite team win!");
            SceneManager.LoadScene("EndScene");
        }
    }
}

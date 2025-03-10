using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    // Вызывается кнопкой "Старт игры"
    public void StartGame()
    {
        // Замените "GameScene" на название вашей игровой сцены
        SceneManager.LoadScene("GameScene");
    }

    // Вызывается кнопкой "Выход"
    public void ExitGame()
    {
        // Если игра запущена вне редактора
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    // ���������� ������� "����� ����"
    public void StartGame()
    {
        // �������� "GameScene" �� �������� ����� ������� �����
        SceneManager.LoadScene("GameScene");
    }

    // ���������� ������� "�����"
    public void ExitGame()
    {
        // ���� ���� �������� ��� ���������
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

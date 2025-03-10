using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndSceneManager : MonoBehaviour
{
    public TextMeshProUGUI winnerText;

    void Start()
    {
        if (PlayerPrefs.HasKey("Winner"))
        {
            winnerText.text = PlayerPrefs.GetString("Winner");
        }

    }
}

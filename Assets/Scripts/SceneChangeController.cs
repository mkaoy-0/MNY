using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeController : MonoBehaviour
{
   // [SerializeField] private string nextScene = "MainScene";
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void StartButtonClicked()
    {
        PenaltySettings.instance.IsntPenaltyMode(true);
        Debug.Log("スタートボタン押した！ペナルティTRUE");
        ChangeScene("MainScene");
    }

    public void MissPenaltyButtonClicked()
    {
        PenaltySettings.instance.IsntPenaltyMode(false);
        Debug.Log("ペナルティなしボタン押した！ペナルティFALSE");
        ChangeScene("MainScene");
    }

    /*/
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                ChangeScene(nextScene);
            }
        }
    }
    /**/
}

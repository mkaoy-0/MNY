using UnityEngine;
using System.Collections.Generic;

public class EndingManager : MonoBehaviour
{
    public GameManager gameManager;
    public ParameterManager parameterManager;
    public DataManager dataManager;
    public FadeManager fadeManager;

    [Header("支持率→SAN値の順")]
    [SerializeField] private EndingData[] gameOverEnding = new EndingData[2];
    public EndingData[] GameOverEndhing => gameOverEnding;
    [SerializeField] private EndingData anotherEnding;

    public EndingScreenEffect endingScreenEffect;

    public List<string> endNameList;
    private string _endName;
    public string EndName => _endName;



    // セリフ番号と暗くする段階を照合する関数
    public void CheckAndDarken(int currentIndex, int totalMessages)
    {
        if (dataManager.CurrentEndingData != anotherEnding)
        {
            endingScreenEffect._CheckAndDarken(currentIndex, totalMessages);
        }
    }

    public void GameOver()
    {
        Debug.Log("ゲームオーバー");
   
        BGMManager.instance.FadeChangeBGM(BGMManager.instance.gameOverBGM);

        if (parameterManager.SupportRate <= 0)
        {
            Debug.Log("支持率０エンド");
            dataManager.StartEnding(gameOverEnding[0]);
        }
        else
        {
            Debug.Log("SAN値０エンド");
            dataManager.StartEnding(gameOverEnding[1]);
        }
        _endName = "ゲームオーバー";
    }


    public void AnotherEnding()
    {
        //  StartCoroutine(fadeManager.FadeOutBuka(endingScreenEffect.blackOverlay, "fade"));
        endingScreenEffect.endText.alpha = 0f;
        Debug.Log("アナザーエンド");
        dataManager.StartAnotherEnding(anotherEnding);
        _endName = "Another End";
        BGMManager.instance.FadeChangeBGM(BGMManager.instance.prologueBGM);
    }

    public void TriggerEnding()
    {
        // ParameterManagerに作っておいた判定関数を使う
        string endingType = parameterManager.GetEndingType();

        // endingTypeに合わせて、各エンディングシーンへ飛ばす
        Debug.Log("エンディング到達: " + endingType);
        // SceneManager.LoadScene(endingType); // など

        switch (endingType) {
            case "spHsnH":
                gameManager.StartEnding(0);
                _endName = endNameList[0];
                break;
            case "spLsnH":
                gameManager.StartEnding(1);
                _endName = endNameList[1];
                break;
            case "spHsnL":
                gameManager.StartEnding(2);
                _endName = endNameList[2];
                break;
            case "spLsnL":
                gameManager.StartEnding(3);
                _endName = endNameList[3];
                break;
            default:
                break;
        }
    }

    public bool CheckAnotherEndingCondition()
    {
        // 特定のエンディング名かつ、姫拉致フラグが立っているか
        // データマネージャーやパラメータマネージャーから値を取得する
        return (_endName == endNameList[0] && parameterManager.IsHimeRachi);
    }
}

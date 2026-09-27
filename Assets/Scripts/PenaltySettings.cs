using UnityEngine;

public class PenaltySettings : MonoBehaviour
{
    public static PenaltySettings instance;

    [SerializeField] private bool _isPenalty = true;

    private void Awake()
    {
        // シーンをまたいでも instance が使えるように自分を登録する
        if (instance == null)
        {
            instance = this;
            // 親が DontDestroyOnLoad でも、念のため自分も消えないように設定
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void IsntPenaltyMode(bool res)
    {
        _isPenalty = res;
    }

    public bool GetNoePenaltySetting()
    {
        return _isPenalty;
    }
}

using UnityEngine;

public class TimePenaltyManager : MonoBehaviour
{
    // 時間経過で反応する
    public ParameterManager parameterManager;
    public UIManager uiManager;

    [SerializeField] private float _timeLimitUnit = 5f;
    public float TimeLimitUnit => _timeLimitUnit;

    public void ApplyTimePenalty(DayData data)
    {
        // 1. 値の減少（ミス用を流用するか、専用のを作る）
        parameterManager.ApplyMissPenalty();

        // 2. 部下のセリフを変更
      //  uiManager.TimeLimitReaction(data);
        uiManager.ShowBukaTemporaryReaction(data, data.timeLimitReaction);
    }
}

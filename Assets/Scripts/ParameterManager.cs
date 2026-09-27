using UnityEngine;

public class ParameterManager : MonoBehaviour
{
    public ParaUIManager paraUIManager; // インスペクターでアタッチ

    [Header("Parameters_初期値")]
    [SerializeField] private int _supportRate = 50; // 初期値
    [SerializeField] private int _sanity = 50;
    // 外から値を読み取る専用の窓口（プロパティ）
    public int SupportRate => _supportRate;
    public int Sanity => _sanity;

    [Header("Parameters_最大値")]
    [SerializeField] private int _maxSupportRate = 100; // 最大値
    [SerializeField] private int _maxSanity = 100;
    public int MaxSupportRate => _maxSupportRate;
    public int MaxSanity => _maxSanity;

    // ミスした時のペナルティ量
    private int _missSupportPenalty = -1;
    private int _missSanityPenalty = -1;

    public bool IsHimeRachi { get; private set; } = false; // 初期値は拉致してない


    void Start()
    {
        // ゲーム開始時にスライダーの初期位置を合わせる
        paraUIManager.supportSlider.maxValue = (float)_maxSupportRate;
        paraUIManager.sanitySlider.maxValue = (float)_maxSanity;
        // 初期化時はアニメーションさせずに値をセット
     //   paraUIManager.UpdateSupport(_supportRate, "");
     //   paraUIManager.UpdateSanity(_sanity, "");
        //
        paraUIManager.supportSlider.value = (float)_supportRate;
        paraUIManager.sanitySlider.value = (float)_sanity;
        /**/

        if (PenaltySettings.instance == null)
        {
            Debug.LogError("PenaltySettingsのインスタンスが見つかりません！シーンAから正しく引き継がれていますか？");
            return; // エラー回避
        }

        if (PenaltySettings.instance.GetNoePenaltySetting())
        {
            _missSupportPenalty = -5;
            _missSanityPenalty = -6;
            Debug.Log("ミス：SP-5, SN-6");
        }
        else
        {
            _missSupportPenalty = 0;
            _missSanityPenalty = 0;
            Debug.Log("ミスペナルティなし");
        }
    }

    // ★全ての値変更をこの「窓口」に集約する
    private void SetSupport(int amount)
    {
        int oldVal = _supportRate;
        _supportRate = Mathf.Clamp(_supportRate + amount, 0, _maxSupportRate);

        // 増減判定
        string trigger = "";
        if (_supportRate > oldVal) trigger = "valueUp";
        else if (_supportRate < oldVal) trigger = "valueDown";

        paraUIManager.UpdateSupport(_supportRate, trigger);
    }

    private void SetSanity(int amount)
    {
        int oldVal = _sanity;
        _sanity = Mathf.Clamp(_sanity + amount, 0, _maxSanity);

        string trigger = "";
        if (_sanity > oldVal) trigger = "valueUp";
        else if (_sanity < oldVal) trigger = "valueDown";

        paraUIManager.UpdateSanity(_sanity, trigger);
    }

    // GameManagerから呼ばれる変動関数
    public void AddValues(int sChange, int nChange)
    {
        Debug.Log($"[パラメータ] 支持率: {_supportRate}, SAN値: {_sanity}");

        SetSupport(sChange);
        SetSanity(nChange);
    }

    // ゲームオーバーしたか判定
    public bool IsGameOver()
    {
        return _supportRate <= 0 || _sanity <= 0;
    }

    // エンディング判定
    public string GetEndingType()
    {
        // ★判定ラインを「65」や「70」に下げる
        float highLine = 70f;
        float lowLine = 45f;

        // 1. 両方高い（ホワイト魔王）：初期値から微増、または維持できていればOK
        if (_supportRate >= highLine && _sanity >= highLine) return "spHsnH";

        // 2. 両方低い（自滅）：初期値から大幅に下げた場合
        if (_supportRate < lowLine && _sanity < lowLine) return "spLsnL";

        // 3. どっちつかず / 特化型
        // 支持率がSAN値より高いなら「軍人寄り」、逆なら「聖人寄り」
        return (_supportRate >= _sanity) ? "spHsnL" : "spLsnH";
    }

    // ミスしたときのペナルティ
    public void ApplyMissPenalty()
    {
        SetSupport(_missSupportPenalty);
        SetSanity(_missSanityPenalty);
    }

    public void SetHimeRachi(bool value)
    {
        IsHimeRachi = value;
        Debug.Log($"姫拉致フラグが {value} に更新されました");
    }

}
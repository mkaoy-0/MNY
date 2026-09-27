using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public GameManager gameManager; // currentDay取得用
    public TypingManager typingManager; // 打ち始めた選択肢判別用(StartSelectionPhase()で使用)
   // public DayData[] dayData; // インスペクターで作成したDay1_Knightをアタッチする
    public UIManager uiManager;
    public ParaUIManager paraUIManager;
    public EndingManager endingManager;

    [Header("通常ルートのリスト")]
    [SerializeField] private List<DayData> regularDays;

    [SerializeField] private List<DayData> endingDays;
    public List<DayData> EndingDays => endingDays;

    private DayData _currentDayData;

    private int messageIndex = 0;

   // private List<Message> _currentMessageList; // 現在再生中のセリフリスト
    private Choice _lastSelectedChoice; // 直前に選んだ選択肢を保持

    private Message _currentMessage;

    private EndingData _currentEndingData;
    public EndingData CurrentEndingData => _currentEndingData;
  //  private bool _isEndingMode = false;

    private bool _isUsingEndingSO = false; // EndingData(SO)を使っているかフラグ

    public bool IsEndingMode { get; private set; } // これを各Load関数で書き換える


    // GameManagerから「?日目を準備せよ」と命令される関数
    public void LoadDayData(int day, int currentSupport, int currentSan)
    {
        IsEndingMode = false; // 通常モード

        messageIndex = 0;

        // リストから該当日を取得
        _currentDayData = regularDays[day];

        uiManager.SetBukaLayout(_currentDayData);

        paraUIManager.paraSet.SetActive(true);

        Debug.Log($"[DataManager] {day}日目のデータをロード完了: {_currentDayData.name}");
    }

    // エンディング用
    public void LoadEndingDayData(int day, int currentSp, int currentSn)
    {
        IsEndingMode = true; // エンディングモード
        

        messageIndex = 0;
        _isUsingEndingSO = false; // SOモードOFF（DayDataモード）
        _currentDayData = endingDays[day];

        uiManager.SetBukaLayout(_currentDayData);


        uiManager.InitializeUI();

        uiManager.bukaGroup.alpha = 0f;

    }

    public bool ShowNextIntro()
    {
        if (_currentDayData == null)
        {
            Debug.Log("dayDataがない！");
            return false;
        }

        if (messageIndex < _currentDayData.introMessages.Count)
        {
            // 最初に現在のメッセージとして保持する
            _currentMessage = _currentDayData.introMessages[messageIndex];

            // UIマネージャーにデータを投げて、画面を更新してもらう
            uiManager.UpdateTalk(_currentDayData.introMessages[messageIndex]);
            messageIndex++;
            return true; // まだセリフがある
        }
        // セリフがもうない
       // Debug.Log("導入セリフ終了。選択肢フェーズへ");
        return false;
    }

    public bool ShowNextReaction()
    {
        if (messageIndex < _lastSelectedChoice.reactions.Count)
        {
            // 最初に代入
            _currentMessage = _lastSelectedChoice.reactions[messageIndex];
            uiManager.UpdateTalk(_lastSelectedChoice.reactions[messageIndex]);
            messageIndex++;
            return true;
        }

        return false; // 全て読み終わった
    }

    // GameManagerから呼ばれるチェック用関数
    public bool HasReasonPhase()
    {
        return _lastSelectedChoice.hasReasonSelection;
    }


    // 理由フェーズかどうかの引数を受けるように調整
    public void StartSelectionPhase(bool isReason)
    {
        Debug.Log(isReason ? "理由選択フェーズ開始" : "決断選択フェーズ開始");

        // 理由なら _lastSelectedChoice.reasonChoices、最初なら dayData.firstChoices
        List<Choice> choices = isReason ? _lastSelectedChoice.reasonChoices : _currentDayData.firstChoices;

        uiManager.SetChoices(choices);
        typingManager.PrepareSelection(choices);
    }

    public void SetAfterTypingSequences(Choice choice)
    {
        _lastSelectedChoice = choice;
        messageIndex = 0; // メッセージのインデックスをリセット

        ShowNextReaction();
    }


    // 最初の選択肢リストが空、あるいは設定されていないかチェック
    public bool IsFirstChoiceEmpty()
    {
        // _currentDayDataがnullでないことを確認し、リストが0件ならtrue
        return _currentDayData == null || _currentDayData.firstChoices == null || _currentDayData.firstChoices.Count == 0;
    }

    // GameManagerから今のメッセージの状態を聞くための関数を追加
    public Message GetCurrentMessage()
    {
        return _currentMessage;
    }

    // 現在のDayDataを取得
    public DayData GetCurrentDayData()
    {
        return _currentDayData;
    }

    // 最終日かどうかを取得
    public bool IsLastDay()
    {
        return (_currentDayData == regularDays[regularDays.Count - 1]);
    }


    // ゲームオーバー
    // エンディングデータをセットして再生開始する
    public void StartEnding(EndingData data)
    {
        _currentEndingData = data;
        _isUsingEndingSO = true; // SOモードON
        messageIndex = 0;
        if (_currentEndingData == endingManager.GameOverEndhing[0])
        {
            uiManager.SetBukaLayout(endingDays[0]);
            uiManager.bukaGroup.alpha = 1f;
        }


        // ここで最初の1件目を _currentMessage に入れておく！
        if (_currentEndingData.dialogues.Count > 0)
        {
            _currentMessage = _currentEndingData.dialogues[0];
        }

        // ShowNextEndingMessage();

        // UIに表示
        uiManager.UpdateTalk(_currentMessage);

        // 次回クリックに備えてインデックスを進める
        messageIndex++;
    }

    public void StartAnotherEnding(EndingData data)
    {
        _currentEndingData = data;
        _isUsingEndingSO = true; // SOモードON
        messageIndex = 0;

        uiManager.AnotherEndSetBukaLayout(data);
        uiManager.bukaGroup.alpha = 1f;
        if (_currentEndingData.dialogues.Count > 0)
        {
            _currentMessage = _currentEndingData.dialogues[0];
        }
        uiManager.UpdateTalk(_currentMessage);
        messageIndex++;
    }

    public bool ShowNextEndingMessage()
    {
        if (messageIndex < _currentEndingData.dialogues.Count)
        {
            _currentMessage = _currentEndingData.dialogues[messageIndex];
            uiManager.UpdateTalk(_currentMessage);
            messageIndex++;
            return true;
        }
        return false; // 全て終了
    }


    // GameManagerから「次を流して」と言われたときに振り分ける関数
    public bool ShowNextAnyMessage()
    {
        if (_isUsingEndingSO)
        {
            return ShowNextEndingMessage(); // EndingData(SO)のリストを進める
        }
        else
        {
            return ShowNextIntro(); // DayDataのintroMessagesを進める
        }
    }


    public int GetMessageIndex()
    {
        return messageIndex;
    }
    // DataManager.cs

    public int GetTotalMessageCount()
    {
        // EndingData(SO)モードか DayDataモードかによって数えるリストを変える
        if (_isUsingEndingSO)
            return _currentEndingData.dialogues.Count;
        else
            return _currentDayData.introMessages.Count;
    }
}
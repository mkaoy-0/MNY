using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public ParameterManager parameterManager;
    public ParaUIManager paraUIManager;
    public TypingManager typingManager;
    public DataManager dataManager;
    public UIManager uiManager;
    public FadeManager fadeManager;
    public TimePenaltyManager timePenaltyManager;
    public EndingManager endingManager;
    public EndingScreenEffect endingScreenEffect;
    public SceneChangeController sceneChangeManager;

    public enum GameState { Intro, Selecting, Typing, Reacting, SelectingReason, EndingCutscene, Finished, RuleExplanation }
    public GameState currentState;

    [SerializeField] private int _currentDay = 0;
    public int CurrentDay => _currentDay;

    // 時間経過
    [SerializeField] private float _timer = 0f;

    public GameObject rulePanel;

    public GameObject skipButton;

    void Start()
    {
        BGMManager.instance.FadeChangeBGM(null);

        endingScreenEffect.blackOverlay.alpha = 1f;

        StartCoroutine(fadeManager.FadeOutBuka(endingScreenEffect.blackOverlay, "fade"));
        _currentDay = 0;

        // 初期設定
        uiManager.InitializeUI();

        // day0スキップボタン
        skipButton.SetActive(true);

        SetState(GameState.Intro);
        // 最初の日の、最初の決断を開始するような流れ
        StartNewDay(_currentDay);
    }

    void Update()
    {
        // ルールパネルが表示されている時だけ、クリックを待つ
        if (rulePanel.activeSelf && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            rulePanel.SetActive(false);
            StartCoroutine(fadeManager.FadeOutBuka(endingScreenEffect.blackOverlay, "fade"));
            GoToNextDay(); // 1日目を開始する処理へ

            // BGM
            BGMManager.instance.FadeChangeBGM(BGMManager.instance.nomalBGM);
        }
        if (currentState == GameState.RuleExplanation) return;


        // 終了後
        // 演出が完了しており、かつクリックかスペースが押されたら
        // エンディングが完全に終わった後の入力待ち
        if (currentState == GameState.Finished && endingScreenEffect.CanGoToTitle)
        {

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("エンディング後のクリックを検知！遷移します。");
                endingScreenEffect.CanGoToTitle = false; // 二重に実行されないように即座に折る

                // 条件判定
                if (endingManager.CheckAnotherEndingCondition())
                {
                    // アナザーエンディングを開始
                    endingManager.AnotherEnding();
                    SetState(GameState.EndingCutscene);
                }
                else
                {
                    // 通常：タイトルシーンへ（別スクリプトの関数を呼ぶ）
                    sceneChangeManager.ChangeScene("TitleScene");
                }
                return; // ここで処理を抜ける
            }
        }

        if (currentState == GameState.Selecting || currentState == GameState.SelectingReason)
        {
            _timer += Time.deltaTime;

            // DayDataで設定した単位時間を超えたらペナルティ
            DayData currentData = dataManager.GetCurrentDayData();
            if (_timer >= timePenaltyManager.TimeLimitUnit)
            {
                timePenaltyManager.ApplyTimePenalty(currentData);
                _timer = 0f; // タイマーリセットして次の〇秒へ
            }
        }
        else
        {
            _timer = 0f; // 選択中以外はリセット
        }


        // UIManagerがフェード中なら、クリック入力を一切受け付けない
        if (fadeManager.IsFading) return;

        // クリック/スペース入力の共通判定
        bool isNextPressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);

        if (!isNextPressed) return;

        // 状態ごとのクリック処理（既存のコード）
        HandleNextStep();

    }


    // UIManagerからフェード完了時に呼ばれる
    public void OnFadeComplete()
    {
        // 自動で次のセリフを表示する
        HandleNextStep();
    }

    // 共通の「次へ進む」処理を関数化しておくと便利
    void HandleNextStep()
    {
        // Finished 状態なら、通常の進行処理を一切行わない
        if (currentState == GameState.Finished) return;


        // 0. 文字送り中なら、次のセリフに行かずに「全表示（スキップ）」だけする
        // どのTypeWriteEffectが動いているかチェック（UIManager経由で確認）
        if (uiManager.IsAnyTextPlaying())
        {
            uiManager.SkipAnyText();
            return; // ここで終了（セリフは進まない）
        }

        // DataManagerから「今表示されているメッセージ」を取得
        Message currentMsg = dataManager.GetCurrentMessage();

        // そのメッセージに演出（入退場）があるか確認し、あれば実行して止まる
        // UIManagerのCheckAndExitがtrueを返せばフェードが始まっているので、ここでは何もしない
        if (uiManager.CheckAndExit(currentMsg)) return;

        // 演出がなければ（または終わっていれば）通常の進行
        switch (currentState)
        {
            case GameState.Intro:
                // 導入セリフを送り、終わったら（falseが返ってきたら）
                if (!dataManager.ShowNextIntro())
                {
                    // 選択肢があるかチェック
                    if (dataManager.IsFirstChoiceEmpty())
                    {
                        //Debug.Log("選択肢がないため、そのまま一日の終了判定へ進みます");
                        CheckEndCondition(); // 前に作った終了・次日判定関数
                    }
                    else
                    {
                        SetState(GameState.Selecting); // 選択肢があれば通常通り選択フェーズへ
                    }
                }
                break;
            case GameState.EndingCutscene:
                // セリフを次に進める
                if (dataManager.ShowNextAnyMessage())
                {
                    // 現在の番号と、今ロードされているリストの総数を渡す
                    int cur = dataManager.GetMessageIndex();
                    int total = dataManager.GetTotalMessageCount(); // DataManagerにこれを作る

                    endingManager.CheckAndDarken(cur, total);
                }
                else
                {
                    SetState(GameState.Finished);

                    Debug.Log("エンディング終了");
                    // uiManager.ShowResultPanel();
                    uiManager.ResetAnotherText();
                    StartCoroutine(endingScreenEffect.LastBlackout(endingManager.EndName));
                }
                break;
            case GameState.Reacting:
                // 反応セリフ（reactions）を送り、終わったら分岐
                if (!dataManager.ShowNextReaction())
                {
                    if (dataManager.IsEndingMode)
                    {
                        // ステートを Finished にして、通常のクリック進行を止める
                        SetState(GameState.Finished);

                        Debug.Log("エンディングの全行程が終了しました");
                        //  uiManager.ShowResultPanel(); // 「完」を表示
                      //  endingScreenEffect.CanGoToTitle = false;
                        StartCoroutine(endingScreenEffect.LastBlackout(endingManager.EndName));
                    }
                    else
                    {
                        // 「理由フェーズ」があるかチェック
                        if (dataManager.HasReasonPhase()) SetState(GameState.SelectingReason);
                        else CheckEndCondition();
                    }
                }
                break;
            default:
                break;
        }
    }


    public void StartNewDay(int day)
    {
        Debug.Log($"{day}日目開始");

        _currentDay = day;

        // 支持率を添えて DataManager に投げる
        dataManager.LoadDayData(_currentDay, parameterManager.SupportRate, parameterManager.Sanity);

        // セリフ開始
        dataManager.ShowNextIntro();
        SetState(GameState.Intro);
    }

    // エンディング
    public void StartEnding(int num)
    {
        Debug.Log($"エンディング{num}開始");

        _currentDay = num;

        // 支持率を添えて DataManager に丸投げ！
        dataManager.LoadEndingDayData(_currentDay, parameterManager.SupportRate, parameterManager.Sanity);
        
        // タイピングをさせたいので、通常の日と同じく Intro から始める
        dataManager.ShowNextIntro();
        SetState(GameState.Intro);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"【State Changed to: {currentState}】");

        switch (currentState)
        {
            case GameState.Selecting:
                // 最初の決断。isReason = false
                dataManager.StartSelectionPhase(false);
                break;

            case GameState.SelectingReason:
                // 理由の決断。isReason = true
                dataManager.StartSelectionPhase(true);
                break;

            case GameState.Typing:
                // タイピング開始時の演出など
                break;
        }
    }



    // タイピングが終わった時にTypingManagerから（あるいはEventで）呼ばれる
    // どのChoiceを打ち終わったかを受け取る
    public void OnTypeFinished(Choice selectedChoice)
    {
        // パラメータを動かす
        parameterManager.AddValues(selectedChoice.supportChange, selectedChoice.sanChange);

        // DataManagerに「この選択肢の後のセリフ」をセットさせる
        dataManager.SetAfterTypingSequences(selectedChoice);

        // 状態を「反応フェーズ」へ移行
        SetState(GameState.Reacting);
    }

  
    void CheckEndCondition()
    {
        // ゲームオーバー、または最終日（4日目終了時）なら特別な演出へ
        if (parameterManager.IsGameOver() || dataManager.IsLastDay())
        {
            StartCoroutine(SpecialTransitionSequence());
        }
        else if (_currentDay == 0) // プロローグ終了
        {
            skipButton.SetActive(false); // スキップボタン消す
            StartCoroutine(fadeManager.FadeInBuka(endingScreenEffect.blackOverlay, "fade", 0.7f));
            rulePanel.SetActive(true);
            // day0終了時のUIを適用
            uiManager.day0EndUI();
            // ゲームの進行を一時的に止めるステートにする
            SetState(GameState.RuleExplanation);
        }
        else
        {
            // 1~3日目の通常時は、そのまま次の日へ（暗転なし）
            GoToNextDay();
        }
    }

    private IEnumerator SpecialTransitionSequence()
    {
        // エンディング前だけの特別な暗転
        Debug.Log("エンディング前の暗転演出開始");

        // 画面全体を真っ暗にする
        yield return StartCoroutine(fadeManager.FadeInBuka(endingScreenEffect.blackOverlay, "fade"));

        paraUIManager.paraSet.SetActive(false);

        yield return new WaitForSeconds(1.5f); // 少し長めの「間」

        if (parameterManager.IsGameOver())
        {
            // ゲームオーバー演出の準備
            endingManager.GameOver();
            SetState(GameState.EndingCutscene);
        }
        else
        {
            // 最終エンディングの準備（例：5番目のエンディングをロードなど）
            endingManager.TriggerEnding();
        }

        // 準備ができたら明転
        yield return StartCoroutine(fadeManager.FadeOutBuka(endingScreenEffect.blackOverlay, "fade"));

        // 最初のセリフを表示
     //   dataManager.ShowNextIntro();
    }

    private void GoToNextDay()
    {
        Debug.Log("一日終了。次の日へ移行します。");
        // _currentDay++ して StartNewDay() を呼ぶか、リザルト画面へ
        _currentDay++;
        StartNewDay(_currentDay);

    }

    // ミス後のタイマー猶予
    public void PauseTimerForMiss(float duration)
    {
        // タイマーを少し戻すか、一時的にカウントを止める
        _timer -= duration;
        if (_timer < 0) _timer = 0;
    }

    // day0スキップ
    // スキップボタンに登録
    public void day0Skip()
    {
       //  skipButton.SetActive(false);
        CheckEndCondition();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TypingManager : MonoBehaviour
{
    // タイピング終わったことをGameManager.csに伝える(ここのTypeFinished()で実行)
    [SerializeField] private GameManager gameManager;
    // 選択肢タイピング用 && 選択肢非表示用
    [SerializeField] private UIManager uiManager;
    // ミスしたときのパラメータ変動
    [SerializeField] private ParameterManager parameterManager;
    [SerializeField] private DataManager dataManager;


    [SerializeField] private TypingSoundManager typingSoundManager;

    // ミスした回数
    [SerializeField] private int _misCount = 0;
    public int MisCount => _misCount;
    [SerializeField] private int reactionMisCount = 2; // 何回目のミスで部下の反応が変わるか

    private List<Choice> _selectableOptions; // 提示されている3つの選択肢
    private Choice _currentChoice;          // 確定した選択肢
    private int _currentChoiceNum;
    private bool _isSelectionReady = false; // 打ち始めを待っているか
    private bool _isTyping = false;         // 絶賛タイピング中か


  //  [SerializeField] private TextMeshProUGUI textJapanese; // ここに日本語表示のTextMeshProをアタッチする。
    private TextMeshProUGUI textRoman; // ここにローマ字表示のTextMeshProをアタッチする。

    private readonly List<char> _roman = new List<char>();

    private int _romanIndex;


    private bool _isWindows; //追加する
    private bool _isMac; //追加する

    void Start()
    {
        // 以下を追加する
        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            _isWindows = true;
        }

        if (SystemInfo.operatingSystem.Contains("Mac"))
        {
            _isMac = true;
        }
    }

    // DataManagerから呼ばれる。選択肢をセットして待機。
    public void PrepareSelection(List<Choice> options)
    {
        _selectableOptions = options;
        _isSelectionReady = true;
        _isTyping = false;
        _romanIndex = 0;

        _misCount = 0;

        Debug.Log("打ち始めを待機中...");
    }


    private void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown)
        {
            char input = GetCharFromKeyCode(Event.current.keyCode);
            if (input == '\0') return;

            // まだ選択肢が決まっていない「待ち」の状態
            if (_isSelectionReady)
            {
                CheckFirstStroke(input);
            }
            // 既に選択肢が決まってタイピング中の状態
            else if (_isTyping)
            {
                HandleTyping(input);
            }
        }
    }

    // 最初の一打をチェック
    void CheckFirstStroke(char input)
    {
        for (int i = 0; i < _selectableOptions.Count; i++) // 選択肢の数分回す
        {
            // ローマ字の1文字目が一致するか
            if (_selectableOptions[i].typeRoman[0] == input)
            {
                // 選択肢確定！
                _currentChoice = _selectableOptions[i];
                _currentChoiceNum = i;
                _isSelectionReady = false;
                _isTyping = true;

                if (_selectableOptions[i].triggerHimeRachi) parameterManager.SetHimeRachi(true); // 姫拉致フラグ

                // TypingManager内のリスト（_roman）に確定した文字をセット
                SetTargetText(_currentChoice.typeRoman);

                // インデックスを1進める（1文字目は今打ったので）
                _romanIndex = 1;

                // UI表示更新
              //  textJapanese = uiManager.choiceTexts_jp[i];
                textRoman = uiManager.choiceTexts_roman[i];
                textRoman.text = GenerateTextRoman();
                // 吹き出しフォーカス
                uiManager.FocusAtChoice(i);
                

                Debug.Log($"選択肢確定: {_currentChoice.typeJapanese}");
                return;
            }
        }
        Debug.Log("不一致なキーです");
    }

    // 文字列をセットする（以前のInitializeQuestionの一部を流用）
    void SetTargetText(string targetRoman)
    {
        _roman.Clear();
        foreach (char c in targetRoman)
        {
            _roman.Add(c);
        }
        _roman.Add('@');
    }

    // 正解・不正解判定
    void HandleTyping(char input)
    {
        int result = InputKey(input);

        switch (result)
        {
            case 1: // 正解タイプ時
                _romanIndex++;

                typingSoundManager.PlayTypeSound();

                // 次の文字が末尾の '@' かチェック
                if (_roman[_romanIndex] == '@')
                {
                    textRoman.text = GenerateTextRoman(); // 最後の1文字を色変えするために更新
                    TypeFinished();
                }
                else
                {
                    textRoman.text = GenerateTextRoman();
                }
                break;
            case 2: // ミスタイプ時
                MissType();
                break;
        }
    }

    // ミスタイプしたときの処理
    void MissType()
    {
        Debug.Log("ミス！");
        typingSoundManager.PlayTypeMissSound();

        _misCount++;

        // ミスしたらタイマーを一時停止（GameManagerにフラグを持たせるか、時間を巻き戻す）
      //  gameManager.PauseTimerForMiss(1.0f);

        // 吹き出し、魔王の顔
        uiManager.MissedAnimation(_currentChoiceNum);
        uiManager.ShowMaouDamage();

        // 一定回数ミスったら
        if (_misCount > reactionMisCount)
        {
            // ★間違い！ペナルティ実行
            parameterManager.ApplyMissPenalty();
            // 部下の反応を変える
            DayData data = dataManager.GetCurrentDayData();
            uiManager.ShowBukaTemporaryReaction(data, data.missReaction);
          //  _misCount = 0;
        }
    }

    void TypeFinished()
    {
        Debug.Log("全部打ち終わった!");
        // 
        uiManager.CancelBukaReaction();
        // 選択肢非表示
        uiManager.HideAllChoices();
        // タイピング終わったことをGameManager.csに伝える
        gameManager.OnTypeFinished(_currentChoice);
    }


    string GenerateTextRoman()
    {
        string text = "<style=typed>";
        for (int i = 0; i < _roman.Count; i++)
        {
            if (_roman[i] == '@')
            {
                break;
            }

            if (i == _romanIndex)
            {
                text += "</style><style=untyped>";
            }

            text += _roman[i];
        }

        text += "</style>";

        return text;
    }


    // 正誤判定
    int InputKey(char inputChar)
    {
        // 追加：すでに最後まで打ち終わっていたら何もしない
        if (_romanIndex >= _roman.Count || _roman[_romanIndex] == '@')
        {
            return 0;
        }

        char prevChar3 = _romanIndex >= 3 ? _roman[_romanIndex - 3] : '\0';
        char prevChar2 = _romanIndex >= 2 ? _roman[_romanIndex - 2] : '\0';
        char prevChar = _romanIndex >= 1 ? _roman[_romanIndex - 1] : '\0';
        char currentChar = _roman[_romanIndex];
        char nextChar = _roman[_romanIndex + 1];
        char nextChar2 = nextChar == '@' ? '@' : _roman[_romanIndex + 2];

        if (inputChar == '\0')
        {
            return 0;
        }

        if (inputChar == currentChar)
        {
            return 1;
        }

        //「い」の柔軟な入力（Windowsのみ）
        if (_isWindows && inputChar == 'y' && currentChar == 'i' &&
            (prevChar == '\0' || prevChar == 'a' || prevChar == 'i' || prevChar == 'u' || prevChar == 'e' ||
             prevChar == 'o'))
        {
            _roman.Insert(_romanIndex, 'y');
            return 1;
        }

        if (_isWindows && inputChar == 'y' && currentChar == 'i' && prevChar == 'n' && prevChar2 == 'n' &&
            prevChar3 != 'n')
        {
            _roman.Insert(_romanIndex, 'y');
            return 1;
        }

        if (_isWindows && inputChar == 'y' && currentChar == 'i' && prevChar == 'n' && prevChar2 == 'x')
        {
            _roman.Insert(_romanIndex, 'y');
            return 1;
        }

        //「う」の柔軟な入力（「whu」はWindowsのみ）
        if (inputChar == 'w' && currentChar == 'u' && (prevChar == '\0' || prevChar == 'a' || prevChar == 'i' ||
                                                       prevChar == 'u' || prevChar == 'e' || prevChar == 'o'))
        {
            _roman.Insert(_romanIndex, 'w');
            return 1;
        }

        if (inputChar == 'w' && currentChar == 'u' && prevChar == 'n' && prevChar2 == 'n' && prevChar3 != 'n')
        {
            _roman.Insert(_romanIndex, 'w');
            return 1;
        }

        if (inputChar == 'w' && currentChar == 'u' && prevChar == 'n' && prevChar2 == 'x')
        {
            _roman.Insert(_romanIndex, 'w');
            return 1;
        }

        if (_isWindows && inputChar == 'h' && prevChar2 != 't' && prevChar2 != 'd' && prevChar == 'w' &&
            currentChar == 'u')
        {
            _roman.Insert(_romanIndex, 'h');
            return 1;
        }

        //「か」「く」「こ」の柔軟な入力（Windowsのみ）
        if (_isWindows && inputChar == 'c' && prevChar != 'k' &&
            currentChar == 'k' && (nextChar == 'a' || nextChar == 'u' || nextChar == 'o'))
        {
            _roman[_romanIndex] = 'c';
            return 1;
        }

        //「く」の柔軟な入力（Windowsのみ）
        if (_isWindows && inputChar == 'q' && prevChar != 'k' && currentChar == 'k' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'q';
            return 1;
        }

        //「し」の柔軟な入力
        if (inputChar == 'h' && prevChar == 's' && currentChar == 'i')
        {
            _roman.Insert(_romanIndex, 'h');
            return 1;
        }

        //「じ」の柔軟な入力
        if (inputChar == 'j' && currentChar == 'z' && nextChar == 'i')
        {
            _roman[_romanIndex] = 'j';
            return 1;
        }

        //「しゃ」「しゅ」「しぇ」「しょ」の柔軟な入力
        if (inputChar == 'h' && prevChar == 's' && currentChar == 'y')
        {
            _roman[_romanIndex] = 'h';
            return 1;
        }

        //「じゃ」「じゅ」「じぇ」「じょ」の柔軟な入力
        if (inputChar == 'z' && prevChar != 'j' && currentChar == 'j' &&
            (nextChar == 'a' || nextChar == 'u' || nextChar == 'e' || nextChar == 'o'))
        {
            _roman[_romanIndex] = 'z';
            _roman.Insert(_romanIndex + 1, 'y');
            return 1;
        }

        if (inputChar == 'y' && prevChar == 'j' &&
            (currentChar == 'a' || currentChar == 'u' || currentChar == 'e' || currentChar == 'o'))
        {
            _roman.Insert(_romanIndex, 'y');
            return 1;
        }

        //「し」「せ」の柔軟な入力（Windowsのみ）
        if (_isWindows && inputChar == 'c' && prevChar != 's' && currentChar == 's' &&
            (nextChar == 'i' || nextChar == 'e'))
        {
            _roman[_romanIndex] = 'c';
            return 1;
        }

        //「ち」の柔軟な入力
        if (inputChar == 'c' && prevChar != 't' && currentChar == 't' && nextChar == 'i')
        {
            _roman[_romanIndex] = 'c';
            _roman.Insert(_romanIndex + 1, 'h');
            return 1;
        }

        //「ちゃ」「ちゅ」「ちぇ」「ちょ」の柔軟な入力
        if (inputChar == 'c' && prevChar != 't' && currentChar == 't' && nextChar == 'y')
        {
            _roman[_romanIndex] = 'c';
            return 1;
        }

        //「cya」=>「cha」
        if (inputChar == 'h' && prevChar == 'c' && currentChar == 'y')
        {
            _roman[_romanIndex] = 'h';
            return 1;
        }

        //「つ」の柔軟な入力
        if (inputChar == 's' && prevChar == 't' && currentChar == 'u')
        {
            _roman.Insert(_romanIndex, 's');
            return 1;
        }

        //「つぁ」「つぃ」「つぇ」「つぉ」の柔軟な入力
        if (inputChar == 'u' && prevChar == 't' && currentChar == 's' &&
            (nextChar == 'a' || nextChar == 'i' || nextChar == 'e' || nextChar == 'o'))
        {
            _roman[_romanIndex] = 'u';
            _roman.Insert(_romanIndex + 1, 'x');
            return 1;
        }

        if (inputChar == 'u' && prevChar2 == 't' && prevChar == 's' &&
            (currentChar == 'a' || currentChar == 'i' || currentChar == 'e' || currentChar == 'o'))
        {
            _roman.Insert(_romanIndex, 'u');
            _roman.Insert(_romanIndex + 1, 'x');
            return 1;
        }

        //「てぃ」の柔軟な入力
        if (inputChar == 'e' && prevChar == 't' && currentChar == 'h' && nextChar == 'i')
        {
            _roman[_romanIndex] = 'e';
            _roman.Insert(_romanIndex + 1, 'x');
            return 1;
        }

        //「でぃ」の柔軟な入力
        if (inputChar == 'e' && prevChar == 'd' && currentChar == 'h' && nextChar == 'i')
        {
            _roman[_romanIndex] = 'e';
            _roman.Insert(_romanIndex + 1, 'x');
            return 1;
        }

        //「でゅ」の柔軟な入力
        if (inputChar == 'e' && prevChar == 'd' && currentChar == 'h' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'e';
            _roman.Insert(_romanIndex + 1, 'x');
            _roman.Insert(_romanIndex + 2, 'y');
            return 1;
        }

        //「とぅ」の柔軟な入力
        if (inputChar == 'o' && prevChar == 't' && currentChar == 'w' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'o';
            _roman.Insert(_romanIndex + 1, 'x');
            return 1;
        }
        //「どぅ」の柔軟な入力
        if (inputChar == 'o' && prevChar == 'd' && currentChar == 'w' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'o';
            _roman.Insert(_romanIndex + 1, 'x');
            return 1;
        }

        //「ふ」の柔軟な入力
        if (inputChar == 'f' && currentChar == 'h' && nextChar == 'u')
        {
            _roman[_romanIndex] = 'f';
            return 1;
        }

        //「ふぁ」「ふぃ」「ふぇ」「ふぉ」の柔軟な入力（一部Macのみ）
        if (inputChar == 'w' && prevChar == 'f' &&
            (currentChar == 'a' || currentChar == 'i' || currentChar == 'e' || currentChar == 'o'))
        {
            _roman.Insert(_romanIndex, 'w');
            return 1;
        }

        if (inputChar == 'y' && prevChar == 'f' && (currentChar == 'i' || currentChar == 'e'))
        {
            _roman.Insert(_romanIndex, 'y');
            return 1;
        }

        if (inputChar == 'h' && prevChar != 'f' && currentChar == 'f' &&
            (nextChar == 'a' || nextChar == 'i' || nextChar == 'e' || nextChar == 'o'))
        {
            if (_isMac)
            {
                _roman[_romanIndex] = 'h';
                _roman.Insert(_romanIndex + 1, 'w');
            }
            else
            {
                _roman[_romanIndex] = 'h';
                _roman.Insert(_romanIndex + 1, 'u');
                _roman.Insert(_romanIndex + 2, 'x');
            }
            return 1;
        }

        if (inputChar == 'u' && prevChar == 'f' &&
            (currentChar == 'a' || currentChar == 'i' || currentChar == 'e' || currentChar == 'o'))
        {
            _roman.Insert(_romanIndex, 'u');
            _roman.Insert(_romanIndex + 1, 'x');
            return 1;
        }

        if (_isMac && inputChar == 'u' && prevChar == 'h' && currentChar == 'w' &&
            (nextChar == 'a' || nextChar == 'i' || nextChar == 'e' || nextChar == 'o'))
        {
            _roman[_romanIndex] = 'u';
            _roman.Insert(_romanIndex + 1, 'x');
            return 1;
        }

        //「ん」の柔軟な入力（「n'」には未対応）
        if (inputChar == 'n' && prevChar2 != 'n' && prevChar == 'n' && currentChar != 'a' && currentChar != 'i' &&
            currentChar != 'u' && currentChar != 'e' && currentChar != 'o' && currentChar != 'y')
        {
            _roman.Insert(_romanIndex, 'n');
            return 1;
        }

        if (inputChar == 'x' && prevChar != 'n' && currentChar == 'n' && nextChar != 'a' && nextChar != 'i' &&
            nextChar != 'u' && nextChar != 'e' && nextChar != 'o' && nextChar != 'y')
        {
            if (nextChar == 'n')
            {
                _roman[_romanIndex] = 'x';
            }
            else
            {
                _roman.Insert(_romanIndex, 'x');
            }
            return 1;
        }

        //「うぃ」「うぇ」「うぉ」を分解する
        if (inputChar == 'u' && currentChar == 'w' && nextChar == 'h' && (nextChar2 == 'a' || nextChar2 == 'i' || nextChar2 == 'e' || nextChar2 == 'o'))
        {
            _roman[_romanIndex] = 'u';
            _roman[_romanIndex] = 'x';
        }

        //「きゃ」「にゃ」などを分解する
        if (inputChar == 'i' && currentChar == 'y' &&
            (prevChar == 'k' || prevChar == 's' || prevChar == 't' || prevChar == 'n' || prevChar == 'h' ||
             prevChar == 'm' || prevChar == 'r' || prevChar == 'g' || prevChar == 'z' || prevChar == 'd' ||
             prevChar == 'b' || prevChar == 'p') &&
            (nextChar == 'a' || nextChar == 'u' || nextChar == 'e' || nextChar == 'o'))
        {
            if (nextChar == 'e')
            {
                _roman[_romanIndex] = 'i';
                _roman.Insert(_romanIndex + 1, 'x');
            }
            else
            {
                _roman.Insert(_romanIndex, 'i');
                _roman.Insert(_romanIndex + 1, 'x');
            }
            return 1;
        }

        //「しゃ」「ちゃ」などを分解する
        if (inputChar == 'i' &&
            (currentChar == 'a' || currentChar == 'u' || currentChar == 'e' || currentChar == 'o') &&
            (prevChar2 == 's' || prevChar2 == 'c') && prevChar == 'h')
        {
            if (nextChar == 'e')
            {
                _roman.Insert(_romanIndex, 'i');
                _roman.Insert(_romanIndex + 1, 'x');
            }
            else
            {
                _roman.Insert(_romanIndex, 'i');
                _roman.Insert(_romanIndex + 1, 'x');
                _roman.Insert(_romanIndex + 2, 'y');
            }
            return 1;
        }

        //「しゃ」を「c」で分解する（Windows限定）
        if (_isWindows && inputChar == 'c' && currentChar == 's' && prevChar != 's' && nextChar == 'y' &&
            (nextChar2 == 'a' || nextChar2 == 'u' || nextChar2 == 'e' || nextChar2 == 'o'))
        {
            if (nextChar2 == 'e')
            {
                _roman[_romanIndex] = 'c';
                _roman[_romanIndex + 1] = 'i';
                _roman.Insert(_romanIndex + 1, 'x');
            }
            else
            {
                _roman[_romanIndex] = 'c';
                _roman.Insert(_romanIndex + 1, 'i');
                _roman.Insert(_romanIndex + 2, 'x');
            }
            return 1;
        }

        //「っ」の柔軟な入力
        if ((inputChar == 'x' || inputChar == 'l') &&
            (currentChar == 'k' && nextChar == 'k' || currentChar == 's' && nextChar == 's' ||
             currentChar == 't' && nextChar == 't' || currentChar == 'g' && nextChar == 'g' ||
             currentChar == 'z' && nextChar == 'z' || currentChar == 'j' && nextChar == 'j' ||
             currentChar == 'd' && nextChar == 'd' || currentChar == 'b' && nextChar == 'b' ||
             currentChar == 'p' && nextChar == 'p'))
        {
            _roman[_romanIndex] = inputChar;
            _roman.Insert(_romanIndex + 1, 't');
            _roman.Insert(_romanIndex + 2, 'u');
            return 1;
        }

        //「っか」「っく」「っこ」の柔軟な入力（Windows限定）
        if (_isWindows && inputChar == 'c' && currentChar == 'k' && nextChar == 'k' &&
            (nextChar2 == 'a' || nextChar2 == 'u' || nextChar2 == 'o'))
        {
            _roman[_romanIndex] = 'c';
            _roman[_romanIndex + 1] = 'c';
            return 1;
        }

        //「っく」の柔軟な入力（Windows限定）
        if (_isWindows && inputChar == 'q' && currentChar == 'k' && nextChar == 'k' && nextChar2 == 'u')
        {
            _roman[_romanIndex] = 'q';
            _roman[_romanIndex + 1] = 'q';
            return 1;
        }

        //「っし」「っせ」の柔軟な入力（Windows限定）
        if (_isWindows && inputChar == 'c' && currentChar == 's' && nextChar == 's' &&
            (nextChar2 == 'i' || nextChar2 == 'e'))
        {
            _roman[_romanIndex] = 'c';
            _roman[_romanIndex + 1] = 'c';
            return 1;
        }

        //「っちゃ」「っちゅ」「っちぇ」「っちょ」の柔軟な入力
        if (inputChar == 'c' && currentChar == 't' && nextChar == 't' && nextChar2 == 'y')
        {
            _roman[_romanIndex] = 'c';
            _roman[_romanIndex + 1] = 'c';
            return 1;
        }

        //「っち」の柔軟な入力
        if (inputChar == 'c' && currentChar == 't' && nextChar == 't' && nextChar2 == 'i')
        {
            _roman[_romanIndex] = 'c';
            _roman[_romanIndex + 1] = 'c';
            _roman.Insert(_romanIndex + 2, 'h');
            return 1;
        }

        //「l」と「x」の完全互換性
        if (inputChar == 'x' && currentChar == 'l')
        {
            _roman[_romanIndex] = 'x';
            return 1;
        }

        if (inputChar == 'l' && currentChar == 'x')
        {
            _roman[_romanIndex] = 'l';
            return 1;
        }

        return 2;
    }


    char GetCharFromKeyCode(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.A:
                return 'a';
            case KeyCode.B:
                return 'b';
            case KeyCode.C:
                return 'c';
            case KeyCode.D:
                return 'd';
            case KeyCode.E:
                return 'e';
            case KeyCode.F:
                return 'f';
            case KeyCode.G:
                return 'g';
            case KeyCode.H:
                return 'h';
            case KeyCode.I:
                return 'i';
            case KeyCode.J:
                return 'j';
            case KeyCode.K:
                return 'k';
            case KeyCode.L:
                return 'l';
            case KeyCode.M:
                return 'm';
            case KeyCode.N:
                return 'n';
            case KeyCode.O:
                return 'o';
            case KeyCode.P:
                return 'p';
            case KeyCode.Q:
                return 'q';
            case KeyCode.R:
                return 'r';
            case KeyCode.S:
                return 's';
            case KeyCode.T:
                return 't';
            case KeyCode.U:
                return 'u';
            case KeyCode.V:
                return 'v';
            case KeyCode.W:
                return 'w';
            case KeyCode.X:
                return 'x';
            case KeyCode.Y:
                return 'y';
            case KeyCode.Z:
                return 'z';
            case KeyCode.Alpha0:
                return '0';
            case KeyCode.Alpha1:
                return '1';
            case KeyCode.Alpha2:
                return '2';
            case KeyCode.Alpha3:
                return '3';
            case KeyCode.Alpha4:
                return '4';
            case KeyCode.Alpha5:
                return '5';
            case KeyCode.Alpha6:
                return '6';
            case KeyCode.Alpha7:
                return '7';
            case KeyCode.Alpha8:
                return '8';
            case KeyCode.Alpha9:
                return '9';
            case KeyCode.Minus:
                return '-';
            case KeyCode.Caret:
                return '^';
            case KeyCode.Backslash:
                return '\\';
            case KeyCode.At:
                return '@';
            case KeyCode.LeftBracket:
                return '[';
            case KeyCode.Semicolon:
                return ';';
            case KeyCode.Colon:
                return ':';
            case KeyCode.RightBracket:
                return ']';
            case KeyCode.Comma:
                return ',';
            case KeyCode.Period:
                return '.';
            case KeyCode.Slash:
                return '/';
            case KeyCode.Underscore:
                return '_';
            case KeyCode.Backspace:
                return '\b';
            case KeyCode.Return:
                return '\r';
            case KeyCode.Space:
                return ' ';
            default: //上記以外のキーが押された場合は「null文字」を返す。
                return '\0';
        }
    }
}

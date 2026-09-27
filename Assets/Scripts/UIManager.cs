using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("顔_部下")]
    public CanvasGroup bukaGroup;
    public Image _bukaBase;
    public Image _bukaEye;
    public Image _bukaMouth;
    public Image _bukaDeco;
    public Image _bukaKage;

    [Header("顔_zht")]
    public GameObject _zhtSet;
    public Image _zhtEye;
    public Image _zhtMouth;
    public Image _zhtDeco;
    public Image _zhtKage;
    public Sprite[] _day0EndZhtFace = new Sprite[3]; // day0終了時＝day1開始時の顔

    public Sprite[] damageFace = new Sprite[3];
    public float missedFaceDuration = 0.5f;
    private Sprite[] _savedNormalFace = new Sprite[3]; // ダメージを受ける前の顔を保持する変数
    private bool _isShowingDamage = false; // 今ダメージ演出中か？
    private bool _isPriorityEffectPlaying = false; // ミス演出などの優先表示中か？
    private Sprite[] _savedBukaNormalFace = new Sprite[3];
    public float missedBukaDuration = 3f;
    private string _savedBukaNormalText;
    private bool _isBukaReacting = false; // 特殊反応中フラグ
    private Coroutine _bukaReactionCoroutine; // コルーチンの「実体」を保存する変数

    [Header("吹き出し_部下")]
    public GameObject _bukaHukidasi;
    public Image _bukaHukidasiBG;
    public TextMeshProUGUI _bukaTalk;
    public TypeWriteEffect bukaTE;
    public GameObject nameTag;
    public Image nameTagBG;
    public TextMeshProUGUI nameText;

    [Header("吹き出し_zht")]
    public GameObject _zhtHukidasi;
    public Image _zhtHukidasiBG;
    public TextMeshProUGUI _zhtTalk;
    public TypeWriteEffect zhtTE;

    [Header("選択肢")]
    public GameObject[] choiceObjects; // 3つの吹き出しオブジェクト
    public Image[] choiceBG;
    public TextMeshProUGUI[] choiceTexts_jp; // その中のテキスト(日本語)
    public TextMeshProUGUI[] choiceTexts_roman; // その中のテキスト(ローマ字)
    public TypeWriteEffect[] choiceTE_jp;
    public TypeWriteEffect[] choiceTE_roman;


    public FadeManager fadeManager; // フェード用

    [Header("画面暗転")]
    public EndingScreenEffect endingScreenEffect;

    [SerializeField] private Vector2 firiaScale;
    [SerializeField] private Vector2 firiaPos;
    [SerializeField] private Vector2 firiaMouthPos;
    [SerializeField] private Sprite firiaBase, firiaMouth;

    public TypeWriteEffect anotherEndText;

    public TypingSoundManager soundManager;

    // 顔とテキストを同時に更新する
    public void UpdateTalk(Message message)
    {
        // 新しいセリフが来たら、部下の一時反応は強制終了してフラグを下ろす
        CancelBukaReaction();

        if (message.text.Contains("<size="))
        {
            soundManager.PlaySoundInPrologue();
            BGMManager.instance.ChangeBGM(BGMManager.instance.prologueBGM);
        }

         switch (message.speaker)
        {
            case 0: // 部下のセリフ
                // 色変更（話してない方を暗くする）
                _bukaHukidasiBG.color = new Color32(255, 255, 255, 255);
                nameTagBG.color = _bukaHukidasiBG.color;
                _zhtHukidasiBG.color = new Color32(120, 120, 120, 255);
                if (!_bukaHukidasi.activeSelf) _bukaHukidasi.SetActive(true); // 吹き出し表示                
                //  _bukaTalk.text = message.text; // テキスト更新
                bukaTE.SetTextAndShow(message.text); // 直接 .text に入れるのではなく、演出スクリプトに投げる
                anotherEndText.GetComponent<TextMeshProUGUI>().enabled = false;
                break;
            case 1: // 魔王のセリフ
                // 色変更（話してない方を暗くする）
                _zhtHukidasiBG.color = new Color32(255, 255, 255, 255);
                _bukaHukidasiBG.color = new Color32(120, 120, 120, 255);
                nameTagBG.color = _bukaHukidasiBG.color;
                if (!_zhtHukidasi.activeSelf) _zhtHukidasi.SetActive(true);
                // _zhtTalk.text = message.text;
                zhtTE.SetTextAndShow(message.text);
                anotherEndText.GetComponent<TextMeshProUGUI>().enabled = false;
                break;
            // 
            case 2:
                if (!message.isExit)
                {
                    anotherEndText.GetComponent<TextMeshProUGUI>().enabled = true;
                    anotherEndText.SetTextAndShow(message.text);
                }
                else
                {
                    _zhtHukidasiBG.color = new Color32(255, 255, 255, 255);
                    _bukaHukidasiBG.color = new Color32(120, 120, 120, 255);
                    nameTagBG.color = _bukaHukidasiBG.color;
                    if (!_zhtHukidasi.activeSelf) _zhtHukidasi.SetActive(true);
                    // _zhtTalk.text = message.text;
                    zhtTE.SetTextAndShow(message.text);
                    anotherEndText.GetComponent<TextMeshProUGUI>().enabled = false;
                    SetFace(1, message.face);
                }
                break;
            default:
                break;
        }

        // FaceSetの各パーツを適用
        SetFace(message.speaker, message.face);

    }


    // GameManagerから「クリックされたから次の挙動をして」と呼ばれた時に使う
    public bool CheckAndExit(Message message)
    {
        // 退場フラグがあり、まだ表示されている場合
        if (message.isExit)
        {
            if (message.speaker == 2 && endingScreenEffect.blackOverlay.alpha < 0.1f)
            {
                StartCoroutine(fadeManager.FadeInBuka(endingScreenEffect.blackOverlay, "fade"));
                return true;
            }
            if (message.speaker == 1)
            {
                StartCoroutine(endingScreenEffect.EndingSpLSnHBlackout());
                _zhtSet.SetActive(false);
              //  return true;
            }
            if (message.speaker == 0 && bukaGroup.alpha > 0.99f)
            {
                StartCoroutine(fadeManager.FadeOutBuka(bukaGroup));
                _bukaHukidasi.SetActive(false);
                //   nameTag.SetActive(false);
                nameTagBG.enabled = false;
                nameText.enabled = false;
                return true;
            }
        }
        // 入場フラグがあり、まだ透明な場合
        if (message.isEnter && bukaGroup.alpha < 0.1f)
        {
            StartCoroutine(fadeManager.FadeInBuka(bukaGroup));
            //  nameTag.SetActive(true);
            nameTagBG.enabled = true;
            nameText.enabled = true;
            return true;
        }
        // 
        if (message.isEnter && message.speaker == 2 && endingScreenEffect.blackOverlay.alpha > 0.99f)
        {
            StartCoroutine(fadeManager.FadeOutBuka(endingScreenEffect.blackOverlay, "fade"));
            return true;
        }
        return false;
    }

    // 部下の立ち絵と位置設定
    public void SetBukaLayout(DayData data)
    {
        _zhtTalk.text = "";
        _bukaTalk.text = "";


        // 名前設定
        nameText.text = data.bukaName;

        // 立ち絵設定
        _bukaBase.sprite = data.bukaBase;
        _bukaBase.SetNativeSize();
        _bukaEye.sprite = data.firstBukaFace[0];
        _bukaEye.SetNativeSize();
        _bukaMouth.sprite = data.firstBukaFace[1];
        _bukaMouth.SetNativeSize();
        _bukaDeco.sprite = data.firstBukaFace[2];
        _bukaDeco.SetNativeSize();
        _bukaKage.sprite = data.bukaBase;
        _bukaKage.SetNativeSize();

        // bukaGroup (CanvasGroup) 自体を動かす
        RectTransform rt = bukaGroup.GetComponent<RectTransform>();
        rt.anchoredPosition = data.bukaPosition;
        rt.localScale = data.bukaScale;

        _bukaEye.GetComponent<RectTransform>().anchoredPosition = data.bukaEyePosition;
        _bukaMouth.GetComponent<RectTransform>().anchoredPosition = data.bukaMouthPosition;
        _bukaDeco.GetComponent<RectTransform>().anchoredPosition = data.bukaDecoPosition;
    }


    public void AnotherEndSetBukaLayout(EndingData data)
    {
        _zhtTalk.text = "";
        _bukaTalk.text = "";

        // 名前設定
        nameTagBG.enabled = true;
        nameText.enabled = true;
        nameText.text = "フィーリア・アルストム・バレイシア";

        // 立ち絵設定
        _bukaBase.sprite = firiaBase;
        _bukaBase.SetNativeSize();
        _bukaMouth.sprite = firiaMouth;
        _bukaMouth.SetNativeSize();
        _bukaKage.sprite = firiaBase;
        _bukaKage.SetNativeSize();

        // bukaGroup (CanvasGroup) 自体を動かす
        RectTransform rt = bukaGroup.GetComponent<RectTransform>();
        rt.anchoredPosition = firiaPos;
        rt.localScale = firiaScale;

        _bukaMouth.GetComponent<RectTransform>().anchoredPosition = firiaMouthPos;
        _bukaEye.enabled = false;
        _bukaDeco.enabled = false;

        _zhtEye.sprite = data.dialogues[1].face.eye;
        _zhtMouth.sprite = data.dialogues[1].face.mouth;
        _zhtDeco.sprite = data.dialogues[1].face.deco;
        _zhtDeco.enabled = data.dialogues[1].face.deco != null;
        _zhtMouth.enabled = data.dialogues[1].face.mouth != null;
    }


    public void SetFace(int speaker,FaceSet face)
    {
        // =====================
        // 目口装飾全部nullのときはbase含め全部非表示にする？？？？
        // =====================

        // 部下立ち絵ベース切り替え
      //  _bukaBase.sprite = bukaBaseImg[gameManager.CurrentDay];

        switch (speaker)
        {
            case 0:
                // スプライトが設定されている場合のみ更新
                _bukaEye.sprite = face.eye;
                _bukaMouth.sprite = face.mouth;
                _bukaDeco.sprite = face.deco;
                // 全てのレイヤーを表示状態に
                _bukaEye.enabled = (face.eye != null);
                _bukaMouth.enabled = (face.mouth != null);
                _bukaDeco.enabled = (face.deco != null);
                // 話してない方を暗くする
                _zhtKage.enabled = true;
                _bukaKage.enabled = false;
                break;
            case 1:
                // スプライトが設定されている場合のみ更新
                _zhtEye.sprite = face.eye;
                _zhtMouth.sprite = face.mouth;
                _zhtDeco.sprite = face.deco;
                // 全てのレイヤーを表示状態に
                _zhtEye.enabled = (face.eye != null);
                _zhtMouth.enabled = (face.mouth != null);
                _zhtDeco.enabled = (face.deco != null);
                // 話してない方を暗くする
                _zhtKage.enabled = false;
                _bukaKage.enabled = true;
                break;
            default:
                break;
        }

    }


    // 選択肢をリセット（非表示）にする
    public void HideAllChoices()
    {
        foreach (var obj in choiceObjects)
        {
            obj.SetActive(false);
        }
    }

    // 選択肢を表示する
    public void SetChoices(List<Choice> choices)
    {
        // 魔王の吹き出し非表示
        _zhtHukidasi.SetActive(false);

        HideAllChoices();
        for (int i = 0; i < choices.Count && i < choiceObjects.Length; i++)
        {
            choiceBG[i].color = new Color32(255, 255, 255, 255);
            choiceObjects[i].SetActive(true);
            //  choiceTexts_jp[i].text = choices[i].typeJapanese;
            //  choiceTexts_roman[i].text = choices[i].typeRoman;
            choiceTE_jp[i].SetTextAndShow(choices[i].typeJapanese);
            choiceTE_roman[i].SetTextAndShow(choices[i].typeRoman);
        }
    }

    // 選ばなかった選択肢を暗くする
    public void FocusAtChoice(int currentChoiceNum)
    {
        for (int i = 0; i < choiceBG.Length; i++)
        {
            if (i != currentChoiceNum) choiceBG[i].color = new Color32(120, 120, 120, 255);
        }
    }


    // 文字送り用
    public bool IsAnyTextPlaying()
    {
        // どちらかのテキストが再生中ならtrue
        return bukaTE.IsPlaying || zhtTE.IsPlaying;
    }
    public void SkipAnyText()
    {
        if (bukaTE.IsPlaying) bukaTE.Skip();
        if (zhtTE.IsPlaying) zhtTE.Skip();
    }


    // ミスした時用
    public void MissedAnimation(int choiceNum)
    {
        // 吹き出し動かす
        Animator anim = choiceObjects[choiceNum].transform.GetChild(0).GetComponent<Animator>();
        anim.SetTrigger("missed");

    }

    // 魔王の顔一瞬変化
    public void ShowMaouDamage()
    {
        // すでに演出中なら、コルーチンだけを再開させる（顔の保存はしない）
        if (_isShowingDamage)
        {
            StopCoroutine("MaouDamageCoroutine");
            StartCoroutine(MaouDamageCoroutine(missedFaceDuration, false));
        }
        else
        {
            // 初めてのミスなら、今の顔をしっかり保存する
            _savedNormalFace[0] = _zhtEye.sprite;
            _savedNormalFace[1] = _zhtMouth.sprite;
            _savedNormalFace[2] = _zhtDeco.sprite;
            StartCoroutine(MaouDamageCoroutine(missedFaceDuration, true));
        }
    }

    private IEnumerator MaouDamageCoroutine(float duration, bool isFirstMiss)
    {
        _isShowingDamage = true;

        // ダメージ顔に変更
        _zhtEye.sprite = damageFace[0];
        _zhtMouth.sprite = damageFace[1];
        _zhtDeco.sprite = damageFace[2];
        _zhtDeco.enabled = (damageFace[2] != null);

        // 指定時間待機
        yield return new WaitForSeconds(duration);

        // 元の顔に戻す
        _zhtEye.sprite = _savedNormalFace[0];
        _zhtMouth.sprite = _savedNormalFace[1];
        _zhtDeco.sprite = _savedNormalFace[2];
        _zhtDeco.enabled = (_savedNormalFace[2] != null);

        _isShowingDamage = false;
    }



    public void ShowBukaTemporaryReaction(DayData data, string tempText)
    {
        // もし既に動いていたら、そのコルーチンを確実に殺す
        if (_bukaReactionCoroutine != null)
        {
            StopCoroutine(_bukaReactionCoroutine);
            _bukaReactionCoroutine = null;
        }
        // すでに反応中なら、タイマーだけリセットして上書き
        if (!_isBukaReacting)
        {
            // 初めての反応なら、今の「正しい状態」を保存
            _savedBukaNormalFace[0] = _bukaEye.sprite;
            _savedBukaNormalFace[1] = _bukaMouth.sprite;
            _savedBukaNormalFace[2] = _bukaDeco.sprite;
            _savedBukaNormalText = _bukaTalk.text;
        }

        // 新しくコルーチンを開始し、変数に代入
        _bukaReactionCoroutine = StartCoroutine(BukaReactionCoroutine(data, tempText));
    }

    private IEnumerator BukaReactionCoroutine(DayData data, string tempText)
    {
        _isBukaReacting = true;

        // 一時的な顔とテキストをセット
        // ミスったときの部下の顔とセリフ
        _bukaEye.sprite = data.missBukaFace[0];
        _bukaMouth.sprite = data.missBukaFace[1];
        _bukaDeco.sprite = data.missBukaFace[2];
        _bukaEye.enabled = (data.missBukaFace[0] != null);
        _bukaMouth.enabled = (data.missBukaFace[1] != null);
        _bukaDeco.enabled = (data.missBukaFace[2] != null);
        bukaTE.SetTextAndShow(tempText); // 文字送りを止めて表示

        yield return new WaitForSeconds(missedBukaDuration);

        // 元の状態に戻す
        _bukaEye.sprite = _savedBukaNormalFace[0];
        _bukaMouth.sprite = _savedBukaNormalFace[1];
        _bukaDeco.sprite = _savedBukaNormalFace[2];
        _bukaEye.enabled = (_savedBukaNormalFace[0] != null);
        _bukaMouth.enabled = (_savedBukaNormalFace[1] != null);
        _bukaDeco.enabled = (_savedBukaNormalFace[2] != null);
        bukaTE.SetTextAndShow(_savedBukaNormalText);

        _isBukaReacting = false;
        _bukaReactionCoroutine = null;
    }

    // コルーチン停止
    public void CancelBukaReaction()
    {
        if (_bukaReactionCoroutine != null)
        {
            StopCoroutine(_bukaReactionCoroutine);
            _bukaReactionCoroutine = null;
            _isBukaReacting = false; // フラグもここで折る
            Debug.Log("部下の一時反応を強制停止しました。戻り処理は走りません。");
        }
    }


    // 初期のUI表示設定
    public void InitializeUI()
    {
        _zhtEye.enabled = false;
        _zhtMouth.enabled = false;
        _zhtDeco.enabled = false;

        bukaGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(fadeManager.slideOffset, -100f);
        bukaGroup.alpha = 1f;

        _bukaHukidasi.SetActive(false);
        _zhtHukidasi.SetActive(false);

        foreach (GameObject c in choiceObjects)
        {
            c.SetActive(false);
        }
    }

    // アナザーエンディングのつじつま合わせ
    public void ResetAnotherText()
    {
        anotherEndText.GetComponent<TextMeshProUGUI>().enabled = false;
    }

    // day0終了時に適用
    public void day0EndUI()
    {
        // 部下を一旦非表示にして退場させる
        bukaGroup.alpha = 0f;
        StartCoroutine(fadeManager.FadeOutBuka(bukaGroup));
        // 吹き出しを消して顔をセット
        _zhtHukidasi.SetActive(false);
        _zhtEye.sprite = _day0EndZhtFace[0];
        _zhtMouth.sprite = _day0EndZhtFace[1];
        _zhtDeco.sprite = _day0EndZhtFace[2];

    }
}

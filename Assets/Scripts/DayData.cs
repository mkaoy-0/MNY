using UnityEngine;
using System.Collections.Generic;

// 顔差分
[System.Serializable]
public struct FaceSet
{
    public Sprite eye;      // 目
    public Sprite mouth;    // 口
    public Sprite deco;     // 装飾（汗、怒りマーク、眼鏡など）
}

// セリフと表情をセットにする構造体
[System.Serializable]
public struct Message
{
    [Header("部下=0, 魔王=1")]
    public int speaker;
    [TextArea(3, 5)] // 入力欄広くする
    public string text;
    public FaceSet face;

    // このセリフの後に部下を登場・退場させるか
    public bool isEnter;
    public bool isExit;
}

// 選択肢のデータ
[System.Serializable]
public struct Choice
{
    public string typeJapanese;    // ボタンに表示する見出し（例：許可）
    public string typeRoman;       // 実際にタイピングさせるフル文章
    public int supportChange;
    public int sanChange;

    // この選択をした直後の反応
    public List<Message> reactions;

    // 理由の選択肢へ続くか
    public bool hasReasonSelection;
    public List<Choice> reasonChoices;

    public bool triggerHimeRachi;
}

[CreateAssetMenu(fileName = "DayData", menuName = "Custom/DayData")]
public class DayData : ScriptableObject
{
    public int dayNumber;
    public string bukaName;
    public Sprite bukaBase;
    public Sprite[] firstBukaFace = new Sprite[3];
    [Header("配置設定")]
    public Vector3 bukaScale = new Vector3(1, 1, 1);    // 基本は(1,1,1)
    public Vector3 bukaPosition = new Vector3(0, 0, 0);
    public Vector3 bukaEyePosition = new Vector3(0, 0, 0);
    public Vector3 bukaMouthPosition = new Vector3(0, 0, 0);
    public Vector3 bukaDecoPosition = new Vector3(0, 0, 0);
    [Header("ミスしたときの顔とセリフ")]
    public Sprite[] missBukaFace = new Sprite[3];
    public string missReaction;
    public string timeLimitReaction;


    // 導入セリフリスト
    public List<Message> introMessages;

    // 最初の決断リスト
    public List<Choice> firstChoices;
}
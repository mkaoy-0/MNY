using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EndingData", menuName = "Custom/EndingData")] // Customに統一
public class EndingData : ScriptableObject
{
    public string endingName;

    [Header("エンディングのセリフ")]
    public List<Message> dialogues;


    // public AudioClip endingBGM;
}
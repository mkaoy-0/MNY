using UnityEngine;

public class TitleSceneBgmManager : MonoBehaviour
{
    void Start()
    {
        BGMManager.instance.FadeChangeBGM(BGMManager.instance.titleBGM);
    }

}

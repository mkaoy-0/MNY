using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;
    [SerializeField] private AudioSource audioSource;

    public AudioClip gameOverBGM;
    public AudioClip nomalBGM;
    public AudioClip titleBGM;
    public AudioClip prologueBGM;

    public void ChangeBGM(AudioClip newClip)
    {
        if (audioSource.clip == newClip) return; // 同じ曲ならスルー

        audioSource.Stop();       // 今の曲を止める
        audioSource.volume = 0.7f;
        audioSource.clip = newClip; // 新しい曲をセット
        audioSource.Play();        // 再生
    }

    void Awake()
    {
        // 重複して作られないようにする
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも壊さない
        }
        else
        {
            Destroy(gameObject); // すでに存在してたら新しい方を消す
        }
    }

    public void FadeChangeBGM(AudioClip newClip, float duration = 1.0f)
    {
        StartCoroutine(FadeRoutine(newClip, duration));
    }

    private IEnumerator FadeRoutine(AudioClip newClip, float duration)
    {
        float startVolume = audioSource.volume;

        // フェードアウト
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
        audioSource.volume = 0;

        if (newClip != null)
        {
            startVolume = 0.7f;

            // 曲の差し替え
            audioSource.clip = newClip;
            audioSource.Play();
            Debug.Log("BGMを差し替え");

            // フェードイン
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(0, startVolume, t / duration);
                yield return null;
            }
            audioSource.volume = startVolume;
        }
    }
}
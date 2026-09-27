using UnityEngine;

public class TypingSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSound; // タイピング音のファイル
    [SerializeField] private AudioClip typeMissSound;

    [SerializeField] private AudioClip inPrologueSound;

    // ピッチ（音の高さ）をわずかにランダムに変えると、機械感が消えて自然になります
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    public void PlayTypeSound()
    {
        if (audioSource == null || typeSound == null) return;

        // 音の高さをランダムにして「カチッ」「コチッ」と変化をつける
        audioSource.pitch = Random.Range(minPitch, maxPitch);

        // PlayOneShot を使うと、前の音が鳴り終わる前に次の音が重なって鳴ります（重要！）
        audioSource.PlayOneShot(typeSound);
    }

    public void PlayTypeMissSound()
    {
        if (audioSource == null || typeMissSound == null) return;
        audioSource.PlayOneShot(typeMissSound);
    }

    public void PlaySoundInPrologue()
    {
        if (audioSource == null || inPrologueSound == null) return;
        audioSource.PlayOneShot(inPrologueSound);
    }
}
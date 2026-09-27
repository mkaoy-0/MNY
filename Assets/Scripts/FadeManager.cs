using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private DataManager dataManager;

    [SerializeField] private float duration = 0.5f;

    public float slideOffset = 400f;

    [SerializeField] private float meitenWaitTime = 1.3f;


    public bool IsFading { get; private set; } // フェード中フラグ

    public IEnumerator FadeInBuka(CanvasGroup cg, string fade = null, float max = 1f)
    {
        IsFading = true;
        RectTransform rt = cg.GetComponent<RectTransform>();
        // 最終的な目標地点
        DayData data = dataManager.GetCurrentDayData();
        Vector2 targetPos = new Vector2(data.bukaPosition.x, data.bukaPosition.y);
        Vector2 startPos = new Vector2(slideOffset, data.bukaPosition.y);

        if (fade == null)
        {
            rt.anchoredPosition = startPos;
            cg.alpha = 1f;
        }

        float elapsed = 0f; // 0からスタート
        while (elapsed < duration) // elapsedがdurationに届くまで
        {
            elapsed += Time.deltaTime;
            // 位置を Lerp でスライドさせる
            if (fade == null) rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            else cg.alpha = Mathf.Lerp(0f, max, elapsed / duration);
                yield return null;
        }
        if (fade == null) rt.anchoredPosition = targetPos;
        cg.alpha = 1f;
        IsFading = false;
        Debug.Log("フェードイン完了");

        // フェード完了をGameManagerに伝える（もし自動進行させたいなら）
        if (fade == null) gameManager.OnFadeComplete();
    }

    public IEnumerator FadeOutBuka(CanvasGroup cg, string fade = null, float min = 0f)
    {
        IsFading = true;
        RectTransform rt = cg.GetComponent<RectTransform>();

        Vector2 startPos = rt.anchoredPosition;
        // 退場先（画面外へ）
        Vector2 targetPos = new Vector2(slideOffset, rt.anchoredPosition.y);


        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (fade == null) rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            else cg.alpha = Mathf.Lerp(1f, min, elapsed / duration);
            yield return null;
        }
        if (fade == null) rt.anchoredPosition = targetPos;
        cg.alpha = 0f;
        IsFading = false;
        Debug.Log("フェードアウト完了");

        // フェード完了をGameManagerに伝える（もし自動進行させたいなら）
        if (fade == null) gameManager.OnFadeComplete();
    }



}

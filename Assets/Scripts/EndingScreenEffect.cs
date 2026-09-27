using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class EndingScreenEffect : MonoBehaviour
{

    public FadeManager fadeManager;
    public Image blackBG;
    public UIManager uiManager;
    public DataManager dataManager;
    public ParaUIManager paraUIManager;

    [Header("ゲームオーバー演出用")]
    public CanvasGroup blackOverlay; // 黒い画像の CanvasGroup
    [SerializeField] private int totalStages = 6; // 何段階で真っ暗にするか
    private float maxAlpha = 0.85f; // インスペクターで後から調整できるように変数化
    public CanvasGroup endText;

    //  private bool _canGoToTitle = false; // 入力待ちフラグ
    //  public bool CanGoToTitle { get; set; }
    public bool CanGoToTitle = false;

    public void _CheckAndDarken(int currentIndex, int totalMessages)
    {
        Debug.Log(currentIndex + ", " + totalMessages);

        if (totalMessages <= 1) return;

        // 「最後のセリフ」を 1.0 (100%) とした時の、現在の進捗率を出す
        float progress = (float)(currentIndex - 1) / (totalMessages - 1);

        // 進捗率に最大アルファ値を掛ける
        float targetAlpha = progress * maxAlpha;

        // 安全策：最後なら確実に最大値にする
        if ((currentIndex - 1) >= totalMessages - 1)
        {
            targetAlpha = 1f;
        }

        // 反映
        blackOverlay.alpha = targetAlpha;

        Debug.Log($"進捗:{(progress * 100):F0}% Alpha:{targetAlpha:F2}");
    }


    public IEnumerator EndingSpLSnHBlackout()
    {
        Debug.Log("エンディングSpLSnHの暗転演出開始");

        uiManager._zhtHukidasi.SetActive(false);
        uiManager._bukaHukidasi.SetActive(false);

        // 画面全体を真っ暗にする
        yield return StartCoroutine(fadeManager.FadeInBuka(blackOverlay, "fade"));

        yield return new WaitForSeconds(1.5f); // 少し長めの「間」

        blackBG.enabled = true;
        paraUIManager.paraSet.SetActive(false);
        uiManager.SetBukaLayout(dataManager.EndingDays[0]);
        uiManager.bukaGroup.alpha = 1f;

        yield return StartCoroutine(fadeManager.FadeOutBuka(blackOverlay, "fade", 0.6f));
    }


    public IEnumerator LastBlackout(string endName)
    {
        // スタックトレース（呼び出し履歴）を表示して、犯人を特定する
        Debug.Log("LastBlackoutコルーチンが開始されました"); // これが出ないなら StartCoroutine 忘れ
      //  _canGoToTitle = false; // 開始時はリセット
        endText.alpha = 0f;

        if (blackOverlay.alpha < 0.1f)
        {
            yield return StartCoroutine(fadeManager.FadeInBuka(blackOverlay, "fade"));
        }
        Debug.Log("暗転フェードが完了しました"); // これが出ないなら FadeInBuka の中で止まってる

        yield return new WaitForSeconds(1f);

        endText.GetComponent<TextMeshProUGUI>().text = endName;
        yield return StartCoroutine(fadeManager.FadeInBuka(endText, "fade"));
        Debug.Log("エンディング名が表示されました");

        // 演出完了。ここで入力を受け付ける準備ができたことを知らせる
        CanGoToTitle = true;
        Debug.Log("クリックでタイトルまたはアナザーへ進めます" + CanGoToTitle);
    }
}
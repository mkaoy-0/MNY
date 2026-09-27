using System.Collections;
using TMPro;
using UnityEngine;

public class TypeWriteEffect : MonoBehaviour
{
    // 対象のテキスト
    [SerializeField] private TMP_Text _text;

    // 次の文字を表示するまでの時間[s]
    [SerializeField] private float _delayDuration = 0.1f;

    private Coroutine _showCoroutine;
    private int _totalVisibleCharacters;

    // ★追加：今文字送り中かどうかを外から確認できるプロパティ
    public bool IsPlaying => _showCoroutine != null;

    /// <summary>
    /// 文字送り演出を表示する
    /// </summary>
    public void SetTextAndShow(string newText)
    {
        if (_text == null) _text = GetComponent<TextMeshProUGUI>();

        _text.text = newText; // 文字列をセット
        _text.ForceMeshUpdate();
        _totalVisibleCharacters = _text.textInfo.characterCount;
        _text.maxVisibleCharacters = 0; // 最初は全部隠す

        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        _showCoroutine = StartCoroutine(ShowCoroutine());
    }

    // １文字ずつ表示する演出のコルーチン
    private IEnumerator ShowCoroutine()
    {
        var delay = new WaitForSeconds(_delayDuration);

        // １文字ずつ表示する演出
        for (var i = 0; i <= _totalVisibleCharacters; i++)
        {
            // 徐々に表示文字数を増やしていく
            _text.maxVisibleCharacters = i;
            // 一定時間待機
            yield return delay;
        }
        _text.maxVisibleCharacters = 999;
        _showCoroutine = null;
    }

    // ★追加：演出をすっ飛ばして全表示する
    public void Skip()
    {
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }
        //  _text.maxVisibleCharacters = _totalVisibleCharacters;
        _text.maxVisibleCharacters = 999;
    }
}
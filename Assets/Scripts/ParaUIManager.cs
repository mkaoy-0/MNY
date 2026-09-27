using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ParaUIManager : MonoBehaviour
{
    public ParameterManager parameterManager;

    [Header("Sliders")]
    public GameObject paraSet;
    public Slider supportSlider;
    public Slider sanitySlider;

    public float animationDuration = 0.5f;

    public Animator spAnim;
    public Animator snAnim;

    public TextMeshProUGUI SpValueText, SnValueText;


    private void Start()
    {
        paraSet.SetActive(false);
        SpValueText.text = $"{parameterManager.SupportRate}/{parameterManager.MaxSupportRate}";
        SnValueText.text = $"{parameterManager.Sanity}/{parameterManager.MaxSanity}";

    }

    // 支持率スライダーを更新
    public void UpdateSupport(int targetValue, string triggerName)
    {
        // トリガー名が空でなければアニメーション実行
        if (!string.IsNullOrEmpty(triggerName))
        {
            spAnim.SetTrigger(triggerName);
        }

        StopCoroutine("AnimateSupport");
        StartCoroutine(AnimateSlider(supportSlider, targetValue));

        SpValueText.text = $"{targetValue}/{parameterManager.MaxSupportRate}";
    }

    // SAN値スライダーを更新
    public void UpdateSanity(int targetValue, string triggerName)
    {
        if (!string.IsNullOrEmpty(triggerName))
        {
            snAnim.SetTrigger(triggerName);
        }

        StopCoroutine("AnimateSanity");
        StartCoroutine(AnimateSlider(sanitySlider, targetValue));

        SnValueText.text = $"{targetValue}/{parameterManager.MaxSanity}";
    }

    private IEnumerator AnimateSlider(Slider slider, float target)
    {
        float startValue = slider.value;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, target, elapsed / animationDuration);
            yield return null;
        }

        slider.value = target;
    }
}
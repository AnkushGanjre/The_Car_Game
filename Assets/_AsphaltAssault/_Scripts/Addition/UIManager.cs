using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _startCountDownText;
    [SerializeField] Image _nosBar;
    float _currentNosValue;
    float _previousNosValue;


    void Start()
    {
        if (_startCountDownText == null)
        {
            Debug.LogWarning("Assign CountDown Text Gameobject");
        }
        else
        {
            _startCountDownText.text = "";
            StartCoroutine(StartCountDown());
        }
    }

    IEnumerator StartCountDown()
    {
        if ( _startCountDownText == null)
            yield break;

        yield return new WaitForSeconds(0.25f);
        _startCountDownText.text = "3";
        yield return new WaitForSeconds(1);
        _startCountDownText.text = "2";
        yield return new WaitForSeconds(1);
        _startCountDownText.text = "1";
        yield return new WaitForSeconds(1);
        _startCountDownText.text = "GO";
        GameManagerUno.Instance.IsRaceOn = true;
        yield return new WaitForSeconds(1);
        _startCountDownText.text = "";
        _nosBar.transform.parent.gameObject.SetActive(true);
    }

    public void UpdateNosUI(float value)
    {
        if (Mathf.Abs(value - _previousNosValue) >= 0.1f)
        {
            // Start the coroutine to smoothly update the UI bar
            StartCoroutine(UpdateUISmoothly(value));
        }
        else // Update the UI bar instantly for small changes
        {
            _nosBar.fillAmount = value;
        }

        // Update previousValue to currentValue for the next frame
        _previousNosValue = value;
    }

    private IEnumerator UpdateUISmoothly(float targetValue)
    {
        float elapsedTime = 0f;
        float startValue = _nosBar.fillAmount;

        while (elapsedTime < 0.5f) // Smoothly update over 0.5 seconds
        {
            elapsedTime += Time.deltaTime;
            _nosBar.fillAmount = Mathf.Lerp(startValue, targetValue, elapsedTime / 0.5f);
            yield return null;
        }

        // Ensure the final value is set accurately
        _nosBar.fillAmount = targetValue;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [SerializeField] Transform _steerControlHolder;

    void Start()
    {
        if (PlayerPrefs.HasKey("CurrentSteerType"))
        {
            int num = PlayerPrefs.GetInt("CurrentSteerType");
            Button button = _steerControlHolder.GetChild(num).GetComponent<Button>();
            button.onClick.Invoke();
        }
        else
        {
            int num = 1;
            Button button = _steerControlHolder.GetChild(num).GetComponent<Button>();
            button.onClick.Invoke();
            PlayerPrefs.SetInt("CurrentSteerType", num);
        }
    }

    public void SetSteeringType(int num)
    {
        // 0 for tilt && 1 for tap
        PlayerPrefs.SetInt("CurrentSteerType", num);
    }
}

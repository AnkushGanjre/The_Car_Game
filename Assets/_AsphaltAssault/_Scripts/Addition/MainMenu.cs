using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    #region Turn Table
    [Header("Turn Table")]
    [SerializeField] Transform _turnTable;
    [SerializeField] float rotateSpeed = 10f;
    #endregion

    #region Main Menu
    [Header("Main Menu")]
    [SerializeField] GameObject _mainMenu;
    [SerializeField] Button _playButton;
    [SerializeField] Button _garageButton;
    #endregion

    #region Garage Menu
    [Header("Garage Menu")]
    [SerializeField] GameObject _garageMenu;
    [SerializeField] Button _leftVehicleButton;
    [SerializeField] Button _rightVehicleButton;
    [SerializeField] Button _selectVehicleButton;
    int _displayCarNum;
    #endregion

    #region Cars
    [Header("Cars")]
    [SerializeField] GameObject[] _vehicleList;
    [SerializeField] GameObject _activeVehicle;
    #endregion


    void Start()
    {
        _playButton.onClick.AddListener(() => { OnPlayButton(); });
        _garageButton.onClick.AddListener(() => { OnGarageButton(); });
        _leftVehicleButton.onClick.AddListener(() => { OnLeftVehicleButton(); });
        _rightVehicleButton.onClick.AddListener(() => { OnRightVehicleButton(); });
        _selectVehicleButton.onClick.AddListener(() => { OnSelectVehicleButton(); });

        if (PlayerPrefs.HasKey("CurrentCarNum"))
        {
            int num = PlayerPrefs.GetInt("CurrentCarNum");
            _activeVehicle = _vehicleList[num];
        }
        else
        {
            int num = 0;
            _activeVehicle = _vehicleList[num];
            PlayerPrefs.SetInt("CurrentCarNum", num);
        }

        SetCarActive(GetActiveCarNum());
    }

    private void Update()
    {
        if (_turnTable != null)
        {
            _turnTable.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        }
    }

    private void OnPlayButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnGarageButton()
    {
        _mainMenu.SetActive(false);
        _garageMenu.SetActive(true);
        _displayCarNum = GetActiveCarNum();

        if (_displayCarNum == 0)
        {
            _leftVehicleButton.gameObject.SetActive(false);
            _rightVehicleButton.gameObject.SetActive(true);
        }
        if (_displayCarNum == 2)
        {
            _leftVehicleButton.gameObject.SetActive(true);
            _rightVehicleButton.gameObject.SetActive(false);
        }
    }

    private void OnLeftVehicleButton()
    {
        _leftVehicleButton.gameObject.SetActive(true);
        _rightVehicleButton.gameObject.SetActive(true);

        _displayCarNum--;
        if (_displayCarNum <= 0)
        {
            _displayCarNum = 0;
            _leftVehicleButton.gameObject.SetActive(false);
        }

        SetCarActive(_displayCarNum);
    }

    private void OnRightVehicleButton()
    {
        _leftVehicleButton.gameObject.SetActive(true);
        _rightVehicleButton.gameObject.SetActive(true);

        _displayCarNum++;
        if (_displayCarNum >= 2)
        {
            _displayCarNum = 2;
            _rightVehicleButton.gameObject.SetActive(false);
        }

        SetCarActive(_displayCarNum);
    }

    private void OnSelectVehicleButton()
    {
        _mainMenu.SetActive(true);
        _garageMenu.SetActive(false);

        _activeVehicle = _vehicleList[_displayCarNum];
        PlayerPrefs.SetInt("CurrentCarNum", _displayCarNum);
    }

    int GetActiveCarNum()
    {
        for (int i = 0; i < _vehicleList.Length; i++)
        {
            if (_activeVehicle == _vehicleList[i])
            {
                int a = i;
                return a;
            }
        }

        return 0;
    }

    void SetCarActive(int carNum)
    {
        foreach (var car in _vehicleList)
        {
            car.SetActive(false);
        }
        _vehicleList[carNum].SetActive(true);
    }
}

using UnityEngine;

public class GameManagerUno : MonoBehaviour
{
    [Header("Singleton Instance")]
    public static GameManagerUno Instance;

    #region Cars
    [Header("Cars")]
    [SerializeField] GameObject[] _trackList;
    [SerializeField] Transform[] _startPosition;
    [SerializeField] GameObject[] _vehicleList;
    public GameObject ActiveVehicle;
    int currentCarNum;
    int currentTrackNum;
    #endregion

    public bool IsRaceOn = false;
    PlayerInputManager playerInputManager;


    void Awake()
    {
        Instance = Instance ?? this;  // Setting Singleton Instance
        if (Instance != this) Destroy(gameObject);  // If not Active Singleton, destroy it
        //DontDestroyOnLoad(gameObject);  // Ensure that the Singleton persists across scene changes

        playerInputManager = FindObjectOfType<PlayerInputManager>();

        if (PlayerPrefs.HasKey("CurrentTrackNum"))
        {
            currentTrackNum = PlayerPrefs.GetInt("CurrentTrackNum");
        }
        else currentTrackNum = 0;

        if (PlayerPrefs.HasKey("CurrentCarNum"))
        {
            currentCarNum = PlayerPrefs.GetInt("CurrentCarNum");
        }
        else currentCarNum = 0;

        ChangeTrack(currentTrackNum);
        ChangeCar(currentCarNum);
    }

    public void ResetCarPosition()
    {
        if (ActiveVehicle != null)
        {
            Rigidbody rb = ActiveVehicle.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            ActiveVehicle.transform.position = _startPosition[currentTrackNum].position;
            ActiveVehicle.transform.rotation = _startPosition[currentTrackNum].rotation;

            Camera cam = Camera.main;
            cam.transform.position = _startPosition[currentTrackNum].GetChild(0).localPosition;
            cam.transform.rotation = _startPosition[currentTrackNum].GetChild(0).localRotation;
            rb.isKinematic = false;
            cam.GetComponent<CameraControl>().TargetSetup();
        }
    }

    public void ChangeTrack(int num)
    {
        if (num == currentTrackNum && _trackList[currentTrackNum].activeInHierarchy) return;
        foreach (var track in _trackList) { track.SetActive(false); }
        foreach (var pos in _startPosition) { pos.gameObject.SetActive(false); }

        PlayerPrefs.SetInt("CurrentTrackNum", num);
        currentTrackNum = num;
        _trackList[currentTrackNum].SetActive(true);
        _startPosition[currentTrackNum].gameObject.SetActive(true);
        //Debug.Log(_trackList[currentTrackNum].name + " Initiated");
        ResetCarPosition();
    }

    public void ChangeCar(int num)
    {
        if (num == currentCarNum && _vehicleList[currentCarNum].activeInHierarchy) return;
        foreach (var vehicle in _vehicleList) { vehicle.SetActive(false); }

        PlayerPrefs.SetInt("CurrentCarNum", num);
        currentCarNum = num;
        ActiveVehicle = _vehicleList[currentCarNum];
        ActiveVehicle.SetActive(true);
        //Debug.Log(ActiveVehicle.name + " Initiated");
        playerInputManager.SetUpCarController(ActiveVehicle);
        playerInputManager.SetupGearChangeSpeed();
        ResetCarPosition();
    }
}

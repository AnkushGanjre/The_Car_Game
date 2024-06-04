using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

public class PlayerInputManager : MonoBehaviour, ICarControl
{
    public CarController CarController { get; private set; }

    #region INPUT ACTION
    [SerializeField] InputActionAsset _inputActions;
    InputActionMap _playerActionMap;
    InputAction _brakeInputAction;
    InputAction _steeringInputAction;
    InputAction _boostInputAction;
    #endregion

    #region INTERFACE IMPLEMENTATION
    public float AccelerationFactor {  get; set; }
    public float BrakingFactor {  get; set; }
    public float TopSpeedValue {  get; set; }
    public float SteeringValue { get; set; }
    public float SuspensionValue { get; set; }
    public bool IsBraking { get; set; }
    public bool IsBoosting { get; set; }
    public bool IsDrifting { get; set; }
    #endregion

    #region Slider
    [Header("Slider's Control")]
    [SerializeField] TextMeshProUGUI _accelerationSliderText;
    [SerializeField] TextMeshProUGUI _topSpeedSliderText;
    [SerializeField] TextMeshProUGUI _suspensionSliderText;
    [SerializeField] TextMeshProUGUI _handlingSliderText;
    [SerializeField] TextMeshProUGUI _brakeSliderText;
    [SerializeField] TextMeshProUGUI _gyroSliderText;
    float _accelerationSensitivity = 5f;
    float _topSpeedSensitivity = 100f;
    float _brakeSensitivity = 2f;
    float _handlingSensitivity = 0.5f;
    float _suspensionSensitivity = 8.5f;
    float _gyroSensitivity = 2f;
    #endregion

    #region Accel, Brake, Steer
    [Header("Acceleration")]
    float currentAccelerationVelocity; // Velocity of the smoothing

    [Header("Braking")]
    private float lastBrakePressTime = -1f;
    private const float doublePressThreshold = 1f; // Threshold for detecting a double press in seconds
    float currentBrakingVelocity; // Velocity of the smoothing

    [Header("Steering")]
    bool IsGyroscopeOn;
    float currentSteeringVelocity; // Velocity of the smoothing
    float targetSteeringValue; // Target steering value to smoothly transition towards
    float driftSteeringValue = 1.5f;
    #endregion

    private void Awake()
    {
        // Input Action
        _playerActionMap = _inputActions.FindActionMap("PlayerActionMap");
        _brakeInputAction = _playerActionMap.FindAction("AccelerationBraking");
        _steeringInputAction = _playerActionMap.FindAction("Steering");
        _boostInputAction = _playerActionMap.FindAction("Boost");

        _brakeInputAction.performed += GetBrakeInput;
        _brakeInputAction.canceled += GetBrakeInput;

        _steeringInputAction.performed += GetSteeringInput;
        _steeringInputAction.canceled += GetSteeringInput;

        _boostInputAction.performed += GetBoostInput;
    }

    private void Start()
    {
        SetupGearChangeSpeed();
    }

    private void Update()
    {
        if (!GameManagerUno.Instance.IsRaceOn) return;

        // Auto Acceleration & Braking System
        if (IsBraking)
        {
            AccelerationFactor = 0f;
            BrakingFactor = Mathf.SmoothDamp(BrakingFactor, -1f, ref currentBrakingVelocity, _brakeSensitivity);
        }
        else
        {
            BrakingFactor = 0f;
            AccelerationFactor = Mathf.SmoothDamp(AccelerationFactor, 1f, ref currentAccelerationVelocity, _accelerationSensitivity);
        }

        // Top Speed
        TopSpeedValue = _topSpeedSensitivity;


        // For Steering - How quickly tire turns
        if (IsGyroscopeOn)
        {
            float sign = Mathf.Sign(Input.acceleration.x);
            // Check if the gyro is on and drifting has started
            if (IsDrifting)
            {
                // Check if the absolute difference between acceleration and zero is within a threshold
                if (Mathf.Abs(Input.acceleration.x) < 0.1f) { IsDrifting = false; }

                // Set the steering value to the drift value
                SteeringValue = driftSteeringValue * sign;
            }
            else
            {
                // Otherwise, continue normal steering input from gyro
                SteeringValue = Mathf.Clamp(Input.acceleration.x * _gyroSensitivity, -1f, 1f);
            }
        }
        else
        {
            // When gyro is off, use SmoothDamp to handle steering

            if (IsDrifting && targetSteeringValue == 0) IsDrifting = false;
            if (IsDrifting && Mathf.Abs(targetSteeringValue) > 0)
            {
                float sign = Mathf.Sign(targetSteeringValue);
                targetSteeringValue = driftSteeringValue * sign;
            }
            SteeringValue = Mathf.SmoothDamp(SteeringValue, targetSteeringValue, ref currentSteeringVelocity, _handlingSensitivity);
        }

        if (Mathf.Abs(SteeringValue) < 0.05f) SteeringValue = 0f;


        // Suspension
        SuspensionValue = _suspensionSensitivity;
    }

    private void OnEnable()
    {
        _brakeInputAction.Enable();
        _steeringInputAction.Enable();
        _boostInputAction.Enable();
    }

    private void OnDisable()
    {
        _brakeInputAction.Disable();
        _steeringInputAction.Disable();
        _boostInputAction.Disable();
    }


    #region Buttons Input

    void GetBrakeInput(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        if (value < 0)
        {
            // Drifting logic based on double tap of brake
            if (Time.time - lastBrakePressTime <= doublePressThreshold)
                IsDrifting = true;
            else IsDrifting = false;

            if (IsBoosting) IsBoosting = false;
            IsBraking = IsDrifting ? false : true;
            lastBrakePressTime = Time.time;
        }
        else
        {
            IsBraking = false;
            IsDrifting = false;
        }
    }

    void GetSteeringInput(InputAction.CallbackContext context)
    {
        targetSteeringValue = context.ReadValue<float>();
    }

    void GetBoostInput(InputAction.CallbackContext context)
    {
        IsBoosting = true;
    }

    #endregion

    #region UI Slider Control Settings

    public void ToggleGyroscope(bool value)
    {
        IsGyroscopeOn = value;
    }

    public void GetAccelerationSenstivity(float value)
    {
        // Convert the slider value to the desired range
        _accelerationSensitivity = Mathf.Lerp(5f, 0.1f, value); // Convert to desired range
        float clampVal = Mathf.Lerp(10f, 2f, value);
        // Display Value in UI
        _accelerationSliderText.text = "(10-2) Acceleration - " + clampVal.ToString("F1");
    }

    public void GetTopSpeedSenstivity(float value)
    {
        // Convert the slider value to the desired range
        _topSpeedSensitivity = Mathf.RoundToInt(Mathf.Lerp(100f, 350f, value)); // Convert to desired range
        // Display Value in UI
        _topSpeedSliderText.text = "(100-350) Top Speed - " + _topSpeedSensitivity;
        SetupGearChangeSpeed();
    }

    public void GetBrakeSenstivity(float value)
    {
        // Convert the slider value to the desired range
        _brakeSensitivity = Mathf.Lerp(2f, 0.1f, value); // Convert to desired range

        // Display Value in UI
        float clampVal = Mathf.Lerp(5f, 1f, value);
        _brakeSliderText.text = "(5-1) Brake - " + clampVal.ToString("F1");
    }

    public void GetHandlingSenstivity(float value)
    {
        // Convert the slider value to the desired range
        _handlingSensitivity = Mathf.Lerp(1, 0.1f, value); // Convert to desired range
        // Display Value in UI
        _handlingSliderText.text = "(10-1) Handling - " + (_handlingSensitivity * 10).ToString("F1");
    }

    public void GetSuspensionSenstivity(float value)
    {
        // Convert the slider value to the desired range
        _suspensionSensitivity = Mathf.Lerp(3f, 20f, value); // Convert to desired range
        // Display Value in UI
        _suspensionSliderText.text = "(3-20) Suspension - " + _suspensionSensitivity.ToString("F1");
    }

    public void GetGyroSenstivity(float value)
    {
        // Convert the slider value to the desired range
        _gyroSensitivity = Mathf.Lerp(1f, 3f, value); // Convert to desired range
        // Display Value in UI
        _gyroSliderText.text = "(1-3) Gyro - " + _gyroSensitivity.ToString("F1");
    }

    #endregion

    #region Car Controller

    public void SetUpCarController(GameObject car)
    {
        CarController = car.GetComponent<CarController>();
        CarController.InputManager = this;
    }

    public void SetupGearChangeSpeed()
    {
        CarController controller = GameManagerUno.Instance.ActiveVehicle.GetComponent<CarController>();
        float[] ratioArray = controller.GearRatio;
        int[] gearChangeSpeedArray = new int[ratioArray.Length + 1];

        for (int i = 0; i < ratioArray.Length; i++)
        {
            gearChangeSpeedArray[i+1] = Mathf.RoundToInt(_topSpeedSensitivity / ratioArray[i]);
        }
        gearChangeSpeedArray[0] = 1;
        controller.GearChangeSpeed = gearChangeSpeedArray;
    }

    #endregion
}

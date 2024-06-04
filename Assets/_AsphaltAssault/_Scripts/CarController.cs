using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarController : MonoBehaviour
{
    internal enum driveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }
    [SerializeField] driveType _drive;
    [SerializeField] TextMeshProUGUI _speedDisplayText;

    //other classes ->
    public ICarControl InputManager;
    carEffects _carEffects;
    UIManager _uiManager;


    [Header("Wheels")]
    [SerializeField] private WheelCollider[] _wheelColliders;
    [SerializeField] private GameObject[] _wheelMesh;


    [Header("Steering")]
    [SerializeField] float _turnRadius = 6f;
    [SerializeField] float _airborneMoveStrength = 50f;
    [SerializeField] float _airborneRotStrength = 15f;


    [Header("Nitros")]
    public bool nitrusFlag = false;
    float _nitrosValue = 1;
    float _maxNitrosValue = 10f;
    float _boostAmount = 5000f;


    [Header("Input Values")]
    float _accelerationValue;
    float _topSpeedValue;
    float _brakingValue;
    float _steeringValue;


    [Header("Drift Values")]
    public bool playPauseSmoke = false;
    WheelFrictionCurve  initialForwardFriction, initialSidewaysFriction;
    float _driftFactor;


    [Header("Engine Config")]
    public float[] GearRatio;
    public int[] GearChangeSpeed;
    public bool isVehicleInReverse = false;
    public int CurrentGear = 0;
    float _motorTorque = 0;
    float _brakeTorque = 0;
    float avgWheelRPM;


    [Header("Main Variables")]
    [SerializeField] private GameObject _centerOfMass;
    public float SPEED;
    float _downForceValue = 10f;
    Rigidbody _rigidbody;
    bool _isControlBlocked { get { return !GameManagerUno.Instance.IsRaceOn; } }


    [Header("Suspension")]
    float _suspensionFrequency = 10;
    float dampingRatio = 0.8f;
    float forceShift = 0.03f;
    bool setSuspensionDistance = true;

    [SerializeField] int _accelerationTorqueFactor = 100;
    [SerializeField] int BrakeForceValue = 100;
    [SerializeField] float _brakeTorqueFactor = 1000;
    [SerializeField] float maxReverseTorque = 1000;
    [SerializeField] int maxReverseSpeed = 25;


    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != "GameScene") return;

        _carEffects = GetComponent<carEffects>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = _centerOfMass.transform.localPosition;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "GameScene") return;
        _uiManager = GameManagerUno.Instance.GetComponent<UIManager>();


        initialForwardFriction = _wheelColliders[0].forwardFriction;
        initialSidewaysFriction = _wheelColliders[0].sidewaysFriction;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "GameScene") return;
        if (_isControlBlocked) return;

        // Input Value
        _accelerationValue = InputManager.AccelerationFactor;
        _brakingValue = Mathf.Abs(InputManager.BrakingFactor);
        _topSpeedValue = InputManager.TopSpeedValue;
        _steeringValue = InputManager.SteeringValue;
        _suspensionFrequency = InputManager.SuspensionValue;


        AddDownForce();
        WheelRPM();

        CalculateMotorTorque();
        BrakeVehicle();

        SteerVehicle();
        CheckDrift();

        AnimateWheels();
        Suspension();

        MoveVehicle();
        ActivateNitrus();

        if (isVehicleInReverse) CurrentGear = 0;
        SPEED = _rigidbody.velocity.magnitude * 3.6f;

        string fwdSlip = "\nForward Slip\n";
        string sideSlip = "\nSideSlip\n";

        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            WheelHit wheelHit;
            _wheelColliders[i].GetGroundHit(out wheelHit);
            fwdSlip += wheelHit.forwardSlip.ToString("F1") + ", ";
            sideSlip += wheelHit.sidewaysSlip.ToString("F1") + ", ";
        }

        _speedDisplayText.text = "SPEED: " + Mathf.RoundToInt(SPEED) + "\nGEAR: " + (isVehicleInReverse? "R":(CurrentGear - 1))
                        + "\nDrift: " + InputManager.IsDrifting +"\nMotor Torque: " + Mathf.RoundToInt(_motorTorque)
                        + "\nBrake Torque: " + Mathf.RoundToInt(_brakeTorque) + fwdSlip + sideSlip;
    }



    private void AddDownForce()
    {
        _rigidbody.AddForce(-transform.up * _downForceValue * _rigidbody.velocity.magnitude);
    }

    private void WheelRPM()
    {
        float sum = 0;
        int R = 0;
        for (int i = 0; i < 4; i++)
        {
            sum += _wheelColliders[i].rpm;
            R++;
        }
        avgWheelRPM = (R != 0) ? sum / R : 0;

        if (avgWheelRPM < 0 && !isVehicleInReverse)
        {
            isVehicleInReverse = true;
        }
        else if (avgWheelRPM > 0 && isVehicleInReverse)
        {
            isVehicleInReverse = false;
        }
    }

    private void CalculateMotorTorque()
    {
        if (_accelerationValue != 0)
        {
            _rigidbody.drag = 0.005f;
        }
        if (_accelerationValue == 0)
        {
            _rigidbody.drag = 0.1f;
        }

        // Determine which gear (index) the SPEED falls into
        float targetSpeed;
        CurrentGear = 0;
        for (int i = 0; i < GearChangeSpeed.Length; i++)
        {
            if (SPEED <= GearChangeSpeed[i])
            {
                CurrentGear = i+1;
                break;
            }
        }

        // Update targetSpeed based on the current gear
        if (CurrentGear < GearChangeSpeed.Length)
        {
            targetSpeed = GearChangeSpeed[CurrentGear];
        }
        else
        {
            // If SPEED exceeds all values in the array, use the last value
            targetSpeed = GearChangeSpeed[GearChangeSpeed.Length - 1];
        }

        _motorTorque = _accelerationValue * _accelerationTorqueFactor * (targetSpeed - SPEED);
    }

    private void BrakeVehicle()
    {
        if (InputManager.IsBraking && SPEED <= maxReverseSpeed) { _motorTorque = -maxReverseTorque; }
        if (InputManager.IsBraking && SPEED > 1 && !isVehicleInReverse)
        {
            _rigidbody.AddForce(-transform.forward * BrakeForceValue * SPEED, ForceMode.Force);
            _brakeTorque = _brakeTorqueFactor * _brakingValue;
        }
        else
        {
            _brakeTorque = 0;
        }
    }

    [Header("Drift")]
    [SerializeField] float DriftFwdForce = 25f;
    [SerializeField] float DriftSideForce = 50f;
    [SerializeField] float DriftSideImpulse = 15f;
    private void CheckDrift()
    {
        //time it takes to go from normal drive to drift 
        float driftSmoothFactor = .7f * Time.deltaTime;

        if (InputManager.IsDrifting)
        {
            playPauseSmoke = true;

            // Friction values
            var fwdFriction = initialForwardFriction;
            var sideFriction = initialSidewaysFriction;

            fwdFriction.extremumSlip = 0.3f;
            fwdFriction.extremumValue = 0.4f;
            fwdFriction.asymptoteSlip = 0.6f;
            fwdFriction.asymptoteValue = 0.3f;
            fwdFriction.stiffness = 1f;

            sideFriction.extremumSlip = 0.5f;
            sideFriction.extremumValue = 1.2f;
            sideFriction.asymptoteSlip = 0.7f;
            sideFriction.asymptoteValue = 1.0f;
            sideFriction.stiffness = 1f;

            foreach (var wheel in _wheelColliders)
            {
                wheel.forwardFriction = fwdFriction;
                wheel.sidewaysFriction = sideFriction;
            }

            // Apply torque around the vehicle's up axis (vertical axis)
            float sign = Mathf.Sign(_steeringValue);
            _rigidbody.AddTorque(transform.up * DriftSideImpulse * sign, ForceMode.Impulse);
            _rigidbody.AddTorque(transform.up * DriftSideForce * sign, ForceMode.Force);
            _rigidbody.AddForce(transform.forward * SPEED * DriftFwdForce, ForceMode.Force);

            // differential lock

            // drift fractor
        }
        else
        {
            playPauseSmoke = false;

            for (int i = 0; i < 4; i++)
            {
                _wheelColliders[i].forwardFriction = initialForwardFriction;
                _wheelColliders[i].sidewaysFriction = initialSidewaysFriction;
            }
        }

        //checks the amount of slip to control the drift
        for (int i = 2; i < 4; i++)
        {
            WheelHit wheelHit;
            _wheelColliders[i].GetGroundHit(out wheelHit);

            if (wheelHit.sidewaysSlip < 0) _driftFactor = (2f + -InputManager.SteeringValue) * Mathf.Abs(wheelHit.sidewaysSlip);

            if (wheelHit.sidewaysSlip > 0) _driftFactor = (2f + InputManager.SteeringValue) * Mathf.Abs(wheelHit.sidewaysSlip);
        }
    }

    private void SteerVehicle()
    {
        if (isGrounded())
        {
            float turnRadius = (SPEED < 50f) ? (_turnRadius - 2f) : _turnRadius;

            //acerman steering formula
            //steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius + (rearTrack / 2))) * horizontalInput;

            //rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            float rearTrack = 1.5f;                     // Distance btn the rear tires
            float wheelbase = 2.55f;                    // distance btn centre of front & rear tires
            float leftWheelAngle;
            float rightWheelAngle;
            if (_steeringValue > 0)
            {
                leftWheelAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius + (rearTrack / 2))) * _steeringValue;
                rightWheelAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius - (rearTrack / 2))) * _steeringValue;
            }
            else if (_steeringValue < 0)
            {
                leftWheelAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius - (rearTrack / 2))) * _steeringValue;
                rightWheelAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius + (rearTrack / 2))) * _steeringValue;
            }
            else
            {
                leftWheelAngle = 0;
                rightWheelAngle = 0;
            }

            _wheelColliders[0].steerAngle = leftWheelAngle;
            _wheelColliders[1].steerAngle = rightWheelAngle;
        }
        else                // Steer in air
        {
            if (_steeringValue < 0)
            {
                // Apply force to the left
                _rigidbody.AddForce(-transform.right * _airborneMoveStrength, ForceMode.Impulse);
                // Add rotation to the left
                _rigidbody.AddTorque(-Vector3.up * _airborneRotStrength, ForceMode.Impulse);
            }
            else if (_steeringValue > 0)
            {
                // Apply force to the right
                _rigidbody.AddForce(transform.right * _airborneMoveStrength, ForceMode.Impulse);
                // Add rotation to the right
                _rigidbody.AddTorque(Vector3.up * _airborneRotStrength, ForceMode.Impulse);
            }
        }
    }

    private void AnimateWheels()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            _wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);

            _wheelMesh[i].transform.position = wheelPosition;
            _wheelMesh[i].transform.rotation = wheelRotation;
        }
    }

    private void Suspension()
    {
        // work out the stiffness and damper parameters based on the better spring model
        foreach (WheelCollider wc in _wheelColliders)
        {
            JointSpring spring = wc.suspensionSpring;

            spring.spring = Mathf.Pow(Mathf.Sqrt(wc.sprungMass) * _suspensionFrequency, 2);
            spring.damper = 2 * dampingRatio * Mathf.Sqrt(spring.spring * wc.sprungMass);

            wc.suspensionSpring = spring;

            Vector3 wheelRelativeBody = transform.InverseTransformPoint(wc.transform.position);
            float distance = GetComponent<Rigidbody>().centerOfMass.y - wheelRelativeBody.y + wc.radius;

            wc.forceAppPointDistance = distance - forceShift;

            // the following line makes sure the spring force at maximum droop is exactly zero
            if (spring.targetPosition > 0 && setSuspensionDistance)
                wc.suspensionDistance = wc.sprungMass * Physics.gravity.magnitude / (spring.targetPosition * spring.spring);
        }
    }

    private void MoveVehicle()
    {
        if (_drive == driveType.allWheelDrive)
        {
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                _wheelColliders[i].motorTorque = (_motorTorque / 4);
            }
        }
        else if (_drive == driveType.rearWheelDrive)
        {
            _wheelColliders[2].motorTorque = (_motorTorque / 2);
            _wheelColliders[3].motorTorque = (_motorTorque / 2);
        }
        else
        {
            _wheelColliders[0].motorTorque = (_motorTorque / 2);
            _wheelColliders[1].motorTorque = (_motorTorque / 2);
        }

        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            _wheelColliders[i].brakeTorque = _brakeTorque;
        }
    }

    public void ActivateNitrus()
    {
        if (!InputManager.IsBoosting && _nitrosValue <= _maxNitrosValue)
        {
            _nitrosValue += Time.deltaTime / (_maxNitrosValue * 2);
        }
        else
        {
            if (_nitrosValue <= 0)
            {
                _nitrosValue = 0;
                InputManager.IsBoosting = false;
            }
        }

        if (InputManager.IsBoosting)
        {
            if (_nitrosValue > 0 && SPEED > 5)
            {
                _nitrosValue -= Time.deltaTime;
                _carEffects.startNitrusEmitter();
                _rigidbody.AddForce(transform.forward * _boostAmount);
            }
            else
            {
                _carEffects.stopNitrusEmitter();
                InputManager.IsBoosting = false;
            }
        }
        else _carEffects.stopNitrusEmitter();

        float fillAmount = _nitrosValue / _maxNitrosValue;
        _uiManager.UpdateNosUI(fillAmount);
    }

    private bool isGrounded()
    {
        if (_wheelColliders[0].isGrounded && _wheelColliders[1].isGrounded && _wheelColliders[2].isGrounded && _wheelColliders[3].isGrounded)
            return true;
        else
            return false;
    }

    public void CollectNitros()
    {
        _nitrosValue += 2;
        if (_nitrosValue >= _maxNitrosValue)
        {
            _nitrosValue = _maxNitrosValue;
        }
    }
}

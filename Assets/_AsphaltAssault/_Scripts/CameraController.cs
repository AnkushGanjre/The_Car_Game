using UnityEngine;

public class CameraControl : MonoBehaviour 
{
    private CarController _carController;
    private GameObject _cameralookAt, _cameraPos;
    private float _speed = 0;
    private float _defaltFOV = 0, _desiredFOV = 0;
    [Range (0, 50)] public float _smoothTime = 9.5f;

    private void Start ()
    {
        TargetSetup();

        _defaltFOV = Camera.main.fieldOfView;
        _desiredFOV = _defaltFOV + 15;
    }

    private void FixedUpdate()
    {
        follow ();
        boostFOV ();
    }

    public void TargetSetup()
    {
        _carController = GameManagerUno.Instance.ActiveVehicle.GetComponent<CarController>();
        _cameralookAt = _carController.transform.Find("CameraLookAt").gameObject;
        _cameraPos = _carController.transform.Find("CameraConstraint").gameObject;
    }

    private void follow()
    {
        _speed = _carController.SPEED / _smoothTime;
        gameObject.transform.position = Vector3.Lerp (transform.position, _cameraPos.transform.position ,  Time.deltaTime * _speed);
        gameObject.transform.LookAt (_cameralookAt.gameObject.transform.position);
    }

    private void boostFOV()
    {
        if (_carController.nitrusFlag)
            Camera.main.fieldOfView = Mathf.Lerp (Camera.main.fieldOfView, _desiredFOV, Time.deltaTime * 5);
        else
            Camera.main.fieldOfView = Mathf.Lerp (Camera.main.fieldOfView, _defaltFOV, Time.deltaTime * 5);
    }

}
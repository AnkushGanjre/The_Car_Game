using UnityEngine;

public class NitrosCollector : MonoBehaviour
{
    CarController _carController;

    private void Start()
    {
        _carController = GetComponent<CarController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Nitros"))
        {
            Nitros nos = other.GetComponent<Nitros>();
            if (nos != null && !nos.IsCollected)
            {
                nos.CollectNos();
                _carController.CollectNitros();
            }
        }
    }
}

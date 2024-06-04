using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nitros : MonoBehaviour
{
    public bool IsCollected = false;
    GameObject _nos;

    private void Start()
    {
        _nos = transform.GetChild(0).gameObject;
    }

    public void CollectNos()
    {
        IsCollected = true;
        _nos.SetActive(false);
        StartCoroutine(ReactivateCoroutine());
    }

    IEnumerator ReactivateCoroutine()
    {
        yield return new WaitForSeconds(10);
        IsCollected = false;
        _nos.SetActive(true);
    }
}

using UnityEngine;

namespace DonzaiGamecorp.TicTacToe
{
    public class SkyboxRotation : MonoBehaviour
    {
        [SerializeField] float rotationSpeed = 0.5f;

        void Update()
        {
            RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
        }
    }
}



using UnityEngine;
using UnityEngine.SceneManagement;

public class carEffects : MonoBehaviour
{
    public Material brakeLights;
    //public AudioSource skidClip;
    public TrailRenderer[] tireMarks;
    public ParticleSystem[] smoke;
    public ParticleSystem[] nitrusSmoke;
    private CarController controller;
    private ICarControl IM;
    private bool smokeFlag  = false , lightsFlag = false , tireMarksFlag;

    private void Start()
    {
        controller = GetComponent<CarController>();
        IM = controller.InputManager;
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name != "GameScene") return;
        //chectDrift();
        activateSmoke();
        activateLights();
    }

    private void activateSmoke()
    {
        if (controller.playPauseSmoke) startSmoke();
        else stopSmoke();

        if (smokeFlag)
        {
            for (int i = 0; i < smoke.Length; i++)
            {
                var emission = smoke[i].emission;
                emission.rateOverTime = ((int)controller.SPEED * 10 <= 2000) ? (int)controller.SPEED * 10 : 2000;
            }
        }
    }

    public void startSmoke()
    {
        if(smokeFlag)return;
        for (int i = 0; i < smoke.Length; i++)
        {
            var emission = smoke[i].emission;
            emission.rateOverTime = ((int) controller.SPEED *2 >= 2000) ? (int) controller.SPEED * 2 : 2000;
            smoke[i].Play();
        }
        smokeFlag = true;

    }

    public void stopSmoke()
    {
        if(!smokeFlag) return;
        for (int i = 0; i < smoke.Length; i++)
        {
            smoke[i].Stop();
        }
        smokeFlag = false;
    }

    public void startNitrusEmitter()
    {
        if (controller.nitrusFlag) return;
        for (int i = 0; i < nitrusSmoke.Length; i++)
        {
            nitrusSmoke[i].Play();
        }

        controller.nitrusFlag = true;
    }
    public void stopNitrusEmitter()
    {
        if (!controller.nitrusFlag) return;
        for (int i = 0; i < nitrusSmoke.Length; i++)
        {
            nitrusSmoke[i].Stop();
        }
        controller.nitrusFlag = false;

    }

    private void activateLights()
    {
        if (IM.BrakingFactor < 0 || controller.SPEED <= 1) turnLightsOn();
        else turnLightsOff();
    }

    private void turnLightsOn()
    {
        if (lightsFlag) return;
        brakeLights.SetColor("_EmissionColor", Color.red *5);
        lightsFlag = true;
        //lights.SetActive(true);
    }    
    
    private void turnLightsOff()
    {
        if (!lightsFlag) return;
        brakeLights.SetColor("_EmissionColor", Color.black);
        lightsFlag = false;
        //lights.SetActive(false);
    }

    private void chectDrift()
    {
        if (IM.IsDrifting) startEmitter();
        else stopEmitter();
    }

    private void startEmitter()
    {
        if (tireMarksFlag) return;
        foreach (TrailRenderer T in tireMarks)
        {
            T.emitting = true;
        }
        //skidClip.Play();
        tireMarksFlag = true;
    }

    private void stopEmitter()
    {
        if (!tireMarksFlag) return;
        foreach (TrailRenderer T in tireMarks)
        {
            T.emitting = false;
        }
        //skidClip.Stop();
        tireMarksFlag = false;
    }
}

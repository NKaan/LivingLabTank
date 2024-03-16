using UnityEngine;
using System.Collections;

// This script must be attached to "Engine_Sound".
namespace ChobiAssets.KTP
{

    public class SE_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the "Engine_Sound" object in the tank.
		 * This script controls the engine sound in the tank.
		*/

        [Header("Engine Sound settings")]
        [Tooltip("Minimum Pitch")] public float minPitch = 1.0f;
        [Tooltip("Maximum Pitch")] public float maxPitch = 2.0f;
        [Tooltip("Minimum Volume")] public float minVolume = 0.1f;
        [Tooltip("Maximum Volume")] public float maxVolume = 0.3f;
        [Tooltip("Denominator for Pivot-Turning (radian per second)")] public float pivotTurningDenominator = 4.5f;
        [Tooltip("Standard Speed of this tank (meter per second)")] public float standardSpeed = 12.0f;


        AudioSource thisAudioSource;
        Wheel_Control_CS wheelControlScript;
        float currentRate;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            wheelControlScript = idScript.wheelControlScript;

            // Setup the AudioSource.
            thisAudioSource = GetComponent<AudioSource>();
            if (thisAudioSource == null)
            {
                Debug.LogError("AudioSource is not attached to" + this.name);
                Destroy(this);
                return;
            }
            thisAudioSource.loop = true;
            thisAudioSource.volume = 0.0f;
            thisAudioSource.Play();
        }


        void Update()
        {
            Engine_Sound();
        }


        float soundVelocity;
        void Engine_Sound()
        {
            if (wheelControlScript == null)
            {
                return;
            }

            float targetRate;
            if (wheelControlScript.moveAxis.y == 0.0f)
            { // The tank is doing a pivot-turn.
                targetRate = wheelControlScript.currentAngularVelocityMag / (pivotTurningDenominator * (wheelControlScript.maxSpeed / standardSpeed));
            }
            else
            { // Not pivot-turn.
                targetRate = wheelControlScript.currentVelocityMag / wheelControlScript.maxSpeed;
            }
            targetRate = Mathf.Clamp01(targetRate);

            currentRate = Mathf.SmoothDamp(currentRate, targetRate, ref soundVelocity, 2.0f * Time.deltaTime, 2.0f);
            thisAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, currentRate);
            thisAudioSource.volume = Mathf.Lerp(minVolume, maxVolume, currentRate);
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".
            Destroy(this.gameObject);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
            if (isPaused)
            {
                thisAudioSource.Stop();
            }
            else
            {
                thisAudioSource.Play();
            }
        }
       
    }

}

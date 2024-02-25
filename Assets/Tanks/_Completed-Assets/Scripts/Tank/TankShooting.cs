using Mirror;
using SL.Wait;
using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankShooting : NetworkBehaviour
    {
        public int m_PlayerNumber = 1;              // Used to identify the different players.
        public Rigidbody m_Shell;                   // Prefab of the shell.
        public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
        public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
        public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
        public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
        public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
        public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
        public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.


        private string m_FireButton;                // The input axis that is used for launching shells.
        private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
        private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
        [SyncVar(hook = nameof(FireMethod))] public bool m_Fired = false;
        public bool m_Fired_Client = false;

        public float fireTime = 1f;

        public Player myPlayer;

        private void Awake()
        {
            myPlayer = GetComponent<Player>();
        }

        public void FireMethod(bool oldValue,bool nextValue)
        {
            m_Fired_Client = nextValue;
            m_CurrentLaunchForce = m_MinLaunchForce;
        }

        private void OnEnable()
        {
            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;
        }


        private void Start ()
        {
            // The fire axis is based on the player number.
            m_FireButton = "Jump";

            // The rate that the launch force charges up is the range of possible forces by the max charge time.
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
        }

        public void Update ()
        {
            if (!isLocalPlayer)
                return;

            if (myPlayer.health.m_Dead)
                return;

            // The slider should have a default value of the minimum launch force.
            m_AimSlider.value = m_MinLaunchForce;

            // If the max force has been exceeded and the shell hasn't yet been launched...
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired_Client)
            {
                m_CurrentLaunchForce = m_MaxLaunchForce;
                ClientFire();
            }
            // Otherwise, if the fire button has just started being pressed...
            else if (Input.GetButtonDown (m_FireButton) && !m_Fired_Client)
            {
                m_CurrentLaunchForce = m_MinLaunchForce;

                // Change the clip to the charging clip and start it playing.
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play ();
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (Input.GetButton (m_FireButton) && !m_Fired_Client)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // Otherwise, if the fire button is released and the shell hasn't been launched yet...
            else if (Input.GetButtonUp (m_FireButton) && !m_Fired_Client)
            {
                // ... launch the shell.
                ClientFire();
            }
        }

        private void ClientFire()
        {
            if (m_Fired_Client)
                return;
            m_Fired_Client = true;
            CmdFire(m_CurrentLaunchForce);
        }


        [Command] //Sadecec Serverda Çalışır
        private void CmdFire (float _m_CurrentLaunchForce)
        {

            if (m_Fired && myPlayer.health.m_Dead)
                return;

            m_Fired = true;

            ShellExplosion shellInstance =
                Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation).GetComponent<ShellExplosion>();

            shellInstance.ownPlayerID = netId;
            shellInstance.m_CurrentLaunchForce = _m_CurrentLaunchForce;

            NetworkServer.Spawn(shellInstance.gameObject);

            RpcFire(_m_CurrentLaunchForce);

            Wait.Seconds(fireTime, () => m_Fired = false).Start();

        }

        [ClientRpc] // Clientlerde çalışır (Tüm)
        private void RpcFire(float _m_CurrentLaunchForce)
        {
            // Change the clip to the firing clip and play it.
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play();
        }
    }
}
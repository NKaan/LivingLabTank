using Mirror;
using Mirror.Examples.Basic;
using UnityEngine;
using UnityEngine.Events;

namespace Complete
{
    public class TankMovement : NetworkBehaviour
    {
        public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
                        // How fast the tank moves forward and back.
        public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
        public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
        public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
        public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
		public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.

        private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
        private string m_TurnAxisName;              // The name of the input axis for turning.
        private Rigidbody m_Rigidbody;              // Reference used to move the tank.
        private float m_MovementInputValue;         // The current value of the movement input.
        private float m_TurnInputValue;             // The current value of the turn input.
        private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.
        private ParticleSystem[] m_particleSystems; // References to all the particles systems used by the Tanks

        [SyncVar]
        public float m_Speed = 12f;

        public float reduceFuelPerSecond = 1f;
        public float reduceMoveFuelPerSecond = 3f;

        [SyncVar] public float m_Max_Fuel = 100f;
        [SyncVar(hook = nameof(FuelSet))] public float m_Fuel = 100f;

        public UnityEvent OnFueldEmpty = new UnityEvent();

        private Player m_Player;

        private void Awake ()
        {
            m_Rigidbody = GetComponent<Rigidbody> ();
            m_Player = GetComponent<Player> ();
            m_Player.playerUI = GameObject.FindObjectOfType<GameUI>();

        }
        public void FuelSet(float oldValue,float nextValue)
        {
            if(isLocalPlayer)
                m_Player.playerUI.playerFuelSlider.value = nextValue;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            if (!isLocalPlayer)
                return;

            
            m_Player.playerUI.playerFuelSlider.maxValue = m_Max_Fuel;

            FuelSet(m_Fuel, m_Fuel);
        }

        private void OnEnable ()
        {
            // When the tank is turned on, make sure it's not kinematic.
            m_Rigidbody.isKinematic = false;

            // Also reset the input values.
            m_MovementInputValue = 0f;
            m_TurnInputValue = 0f;

            // We grab all the Particle systems child of that Tank to be able to Stop/Play them on Deactivate/Activate
            // It is needed because we move the Tank when spawning it, and if the Particle System is playing while we do that
            // it "think" it move from (0,0,0) to the spawn point, creating a huge trail of smoke
            m_particleSystems = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                m_particleSystems[i].Play();
            }
        }


        private void OnDisable ()
        {
            // When the tank is turned off, set it to kinematic so it stops moving.
            m_Rigidbody.isKinematic = true;

            // Stop all particle system so it "reset" it's position to the actual one instead of thinking we moved when spawning
            for(int i = 0; i < m_particleSystems.Length; ++i)
            {
                m_particleSystems[i].Stop();
            }
        }


        private void Start ()
        {
            // The axes names are based on player number.
            m_MovementAxisName = "Vertical";
            m_TurnAxisName = "Horizontal";

            // Store the original pitch of the audio source.
            m_OriginalPitch = m_MovementAudio.pitch;
        }


        public void Update ()
        {
            if (isLocalPlayer && !m_Player.health.m_Dead)
            {
                m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
                m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

                if (m_MovementInputValue != 0 && m_Fuel > 0)
                    Move();

                if (m_TurnInputValue != 0)
                    Turn();

                EngineAudio();
            }

            if(isServer && !m_Player.health.m_Dead)
                SetFuel(reduceFuelPerSecond);
        }

        public void SetFuel(float addFuel)
        {
            if (isServer)
            {
                if (m_Fuel <= 0)
                    return;

                m_Fuel -= Time.deltaTime * addFuel;

                if (m_Fuel <= 0)
                {
                    m_Fuel = 0;
                    OnFueldEmpty?.Invoke();
                }

            }
        }


        private void EngineAudio ()
        {
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
            {
                // ... and if the audio source is currently playing the driving clip...
                if (m_MovementAudio.clip == m_EngineDriving)
                {
                    // ... change the clip to idling and play it.
                    m_MovementAudio.clip = m_EngineIdling;
                    m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play ();
                }
            }
            else
            {
                // Otherwise if the tank is moving and if the idling clip is currently playing...
                if (m_MovementAudio.clip == m_EngineIdling)
                {
                    // ... change the clip to driving and play.
                    m_MovementAudio.clip = m_EngineDriving;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
        }


        private void FixedUpdate ()
        {
            // Adjust the rigidbodies position and orientation in FixedUpdate.
           
        }


        private void Move()
        {
            // Hareket vektörünü oluştur, tankın yönüne dayalı olarak giriş değerleri, hız ve kareler arasındaki zamanla belirlenen büyüklükte.
            Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

            // Bu hareketi transform'un pozisyonuna uygula.
            transform.position += movement;

            if(isServer)
                SetFuel(reduceMoveFuelPerSecond);
        }

        private void Turn()
        {
            // Giriş, hız ve kareler arasındaki zamanla belirlenen dönüş miktarını belirle.
            float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

            // Bu miktarı y ekseninde bir dönüşe dönüştür.
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

            // Bu dönüşü transform'un rotasyonuna uygula.
            transform.rotation *= turnRotation;
        }
    }
}
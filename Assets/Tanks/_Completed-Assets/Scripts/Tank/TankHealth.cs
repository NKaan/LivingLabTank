﻿using Mirror;
using SL.Wait;
using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankHealth : NetworkBehaviour
    {
                     // The amount of health each tank starts with.
        public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
        public Image m_FillImage;                           // The image component of the slider.
        public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
        public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
        public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.
        
        
        private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
        private ParticleSystem m_ExplosionParticles;

        [SyncVar] public float m_StartingHealth = 100f;
        [SyncVar(hook = nameof(SetHealthUI))] public float m_CurrentHealth;                      
        [SyncVar] public bool m_Dead;

        public Player myPlayer;

        private void Awake ()
        {
            // Instantiate the explosion prefab and get a reference to the particle system on it.
            m_ExplosionParticles = Instantiate (m_ExplosionPrefab).GetComponent<ParticleSystem> ();

            // Get a reference to the audio source on the instantiated prefab.
            m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource> ();

            // Disable the prefab so it can be activated when it's required.
            m_ExplosionParticles.gameObject.SetActive (false);

            myPlayer = GetComponent<Player> ();
        }

        private void OnEnable()
        {
            // When the tank is enabled, reset the tank's health and whether or not it's dead.
            m_CurrentHealth = m_StartingHealth;
            m_Dead = false;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            SetHealthUI(m_CurrentHealth, m_CurrentHealth);
        }

        [ServerCallback]
        public void Update()
        {
            if(!m_Dead && transform.position.y < -2)
            {
                m_Dead = true;
                RpcOnDeath(netId);
            }
        }

        [Server]
        public void TakeDamage (float amount,uint firePlayerID)
        {
            // Reduce current health by the amount of damage done.
            m_CurrentHealth -= amount;

            // If the current health is at or below zero and it has not yet been registered, call OnDeath.
            if (m_CurrentHealth <= 0f && !m_Dead)
            {
                m_Dead = true;
                Wait.Seconds(3f, () => 
                {
                    myPlayer.ServerRestart();
                    
                }).Start();

                RpcOnDeath(firePlayerID);
            }
        }

        private void SetHealthUI (float oldHealt, float newHealt)
        {
            m_Slider.maxValue = m_StartingHealth;
            // Set the slider's value appropriately.
            m_Slider.value = newHealt;

            // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
            m_FillImage.color = Color.Lerp (m_ZeroHealthColor, m_FullHealthColor, newHealt / m_StartingHealth);
        }

        [ClientRpc]
        private void RpcOnDeath (uint firePlayerID)
        {

            // Move the instantiated explosion prefab to the tank's position and turn it on.
            m_ExplosionParticles.transform.position = transform.position;
            m_ExplosionParticles.gameObject.SetActive (true);

            // Play the particle system of the tank exploding.
            m_ExplosionParticles.Play ();

            // Play the tank explosion sound effect.
            m_ExplosionAudio.Play();

            // Turn the tank off.
            myPlayer.SetVisable(false);

            Player player1 = NetworkClient.spawned[firePlayerID].GetComponent<Player>();
            myPlayer.playerUI.AddPlayerMiniUI(player1, myPlayer);
            
        }
    }
}
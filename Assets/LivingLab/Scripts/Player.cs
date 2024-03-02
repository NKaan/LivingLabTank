using Complete;
using MasterServerToolkit.Bridges;
using MasterServerToolkit.Bridges.MirrorNetworking;
using MasterServerToolkit.MasterServer;
using Mirror;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Player : NetworkBehaviour
{
    public ParticleSystem levelUpEffect;
    public Transform effectContent;
    public RoomPlayer roomPlayer;
    public GameUI playerUI;
    public TankHealth health;
    public TankMovement movoment;
    public TextMeshProUGUI playerNameUI;
    public CameraController myCamera;

    [SerializeField]
    private float baseExp = 100f;

    [SerializeField]
    private float expDrop = 10f;

    public UnityEvent<uint,uint> OnDeath;

    [SyncVar(hook = nameof(SetNameUI))] public string playerName;
    [SyncVar(hook = nameof(SetColor))] public Color m_PlayerColor;

    MeshRenderer[] myMeshRenderes;

    private void Awake()
    {
        health = GetComponent<TankHealth>();
        movoment = GetComponent<TankMovement>();
        myCamera = Camera.main.GetComponent<CameraController>();
        myMeshRenderes = GetComponentsInChildren<MeshRenderer>();
    }

    public void SetNameUI(string oldName,string nextName)
    {
        playerNameUI.text = nextName;
    }

    public void SetColor(Color oldColor, Color nextColor)
    {
        // Go through all the renderers...
        for (int i = 0; i < myMeshRenderes.Length; i++)
        {
            // ... set their material color to the color specific to this tank.
            myMeshRenderes[i].material.color = nextColor;
        }
    }

    public void SetVisable(bool visible)
    {
        for (int i = 0; i < myMeshRenderes.Length; i++)
        {
            myMeshRenderes[i].enabled = visible;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if(myCamera != null)
        {
            myCamera.SetTarget(this.transform);
        }

    }

    public void LateUpdate()
    {
        if (isClient)
        {
            Vector3 cameraPosition = myCamera.transform.position;

            // Nesnenin yukarý vektörü
            Vector3 upVector = transform.up;

            // Kamera ile nesnenin arasýndaki vektör
            Vector3 lookDirection = cameraPosition - transform.position;

            // Isim etiketinin rotasyonunu belirle
            playerNameUI.transform.rotation = Quaternion.LookRotation(-lookDirection, upVector);

        }   
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        RoomNetworkManager roomNetworkManager = NetworkManager.singleton as RoomNetworkManager;
        roomPlayer = roomNetworkManager.roomServerManager.GetRoomPlayerByRoomPeer(connectionToClient.connectionId);

        playerName = roomPlayer.Profile.Get<ObservableString>(ProfilePropertyOpCodes.displayName).Value;
        m_PlayerColor = roomPlayer.Profile.Get<ObservableColor>(ProfilePropertyOpCodes.tankColor).Value;


        roomPlayer.Profile.Properties[ProfilePropertyOpCodes.playerLevel].OnDirtyEvent += (obs) =>
        {
            int playerLevel = obs.As<ObservableInt>().Value;
            UpLevel(netId);
        };

        OnDeath.AddListener((deathPlayerID, killerPlayerID) => 
        {
            RoomPlayer deatPlayer = NetworkServer.spawned[deathPlayerID].GetComponent<Player>().roomPlayer;
            RoomPlayer killerPlayer = NetworkServer.spawned[killerPlayerID].GetComponent<Player>().roomPlayer;

            AddExp(killerPlayer.Profile,50);

            //Mst.Server.Rooms.PlayerDeath(deatPlayer.UserId, killerPlayer.UserId,(result) => 
            //{
            //    Debug.Log("OnDeath Result " + result.ToString());
            //});

        });

    }

    public float NeedExpCalculate(int playerLevel)
    {
        return (float)(baseExp * Math.Pow(1.1, playerLevel - 1));
    }

    public void AddExp(ObservableServerProfile serverProfile, float addExp)
    {
        addExp += addExp *= expDrop / 100;
        serverProfile.Properties[ProfilePropertyOpCodes.playerExp].As<ObservableInt>().Value += (int)addExp;

        float needExp = NeedExpCalculate(serverProfile.Properties[ProfilePropertyOpCodes.playerLevel].As<ObservableInt>().Value);

        if (serverProfile.Properties[ProfilePropertyOpCodes.playerExp].As<ObservableInt>().Value >= needExp)
        {
            serverProfile.Properties[ProfilePropertyOpCodes.playerLevel].As<ObservableInt>().Value++;
            serverProfile.Properties[ProfilePropertyOpCodes.playerExp].As<ObservableInt>().Value -= (int)needExp;
            AddExp(serverProfile, 0);
            return;
        }
    }


    [ClientRpc]
    public void UpLevel(uint upLevelPlayerID)
    {
        Player upLevelPlayer = NetworkClient.spawned[upLevelPlayerID].GetComponent<Player>();
        ParticleSystem _levelUpEffect = Instantiate(upLevelPlayer.levelUpEffect).GetComponent<ParticleSystem>();
        upLevelPlayer.AddAffect(_levelUpEffect);
        Destroy(_levelUpEffect.gameObject, 3f);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    [Server]
    public void ServerRestart()
    {
        Transform newTransform = NetworkManager.singleton.GetStartPosition();
        health.m_Dead = false;
        health.m_CurrentHealth = health.m_StartingHealth;
        movoment.m_Fuel = movoment.m_Max_Fuel;
        RpcClientRestart(newTransform.position, newTransform.rotation);
    }

    [ClientRpc]
    public void RpcClientRestart(Vector3 pos,Quaternion rot)
    {
        SetPosition(pos, rot);
        SetVisable(true);
    }

    public void SetPosition(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    public void AddAffect(ParticleSystem effect)
    {
        effect.gameObject.transform.SetParent(effectContent,false);
        effect.Play();
    }

}

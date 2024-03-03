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
    public GameUI gameUI;
    public TankHealth health;
    public TankMovement movoment;
    public TextMeshProUGUI playerNameAndLevelUI;
    public CameraController myCamera;

    public PlayerRankMiniUI myMiniRankUI;

    [SerializeField]
    private float baseExp = 100f;

    [SerializeField]
    private float expDrop = 10f;

    public UnityEvent<uint,uint> OnDeath;

    [SyncVar(hook = nameof(SetPointUI))] public int playerPoint;
    [SyncVar(hook = nameof(SetNameUI))] public string playerName;
    [SyncVar(hook = nameof(SetLevelUI))] public int playerLevel;
    [SyncVar(hook = nameof(SetColor))] public Color m_PlayerColor;

    MeshRenderer[] myMeshRenderes;

    private void Awake()
    {
        health = GetComponent<TankHealth>();
        movoment = GetComponent<TankMovement>();
        myCamera = Camera.main.GetComponent<CameraController>();
        myMeshRenderes = GetComponentsInChildren<MeshRenderer>();
        gameUI = GameObject.FindObjectOfType<GameUI>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        RoomNetworkManager roomNetworkManager = NetworkManager.singleton as RoomNetworkManager;
        roomPlayer = roomNetworkManager.roomServerManager.GetRoomPlayerByRoomPeer(connectionToClient.connectionId);

        playerName = roomPlayer.Profile.Get<ObservableString>(ProfilePropertyOpCodes.displayName).Value;
        m_PlayerColor = roomPlayer.Profile.Get<ObservableColor>(ProfilePropertyOpCodes.tankColor).Value;
        playerLevel = roomPlayer.Profile.Get<ObservableInt>(ProfilePropertyOpCodes.playerLevel).Value;

        roomPlayer.Profile.Properties[ProfilePropertyOpCodes.playerLevel].OnDirtyEvent += (obs) =>
        {
            playerLevel = obs.As<ObservableInt>().Value;
        };

        OnDeath.AddListener((deathPlayerID, killerPlayerID) =>
        {
            RoomPlayer deatPlayer = NetworkServer.spawned[deathPlayerID].GetComponent<Player>().roomPlayer;
            RoomPlayer killerPlayer = NetworkServer.spawned[killerPlayerID].GetComponent<Player>().roomPlayer;
            playerPoint = 0;
        });

    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        gameUI.AddPlayerRankMiniUI(this);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        gameUI.DeleteRankUIPlayer(this);
    }

    public void SetNameUI(string oldName,string nextName)
    {
        playerNameAndLevelUI.text = "Lv." + playerLevel.ToString() + " " + nextName;
    }

    public void SetLevelUI(int oldLevel, int nextLevel)
    {
        playerNameAndLevelUI.text = "Lv." + nextLevel.ToString() + " " + playerName;
        if(oldLevel != 0)
            UpLevel();
    }

    public void SetPointUI(int oldPoint, int nextPoint)
    {
        myMiniRankUI?.UpdateTxt();
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

            // Nesnenin yukar� vekt�r�
            Vector3 upVector = transform.up;

            // Kamera ile nesnenin aras�ndaki vekt�r
            Vector3 lookDirection = cameraPosition - transform.position;

            // Isim etiketinin rotasyonunu belirle
            playerNameAndLevelUI.transform.rotation = Quaternion.LookRotation(-lookDirection, upVector);

        }   
    }

    

    public float NeedExpCalculate(int playerLevel)
    {
        return (float)(baseExp * Math.Pow(1.1, playerLevel - 1));
    }

    public void AddExp(float addExp)
    {
        ObservableServerProfile serverProfile = roomPlayer.Profile;
        addExp += addExp *= expDrop / 100;
        serverProfile.Properties[ProfilePropertyOpCodes.playerExp].As<ObservableInt>().Value += (int)addExp;

        float needExp = NeedExpCalculate(serverProfile.Properties[ProfilePropertyOpCodes.playerLevel].As<ObservableInt>().Value);

        if (serverProfile.Properties[ProfilePropertyOpCodes.playerExp].As<ObservableInt>().Value >= needExp)
        {
            serverProfile.Properties[ProfilePropertyOpCodes.playerLevel].As<ObservableInt>().Value++;
            serverProfile.Properties[ProfilePropertyOpCodes.playerExp].As<ObservableInt>().Value -= (int)needExp;
            AddExp(0);
            return;
        }
    }

    public void AddPoint(int addPointCount)
    {
        playerPoint += addPointCount;

        if(addPointCount > 2)
        {
            AddExp(addPointCount / 2);

            if(addPointCount > 4)
                AddGold(addPointCount / 4);
        }
        
    }

    public void AddGold(int addGoldCount)
    {
        roomPlayer.Profile.Properties[ProfilePropertyOpCodes.gold].As<ObservableInt>().Value += addGoldCount;
    }

    public void UpLevel()
    {
        ParticleSystem _levelUpEffect = Instantiate(levelUpEffect).GetComponent<ParticleSystem>();
        AddAffect(_levelUpEffect);
        Destroy(_levelUpEffect.gameObject, 3f);
        myMiniRankUI?.UpdateTxt();
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

using Complete;
using MasterServerToolkit.Bridges;
using MasterServerToolkit.Bridges.MirrorNetworking;
using MasterServerToolkit.MasterServer;
using Mirror;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public RoomPlayer roomPlayer;
    public GameUI playerUI;
    public TankHealth health;
    public TankMovement movoment;
    public TextMeshProUGUI playerNameUI;
    public CameraController myCamera;

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
        transform.position = pos;
        transform.rotation = rot;
        SetVisable(true);
    }



}

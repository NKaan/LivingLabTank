using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public Slider playerFuelSlider;

    public PlayerMiniUI playerMiniUI;
    public Transform rightPanelContent;
    public Transform leftPanelContent;

    public void AddPlayerMiniUI(Player batsman, Player hitPlayer)
    {
        PlayerMiniUI _playerMiniUI = Instantiate(playerMiniUI, rightPanelContent);
        _playerMiniUI.SetUI(batsman.playerName + "->" + hitPlayer.playerName);
        _playerMiniUI.transform.SetAsFirstSibling();

    }
   
}

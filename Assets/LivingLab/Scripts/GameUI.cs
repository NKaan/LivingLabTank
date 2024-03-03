using SL.Wait;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public Slider playerFuelSlider;
    public TextMeshProUGUI playerFuelTxt;

    public PlayerMiniUI playerMiniUI;
    public PlayerRankMiniUI playerRankMiniUI;
    public Transform rightPanelContent;
    public Transform leftPanelContent;
    public Transform leftUpPanel;

    public List<PlayerRankMiniUI> rankUIList;

    private void Awake()
    {
        rankUIList = new List<PlayerRankMiniUI>();

        Wait.Seconds(2, () => 
        {
            if(rankUIList.Count > 0)
            {
                foreach (var item in rankUIList.OrderBy(x => x.myPlayer.playerPoint))
                    item.transform.SetAsFirstSibling();
            }
            
        }).Repeat(0).Start();

    }

    public void AddPlayerMiniUI(Player batsman, Player hitPlayer)
    {
        PlayerMiniUI _playerMiniUI = Instantiate(playerMiniUI, rightPanelContent);
        _playerMiniUI.SetUI(batsman.playerName + "->" + hitPlayer.playerName);
        _playerMiniUI.transform.SetAsFirstSibling();

    }

    public PlayerRankMiniUI AddPlayerRankMiniUI(Player player)
    {
        PlayerRankMiniUI rankUI = Instantiate(playerRankMiniUI, leftUpPanel);
        rankUI.SetUI(player);
        player.myMiniRankUI = rankUI;
        rankUIList.Add(rankUI);
        return rankUI;
    }

    public void DeleteRankUIPlayer(Player player)
    {
        rankUIList.Remove(player.myMiniRankUI);
        Destroy(player.myMiniRankUI.gameObject);
    }
   
}

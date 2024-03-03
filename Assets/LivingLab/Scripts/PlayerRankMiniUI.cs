using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerRankMiniUI : MonoBehaviour
{
    public TextMeshProUGUI playerNameTxt;
    public Player myPlayer;

    public PlayerRankMiniUI SetUI(Player player)
    {
        myPlayer = player;
        playerNameTxt.text = "Lv." + myPlayer.playerLevel + " " + myPlayer.playerName + " (" + myPlayer.playerPoint + ")";
        return this;
    }

    public void UpdateTxt()
    {
        if(myPlayer != null)
        {
            playerNameTxt.text = "Lv." + myPlayer.playerLevel + " " + myPlayer.playerName + " (" + myPlayer.playerPoint + ")";
        }
    }

}

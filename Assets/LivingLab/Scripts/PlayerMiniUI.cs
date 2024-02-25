using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMiniUI : MonoBehaviour
{

    public TextMeshProUGUI shotText;
    Animation createAnim;


    private void Awake()
    {
        createAnim = GetComponent<Animation>();
    }

    public void SetUI(string _shotText)
    {
        shotText.text = _shotText;
        Destroy(gameObject, 5f);

    }


}

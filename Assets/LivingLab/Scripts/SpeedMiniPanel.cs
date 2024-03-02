using SL.Wait;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedMiniPanel : MonoBehaviour
{
    public TextMeshProUGUI nameTxt;
    public Slider progressSlider;


    public void SetUI(ICollectableObject collectableObject)
    {

        CollectableSpeed collectableSpeed = collectableObject as CollectableSpeed;

        if (collectableSpeed != null)
        {
            nameTxt.text = collectableSpeed.collectableObjectName;
            progressSlider.maxValue = collectableSpeed.endTime;
            progressSlider.value = collectableSpeed.endTime;

            Wait.Seconds(1, () => { progressSlider.value -= 1; })
                .Repeat(collectableSpeed.endTime)
                .Chain(()=>Destroy(gameObject))
                .Start();
        }
    }

}

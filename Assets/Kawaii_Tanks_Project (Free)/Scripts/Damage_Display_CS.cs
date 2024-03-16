using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ChobiAssets.KTP
{

    public class Damage_Display_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Text_Damage" prefab.
		 * The appearance and position of the text are controlled by this script.
		 * This script works in combination with the "Damage_Control_CS".
		*/

        [Header("Display settings")]
        [Tooltip("Upper offset for displaying the value.")] public float offset = 256.0f;
        [Tooltip("Displaying time.")] public float time = 1.5f;


        Transform thisTransform;
        Text thisText;

        // Set by "Damage_Control_CS".
        [HideInInspector] public Damage_Control_CS damageControlScript;


        void Start()
        {
            thisTransform = GetComponent<Transform>();
            thisText = GetComponent<Text>();
            thisText.enabled = false;
        }


        public void Get_Damage(float durability, float initialDurability)
        { // Called from "Damage_Control_CS".
            thisText.text = Mathf.Ceil(durability) + "/" + initialDurability;
            StartCoroutine("Display");
        }


        int displayingID;
        IEnumerator Display()
        {
            displayingID += 1;
            int thisID = displayingID;

            Color currentColor = thisText.color;
            float count = 0.0f;
            while (count < time)
            {
                if (thisID < displayingID)
                {
                    yield break;
                }

                if (damageControlScript && damageControlScript.bodyTransform)
                {
                    Set_Position();
                }
                else
                {
                    break;
                }

                // Control the color alpha.
                currentColor.a = Mathf.Lerp(1.0f, 0.0f, count / time);
                thisText.color = currentColor;

                count += Time.deltaTime;
                yield return null;
            }

            displayingID = 0;
            thisText.enabled = false;
        }


        Vector3 zoomScale = new Vector3(2.0f, 2.0f, 2.0f);
        void Set_Position()
        {
            float lodValue = 2.0f * Vector3.Distance(damageControlScript.bodyTransform.position, Camera.main.transform.position) * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
            Vector3 currentPos = Camera.main.WorldToScreenPoint(damageControlScript.bodyTransform.position);
            if (currentPos.z > 0.0f)
            { // Front of the camera.
                thisText.enabled = true;
                currentPos.z = 128.0f;
                currentPos.y += (5.0f / lodValue) * offset;
                thisTransform.position = currentPos;
                thisTransform.localScale = Vector3.one;
            }
            else
            { // Behind of the camera.
                if (damageControlScript.isSelected)
                { // The player is using the gun camera.
                    thisText.enabled = true;
                    currentPos.z = 128.0f;
                    currentPos.y = 100.0f;
                    thisTransform.position = currentPos;
                    thisTransform.localScale = zoomScale;
                }
                else
                {
                    thisText.enabled = false;
                }
            }
        }

    }

}
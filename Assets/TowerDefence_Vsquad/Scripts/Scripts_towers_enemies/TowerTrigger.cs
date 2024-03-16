using UnityEngine;
using System.Collections;

public class TowerTrigger : MonoBehaviour {

    public Tower twr;
    public bool lockE;
    public GameObject curTarget;



    void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("enemyBug") || other.CompareTag("Tank"))) {
            if (lockE == false)
            {
                twr.target = other.gameObject.transform;
                curTarget = other.gameObject;
                lockE = true;
            }

        }
    }
        void Update()
        {
            if (curTarget)
            {
                if (curTarget.CompareTag("Dead")) // get it from EnemyHealth
                {
                
                lockE = false;                
                twr.target = null;               
                
                }
            }




            if (!curTarget)
            {
                lockE = false;
            }
        }
        void OnTriggerExit(Collider other)
        {
            if ((other.CompareTag("enemyBug") || other.CompareTag("Tank"))) {
                if (other.gameObject == curTarget)
                {

                    lockE = false;
                    twr.target = null;               
            }
            }
        }
    }

	


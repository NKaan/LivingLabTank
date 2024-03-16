using UnityEngine;
using System.Collections;

public class TowerBullet : MonoBehaviour {

    public float Speed;
    public Transform target;
    public GameObject impactParticle; // bullet impact
    
    public Vector3 impactNormal; 
    Vector3 lastBulletPosition; 
    public Tower twr; 


    // destroy bullet when get to the target, instantiate hit FX
    void hit()
    {
        Destroy(gameObject);
        impactParticle = Instantiate(impactParticle, target.transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
        if (target)
        {
            impactParticle.transform.parent = target.transform;
        }
        Destroy(impactParticle, 3);
        return;
    }




    void Update() {

        // Bullet move

        if (target)
        {

            transform.LookAt(target);
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * Speed);
            lastBulletPosition = target.transform.position;

            if (transform.position == target.position)
            {
                hit();
            }
        }

        // Move bullet ( enemy was disapeared )

        else

        {


            transform.position = Vector3.MoveTowards(transform.position, lastBulletPosition, Time.deltaTime * Speed);

            if (transform.position == lastBulletPosition)
            {

                hit();

            }
        }     
    }

    // Bullet hit

    void OnTriggerEnter(Collider other) // tower`s hit if bullet reached the enemy
        {
            if (other.gameObject.transform == target)
            {

                if (target.CompareTag("enemyBug"))
                {

                    target.GetComponent<EnemyHp>().Dmg(twr.dmg);
                hit();
            }


                if (other.gameObject.CompareTag("Tank"))
                {
                    target.GetComponent<TowerHP>().Dmg_2(twr.dmg);
                hit();
            }
            }



        
    }
 
}




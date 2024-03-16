using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTank : MonoBehaviour
{


    public GameObject impactParticle; // bullet impact  
    public Transform ShootElement;    
    public int TankDamage;
    

    private void Start()
    {
        TankDamage = 10;
    }

    void Hit_2 ()

    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ShootElement.transform.position, ShootElement.transform.forward, 1000.0f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.tag == "Building")
            {
                Destroy(gameObject); // destroy bullet
                impactParticle = Instantiate(impactParticle, hits[i].point, Quaternion.FromToRotation(Vector3.up, hits[i].normal)) as GameObject;
                Destroy(impactParticle, 2);                
            }

            if (hits[i].collider.gameObject.tag == "Castle")
            {                
                Destroy(gameObject); // destroy bullet
                impactParticle = Instantiate(impactParticle, hits[i].point, Quaternion.FromToRotation(Vector3.up, hits[i].normal)) as GameObject;
                Destroy(impactParticle, 2);
                hits[i].collider.gameObject.GetComponent<TowerHP>().Dmg_2(TankDamage);
            }
            if (hits[i].collider.gameObject.tag == "enemyBug")
            {
                Destroy(gameObject); // destroy bullet
                impactParticle = Instantiate(impactParticle, hits[i].point, Quaternion.FromToRotation(Vector3.up, hits[i].normal)) as GameObject;
                Destroy(impactParticle, 2);
                hits[i].collider.gameObject.GetComponent<EnemyHp>().Dmg(TankDamage);
            }

        }
    }

    
   
    // Bullet hit

    void OnTriggerEnter(Collider other)

    {
        if (other.CompareTag("Castle") || other.CompareTag("enemyBug") || other.CompareTag("Building"))
        {
            Hit_2();

        }

    }

    
}



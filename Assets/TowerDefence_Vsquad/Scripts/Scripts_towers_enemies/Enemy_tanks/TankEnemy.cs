using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemy : MonoBehaviour {


    public Transform shootElement;
    public GameObject bullet;
    public GameObject DestroyParticle;
    public GameObject Enemybug;
    public int Creature_Damage = 10;
    public float Speed;
    // 
    public float shootDelay;
    bool isShoot;
    public Transform[] waypoints;
    int curWaypointIndex = 0;
    public float previous_Speed;    
    public EnemyHp Enemy_Hp;
    public Transform target;
    public GameObject EnemyTarget;
    public ParticleSystem[] ShootFX;
    public Vector3 impactNormal_2;

    void MoveAgain() // Stop to shoot and move again
    {
        Speed = previous_Speed;
        EnemyTarget = null;
        target = null;
        transform.LookAt(waypoints[curWaypointIndex].position);
    }



    void Start()
    {        
        Enemy_Hp = Enemybug.GetComponent<EnemyHp>();
        previous_Speed = Speed;
    }

    // Attack

    void OnTriggerEnter(Collider other)

    {
        if (other.tag == "Tank")
        {
            Speed = 0;
            EnemyTarget = other.gameObject;
            target = other.gameObject.transform;
            Vector3 targetPosition = new Vector3(EnemyTarget.transform.position.x, transform.position.y, EnemyTarget.transform.position.z);
            transform.LookAt(targetPosition);

        }
       
    }

    void OnTriggerExit(Collider other)

    {
        if (other.tag == "Tank")
        {

            MoveAgain();

        }

    }



    void Update()
    {


        // Shooting

        if (target)
        {

            if (!isShoot)
            {
                StartCoroutine(shoot());

            }

            if (target.CompareTag("Castle_Destroyed")) 
            {

                MoveAgain();

            }
        }


        // MOVING

        if (curWaypointIndex < waypoints.Length)
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[curWaypointIndex].position, Time.deltaTime * Speed);

            if (!EnemyTarget)
            {
                transform.LookAt(waypoints[curWaypointIndex].position);
            }

            if (Vector3.Distance(transform.position, waypoints[curWaypointIndex].position) < 0.5f)
            {
                curWaypointIndex++;
            }
        }
        

        // DEATH

        if (Enemy_Hp.EnemyHP <= 0)
        {
            Speed = 0;
            Destroy(gameObject);
            DestroyParticle = Instantiate(DestroyParticle, Enemybug.transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal_2)) as GameObject;
            Destroy(DestroyParticle, 3);
        }

       
    }

    IEnumerator shoot()
    {
        isShoot = true;
        yield return new WaitForSeconds(shootDelay);


        if (target)
        {
            GameObject b = GameObject.Instantiate(bullet, shootElement.position, Quaternion.identity) as GameObject;
            b.GetComponent<TankEnemyBullet>().target = target;            

            // ShootFX
            for (int i = 0; i < ShootFX.Length; i++)
            {
                ShootFX[i].Play();
            }



        }       

        isShoot = false;

    }


}


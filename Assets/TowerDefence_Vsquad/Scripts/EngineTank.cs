using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EngineTank : MonoBehaviour
{

    //скорость движения танка
    float MoveSpeed = 2;
    //скорость поворота танка
    float RotateSpeed = 60;
    //получаем компонент движения танка 
    Rigidbody TankEngine;
    public GameObject Tower;
    
    public Transform[] shootElement;    
    public GameObject bullet; 
    public ParticleSystem[] ShootFX;
    public GameObject DestroyParticle;
    public GameObject Towerbug;
    public TowerHP TowerHp;
    public Camera cam;
    public Vector3 impactNormal_2;    
    public GameObject[] Meshes;
    private bool Check; // check when health = 0 and make tank invisible    
    private int n;



    void Start()
    {
        n = 0;     
        Check = false;
        //получаем компонент движения танка 
        TankEngine = GetComponent<Rigidbody>();
        TowerHp = Towerbug.GetComponent<TowerHP>();
        

    }


    private void Update()
    {




        // Destroy

        if ((TowerHp.CastleHp <= 0) && (Check == false))
        {
            for (int i = 0; i < Meshes.Length; i++)
            {
                Meshes[i].SetActive(false);
            }
            
            //Destroy(gameObject);
            DestroyParticle = Instantiate(DestroyParticle, Towerbug.transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal_2)) as GameObject;
            Destroy(DestroyParticle, 3);
            Check = true;
        }


        //if (n >= shootElement.Length) { n = 1; } // отправляем снаряд по кругу

    }


    void Move()
    {
        //расчитываем куда будет двигаться танг
        Vector3 Move = transform.forward * Input.GetAxis("Vertical") * MoveSpeed * Time.deltaTime;
        //получаем ткущую позицию танка 
        Vector3 Poze = TankEngine.position + Move;
        //пробуем двигаться в указанном направлении
        TankEngine.MovePosition(Poze);
    }

    void Rotates()
    {
        //расчитываем поворот
        float R = Input.GetAxis("Horizontal") * RotateSpeed * Time.deltaTime;

        //создаем новый угол поворота танка 
        Quaternion RotateAngle = Quaternion.Euler(0, R, 0);

        //получим текущий угол поворота танка 
        Quaternion CurrentUgol = TankEngine.rotation * RotateAngle;

        //поворачиваем танк
        TankEngine.MoveRotation(CurrentUgol);

    }   


        //поворот башни
        void RotatesTower()
    {
        // очень сложный кусок - генерация луча, потом создание какого то плэйна...
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            Vector3 target = ray.GetPoint(distance);
            Vector3 direction = target - transform.position;
            float rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Tower.transform.rotation = Quaternion.Euler(0, rotation, 0);
        }


    }


    void Fire()
    {
        //
        //включаем управление через переменную 

        if (Input.GetButtonUp("Fire1"))

        {
            

            for (int i = 0; i < ShootFX.Length; i++)
            {
                ShootFX[i].Play();
            }


            if ((n >= 0) && (n <= shootElement.Length+1))
            {
                //создаем угол поворота для старта снаряда
                Quaternion SpawnRoot = Tower.transform.rotation;


                //создаем снаряд
                GameObject bullet_tank = GameObject.Instantiate(bullet, shootElement[n].position, SpawnRoot) as GameObject;
                bullet_tank.GetComponent<BulletTank>().ShootElement = shootElement[n];

                //пробуем получить компонент 
                Rigidbody bullet_rigidbody = bullet_tank.GetComponent<Rigidbody>();

                //придаем ускорение снаряду
                bullet_rigidbody.AddForce(bullet_tank.transform.forward * 20, ForceMode.Impulse);

                //удаляем партрон через 5  секунд 
                Destroy(bullet_tank, 5);
                //после того как танк выстрелил отключаем выстрел 
                n = n + 1;
                if (n >= shootElement.Length) { n = 0; } // отправляем снаряд по кругу

            }

            


        }

    }

    //shootElement.transform.TransformDirection(Vector3.forward)


    void FixedUpdate()
    {

        // атака
        Fire();
        //двигаем танк
        Move();
        //поворачиваем танк 
        Rotates();
        //вызов поворота башни
        RotatesTower();
    }

     
}
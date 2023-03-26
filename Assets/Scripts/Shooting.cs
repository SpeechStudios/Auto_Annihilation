using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    //public GameObject Targeter;
    public LayerMask WeaponHitMask, Enemymask;
    [Header("Targeting")]
    public GameObject TargeterPrefab;
    public Transform Targeter_Start;
    public float Targeter_Hit_Radius = 1f;
    public float Targeter_Range = 25f;
    [Space]

    public Cannon MyCannon;
    [System.Serializable]
    public class Cannon
    {
        public bool EnableCannon;
        public float CannonDamage;
        public float CannonFireRate;
        public Transform CannonTransform;
        public MeshRenderer AvailableCannon;
        public GameObject CannonExplosion;
        public ParticleSystem CannonFire;
        [HideInInspector] public float CannonTimer;
    }

    public MiniGun MyMinigun;
    [System.Serializable]
    public class MiniGun
    {
        public bool EnableMiniGun;
        public Transform MiniGun1;
        public Transform MiniGun2;
        public float MiniGunDamage;
        public float MiniGunFireRate;
        public ParticleSystem MiniGunFlash1, MiniGunFlash2;
        [HideInInspector] public readonly float PS_EmissionRate = 25;
        [HideInInspector] public float MiniGunTimer;
        [HideInInspector] public float MiniGunStartFireRate = 1;
    }
    public RocketLauncher MyRocket;
    [System.Serializable]
    public class RocketLauncher 
    {
        public bool EnableRocket;
        public GameObject RocketTargeterPrefab;
        public GameObject RocketLauncherPrefab1;
        public GameObject RocketLauncherPrefab2;
        public GameObject Rocket;
        public List<MeshRenderer> LeftAvailableRockets;
        public List<MeshRenderer> RightAvailableRockets;
        public float RocketDamage;
        public float FireRate;
        public float RocketRefreshRate;
        [HideInInspector] public float RocketSpeed = 40;
        [HideInInspector] public int AvailableRockets = 8;
        [HideInInspector] public float RocketTimer;
        [HideInInspector] public float RocketRefreshTimer;
    }

    private void Start()
    {
        MyRocket.RocketTargeterPrefab.transform.position = Targeter_Start.transform.position + Targeter_Start.transform.forward.normalized * Targeter_Range;
        MyRocket.AvailableRockets = 8;
        MyCannon.CannonTimer = MyCannon.CannonFireRate;
        MyMinigun.MiniGunTimer = MyMinigun.MiniGunFireRate;
    }
    // Update is called once per frame
    void Update()
    {
        Targeter();

        if(MyCannon.EnableCannon)
        ShootCannon();
        if(MyMinigun.EnableMiniGun)
        ShootMiniGuns();
        if (MyRocket.EnableRocket)
        ShootRockets();
    }
    void Targeter()
    {
        Targeter_Start.localRotation = Quaternion.Euler(Camera.main.transform.localRotation.eulerAngles.x - 20, Camera.main.transform.localRotation.eulerAngles.y, 0);
        var pos = Targeter_Start.transform.position + Targeter_Start.transform.forward.normalized * Targeter_Range;
        TargeterPrefab.transform.position = pos;
        FindClosestEnemy();
    }
    void FindClosestEnemy()
    {
        RaycastHit[] hits = Physics.SphereCastAll(Targeter_Start.position, Targeter_Hit_Radius, Targeter_Start.transform.forward, Targeter_Range, Enemymask);

        float closestdist = 100;
        Transform closestEnemy = null;
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                float dist = Vector2.Distance(Targeter_Start.position, hits[i].transform.position);
                if (dist < closestdist)
                {
                    closestdist = dist;
                    closestEnemy = hits[i].transform;
                }
            }
        }
        else
        {
            closestEnemy = null;
        }
        VehicleCamera.ClosestEnemy = closestEnemy;
    }
    void ShootCannon()
    {
        MyCannon.CannonTransform.localRotation = Quaternion.Euler(Camera.main.transform.localRotation.eulerAngles.x - 20, Camera.main.transform.localRotation.eulerAngles.y, 0);
        MyCannon.CannonTimer += Time.deltaTime;
        if (Input.GetButton("Fire1"))
        {
            RaycastHit rayhit;
            if (Physics.Raycast(Targeter_Start.position, Targeter_Start.transform.forward, out rayhit, Targeter_Range, WeaponHitMask))
            {
                if (MyCannon.CannonTimer > MyCannon.CannonFireRate)
                {
                    MyCannon.CannonTransform.GetComponent<AudioSource>().Play();
                    MyCannon.CannonTransform.GetComponent<Animation>().Play();
                    MyCannon.CannonFire.Play();
                    GameObject explosion = Instantiate(MyCannon.CannonExplosion, rayhit.point, MyCannon.CannonTransform.localRotation);
                    explosion.GetComponent<ExplosionScript>().Damage = MyCannon.CannonDamage;
                    explosion.GetComponent<ExplosionScript>().dmgType = DamageType.Cannon;
                    MyCannon.CannonTimer = 0;
                }
            }
        }
        MyCannon.AvailableCannon.material.color = Color.red;
        if(MyCannon.CannonTimer > MyCannon.CannonFireRate)
        {
            MyCannon.AvailableCannon.material.color = Color.green;
        }
    }
    void ShootMiniGuns()
    {
        MyMinigun.MiniGun1.localRotation = Quaternion.Euler(Camera.main.transform.localRotation.eulerAngles.x - 20, Camera.main.transform.localRotation.eulerAngles.y, -90);
        MyMinigun.MiniGun2.localRotation = Quaternion.Euler(Camera.main.transform.localRotation.eulerAngles.x - 20, Camera.main.transform.localRotation.eulerAngles.y, -90);
        MyMinigun.MiniGunTimer += Time.deltaTime;
        RaycastHit[] rayHit = Physics.RaycastAll(Targeter_Start.position, Targeter_Start.transform.forward, Targeter_Range, Enemymask);
        RaycastHit[] rayHit2 = Physics.RaycastAll(Targeter_Start.position, Targeter_Start.transform.forward, Targeter_Range, Enemymask);
        MiniGunFX();
        if (Input.GetButton("Fire1"))
        {
            if (MyMinigun.MiniGunTimer > MyMinigun.MiniGunStartFireRate)
            {
                FireMiniGun(rayHit);
                FireMiniGun(rayHit2);
                MyMinigun.MiniGunTimer = 0;
                MyMinigun.MiniGunFlash1.Play();
                MyMinigun.MiniGunFlash2.Play();
            }
        }
        HighlightTargets(rayHit);
        HighlightTargets(rayHit2);
    }

    void ShootRockets()
    {
        MyRocket.RocketTimer += Time.deltaTime;
        MyRocket.RocketRefreshTimer += Time.deltaTime;
        if(Input.GetButton("Fire1"))
        {
            if (MyRocket.RocketTimer > MyRocket.FireRate && MyRocket.AvailableRockets > 0)
            {
                LaunchRocket(MyRocket.RocketLauncherPrefab1.transform, -1);
                LaunchRocket(MyRocket.RocketLauncherPrefab2.transform);
                MyRocket.AvailableRockets--;
                MyRocket.RocketTimer = 0;
                MyRocket.RocketRefreshTimer = 0;
                CheckAvailableRockets();

            }
        }
        if(MyRocket.RocketRefreshTimer > MyRocket.RocketRefreshRate)
        {
            MyRocket.AvailableRockets = 8;
            CheckAvailableRockets();
        }

    }
    void CheckAvailableRockets()
    {
        for (int i = 0; i < MyRocket.LeftAvailableRockets.Count; i++)
        {
            MyRocket.LeftAvailableRockets[i].material.color = Color.red;
            MyRocket.RightAvailableRockets[i].material.color = Color.red;
        }
        for (int i = 0; i < MyRocket.AvailableRockets; i++)
        {
            MyRocket.LeftAvailableRockets[i].material.color = Color.green;
            MyRocket.RightAvailableRockets[i].material.color = Color.green;
        }
    }
    void LaunchRocket(Transform LaunchPosition, int Invert = 1)
    {
        Quaternion RandomRotation = LaunchPosition.transform.rotation * Quaternion.Euler(Random.Range(10f * Invert, 5f * Invert), 0, 0);
        GameObject rocket = Instantiate(MyRocket.Rocket, LaunchPosition.transform.position, RandomRotation);
        Vector3 newPostion = MyRocket.RocketTargeterPrefab.transform.position + (LaunchPosition.forward.normalized * (23 + Camera.main.GetComponent<VehicleCamera>().rotation.y) * 3);
        Vector3 targetPosition = new Vector3(newPostion.x, 0.0f, newPostion.z);
        Rocket theRocket = rocket.GetComponent<Rocket>();
        theRocket.Target = targetPosition;
        theRocket.RocketSpeed = MyRocket.RocketSpeed;
        theRocket.ExplosionDamage = MyRocket.RocketDamage;
        //theRocket.WeaponHitMask = WeaponHitMask;
    }
    void MiniGunFX()
    {
        if (Input.GetButton("Fire1"))
        {
            //Initilize VFX and SFX
            if (!MyMinigun.MiniGun1.GetComponent<AudioSource>().isPlaying)
            {
                MyMinigun.MiniGun1.GetComponent<AudioSource>().Play();
                MyMinigun.MiniGun1.GetComponentInChildren<ParticleSystem>().Play();
                MyMinigun.MiniGun2.GetComponentInChildren<ParticleSystem>().Play();
            }

            //Update VFX and SFX for minigun windup
            if (MyMinigun.MiniGunStartFireRate > MyMinigun.MiniGunFireRate)
            {
                MyMinigun.MiniGun1.GetComponent<AudioSource>().pitch = (1 - MyMinigun.MiniGunStartFireRate) * 1.5f;
                MyMinigun.MiniGunStartFireRate -= Time.deltaTime;

                var ps1 = MyMinigun.MiniGun1.GetComponentInChildren<ParticleSystem>().emission;
                ps1.rateOverTime = (1 - MyMinigun.MiniGunStartFireRate) * MyMinigun.PS_EmissionRate;
                var ps2 = MyMinigun.MiniGun2.GetComponentInChildren<ParticleSystem>().emission;
                ps2.rateOverTime = (1 - MyMinigun.MiniGunStartFireRate) * MyMinigun.PS_EmissionRate;
            }
        }
        else
        {
            if (MyMinigun.MiniGun1.GetComponent<AudioSource>().isPlaying)
            {
                MyMinigun.MiniGun1.GetComponent<AudioSource>().Stop();
                MyMinigun.MiniGun1.GetComponentInChildren<ParticleSystem>().Stop();
                MyMinigun.MiniGun2.GetComponentInChildren<ParticleSystem>().Stop();
                MyMinigun.MiniGunFlash1.Stop();
                MyMinigun.MiniGunFlash2.Stop();
            }
            if (MyMinigun.MiniGunStartFireRate < 1)
            {
                MyMinigun.MiniGunStartFireRate += Time.deltaTime;
            }
        }
    }
    void FireMiniGun(RaycastHit[] rayHit)
    {
        for (int i = 0; i < rayHit.Length; i++)
        {
            RaycastHit hit = rayHit[i];
            IDamageable dmg = hit.collider.transform.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.Damage(MyMinigun.MiniGunDamage, DamageType.MiniGun);
            }
        }
    }
    void HighlightTargets(RaycastHit[] rayHit)
    {
        for (int i = 0; i < rayHit.Length; i++)
        {
            RaycastHit hit = rayHit[i];
            ITargetable target = hit.collider.transform.GetComponent<ITargetable>();
            if (target != null)
            {
                target.Highlight();
            }
        }
    }

}

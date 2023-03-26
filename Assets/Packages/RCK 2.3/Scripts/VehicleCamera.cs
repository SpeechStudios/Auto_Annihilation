using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
public class VehicleCamera : MonoBehaviour
{



    public Transform target;
    public float smooth = 0.3f;
    public float distance = 5.0f;
    public float height = 1.0f;
    public float Angle = 20;


    public float Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }
    [Range(0.1f, 9f)] [SerializeField] float sensitivity = 2f;



    [HideInInspector] public Vector2 rotation = Vector2.zero;
    const string xAxis = "Mouse X"; //Strings in direct code generate garbage, storing and re-using them creates no garbage
    const string yAxis = "Mouse Y";

    public List<Transform> cameraSwitchView;
    public LayerMask lineOfSightMask = 0;

    public CarUIClass CarUI;



    private float yVelocity = 0.0f;
    private float xVelocity = 0.0f;
    [HideInInspector]
    public int Switch;

    private int gearst = 0;
    private float thisAngle = -150;
    private float restTime = 0.0f;


    private Rigidbody myRigidbody;



    private VehicleControl carScript;
    public float rotationSensitivity;
    public static Transform ClosestEnemy;



    [System.Serializable]
    public class CarUIClass
    {

        public Image tachometerNeedle;
        public Slider Nitros;
        public Slider Health;

        public Text speedText;
        public Text GearText;

    }


    


    ////////////////////////////////////////////// TouchMode (Control) ////////////////////////////////////////////////////////////////////




    public void CameraSwitch()
    {
        Switch++;
        if (Switch > cameraSwitchView.Count) { Switch = 0; }
    }


    public void CarAccelForward(float amount)
    {
        carScript.accelFwd = amount;
    }

    public void CarAccelBack(float amount)
    {
        carScript.accelBack = amount;
    }

    public void CarSteer(float amount)
    {
        carScript.steerAmount = amount;
    }

    public void CarHandBrake(bool HBrakeing)
    {
        carScript.brake = HBrakeing;
    }

    public void CarShift(bool Shifting)
    {
        carScript.shift = Shifting;
    }



    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    

    public void RestCar()
    {

        if (restTime == 0)
        {
            myRigidbody.AddForce(Vector3.up * 50000);
            myRigidbody.MoveRotation(Quaternion.Euler(0, transform.eulerAngles.y, 0));
            restTime = 2.0f;
        }

    }




    public void ShowCarUI()
    {



        gearst = carScript.currentGear;
        CarUI.speedText.text = ((int)carScript.speed).ToString();




        if (carScript.carSetting.automaticGear)
        {

            if (gearst > 0 && carScript.speed > 1)
            {
                CarUI.GearText.color = Color.green;
                CarUI.GearText.text = gearst.ToString();
            }
            else if (carScript.speed > 1)
            {
                CarUI.GearText.color = Color.red;
                CarUI.GearText.text = "R";
            }
            else
            {
                CarUI.GearText.color = Color.white;
                CarUI.GearText.text = "N";
            }

        }
        else
        {

            if (carScript.NeutralGear)
            {
                CarUI.GearText.color = Color.white;
                CarUI.GearText.text = "N";
            }
            else
            {
                if (carScript.currentGear != 0)
                {
                    CarUI.GearText.color = Color.green;
                    CarUI.GearText.text = gearst.ToString();
                }
                else
                {

                    CarUI.GearText.color = Color.red;
                    CarUI.GearText.text = "R";
                }
            }

        }





        thisAngle = (carScript.motorRPM / 20) - 175;
        thisAngle = Mathf.Clamp(thisAngle, -180, 90);

        CarUI.tachometerNeedle.rectTransform.rotation = Quaternion.Euler(0, 0, -thisAngle);
        CarUI.Nitros.value = carScript.NitrosAmount / carScript.carSetting.MaxNitros;
        CarUI.Health.value = carScript.currentHealth / carScript.MaxHealth;

    }



    void Start()
    {

        carScript = (VehicleControl)target.GetComponent<VehicleControl>();

        myRigidbody = target.GetComponent<Rigidbody>();

        cameraSwitchView = carScript.carSetting.cameraSwitchView;

        rotation = new Vector2(0, -20);
    }




    void Update()
    {

        if (!target) return;



        if (Input.GetKeyDown(KeyCode.G))
        {
            RestCar();
        }

        if (restTime!=0.0f)
        restTime=Mathf.MoveTowards(restTime ,0.0f,Time.deltaTime);




        ShowCarUI();

        GetComponent<Camera>().fieldOfView = Mathf.Clamp(carScript.speed / 10.0f + 60.0f, 60, 90.0f);



        if (Input.GetKeyDown(KeyCode.C))
        {
            Switch++;
            if (Switch > cameraSwitchView.Count) { Switch = 0; }
        }



        if (Switch == 0)
        {
            // Damp angle from current y-angle towards target y-angle

            float xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x,
           target.eulerAngles.x + Angle, ref xVelocity, smooth);

            float yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
            target.eulerAngles.y, ref yVelocity, smooth);

            // Look at the target
            transform.eulerAngles = new Vector3(xAngle, yAngle,0.0f);

            var direction = transform.rotation * -Vector3.forward;
            var targetDistance = AdjustLineOfSight(target.position + new Vector3(0, height, 0), direction);


            transform.position = target.position + new Vector3(0, height, 0) + direction * targetDistance;
            if (Input.GetButton("Fire2"))
            {
                rotation.x += Input.GetAxis(xAxis) * sensitivity;
                rotation.y += Input.GetAxis(yAxis) * sensitivity;
                rotation.y = Mathf.Clamp(rotation.y, -23f, -5f);
                var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
                var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
                Quaternion targetRotation = xQuat * yQuat;
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, rotationSensitivity * Time.deltaTime);
                AimAssist();
            }
            if(Input.GetButtonUp("Fire2"))
            {
                rotation = new Vector2(0, -20);
            }

        }
        else
        {

            transform.position = cameraSwitchView[Switch - 1].position;

        }

    }


    float AdjustLineOfSight(Vector3 target, Vector3 direction)
    {


        RaycastHit hit;

        if (Physics.Raycast(target, direction, out hit, distance, lineOfSightMask.value))
            return hit.distance;
        else
            return distance;

    }
    public Aimbot aimbot;
    void AimAssist()
    {
        if(ClosestEnemy)
        {
            //var rotationChanges = aimbot.TrackTarget();
            //Debug.Log(rotationChanges.TurnAddition * 10);
            //transform.Rotate(rotationChanges.TurnAddition * 100);

            //Vector3 rot = new Vector3(ClosestEnemy.position.x, ClosestEnemy.position.y - 6f, ClosestEnemy.position.z);
            //Quaternion newRoation = transform.localRotation * Quaternion.Euler(rotationChanges.TurnAddition);
            //Quaternion lookOnLook = Quaternion.LookRotation(rot);
            //transform.localRotation = Quaternion.Slerp(transform.localRotation, lookOnLook, rotationSensitivity * Time.deltaTime);
            //transform.localRotation = lookOnLook;

        }

    }

}

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
    public LayerMask lineOfSightMask = 0;
    private float yVelocity = 0.0f;
    private float xVelocity = 0.0f;
    [HideInInspector]
    private int gearst = 0;
    private float thisAngle = -150;
    private float restTime = 0.0f;
    private Rigidbody myRigidbody;
    private VehicleControl carScript;
    public float rotationSensitivity;
    public static Transform ClosestEnemy;
    public CarUIClass CarUI;
    [System.Serializable]
    public class CarUIClass
    {

        public Image tachometerNeedle;
        public Slider Nitros;
        public Slider Health;

        public Text speedText;
        public Text GearText;

    }

    void Start()
    {
        carScript = target.GetComponent<VehicleControl>();
        myRigidbody = target.GetComponent<Rigidbody>();
        rotation = new Vector2(0, -20);
    }
    void Update()
    {
        if (!target) return;
        ShowCarUI();

        HandleCamera();
        if (Input.GetKeyDown(KeyCode.G))
        {
            ResetCar();
        }

        if (restTime != 0f)
        {
            restTime = Mathf.MoveTowards(restTime, 0f, Time.deltaTime);
        }
    }

    public void ResetCar()
    {
        if (restTime == 0)
        {
            myRigidbody.AddForce(Vector3.up * 50000);
            myRigidbody.MoveRotation(Quaternion.Euler(0, transform.eulerAngles.y, 0));
            restTime = 2f;
        }
    }
    void ShowCarUI()
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
    void HandleCamera()
    {
        GetComponent<Camera>().fieldOfView = Mathf.Clamp(carScript.speed / 10.0f + 60.0f, 60, 90.0f);
        float xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x,target.eulerAngles.x + Angle, ref xVelocity, smooth);
        float yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,target.eulerAngles.y, ref yVelocity, smooth);

        // Look at the target
        transform.eulerAngles = new Vector3(xAngle, yAngle, 0.0f);
        var direction = transform.rotation * -Vector3.forward;
        var targetDistance = AdjustLineOfSight(target.position + new Vector3(0, height, 0), direction);
        transform.position = target.position + new Vector3(0, height, 0) + direction * targetDistance;

        if (Input.GetButton("Fire2"))
        {
            EnableFreeLook();
        }
        if (Input.GetButtonUp("Fire2"))
        {
            //Disable FreeLook
            rotation = new Vector2(0, -20);
        }
    }
    void EnableFreeLook()
    {
        rotation.x += Input.GetAxis(xAxis) * sensitivity;
        rotation.y += Input.GetAxis(yAxis) * sensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -23f, -5f);
        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
        Quaternion targetRotation = xQuat * yQuat;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, rotationSensitivity * Time.deltaTime);
    }

    float AdjustLineOfSight(Vector3 target, Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(target, direction, out hit, distance, lineOfSightMask.value))
            return hit.distance;
        else
            return distance;
    }
}

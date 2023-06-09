﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ControlMode { simple = 1, touch = 2 }


public class VehicleControl : MonoBehaviour, IDamageable
{

    public GameManager gm;
    public float MaxHealth;
    public float currentHealth;
    public ControlMode controlMode = ControlMode.simple;

    public bool activeControl = false;


    // Wheels Setting /////////////////////////////////

    public CarWheels carWheels;

    [System.Serializable]
    public class CarWheels
    {
        public ConnectWheel wheels;
        public WheelSetting setting;
    }

    [System.Serializable]
    public class ConnectWheel
    {
        public bool frontWheelDrive = true;
        public Transform frontRight;
        public Transform frontLeft;

        public bool backWheelDrive = true;
        public Transform backRight;
        public Transform backLeft;
    }

    [System.Serializable]
    public class WheelSetting
    {
        public float Radius = 0.4f;
        public float Weight = 1000.0f;
        public float Distance = 0.2f;
    }


    // Lights Setting ////////////////////////////////

    public CarLights carLights;

    [System.Serializable]
    public class CarLights
    {
        public Light[] brakeLights;
        public Light[] reverseLights;
    }

    // Car sounds /////////////////////////////////

    public CarSounds carSounds;

    [System.Serializable]
    public class CarSounds
    {
        public AudioSource IdleEngine, LowEngine, HighEngine;

        public AudioSource nitro;
        public AudioSource switchGear;
    }

    // Car Particle /////////////////////////////////

    public CarParticles carParticles;

    [System.Serializable]
    public class CarParticles
    {
        public GameObject brakeParticlePerfab;
        public ParticleSystem shiftParticle1, shiftParticle2;
        public GameObject HitParticle;
        public ParticleSystem InvunrableVFX;
        private GameObject[] wheelParticle = new GameObject[4];
    }

    // Car Engine Setting /////////////////////////////////

    public CarSetting carSetting;

    [System.Serializable]
    public class CarSetting
    {

        public bool showNormalGizmos = false;
        public HitGround[] hitGround;

        public List<Transform> cameraSwitchView;

        public float springs = 25000.0f;
        public float dampers = 1500.0f;

        public float carPower = 120f;
        public float shiftPower = 150f;
        public float brakePower = 8000f;
        public float MaxNitros = 240f;
        public float MinNitrosToStart = 20f;

        public Vector3 shiftCentre = new Vector3(0.0f, -0.8f, 0.0f);

        public float maxSteerAngle = 25.0f;

        public float shiftDownRPM = 1500.0f;
        public float shiftUpRPM = 2500.0f;
        public float idleRPM = 500.0f;

        public float stiffness = 2.0f;

        public bool automaticGear = true;

        public float[] gears = { -10f, 9f, 6f, 4.5f, 3f, 2.5f };


        public float LimitBackwardSpeed = 60.0f;
        public float LimitForwardSpeed = 220.0f;

    }

    [System.Serializable]
    public class HitGround
    { 
        public string tag = "street";
        public bool grounded = false;
        public AudioClip brakeSound;
        public AudioClip groundSound;
        public Color brakeColor;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private float steer = 0;
    private float accel = 0.0f;



    [HideInInspector] public int currentGear = 0;
    [HideInInspector] public float curTorque = 100f;
    [HideInInspector] public float NitrosAmount;
    [HideInInspector] public float speed = 0.0f;
    [HideInInspector] public float motorRPM = 0.0f;
    [HideInInspector] public bool brake;
    [HideInInspector] public bool shift;
    [HideInInspector] public bool NeutralGear = true;
    [HideInInspector] public bool Backward = false;
    [HideInInspector] public bool Invunrable;

    private float lastSpeed = -10.0f;
    private bool shifmotor;

    float[] efficiencyTable = { 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 1.0f, 1.0f, 0.95f, 0.80f, 0.70f, 0.60f, 0.5f, 0.45f, 0.40f, 0.36f, 0.33f, 0.30f, 0.20f, 0.10f, 0.05f };
    float efficiencyTableStep = 250.0f;

    private float Pitch;
    private float PitchDelay;

    private float shiftTime = 0.0f;
    private float shiftDelay = 0.0f;

    ////////////////////////////////////////////// TouchMode (Control) ////////////////////////////////////////////////////////////////////
    [HideInInspector] public float accelFwd = 0.0f;
    [HideInInspector] public float accelBack = 0.0f;
    [HideInInspector] public float steerAmount = 0.0f;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private float wantedRPM = 0.0f;
    private float w_rotate;
    private float slip, slip2 = 0.0f;
    private GameObject[] Particle = new GameObject[4];
    private Rigidbody myRigidbody;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private WheelComponent[] wheels;
    private class WheelComponent
    {

        public Transform wheel;
        public WheelCollider collider;
        public Vector3 startPos;
        public float rotation = 0.0f;
        public float rotation2 = 0.0f;
        public float maxSteer;
        public bool drive;
        public float pos_y = 0.0f;
    }
    private WheelComponent SetWheelComponent(Transform wheel, float maxSteer, bool drive, float pos_y)
    {
        WheelComponent result = new WheelComponent();
        GameObject wheelCol = new GameObject(wheel.name + "WheelCollider");

        wheelCol.transform.parent = transform;
        wheelCol.transform.position = wheel.position;
        wheelCol.transform.eulerAngles = transform.eulerAngles;
        pos_y = wheelCol.transform.localPosition.y;
        wheelCol.AddComponent(typeof(WheelCollider));

        result.wheel = wheel;
        result.collider = wheelCol.GetComponent<WheelCollider>();
        result.drive = drive;
        result.pos_y = pos_y;
        result.maxSteer = maxSteer;
        result.startPos = wheelCol.transform.localPosition;

        return result;
    }


    void Awake()
    {

        if (carSetting.automaticGear) NeutralGear = false;

        myRigidbody = transform.GetComponent<Rigidbody>();

        wheels = new WheelComponent[4];

        wheels[0] = SetWheelComponent(carWheels.wheels.frontRight, carSetting.maxSteerAngle, carWheels.wheels.frontWheelDrive, carWheels.wheels.frontRight.position.y);
        wheels[1] = SetWheelComponent(carWheels.wheels.frontLeft, carSetting.maxSteerAngle, carWheels.wheels.frontWheelDrive, carWheels.wheels.frontLeft.position.y);

        wheels[2] = SetWheelComponent(carWheels.wheels.backRight, 0, carWheels.wheels.backWheelDrive, carWheels.wheels.backRight.position.y);
        wheels[3] = SetWheelComponent(carWheels.wheels.backLeft, 0, carWheels.wheels.backWheelDrive, carWheels.wheels.backLeft.position.y);


        NitrosAmount = carSetting.MaxNitros;
        currentHealth = MaxHealth;
        foreach (WheelComponent w in wheels)
        {
            WheelCollider col = w.collider;
            col.suspensionDistance = carWheels.setting.Distance;
            JointSpring js = col.suspensionSpring;

            js.spring = carSetting.springs;
            js.damper = carSetting.dampers;
            col.suspensionSpring = js;


            col.radius = carWheels.setting.Radius;

            col.mass = carWheels.setting.Weight;


            WheelFrictionCurve fc = col.forwardFriction;

            fc.asymptoteValue = 5000.0f;
            fc.extremumSlip = 2.0f;
            fc.asymptoteSlip = 20.0f;
            fc.stiffness = carSetting.stiffness;
            col.forwardFriction = fc;
            fc = col.sidewaysFriction;
            fc.asymptoteValue = 7500.0f;
            fc.asymptoteSlip = 2.0f;
            fc.stiffness = carSetting.stiffness;
            col.sidewaysFriction = fc;
        }
    }
    void Update()
    {
        if (!carSetting.automaticGear && activeControl)
        {
            if (Input.GetKeyDown("page up"))
            {
                ShiftUp();
            }
            if (Input.GetKeyDown("page down"))
            {
                ShiftDown();
            }
        }
    }
    void FixedUpdate()
    {
        myRigidbody.centerOfMass = carSetting.shiftCentre;
        HandleSpeed();
        HandleInputs();
        HandleGears();
        HandleBreakLights();
        HandleReverseLights();
        HandleCarMovement();
        HandleCarPitch();
    }
    void HandleSpeed()
    {
        speed = myRigidbody.velocity.magnitude * 2.7f;
        if (speed < lastSpeed - 10 && slip < 10)
        {
            slip = lastSpeed / 15;
        }
        lastSpeed = speed;
    }
    void HandleInputs()
    {
        if (activeControl)
        {
            if (controlMode == ControlMode.simple)
            {
                accel = 0;
                brake = false;
                shift = false;

                if (carWheels.wheels.frontWheelDrive || carWheels.wheels.backWheelDrive)
                {
                    steer = Mathf.MoveTowards(steer, Input.GetAxis("Horizontal"), 0.2f);
                    accel = Input.GetAxis("Vertical");
                    brake = Input.GetButton("Brake");
                    shift = Input.GetButton("Nitro");
                }
            }
            else if (controlMode == ControlMode.touch)
            {
                if (accelFwd != 0) { accel = accelFwd; } else { accel = accelBack; }
                steer = Mathf.MoveTowards(steer, steerAmount, 0.07f);
            }
        }
        else
        {
            accel = 0.0f;
            steer = 0.0f;
            brake = false;
            shift = false;
        }
    }
    void HandleGears()
    {
        if (!carWheels.wheels.frontWheelDrive && !carWheels.wheels.backWheelDrive)
        {
            accel = 0.0f;
        }

        if (carSetting.automaticGear && (currentGear == 1) && (accel < 0.0f))
        {
            if (speed < 5.0f) ShiftDown();
        }
        else if (carSetting.automaticGear && (currentGear == 0) && (accel > 0.0f))
        {
            if (speed < 5.0f) ShiftUp();
        }
        else if (carSetting.automaticGear && (motorRPM > carSetting.shiftUpRPM) && (accel > 0.0f) && speed > 10.0f && !brake)
        {
            ShiftUp();
        }
        else if (carSetting.automaticGear && (motorRPM < carSetting.shiftDownRPM) && (currentGear > 1))
        {
            ShiftDown();
        }

        if (speed < 1.0f) Backward = true;

        if (currentGear == 0 && Backward == true)
        {
            if (speed < carSetting.gears[0] * -10)
            {
                accel = -accel;
            }
        }
        else
        {
            Backward = false;
        }
    }
    void HandleBreakLights()
    {
        foreach (Light brakeLight in carLights.brakeLights)
        {
            if (brake || accel < 0 || speed < 1.0f)
            {
                brakeLight.intensity = Mathf.MoveTowards(brakeLight.intensity, 8, 0.5f);
            }
            else
            {
                brakeLight.intensity = Mathf.MoveTowards(brakeLight.intensity, 0, 0.5f);

            }

            brakeLight.enabled = brakeLight.intensity == 0 ? false : true;
        }
    }
    void HandleReverseLights()
    {
        foreach (Light WLight in carLights.reverseLights)
        {
            if (speed > 2.0f && currentGear == 0)
            {
                WLight.intensity = Mathf.MoveTowards(WLight.intensity, 8, 0.5f);
            }
            else
            {
                WLight.intensity = Mathf.MoveTowards(WLight.intensity, 0, 0.5f);
            }
            WLight.enabled = WLight.intensity == 0 ? false : true;
        }
    }
    void HandleCarMovement()
    {
        if (slip2 != 0.0f)
        {
            slip2 = Mathf.MoveTowards(slip2, 0.0f, 0.1f);
        }
        wantedRPM = (5500.0f * accel) * 0.1f + wantedRPM * 0.9f;
        float rpm = 0.0f;
        int motorizedWheels = 0;
        bool floorContact = false;
        int currentWheel = 0;

        foreach (WheelComponent wheel in wheels)
        {
            WheelHit hit;
            WheelCollider col = wheel.collider;

            if (wheel.drive)
            {
                if (!NeutralGear && brake && currentGear < 2)
                {
                    rpm += accel * carSetting.idleRPM;
                }
                else
                {
                    if (!NeutralGear)
                    {
                        rpm += col.rpm;
                    }
                    else
                    {
                        rpm += (carSetting.idleRPM * accel);
                    }
                }
                motorizedWheels++;
            }

            if (brake || accel < 0.0f)
            {

                if ((accel < 0.0f) || (brake && (wheel == wheels[2] || wheel == wheels[3])))
                {

                    if (brake && (accel > 0.0f))
                    {
                        slip = Mathf.Lerp(slip, 0.1f, accel * 0.01f);
                    }
                    else if (speed > 1.0f)
                    {
                        slip = Mathf.Lerp(slip, 1.0f, 0.002f);
                    }
                    else
                    {
                        slip = Mathf.Lerp(slip, 1.0f, 0.02f);
                    }
                    wantedRPM = 0.0f;
                    col.brakeTorque = carSetting.brakePower;
                    wheel.rotation = w_rotate;
                }
            }
            else
            {
                col.brakeTorque = accel == 0 || NeutralGear ? col.brakeTorque = 1000 : col.brakeTorque = 0;
                slip = speed > 0.0f ? (speed > 100 ? slip = Mathf.Lerp(slip, 1.0f + Mathf.Abs(steer), 0.02f) : slip = Mathf.Lerp(slip, 1.5f, 0.02f)) : slip = Mathf.Lerp(slip, 0.01f, 0.02f);
                w_rotate = wheel.rotation;
            }

            WheelFrictionCurve fc = col.forwardFriction;

            fc.asymptoteValue = 5000.0f;
            fc.extremumSlip = 2.0f;
            fc.asymptoteSlip = 20.0f;
            fc.stiffness = carSetting.stiffness / (slip + slip2);
            col.forwardFriction = fc;
            fc = col.sidewaysFriction;
            fc.stiffness = carSetting.stiffness / (slip + slip2);


            fc.extremumSlip = 0.2f + Mathf.Abs(steer);
            col.sidewaysFriction = fc;

            if (shift && shifmotor && accel > 0)
            {

                if (NitrosAmount == 0) { shifmotor = false; }

                NitrosAmount = Mathf.MoveTowards(NitrosAmount, 0.0f, Time.deltaTime * 10.0f);

                carSounds.nitro.volume = Mathf.Lerp(carSounds.nitro.volume, 1.0f, Time.deltaTime * 10.0f);

                if (!carSounds.nitro.isPlaying)
                {
                    carSounds.nitro.GetComponent<AudioSource>().Play();

                }


                curTorque = NitrosAmount > 0 ? carSetting.shiftPower : carSetting.carPower;
                var ps = carParticles.shiftParticle1.emission;
                ps.rateOverTime = Mathf.Lerp(ps.rateOverTime.constant, NitrosAmount > 0 ? 50 : 0, Time.deltaTime * 10.0f);

                var ps2 = carParticles.shiftParticle2.emission;
                ps2.rateOverTime = Mathf.Lerp(ps2.rateOverTime.constant, NitrosAmount > 0 ? 50 : 0, Time.deltaTime * 10.0f);
            }
            else
            {

                if (NitrosAmount > carSetting.MinNitrosToStart)
                {
                    shifmotor = true;
                }

                carSounds.nitro.volume = Mathf.MoveTowards(carSounds.nitro.volume, 0.0f, Time.deltaTime * 2.0f);

                if (carSounds.nitro.volume == 0)
                    carSounds.nitro.Stop();

                NitrosAmount = Mathf.MoveTowards(NitrosAmount, carSetting.MaxNitros, Time.deltaTime * 5.0f);
                curTorque = carSetting.carPower;

                var ps = carParticles.shiftParticle1.emission;
                ps.rateOverTime = Mathf.Lerp(ps.rateOverTime.constant, 0, Time.deltaTime * 10.0f);

                var ps2 = carParticles.shiftParticle2.emission;
                ps2.rateOverTime = Mathf.Lerp(ps2.rateOverTime.constant, 0, Time.deltaTime * 10.0f);
            }


            wheel.rotation = Mathf.Repeat(wheel.rotation + Time.deltaTime * col.rpm * 360.0f / 60.0f, 360.0f);
            wheel.rotation2 = Mathf.Lerp(wheel.rotation2, col.steerAngle, 0.1f);
            wheel.wheel.localRotation = Quaternion.Euler(wheel.rotation, wheel.rotation2, 0.0f);



            Vector3 lp = wheel.wheel.localPosition;


            if (col.GetGroundHit(out hit))
            {


                if (carParticles.brakeParticlePerfab)
                {
                    if (Particle[currentWheel] == null)
                    {
                        Particle[currentWheel] = Instantiate(carParticles.brakeParticlePerfab, wheel.wheel.position, Quaternion.identity) as GameObject;
                        Particle[currentWheel].name = "WheelParticle";
                        Particle[currentWheel].transform.parent = transform;
                        AudioSource aud = Particle[currentWheel].AddComponent<AudioSource>();
                        aud.maxDistance = 50;
                        aud.spatialBlend = 1;
                        aud.dopplerLevel = 5;
                        aud.rolloffMode = AudioRolloffMode.Custom;
                    }


                    var pc = Particle[currentWheel].GetComponent<ParticleSystem>().emission;
                    bool WGrounded = false;


                    for (int i = 0; i < carSetting.hitGround.Length; i++)
                    {

                        if (hit.collider.CompareTag(carSetting.hitGround[i].tag))
                        {
                            WGrounded = carSetting.hitGround[i].grounded;

                            if ((brake || Mathf.Abs(hit.sidewaysSlip) > 0.5f) && speed > 1)
                            {
                                Particle[currentWheel].GetComponent<AudioSource>().clip = carSetting.hitGround[i].brakeSound;
                            }
                            else if (Particle[currentWheel].GetComponent<AudioSource>().clip != carSetting.hitGround[i].groundSound && !Particle[currentWheel].GetComponent<AudioSource>().isPlaying)
                            {

                                Particle[currentWheel].GetComponent<AudioSource>().clip = carSetting.hitGround[i].groundSound;
                            }
                            var ps = Particle[currentWheel].GetComponent<ParticleSystem>().main;
                            ps.startColor = carSetting.hitGround[i].brakeColor;

                        }


                    }

                    if (WGrounded && speed > 5 && !brake)
                    {

                        pc.enabled = true;

                        Particle[currentWheel].GetComponent<AudioSource>().volume = 0.5f;

                        if (!Particle[currentWheel].GetComponent<AudioSource>().isPlaying)
                            Particle[currentWheel].GetComponent<AudioSource>().Play();

                    }
                    else if ((brake || Mathf.Abs(hit.sidewaysSlip) > 0.6f) && speed > 1)
                    {

                        if ((accel < 0.0f) || ((brake || Mathf.Abs(hit.sidewaysSlip) > 0.6f) && (wheel == wheels[2] || wheel == wheels[3])))
                        {

                            if (!Particle[currentWheel].GetComponent<AudioSource>().isPlaying)
                                Particle[currentWheel].GetComponent<AudioSource>().Play();
                            pc.enabled = true;
                            Particle[currentWheel].GetComponent<AudioSource>().volume = 10;

                        }

                    }
                    else
                    {

                        pc.enabled = false;
                        Particle[currentWheel].GetComponent<AudioSource>().volume = Mathf.Lerp(Particle[currentWheel].GetComponent<AudioSource>().volume, 0, Time.deltaTime * 10.0f);
                    }

                }


                lp.y -= Vector3.Dot(wheel.wheel.position - hit.point, transform.TransformDirection(0, 1, 0) / transform.lossyScale.x) - (col.radius);
                lp.y = Mathf.Clamp(lp.y, -10.0f, wheel.pos_y);
                floorContact = floorContact || (wheel.drive);


            }
            else
            {

                if (Particle[currentWheel] != null)
                {
                    var pc = Particle[currentWheel].GetComponent<ParticleSystem>().emission;
                    pc.enabled = false;
                }



                lp.y = wheel.startPos.y - carWheels.setting.Distance;

                myRigidbody.AddForce(Vector3.down * 25000);

            }

            currentWheel++;
            wheel.wheel.localPosition = lp;


        }
        if (motorizedWheels > 1)
        {
            rpm = rpm / motorizedWheels;
        }
        motorRPM = 0.95f * motorRPM + 0.05f * Mathf.Abs(rpm * carSetting.gears[currentGear]);
        if (motorRPM > 5500.0f) motorRPM = 5200.0f;
        int index = (int)(motorRPM / efficiencyTableStep);
        if (index >= efficiencyTable.Length) index = efficiencyTable.Length - 1;
        if (index < 0) index = 0;
        float newTorque = curTorque * carSetting.gears[currentGear] * efficiencyTable[index];
        foreach (WheelComponent w in wheels)
        {
            WheelCollider col = w.collider;

            if (w.drive)
            {

                if (Mathf.Abs(col.rpm) > Mathf.Abs(wantedRPM))
                {

                    col.motorTorque = 0;
                }
                else
                {
                    // 
                    float curTorqueCol = col.motorTorque;

                    if (!brake && accel != 0 && NeutralGear == false)
                    {
                        if ((speed < carSetting.LimitForwardSpeed && currentGear > 0) ||
                            (speed < carSetting.LimitBackwardSpeed && currentGear == 0))
                        {

                            col.motorTorque = curTorqueCol * 0.9f + newTorque * 1.0f;
                        }
                        else
                        {
                            col.motorTorque = 0;
                            col.brakeTorque = 2000;
                        }


                    }
                    else
                    {
                        col.motorTorque = 0;

                    }
                }

            }





            if (brake || slip2 > 2.0f)
            {
                col.steerAngle = Mathf.Lerp(col.steerAngle, steer * w.maxSteer, 0.02f);
            }
            else
            {

                float SteerAngle = Mathf.Clamp(speed / carSetting.maxSteerAngle, 1.0f, carSetting.maxSteerAngle);
                col.steerAngle = steer * (w.maxSteer / SteerAngle);


            }

        }

    }
    void HandleCarPitch()
    {
        Pitch = Mathf.Clamp(1.2f + ((motorRPM - carSetting.idleRPM) / (carSetting.shiftUpRPM - carSetting.idleRPM)), 1.0f, 10.0f);

        shiftTime = Mathf.MoveTowards(shiftTime, 0.0f, 0.1f);

        if (Pitch == 1)
        {
            carSounds.IdleEngine.volume = Mathf.Lerp(carSounds.IdleEngine.volume, 1.0f, 0.1f);
            carSounds.LowEngine.volume = Mathf.Lerp(carSounds.LowEngine.volume, 0.5f, 0.1f);
            carSounds.HighEngine.volume = Mathf.Lerp(carSounds.HighEngine.volume, 0.0f, 0.1f);

        }
        else
        {

            carSounds.IdleEngine.volume = Mathf.Lerp(carSounds.IdleEngine.volume, 1.8f - Pitch, 0.1f);


            if ((Pitch > PitchDelay || accel > 0) && shiftTime == 0.0f)
            {
                carSounds.LowEngine.volume = Mathf.Lerp(carSounds.LowEngine.volume, 0.0f, 0.2f);
                carSounds.HighEngine.volume = Mathf.Lerp(carSounds.HighEngine.volume, 1.0f, 0.1f);
            }
            else
            {
                carSounds.LowEngine.volume = Mathf.Lerp(carSounds.LowEngine.volume, 0.5f, 0.1f);
                carSounds.HighEngine.volume = Mathf.Lerp(carSounds.HighEngine.volume, 0.0f, 0.2f);
            }




            carSounds.HighEngine.pitch = Pitch;
            carSounds.LowEngine.pitch = Pitch;

            PitchDelay = Pitch;
        }
    }
    public void ShiftUp()
    {
        float now = Time.timeSinceLevelLoad;

        if (now < shiftDelay) return;

        if (currentGear < carSetting.gears.Length - 1)
        {

           // if (!carSounds.switchGear.isPlaying)
                carSounds.switchGear.GetComponent<AudioSource>().Play();


                if (!carSetting.automaticGear)
            {
                if (currentGear == 0)
                {
                    if (NeutralGear){currentGear++;NeutralGear = false;}
                    else
                    { NeutralGear = true;}
                }
                else
                {
                    currentGear++;
                }
            }
            else
            {
                currentGear++;
            }


           shiftDelay = now + 1.0f;
           shiftTime = 1.5f;
        }
    }
    public void ShiftDown()
    {
        float now = Time.timeSinceLevelLoad;

       if (now < shiftDelay) return;

        if (currentGear > 0 || NeutralGear)
        {

           //w if (!carSounds.switchGear.isPlaying)
                carSounds.switchGear.GetComponent<AudioSource>().Play();

                if (!carSetting.automaticGear)
            {

                if (currentGear == 1)
                {
                    if (!NeutralGear){currentGear--;NeutralGear = true;}
                }
                else if (currentGear == 0){NeutralGear = false;}else{currentGear--;}
            }
            else
            {
                currentGear--;
            }


            shiftDelay = now + 0.1f;
            shiftTime = 2.0f;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.root.GetComponent<VehicleControl>())
        {
            collision.transform.root.GetComponent<VehicleControl>().slip2 = Mathf.Clamp(collision.relativeVelocity.magnitude, 0.0f, 10.0f);
            myRigidbody.angularVelocity = new Vector3(-myRigidbody.angularVelocity.x * 0.5f, myRigidbody.angularVelocity.y * 0.5f, -myRigidbody.angularVelocity.z * 0.5f);
            myRigidbody.velocity = new Vector3(myRigidbody.velocity.x, myRigidbody.velocity.y * 0.5f, myRigidbody.velocity.z);
        }
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.transform.root.GetComponent<VehicleControl>())
        {
            collision.transform.root.GetComponent<VehicleControl>().slip2 = 5.0f;
        }
    }

    public void Damage(float damage, DamageType dmgType)
    {
        if (!Invunrable)
        {
            currentHealth -= damage;

            //Particle OnHit Effect
            GameObject obj = Instantiate(carParticles.HitParticle, Camera.main.transform);
            var ps1 = obj.GetComponent<ParticleSystem>().emission;
            short vOut = System.Convert.ToInt16(Mathf.RoundToInt(damage * 10));
            ps1.SetBurst(0, new ParticleSystem.Burst(1.0f, vOut, vOut));

            //CheckDeath
            if (currentHealth < 0)
            {
                currentHealth = 0;
                gm.GameOverString = "You Died.";
                gm.SwitchState(gm.LoseState);
            }
        }
    }
}
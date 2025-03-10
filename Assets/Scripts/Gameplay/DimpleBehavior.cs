using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class DimpleBehavior : MonoBehaviour
{
    //XR Rig
    private PlayerHP playerHP;
    private XRState xrstatus;
    public GameObject cameraXR;
    public GameObject leftController;
    public GameObject rightController;

    //animator & sound
    private Animator dimpleAnimate;
    public AudioClip hurtSFX;
    public AudioClip blockSFX;
    public AudioClip alertSFX;
    private AudioSource dimpleAudio;

    //alert component
    public GameObject FistLeft;
    public GameObject FistRight;
    public GameObject DodgeLeft;
    public GameObject DodgeRight;


    public string attackState = "idle";         //idle, runtoplayer, showalert, attack, restattack, runbackward
    public string hookType;

    public int attackType = 0;
    public int attackLoop = 0;

    public float startTime = 0;
    public bool isDelay = false;
    public float delayTime = 0;
    void Awake()
    {
        xrstatus = GameObject.Find("XR Rig").GetComponent<XRState>();
        playerHP = GameObject.Find("Main Camera").GetComponent<PlayerHP>();
        dimpleAudio = GetComponent<AudioSource>();
        dimpleAnimate = GetComponent<Animator>();
        startDelay(3f);
    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        if(!dimpleAnimate.GetBool("isDying"))
        {
            if (isDelay)
            {
                if (Time.time - startTime > delayTime)
                {
                    isDelay = false;
                }

            }

            else if (attackState == "idle")
            {
                attackState = "runtoplayer";
                dimpleAnimate.SetFloat("speed", 0.5f);
            }

            else if (attackState == "runtoplayer" && gameObject.transform.position.x > -214f)
            {
                dimpleAnimate.SetFloat("speed", 0f);

                attackState = "showalert";
                attackType = Random.Range(1, 4);
                if (attackType == 1 || attackType == 2) attackLoop = 2;
                if (attackType == 3) attackLoop = 6;

                startDelay(1f);
            }

            else if (attackState == "showalert")
            {
                gameObject.transform.position = new Vector3(-212.85f, transform.position.y, transform.position.z);
                if (attackLoop > 0)
                {
                    if (Random.Range(0, 2) == 0) hookType = "FL";
                    else hookType = "FR";

                    showAlert(hookType);
                    attackState = "attack";
                    startDelay(0.2f);
                }
            }

            else if (attackState == "attack")
            {
                if (hookType == "FR") dimpleAnimate.SetBool("hookLeft", true);
                else dimpleAnimate.SetBool("hookRight", true);

                attackLoop -= 1;
                attackState = "restattack";
                startDelay(0.67f);
            }

            else if (attackState == "restattack")
            {
                dimpleAnimate.SetBool("hookLeft", false);
                dimpleAnimate.SetBool("hookRight", false);
                hideAlert(hookType);
                if (!dimpleAnimate.GetBool("isTired"))
                {
                    checkDamage(hookType);
                }

                if (attackLoop > 0)
                {
                    attackState = "showalert";
                }
                else
                {
                    if (attackType == 3 && !dimpleAnimate.GetBool("isTired"))
                    {
                        dimpleAnimate.SetBool("isTired", true);
                        startDelay(3f);
                    }
                    else
                    {
                        dimpleAnimate.SetBool("isTired", false);
                        attackState = "runbackward";
                        dimpleAnimate.SetFloat("speed", -0.5f);
                    }
                }
            }

            else if (attackState == "runbackward" && gameObject.transform.position.x < -219.64f)
            {
                dimpleAnimate.SetFloat("speed", 0f);
                attackState = "idle";
                startDelay(3f);
            }
        }
        
    }

    void startDelay(float delayT)
    {
        delayTime = delayT;
        startTime = Time.time;
        isDelay = true;
    }

    void showAlert(string alertType)
    {
        if (alertType == "FL")
        {
            FistLeft.SetActive(true);
        }
        else if (alertType == "FR")
        {
            FistRight.SetActive(true);
        }
        else if (alertType == "DL")
        {
            DodgeLeft.SetActive(true);
        }
        else
        {
            DodgeRight.SetActive(true);
        }

        dimpleAudio.PlayOneShot(alertSFX);

    }
    void hideAlert(string alertType)
    {
        if (alertType == "FL")
        {
            FistLeft.SetActive(false);
        }
        else if (alertType == "FR")
        {
            FistRight.SetActive(false);
        }
        else if (alertType == "DL")
        {
            DodgeLeft.SetActive(false);
        }
        else
        {
            DodgeRight.SetActive(false);
        }
    }

    [System.Obsolete]
    void checkDamage(string hooktype)
    {
        if (hooktype == "FL")
        {
            if (xrstatus.isLeftGripActive && leftController.transform.position.z > cameraXR.transform.position.z-0.3 && 
                leftController.transform.position.y > cameraXR.transform.position.y - 0.03)
            {
                dimpleAudio.PlayOneShot(blockSFX);
                ActivateLeftHaptic();
            }
            else
            {
                dimpleAudio.PlayOneShot(hurtSFX);
                playerHP.getDamage();
            }
        }
        else
        {
            if (xrstatus.isRightGripActive && rightController.transform.position.z > cameraXR.transform.position.z &&
                rightController.transform.position.y > cameraXR.transform.position.y - 0.02)
            {
                dimpleAudio.PlayOneShot(blockSFX);
                ActivateRightHaptic();
            }
            else
            {
                dimpleAudio.PlayOneShot(hurtSFX);
                playerHP.getDamage();
            }
        }
    }

    [System.Obsolete]
    void ActivateLeftHaptic()
    {
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

        UnityEngine.XR.InputDevices.GetDevicesWithRole(UnityEngine.XR.InputDeviceRole.LeftHanded, devices);

        foreach (var device in devices)
        {
            UnityEngine.XR.HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    float amplitude = 0.5f;
                    float duration = 0.25f;
                    device.SendHapticImpulse(channel, amplitude, duration);
                }
            }
        }
    }

    [System.Obsolete]
    void ActivateRightHaptic()
    {
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

        UnityEngine.XR.InputDevices.GetDevicesWithRole(UnityEngine.XR.InputDeviceRole.RightHanded, devices);

        foreach (var device in devices)
        {
            UnityEngine.XR.HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    float amplitude = 0.5f;
                    float duration = 0.25f;
                    device.SendHapticImpulse(channel, amplitude, duration);
                }
            }
        }
    }

    //0.1 함 = 0.16
    //0.2 함 = 0.33
    //0.3 함 = 0.5
    //0.4 함 = 0.67
    //0.5 함 = 0.83
    //1.0 함 = 1
}

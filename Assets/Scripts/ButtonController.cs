using System.Collections;
using System.Collections.Generic;
using Ultraleap.TouchFree.Tooling.Connection;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    MainController mainController;
    MessageReceiver messageReceiver;
    public Animator btnAni;
    public GameObject waterAni;
    public string command;
    public Transform uiToScene;
    bool isBtnHold = false;
    public bool isHovering;
    public GameObject sound;
    public AudioClip tooCloseAudio;
    public AudioClip clickAudio;
    public Text messageText;

    void Start()
    {
        mainController = GameObject.FindGameObjectWithTag("MainManager").GetComponent<MainController>();
        messageReceiver = GameObject.FindGameObjectWithTag("TouchFreeClient").GetComponent<MessageReceiver>();
        messageText = GameObject.Find("message").GetComponent<Text>();
    }

    bool isAbsorbed;
    void Update()
    {
        if (isBtnHold)
            mainController.time = 5;
        if (isHovering && !isAbsorbed && messageReceiver.distanceFromScreen < mainController.tooCloseDistance)//meters
        {
            mainController.isTooClose = true;
            sound.GetComponent<SoundPlayer>().source.clip = tooCloseAudio;
            Instantiate(sound);
            mainController.AbsorbTacs(uiToScene.position, 50f);
            isAbsorbed = true;
        }
    }

    public void OnHover()
    {
        if (command.Contains("door_"))
        {
            isBtnHold = true;
        }
        btnAni.SetBool("isHovered", true);
        isHovering = true;
        mainController.TurnOffTacsFlying();
        mainController.timeCounter = 0;
        //mainController.AbsorbTacs(uiToScene, 10f);
    }

    public void OnLeave()
    {
        if (command.StartsWith("door_"))
        {
            isBtnHold = false;
            btnAni.SetTrigger("left");
        }
        btnAni.SetBool("isHovered", false);
        isHovering = false;
        mainController.TurnOffTacsAbsorbing();
        mainController.isTooClose = false;
        isAbsorbed = false;
    }

    public void OnClicked()
    {
        sound.GetComponent<SoundPlayer>().source.clip = clickAudio;
        Instantiate(sound);
        GameObject water = Instantiate(waterAni);
        water.transform.position = new Vector2(transform.position.x, transform.position.y);
        water.transform.localScale = new Vector2(1f, 1f);
        water.transform.SetParent(GameObject.FindGameObjectWithTag("AnimationArea").transform);
        Destroy(water, 1.2f);
        mainController.BlowOutTacs(uiToScene.localPosition, 1000f, true);
        btnAni.SetTrigger("clicked");
        Invoke("SetClickedFalse", 0.5f);
        mainController.InputCommand(command);
    }

    public void SetClickedFalse()
    {
        btnAni.ResetTrigger("clicked");
    }

    public void InvokeSetDefault(float seconds)
    {
        Invoke("SetDefault", seconds);
    }

    void SetDefault()
    {
        btnAni.SetBool("arrived", false);
    }
}

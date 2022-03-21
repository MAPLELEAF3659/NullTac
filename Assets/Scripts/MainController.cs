using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public int currentFloor = 1;
    public bool[] isStop;
    public bool isMoving;
    public bool isRuning;
    public bool isDoorOpen;
    public bool isDoorWaiting;
    public Text floorNumText;
    public Text elevatorStatusText;
    public Text messageText;
    public Text debugMessageText;
    public GameObject tacPrefab;
    public GameObject[] tac;
    public float timeCounter;
    public float waitingTime;
    public float tooCloseDistance;
    public float bgmMaxVolume;
    public Animator tutorialAni;
    public AudioSource elevatorBGM;
    public GameObject soundPlayer;
    public AudioClip dingAudio;
    public AudioClip[] arriveFloorAudio;
    public AudioClip doorOpeningAudio;
    public AudioClip doorClosingAudio;
    public bool isTooClose;

    void Start()
    {
        floorNumText.text = (currentFloor + 1).ToString("00");
        elevatorStatusText.text = "";
        messageText.text = "";
        GenerateTacRandomly(tac, tac.Length);
        timeCounter = 29f;
    }

    void Update()
    {
        timeCounter += Time.deltaTime;
        if (timeCounter >= waitingTime && tutorialAni.GetBool("play") == false)
        {
            elevatorBGM.volume = bgmMaxVolume;
            tutorialAni.SetBool("play", true);
            messageText.text = "請將手指靠近螢幕，待指標出現後再點擊按鈕";
            BlowOutTacs(new Vector3(0, 0, 0), 500f, false);
        }
        else if (timeCounter < waitingTime && !isTooClose)
        {
            if (bgmMaxVolume > 0.1f)
                elevatorBGM.volume = 0.1f;
            tutorialAni.SetBool("play", false);
            messageText.text = "";
        }
        else if (isTooClose)
        {
            messageText.text = "手指與螢幕的距離過近";
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene("main");
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            waitingTime += 1;
            debugMessageText.text = "Adjust waiting time to " + waitingTime + "s";
            Invoke("ClearDebugMessage", 2f);
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) && waitingTime > 0)
        {
            waitingTime -= 1;
            debugMessageText.text = "Adjust waiting time to " + waitingTime + "s";
            Invoke("ClearDebugMessage", 2f);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) && tooCloseDistance > 0)
        {
            tooCloseDistance -= 0.005f;
            debugMessageText.text = "Adjust too close distance to " + Mathf.Round(tooCloseDistance * 1000)/10 + "cm";
            Invoke("ClearDebugMessage", 2f);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            tooCloseDistance += 0.005f;
            debugMessageText.text = "Adjust too close distance to " + Mathf.Round(tooCloseDistance * 1000)/10 + "cm";
            Invoke("ClearDebugMessage", 2f);
        }
        else if (Input.GetKeyUp(KeyCode.KeypadPlus) && bgmMaxVolume < 1)
        {
            bgmMaxVolume += 0.05f;
            debugMessageText.text = "Adjust BGM max volume to " + Mathf.Round(bgmMaxVolume * 100) + "%";
            if (timeCounter >= waitingTime)
                elevatorBGM.volume = bgmMaxVolume;
            Invoke("ClearDebugMessage", 2f);
        }
        else if (Input.GetKeyUp(KeyCode.KeypadMinus) && bgmMaxVolume > 0)
        {
            bgmMaxVolume -= 0.05f;
            debugMessageText.text = "Adjust BGM max volume to " + Mathf.Round(bgmMaxVolume * 100) + "%";
            if (timeCounter >= waitingTime)
                elevatorBGM.volume = bgmMaxVolume;
            Invoke("ClearDebugMessage", 2f);
        }
    }

    public void GenerateTacRandomly(GameObject[] tacArray, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            tacArray[i] = Instantiate(tacPrefab);
            tacArray[i].transform.SetParent(GameObject.FindGameObjectWithTag("TacsArea").transform);
        }
    }

    public void BlowOutTacs(Vector3 btnPos, float power, bool isTurnOff)
    {
        foreach (GameObject tacObj in tac)
        {
            tacObj.GetComponent<TacController>().BlowOut(btnPos, power, isTurnOff);
        }
    }

    public void TurnOffTacsFlying()
    {
        foreach (GameObject tacObj in tac)
        {
            tacObj.GetComponent<TacController>().StartCoroutine(tacObj.GetComponent<TacController>().TurnOffFlyingMode(0f, true));
        }
    }

    public void AbsorbTacs(Vector3 btnPos, float power)
    {
        foreach (GameObject tacObj in tac)
        {
            tacObj.GetComponent<TacController>().Absorb(btnPos, power);
        }
    }

    public void TurnOffTacsAbsorbing()
    {
        //messageText.text = "";
        foreach (GameObject tacObj in tac)
        {
            tacObj.GetComponent<TacController>().TurnOffAbsorbingMode();
        }
    }

    public void InputCommand(string command)
    {
        timeCounter = 0;
        if (command.StartsWith("floor"))
        {
            int floor = int.Parse(command.Substring(command.IndexOf("r") + 1));
            try
            {
                if (currentFloor == floor)
                {
                    SpawnSound(dingAudio);
                    Debug.Log("You are already at this floor.");
                    GameObject.Find("Floor Button " + (currentFloor + 1)).GetComponent<ButtonController>().btnAni.SetBool("arrived", true);
                    GameObject.Find("Floor Button " + (currentFloor + 1)).GetComponent<ButtonController>().InvokeSetDefault(2.5f);
                }
                else if (!isRuning && currentFloor != floor)
                {
                    isStop[floor] = true;
                    StartCoroutine(MoveElevator(currentFloor < floor ? true : false));
                }
                else if (isRuning)
                {
                    isStop[floor] = true;
                }
            }
            catch
            {
                Debug.Log("Floor number " + floor + " is out of index.");
            }

        }
        else if (command.StartsWith("door"))
        {
            bool targetStatus = false;
            try
            {
                targetStatus = command.Substring(command.IndexOf("_") + 1) == "open" ? true : false;
                if (isMoving)
                {
                    Debug.Log("Cannot control the door while the elevator is moving.");
                }
                else if (isDoorOpen == targetStatus)
                {
                    Debug.Log("The door is already " + (targetStatus ? "open" : "close") + ".");
                }
                else if (!isMoving && isDoorOpen != targetStatus)
                {
                    if (isDoorWaiting)
                    {
                        StopAllCoroutines();
                        StartCoroutine(ControlDoor(targetStatus));
                    }
                    else
                    {
                        StartCoroutine(ControlDoor(targetStatus));
                    }
                }
            }
            catch
            {
                Debug.Log("Door status only can be \"open\" or \"close\".");
                return;
            }
        }
        else
            Debug.Log("Unknown command.");
    }

    IEnumerator MoveElevator(bool isRise)
    {
        isRuning = true;
        while (isRuning)
        {
            isMoving = true;
            elevatorStatusText.text = isRise ? "上樓中" : "下樓中";
            isRuning = false;
            if (isRise)
            {
                for (int floor = currentFloor; floor < isStop.Length; floor++)
                {
                    if (isStop[floor])
                    {
                        isRuning = true;
                        break;
                    }
                }
            }
            else if (!isRise)
            {
                for (int floor = currentFloor; floor >= 0; floor--)
                {
                    if (isStop[floor])
                    {
                        isRuning = true;
                        break;
                    }
                }
            }
            if (!isRuning)
            {
                if (isRise)
                {
                    for (int floor = currentFloor; floor >= 0; floor--)
                    {
                        if (isStop[floor])
                        {
                            isRise = false;
                            isRuning = true;
                            break;
                        }
                    }
                    if (!isRuning) break;
                }
                else if (!isRise)
                {
                    for (int floor = currentFloor; floor < isStop.Length; floor++)
                    {
                        if (isStop[floor])
                        {
                            isRise = true;
                            isRuning = true;
                            break;
                        }
                    }
                    if (!isRuning) break;
                }
                elevatorStatusText.text = isRise ? "上樓中" : "下樓中";
            }
            yield return new WaitForSeconds(3f);
            currentFloor = isRise ? currentFloor + 1 : currentFloor - 1;
            floorNumText.text = (currentFloor + 1).ToString("00");
            if (isStop[currentFloor])
            {
                soundPlayer.GetComponent<SoundPlayer>().source.clip = dingAudio;
                Instantiate(soundPlayer);
                isMoving = false;
                isStop[currentFloor] = false;
                SpawnSound(arriveFloorAudio[currentFloor]);
                elevatorStatusText.text = (currentFloor + 1) + "樓到了";
                GameObject.Find("Floor Button " + (currentFloor + 1)).GetComponent<ButtonController>().btnAni.SetBool("arrived", true);
                yield return new WaitForSeconds(3f);
                SpawnSound(doorOpeningAudio);
                elevatorStatusText.text = "開門中";
                yield return new WaitForSeconds(1.5f);
                isDoorOpen = true;
                GameObject.Find("Floor Button " + (currentFloor + 1)).GetComponent<ButtonController>().btnAni.SetBool("arrived", false);
                elevatorStatusText.text = "請小心間隙";
                yield return new WaitForSeconds(3f);
                SpawnSound(doorClosingAudio);
                elevatorStatusText.text = "關門中";
                yield return new WaitForSeconds(1.5f);
                isDoorOpen = false;
            }
        }

        elevatorStatusText.text = "";
        isMoving = false;
        isRuning = false;
    }

    public float time = 0, doorMovingTime = 0;
    IEnumerator ControlDoor(bool targetStatus)
    {
        elevatorStatusText.text = targetStatus ? "開門中" : "關門中";
        SpawnSound(targetStatus ? doorOpeningAudio : doorClosingAudio);
        isDoorOpen = targetStatus;
        isDoorWaiting = true;
        doorMovingTime = 0;
        while (isDoorWaiting == true)
        {
            doorMovingTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
            if (doorMovingTime > 3)
            {
                isDoorWaiting = false;
            }
        }

        if (targetStatus == true)
        {
            isDoorWaiting = true;
            time = 5;
            while (isDoorWaiting == true)
            {
                elevatorStatusText.text = "請小心間隙";
                time -= Time.deltaTime;
                yield return new WaitForFixedUpdate();
                if (time < 0)
                {
                    isDoorWaiting = false;
                }
            }
            SpawnSound(doorClosingAudio);
            elevatorStatusText.text = "關門中";

            isDoorOpen = false;
            isDoorWaiting = true;
            doorMovingTime = 0;
            while (isDoorWaiting == true)
            {
                doorMovingTime += Time.deltaTime;
                yield return new WaitForFixedUpdate();
                if (doorMovingTime > 3)
                {
                    isDoorWaiting = false;
                }
            }

            elevatorStatusText.text = "";
        }
        else if (targetStatus == false)
        {
            elevatorStatusText.text = "";
        }
    }

    public void ClearDebugMessage()
    {
        debugMessageText.text = "";
    }
    public void SpawnSound(AudioClip sound)
    {
        soundPlayer.GetComponent<SoundPlayer>().source.clip = sound;
        Instantiate(soundPlayer);
    }
}

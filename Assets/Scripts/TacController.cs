using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacController : MonoBehaviour
{
    MainController mainController;
    public float speed;
    public Vector2 newPos;
    public float lerpDistance;

    public Vector2 posAroundBtn;
    bool isFlying;
    bool isAbsorbing;

    void Start()
    {
        mainController = GameObject.FindGameObjectWithTag("MainManager").GetComponent<MainController>();
        SetNewPosAndSpeed(0.5f, 2f, -4f, 4f, -5f, 5f);
        float scale = Random.Range(0.3f, 0.5f);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    void Update()
    {
        if (!isFlying)
        {
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.y), newPos) < lerpDistance)
            {
                if (!isAbsorbing)
                    SetNewPosAndSpeed(0.5f, 2f, -4f, 4f, -5f, 5f);
                else if (isAbsorbing)
                    SetNewPosAndSpeed(0.25f, 1f, posAroundBtn.x - 0.8f, posAroundBtn.x + 0.8f, posAroundBtn.y - 1f, posAroundBtn.y + 1f);
            }
            else
            {
                transform.position = Vector2.Lerp(transform.position, newPos, Time.deltaTime * speed);
            }
        }
    }

    public void BlowOut(Vector3 btnPos, float power, bool isTurnOff)
    {
        isFlying = true;
        Vector3 direation = btnPos - transform.position;
        GetComponent<Rigidbody>().AddForceAtPosition(-direation * power, btnPos);
        StartCoroutine(TurnOffFlyingMode(2f, isTurnOff));
    }
    public void Absorb(Vector3 btnPos, float power)
    {
        isAbsorbing = true;
        posAroundBtn = btnPos;
        SetNewPosAndSpeed(power * 0.01f, power * 0.05f, posAroundBtn.x - 0.8f, posAroundBtn.x + 0.8f, posAroundBtn.y - 1f, posAroundBtn.y + 1f);
    }

    public IEnumerator TurnOffFlyingMode(float waitTime, bool turnOffFlyingBool)
    {
        yield return new WaitForSeconds(waitTime);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(0, 250f, 0);
        isFlying = !turnOffFlyingBool;
    }
    public void TurnOffAbsorbingMode()
    {
        isAbsorbing = false;
        //SetNewPosAndSpeed(0.5f, 2f, -4f, 4f, -5f, 5f);
    }

    void SetNewPosAndSpeed(float speedMin, float speedMax, float xMin, float xMax, float yMin, float yMax)
    {
        speed = Random.Range(speedMin, speedMax);
        newPos.x = Random.Range(xMin, xMax);
        newPos.y = Random.Range(yMin, yMax);
        transform.rotation = Quaternion.Euler(0f, Random.Range(210f, 290f), Random.Range(-30f, 40f));
    }
}

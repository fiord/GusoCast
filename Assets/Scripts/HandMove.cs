using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HandMove : MonoBehaviour
{
    private static readonly Joycon.Button[] m_buttons = Enum.GetValues(typeof(Joycon.Button)) as Joycon.Button[];
    private Joycon leftJoycon;
    private Joycon rightJoycon;
    private Joycon.Button? m_pressedButtonL;
    private Joycon.Button? m_pressedButtonR;

    private GameObject leftHand;
    Quaternion leftGusokuBase = Quaternion.identity, leftJoyconBase = Quaternion.identity;
    private GameObject rightHand;
    Quaternion rightGusokuBase = Quaternion.identity, rightJoyconBase = Quaternion.identity;
    // for test
    private GameObject testLeft, testRight;

    private Quaternion ConvertRot(Quaternion a)
    {
        a = Quaternion.Inverse(new Quaternion(a.x, a.z, a.y, a.w));
        a = Quaternion.Inverse(new Quaternion(a.z, a.y, a.x, a.w));
        return new Quaternion(-a.x, a.y, -a.z, a.w);
    }
    // Start is called before the first frame update
    void Start()
    {
        // set gameobjects
        leftHand = GameObject.Find("Left12");
        rightHand = GameObject.Find("Right12");

        // set joycons
        List<Joycon> joycons = JoyconManager.Instance.j;
        foreach(var joycon in joycons)
        {
            if(joycon.isLeft)
            {
                leftJoycon = joycon;
            }
            else
            {
                rightJoycon = joycon;
            }
        }

        if(leftJoycon == null)
        {
            Debug.Log("Joycon(L)が見つかりませんでした。");
        }
        else
        {
            leftGusokuBase = leftHand.transform.rotation;
            leftJoyconBase = ConvertRot(leftJoycon.GetVector());
        }
        if(rightJoycon == null)
        {
            Debug.Log("Joycon(R)が見つかりませんでした。");
        }
        else
        {
            rightGusokuBase = rightHand.transform.rotation;
            rightJoyconBase = rightJoycon.GetVector();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // left hand
        if (leftJoycon != null)
        {
            m_pressedButtonL = null;
            foreach(var button in m_buttons)
            {
                if(leftJoycon.GetButton(button))
                {
                    m_pressedButtonL = button;
                }
            }
            if(leftJoyconBase == Quaternion.identity)
            {
                leftJoyconBase = ConvertRot(leftJoycon.GetVector());
            }
            else if(m_pressedButtonL == Joycon.Button.SHOULDER_1)
            {
                leftJoyconBase = ConvertRot(leftJoycon.GetVector());
            }
            else
            {
                Quaternion prevRot = leftHand.transform.rotation;
                leftHand.transform.rotation = leftGusokuBase;
                Quaternion orientation = ConvertRot(leftJoycon.GetVector());
                Vector3 baseAxis = (Quaternion.Inverse(leftJoyconBase) * orientation).eulerAngles;
                baseAxis = new Vector3(-baseAxis.x, baseAxis.y, -baseAxis.z);
                leftHand.transform.Rotate(baseAxis);
                leftHand.transform.rotation = Quaternion.Lerp(prevRot, leftHand.transform.rotation, 0.5f);
            }
        }
        // right hand
        if(rightJoycon!=null)
        {
            m_pressedButtonR = null;
            foreach(var button in m_buttons)
            {
                if(rightJoycon.GetButton(button))
                {
                    m_pressedButtonR = button;
                }
            }
            if (rightJoyconBase == Quaternion.identity)
            {
                rightJoyconBase = ConvertRot(leftJoycon.GetVector());
                Debug.Log("right reset " + rightJoyconBase.eulerAngles);
            }
            else if(m_pressedButtonR == Joycon.Button.SHOULDER_1)
            {
                rightJoyconBase = ConvertRot(rightJoycon.GetVector());
            }
            else
            {
                Quaternion prevRot = rightHand.transform.rotation;
                rightHand.transform.rotation = rightGusokuBase;
                Quaternion orientation = ConvertRot(rightJoycon.GetVector());
                Debug.Log(orientation.eulerAngles);
                Vector3 baseAxis = (Quaternion.Inverse(rightJoyconBase) * orientation).eulerAngles;
                baseAxis = new Vector3(baseAxis.x, baseAxis.y, baseAxis.z);
                Debug.Log(orientation.eulerAngles.ToString() + " " + baseAxis);
                rightHand.transform.Rotate(baseAxis);
                rightHand.transform.rotation = Quaternion.Lerp(prevRot, rightHand.transform.rotation, 0.5f);
            }
        }

        // reset
        if (Input.GetKey(KeyCode.R))
        {
            StartCoroutine(DelayMethod(3.0f, () =>
            {
                leftJoyconBase = Quaternion.identity;
                rightJoyconBase = Quaternion.identity;
            }));
        }
    }

    private IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
}

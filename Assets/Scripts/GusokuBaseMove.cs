using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// 瞬きと体の動き(blenderのシェイプキーで対応した箇所)の表現。
public class GusokuBaseMove : MonoBehaviour
{
    // UDP
    [SerializeField]
    private const int LOCAL_PORT = 12345;
    private UdpClient udp;

    Task receiveTask;
    private bool isReceiving = false;

    // eye
    static private GameObject gusoku;
    static private SkinnedMeshRenderer skinnedMeshRenderer;
    static float left_eye, right_eye;
    const float THRESHOLD = 0.16f;

    // face (from vert1 to vert6)
    static private GameObject[] gusokuFace = new GameObject[6];
    static Quaternion[] faceBase = new Quaternion[6];
    readonly float[] lerpT = { 1.0f, 0.8f, 0.6f, 0.8f, 0.9f, 1.0f };
    static Vector3 faceRot;

    // position
    static float BaseY;

    private void OpenServer()
    {
        if (isReceiving) return;
        faceRot = Vector3.zero;
        isReceiving = true;
        if(udp == null)
        {
            udp = new UdpClient(LOCAL_PORT);
        }
        receiveTask = DataReceiveTask();
    }

    private void CloseServer()
    {
        if (!isReceiving) return;
        isReceiving = false;
        if (receiveTask != null && receiveTask.Status == TaskStatus.Running)
        {
            receiveTask.Wait();
        }
    }

    async Task DataReceiveTask()
    {
        await Task.Run(() =>
        {
            while (isReceiving)
            {
                // Debug.log("TEST");
                try
                {
                    IPEndPoint remoteEP = null;
                    byte[] data = udp.Receive(ref remoteEP);
                    string[] texts = Encoding.UTF8.GetString(data).Split(' ');
                    // Debug.Log(String.Join(" ", texts));
                    float[] vals = new float[5];
                    for (int i = 0; i < 5; i++)
                    {
                        vals[i] = float.Parse(texts[i]);
                    }

                    // left eye
                    if (vals[3] > THRESHOLD)
                    {
                        left_eye = Mathf.Max(0.0f, left_eye - 0.2f);
                    }
                    else
                    {
                        left_eye = 1.0f;
                    }
                    // right_eye
                    if (vals[4] > THRESHOLD)
                    {
                        right_eye = Mathf.Max(0.0f, right_eye - 0.2f);
                    }
                    else
                    {
                        right_eye = 1.0f;
                    }

                    // face
                    Vector3 tmp_faceRot = new Vector3(vals[0], vals[2], vals[1] / 3);
                    if(-20 < tmp_faceRot.x && tmp_faceRot.x<15 && Mathf.Abs(tmp_faceRot.y)<15 && Mathf.Abs(tmp_faceRot.z) < 15)
                    {
                        if (faceRot == new Vector3(0, 0, 0))
                        {
                            faceRot = tmp_faceRot;
                        }
                        else
                        {
                            faceRot = Quaternion.Lerp(Quaternion.Euler(faceRot), Quaternion.Euler(tmp_faceRot), 0.5f).eulerAngles;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    left_eye = 0.5f + 0.5f * Mathf.Sin(Time.time);
                    right_eye = 0.5f - 0.5f * Mathf.Sin(Time.time);
                }
            }
        });
    }
    
    // Start is called before the first frame update
    void Start()
    {
        gusoku = GameObject.Find("gusoku_mesh");
        skinnedMeshRenderer = gusoku.GetComponent<SkinnedMeshRenderer>();

        left_eye = 0.0f;
        right_eye = 0.0f;
        BaseY = transform.position.y;
        for (int i = 0; i < 6; i++)
        {
            gusokuFace[i] = GameObject.Find("vert" + (i + 1).ToString().PadLeft(2, '0'));
            faceBase[i] = gusokuFace[i].transform.rotation;
        }
        faceRot = new Vector3(0, 0, 0);

        OpenServer();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(left_eye.ToString()+" "+right_eye.ToString());
        // Debug.Log(faceRot.ToString());
        // left eye
        skinnedMeshRenderer.SetBlendShapeWeight(0, 100 * left_eye);
        // right eye
        skinnedMeshRenderer.SetBlendShapeWeight(1, 100 * right_eye);
        // position
        transform.position = new Vector3(transform.position.x, 0.3f * Mathf.Sin(Time.time) + BaseY, transform.position.z);
        // face
        Quaternion faceRotQ = Quaternion.Euler(faceRot);
        for (int i = 0; i < 2; i++)
        {
            Quaternion target = faceBase[i] * faceRotQ;
            gusokuFace[i].transform.rotation = Quaternion.Lerp(faceBase[i], target, lerpT[i]);
        }
        faceRotQ = Quaternion.Euler(new Vector3(-faceRot.x, -faceRot.y, faceRot.z));
        for (int i = 2; i < 6; i++)
        {
            Quaternion target = faceBase[i] * faceRotQ;
            gusokuFace[i].transform.rotation = Quaternion.Lerp(faceBase[i], target, lerpT[i]);
        }
        // Debug.Log(faceRot);
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            CloseServer();
        }
        else
        {
            OpenServer();
        }
    }

    private void OnApplicationQuit()
    {
        CloseServer();
    }
}

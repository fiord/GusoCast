using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private bool isFullScreen = false;

    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1280, 720, isFullScreen);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            isFullScreen = !isFullScreen;
            Screen.SetResolution(1280, 720, isFullScreen);
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}

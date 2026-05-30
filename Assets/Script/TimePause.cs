using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePause : MonoBehaviour
{
    // Start is called before the first frame update
    void PauseGame()
    {
       if( Input.GetKeyDown(KeyCode.Space))
       {
            Time.timeScale = 0;
        }
        
    }
    void ResumeGame()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
           if(Time.timeScale == 0)
            {
                Time.timeScale = 1;
            }
           else
            {
                Time.timeScale = 0;
            }
        }
            
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 如果装配界面开启，禁止暂停逻辑
        if (SpaceshipAssemblyUI.IsUIOpen) return;

        ResumeGame();
    }
}

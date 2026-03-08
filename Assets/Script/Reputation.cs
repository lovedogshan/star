using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Reputation : MonoBehaviour
{
    private int fameValue;

    // Start is called before the first frame update
    void Start()
    {
        fameValue = Mathf.Clamp(fameValue, -100, 100);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   
}

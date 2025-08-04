using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animatioPlayer_SpalshLight_Script : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayAnimation()
    {
        GetComponent<Animator>().Play("SplashLight");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
    void Start()
    {
        GetComponent<GamePlayManager>().Init();        
    }
}

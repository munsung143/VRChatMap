using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubetest : MonoBehaviour
{
    [SerializeField] Transform trs;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        trs.Rotate(0, Time.deltaTime * 360, 0);
    }
}

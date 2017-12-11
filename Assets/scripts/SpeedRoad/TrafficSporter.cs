using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class TrafficSporter : MonoBehaviour {
    float maxVelocity;
    float velocity;
    List<Vector3> lstTarget = new List<Vector3>();
    int current;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        

    }

    public void Init(List<Vector3> lst, float dur)
    {
        current = 0;
        lstTarget = lst;
        transform.localPosition = lstTarget[current];

        transform.DOPath(lstTarget.ToArray(), dur);
    }

    
}

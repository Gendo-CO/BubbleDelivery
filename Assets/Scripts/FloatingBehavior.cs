using UnityEngine;
using System;
using System.Collections;

public class FloatingBehavior : MonoBehaviour
{
    float originalY;

    public float floatStrength = 0.4f; // You can change this in the Unity Editor to 
                                    // change the range of y positions that are possible.

    void Start()
    {
        this.originalY = this.transform.localPosition.y;
    }

    void Update()
    {
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            originalY + ((float)Math.Sin(Time.time) * floatStrength),
            transform.localPosition.z);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverableHouse : MonoBehaviour
{
    public GameObject mailIcon;
    public bool iWantPackages = false;

    public float timeToWaitMin = 3f;
    public float timeToWaitMax = 10f;

    void PackWant()
    {
        if (iWantPackages)
        {
            float time = Random.Range(timeToWaitMin, timeToWaitMax);
            WaitAndAskForPack(time);
        }
    }

    IEnumerator WaitAndAskForPack(float time)
    {
        // suspend execution for 5 seconds
        yield return new WaitForSeconds(time);
        iWantPackages = true;
        mailIcon.SetActive(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (iWantPackages && collision.gameObject.CompareTag("Bubble") && collision.gameObject.GetComponent<BubblePersonDeliveryScript>().hasBox) 
        {
            iWantPackages = false;
            mailIcon.SetActive(false);
        }
    }
}

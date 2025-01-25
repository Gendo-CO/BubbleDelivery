using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DudeMovement : MonoBehaviour
{
    //click to select, turns on node selection.

    public List<GameObject> nodesToTravelTo = new List<GameObject>();
    public bool currSelected = false;

    private float lerpSpeed;

    private void OnMouseDown()
    {
        currSelected = !currSelected;
    }

    private void MoveToNextNode(GameObject currNode)
    {
        Vector3.Lerp(transform.position, currNode.transform.position, lerpSpeed);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

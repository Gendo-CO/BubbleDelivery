using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BubblePersonDeliveryScript : MonoBehaviour
{
    bool hasBox;

    public SpriteRenderer spriteRenderer;
    public List<Sprite> spriteAssets = new List<Sprite>();

    private void ChangeSprite()
    {
        if (hasBox) { spriteRenderer.sprite = spriteAssets[1]; }
        else { spriteRenderer.sprite = spriteAssets[0]; }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("PostOffice"))
        {
            hasBox = true;
        }
        else if(collision.gameObject.CompareTag("House"))
        {
            hasBox = false;
        }
    }
}

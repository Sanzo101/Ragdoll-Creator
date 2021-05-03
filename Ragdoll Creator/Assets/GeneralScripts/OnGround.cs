using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnGround : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        //This sets the isGround bool to true in the player controller
        //This is so when the body is on the ground after falling or dieing we still know if it is grounded or not
        if(other.gameObject.layer ==8)//8 is the Ground layer
        {           
            this.GetComponentInParent<PlayerController>().isGrounded = true;          
        }
        
    }
}

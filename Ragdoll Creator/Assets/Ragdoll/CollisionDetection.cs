using UnityEngine;


public class CollisionDetection : MonoBehaviour
{
   
    //  VARIABLES
    SlaveControl slaveController;
    LayerMask layerMask;
    public float DeathMassNumber = 3.0f;
    private float Impact = 20.0f;

    //private CapsuleCollider[] SlaveCollider;
    //private BoxCollider[] SlaveColliderBox;
    private GameObject Slave;
    Collider[] RagdollCollider;
    void Start()
    {
        HumanoidSetUp setUp = this.GetComponentInParent<HumanoidSetUp>();
        slaveController = setUp.slaveController;      
        layerMask = setUp.dontLooseStrengthLayerMask;
        Slave = GameObject.FindGameObjectWithTag("Slave");
        RagdollCollider = GetComponentsInChildren<Collider>();
    }
    private bool CheckIfLayerIsInLayerMask(int layer)
    {
        return layerMask == (layerMask | (1 << layer));
    }
    private void OnCollisionEnter(Collision collision)
    {

        // Plays body hitting floor sound
        //depends on how hard the collision is
        //only playes if player is either dead of falling
      if (!slaveController.animFollow.isAlive)
      {
            if (collision.relativeVelocity.magnitude > Impact)
            {
                Slave.GetComponent<SFX>().PlayBodyFallSound();
            }
        }
      if(slaveController.animFollow.falling)
      {
            if (collision.relativeVelocity.magnitude > Impact)
            {
                Slave.GetComponent<SFX>().PlayBodyFallSound();
            }
        }

            
        //for (int i = 0; i < Slave.Length; i++)
        //{
        //    if (Slave[i].GetComponent<CapsuleCollider>() != null)
        //    {
        //        for (int j = 0; j < SlaveCollider.Length; j++)
        //        {
        //            SlaveCollider[j] = Slave[i].GetComponent<CapsuleCollider>();
        //            if (collision.gameObject.tag.Contains("Master"))
        //            {
        //                Physics.IgnoreCollision(collision.collider, (SlaveCollider[j]));                       

        //            }
        //        }
        //    }
        //    if(Slave[i].GetComponent<BoxCollider>() != null)
        //    {
        //        for (int k = 0; k < SlaveCollider.Length; k++)
        //        {
        //            SlaveColliderBox[k] = Slave[i].GetComponent<BoxCollider>();
        //            if (collision.gameObject.tag.Contains("Master"))
        //            {                     
        //                Physics.IgnoreCollision(collision.collider, (SlaveColliderBox[k]));

        //            }
        //        }             
        //    }   
        //}
        if (!CheckIfLayerIsInLayerMask(collision.gameObject.layer))
        {
            slaveController.currentNumberOfCollisions++;
            if (collision.gameObject.GetComponent<Rigidbody>() != null) //Kills player if big object hits (change to specific layer mask)
            {
                //Change the Deathmass number in the inspector
                //This number is the mass of the rigid body hittin the player
                if (collision.gameObject.GetComponent<Rigidbody>().mass > DeathMassNumber)
                {
                    foreach(Collider col in RagdollCollider)
                    {
                        if (col.GetComponent<Rigidbody>() != null)
                        {
                            col.GetComponent<Rigidbody>().AddExplosionForce(50f, collision.transform.position, 25f, 25f, ForceMode.Impulse);
                        }
                    }
                    //Plays bomb sound when hit by big mass

                    collision.gameObject.GetComponent<SFX>().PlayBombSound();
                    slaveController.Die();             
                }                
            }
        }  
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!CheckIfLayerIsInLayerMask(collision.gameObject.layer))
        {
            slaveController.currentNumberOfCollisions--;
        }
    }

}


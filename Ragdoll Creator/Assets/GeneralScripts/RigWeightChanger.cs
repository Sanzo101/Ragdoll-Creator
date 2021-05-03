using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RigWeightChanger : MonoBehaviour
{
    //Changes weight of rig
    public enum ElbowType
    {
        LeftElbow,
        RightElbow
    }
    public float RightElbowWeight,LeftElbowWeight;
    public ElbowType ChooseElbow;
    // Update is called once per frame
    void Update()
    {
        switch (ChooseElbow)
        {
            case ElbowType.LeftElbow:
                WeightChanger(LeftElbowWeight);
                break;
            case ElbowType.RightElbow:
                WeightChanger(RightElbowWeight);
                break;
        }      
    }
    public void WeightChanger(float RigWeigthNum)
    {
        this.GetComponent<MultiAimConstraint>().weight = RigWeigthNum;
    }
}

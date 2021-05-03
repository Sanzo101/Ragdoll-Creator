﻿using UnityEngine;
using System;


public class HumanoidSetUp : MonoBehaviour
{
    // THIS NEEDS TO BE SET UP IN INSPECTOR 
    [Tooltip("Static animator hips.")]
    public Transform masterRoot;
    [Tooltip("Ragdoll hips.")]
    public Transform slaveRoot;
    [Tooltip("Camera following the character.")]
    public Camera characterCamera;
    [Tooltip("Ragdoll looses strength when colliding with other objects except for objects with layers contained in this mask.")]
    public LayerMask dontLooseStrengthLayerMask;

    // THIS IS SET UP AUTOMATICALLY
    [NonSerialized]
    public PlayerController masterController;
    [NonSerialized]
    public SlaveControl slaveController;
    [NonSerialized]
    public ConfigurableJoints animFollow;
    [NonSerialized]
    public Animator anim;
    [NonSerialized]
    public Animator Slaveanim;

    // Awake() is called before all Start() methods
    void Awake()
    {
        if (masterRoot == null) Debug.LogError("masterRoot not assigned.");
        if (slaveRoot == null) Debug.LogError("slaveRoot not assigned.");
        if (characterCamera == null) Debug.LogError("characterCamera not assigned.");

        masterController = this.GetComponentInChildren<PlayerController>();
        if (masterController == null) Debug.LogError("MasterControler not found.");

        slaveController = this.GetComponentInChildren<SlaveControl>();
        if (slaveController ==null) Debug.LogError("SlaveController not found.");

        animFollow = this.GetComponentInChildren<ConfigurableJoints>();
        if (animFollow == null) Debug.LogError("AnimationFollowing not found.");

        anim = this.GetComponentInChildren<Animator>();
        if (anim == null) Debug.LogError("Animator not found.");

        Slaveanim = slaveRoot.gameObject.GetComponentInParent<Animator>();          
        if (Slaveanim == null) Debug.LogError("Slave Animator not found.");
    }
}

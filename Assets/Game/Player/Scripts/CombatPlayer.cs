using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatPlayer : MonoBehaviour
{
    private static CombatPlayer instance;
    public static CombatPlayer Instance => instance;

    public Transform TR;
    public Rigidbody RB;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        
    }

}

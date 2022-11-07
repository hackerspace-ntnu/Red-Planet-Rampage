using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBody : MonoBehaviour
{
    [SerializeField]
    private GunStats stats;

    // Base stats of the gun
    public GunStats Stats { get => Instantiate(stats); }

    // Where to attach barrel
    public Transform attachmentSite;
}

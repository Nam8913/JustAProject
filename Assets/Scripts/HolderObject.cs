using System.Collections.Generic;
using UnityEngine;

public class HolderObject : MonoBehaviour
{
    private void Start()
    {
        HolderManager.AddHolderObject(gameObject.name, this);

        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class respawnScript : MonoBehaviour
{

    public float minYPosition;
    public Vector3 position;

    Rigidbody objectToRespawn;
    // Start is called before the first frame update
    void Start()
    {
        
        objectToRespawn= GetComponent<Rigidbody>();
        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
             if(objectToRespawn.position.y < minYPosition)
        {
            objectToRespawn.position = position;
        }
    }
}

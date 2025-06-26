using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScript : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject gameObject;

    void Start()
    {
        gameObject.SetActive(false); // Initially set the object to inactive


    }

    // Update is called once per frame
    void Update()
    {



    }
    public void setVisible()
    {
        // This function will handle the visibility of the object
        // You can implement your visibility logic here
        // For example, you might want to enable or disable the GameObject
        gameObject.SetActive(true); // Example: make the object visible



    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeModel : MonoBehaviour
{
    private int remainWood;
    void Start()
    {
        remainWood = 200;
        gameObject.tag = "Tree";
    }

    public void Collect(int amount)
    {
        remainWood -= amount;
        if (remainWood <= 0)
        {
            
            Destroy(gameObject);
        }
    }
    
        
    
}

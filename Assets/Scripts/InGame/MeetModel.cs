using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeatModel : MonoBehaviour
{
    private int remainMeet;
    void Start()
    {
        remainMeet = 200;
        gameObject.tag = "Meat";
    }

    public void Collect(int amount)
    {
        remainMeet -= amount;
        if (remainMeet <= 0)
        {

            Destroy(gameObject);
        }
    }

}
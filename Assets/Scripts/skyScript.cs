using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skyScript : MonoBehaviour
{
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0))
        {
            starSystemScript.S.newStar();
        }
    }
}

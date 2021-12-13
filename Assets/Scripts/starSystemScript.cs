using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;


public class starSystemScript : MonoBehaviour
{
    private int _constSelect = 1;
    private int _numStars = 0;
    private Color trans = Color.white;
    private readonly Color solid = Color.white;
    private readonly int[] pointerPos = {456, 324, 193, 33, -113, -326, -502};

    public Image pointer;
    public Image[] selectors;

    public static starSystemScript S;
    void Awake()
    {
        if (S == null) S = this;
        trans.a = 0.4f;
    }

    public void newStar()
    {
        currentConstellation().GetComponent<ConstellationScript>().spawnStar();
        musicManagerScript.M.updateCounts();
        _numStars++;
    }

    public int currentConstellationNum()
    {
        return _constSelect;
    }

    public GameObject currentConstellation()
    {
        return this.transform.Find("C" + _constSelect).gameObject;
    }
    
    public void changeConstellation(int num)
    {
        int old = _constSelect;
        selectors[old - 1].color = trans;
        selectors[num - 1].color = solid;
        _constSelect = num;

        var transform1 = pointer.transform;
        Vector3 currPos = transform1.position;
        Vector3 newPos = currPos;
        newPos.y = pointerPos[num - 1]+1080;
        transform1.position = newPos;
    }
}

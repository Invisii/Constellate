using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class musicManagerScript : MonoBehaviour
{
    //C1 = String Pads
    //C2 = 
    //C3 = Music Box
    
    public static musicManagerScript M;

    public int count1, count2, count3 = 0;
    public GameObject pads;
    public AudioClip[] chimes;

    private GameObject c1, c2, c3;
    private List<GameObject> _padnotes = new List<GameObject>();

    private void Start()
    {
        M = this;
        c1 = GameObject.Find("C1");
        c2 = GameObject.Find("C2");
        c3 = GameObject.Find("C3");
        
        foreach (Transform child in pads.transform)
        {
            _padnotes.Add(child.gameObject);
        }
    }

    public void updateCounts()
    {
        if (count1 < c1.GetComponent<ConstellationScript>().getNumStars())
        {
            padStart();
            count1 = c1.GetComponent<ConstellationScript>().getNumStars();
        }
        
        if (count2 < c2.GetComponent<ConstellationScript>().getNumStars())
        {
            count2 = c2.GetComponent<ConstellationScript>().getNumStars();
        }
        
        if (count3 < c3.GetComponent<ConstellationScript>().getNumStars())
        {
            count3 = c3.GetComponent<ConstellationScript>().getNumStars();
        }
    }
    
    // Pads
    private void padStart()
    {
        if (count1/2 < _padnotes.Count && count1 % 2 == 1)
        {
            Debug.Log(_padnotes[count1/2].name);
            _padnotes[count1/2].GetComponent<PadNoteScript>().startPad();
        }
    }

    public AudioClip rndSingleNote()
    {
        int r = Random.Range(0, chimes.Length);
        return chimes[r];
    }
}

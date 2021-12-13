using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PadNoteScript : MonoBehaviour {
    private AudioSource padNote;

    private float initVol, holdTime, transTime;
    private float currVol, newVol;

    //Init note at random volume
    //Determine length of note before transition
    
    //Transition Note:
    //Pick new landing volume
    //Choose time to transition
    //Adjust volume over time
    //Choose Hold Time
    //Hold, then rerun after time has passed.

    void Start() {
        padNote = GetComponent<AudioSource>();
        initVol = Random.Range(0f, 1f);
        padNote.volume = initVol;
    }

    public void startPad()
    {
        padNote.Play();
        NoteSetup();
    }

    private void NoteSetup()
    {
        holdTime = Random.Range(4f, 10f);
        currVol = padNote.volume;
        newVol = currVol > 0.5f ? Random.Range(0f, 0.5f) : Random.Range(.5f, 1f);
        transTime = Random.Range(2f, 10f);
        StartCoroutine(NoteChange());
    }

    private IEnumerator NoteChange()
    {
        //play vol for hold time
        yield return new WaitForSeconds(holdTime);
        
        //now transition
        if (newVol > currVol) //increasing
        {
            while (padNote.volume <= newVol)
            {
                padNote.volume += 0.05f;
                yield return new WaitForSeconds(transTime/100f);
            }
        }
        else //decreasing
        {
            while (padNote.volume >= newVol)
            {
                padNote.volume -= 0.05f;
                yield return new WaitForSeconds(transTime/100f);
            }
        }
        NoteSetup();
        yield return null;
    }

    private IEnumerator startWait()
    {
        float seconds = Random.Range(0f, 3f);
        yield return new WaitForSeconds(seconds);
        padNote.Play();
        NoteSetup();
        yield return null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneOneScript : MonoBehaviour
{
    [SerializeField] DialogueScript dialogueScript;
    [SerializeField] PlayableDirector playableDirector;
    int currentTime;
    bool s1;
    bool s2;
    bool s3;
    bool s4;
    bool s5;
    bool s6;

    void Update()
    {
        currentTime = (int)playableDirector.time;
        Debug.Log(currentTime);

        if(currentTime == 9)
        {
            dialogueScript.dialogue("I'm so tired...",0.01f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RetinaNetworking.Server;

public class MoodColorChanger : MonoBehaviour
{

    public ConnectionParams connectionParams;
    public SlimeSettings activeSlimeSettings;

    private Mood lastMood;
    private Mood currentMood;

    private void Start()
    {
        lastMood = connectionParams.FetchMood();
        currentMood = lastMood;

    }

    private void Update()
    {
        
    }

}

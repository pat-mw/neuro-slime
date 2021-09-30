using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace RetinaNetworking.Server
{
    [CreateAssetMenu(menuName = "MOOD REPORT")]
    public class MoodReport : SerializedScriptableObject
    {
        public Mood inferedMood = Mood.NEUTRAL;
        public string perceivedMood = "neutral";
        [Range(0, 5)] public int happy = 0;
        [Range(0, 5)] public int control = 0;
        [Range(0, 5)] public int excited = 0;

        public void SetInferedMood(Mood _mood)
        {
            inferedMood = _mood;
        }
    }
}

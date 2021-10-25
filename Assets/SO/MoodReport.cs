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

        public void SetInferedMood(Mood _mood)
        {
            inferedMood = _mood;
        }
    }
}

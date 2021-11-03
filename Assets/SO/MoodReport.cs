using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using ScriptableObjectArchitecture;

namespace RetinaNetworking.Server
{
    [CreateAssetMenu(menuName = "MOOD REPORT")]
    public class MoodReport : SerializedScriptableObject
    {
        public Mood inferedMood = Mood.NEUTRAL;

        public List<Mood> moodLog = new List<Mood>();

        public void SetInferedMood(Mood _mood)
        {
            // back up mood and set new mood
            moodLog.Add(inferedMood);
            inferedMood = _mood;
        }

        public void ClearMoodLog()
        {
            moodLog.Clear();
        }


    }
}

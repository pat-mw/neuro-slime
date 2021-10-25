using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace RetinaNetworking.Server
{
    [CreateAssetMenu(menuName = "CONNECTION PARAMS")]
    public class ConnectionParams : SerializedScriptableObject
    {
        [Header("DEBUG")]
        public bool debugMode = false;

        [Header("PARAMS")]
        [HideIf("debugMode")]
        [SerializeField] int userID;
        [HideIf("debugMode")]
        [SerializeField] string fullName;
        [HideIf("debugMode")]
        [SerializeField] string username;
        [HideIf("debugMode")]
        [SerializeField] string jwt;

        [Header("PARAMS")]
        [ShowIf("debugMode", true)]
        [SerializeField] int debugUserID = 186;
        [ShowIf("debugMode", true)]
        [SerializeField] string debugFullName = "Papa";
        [ShowIf("debugMode", true)]
        [SerializeField] string debugUsername = "popsjohns";
        [ShowIf("debugMode", true)]
        [SerializeField] string debugJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MTg2LCJpYXQiOjE2MzI5OTYzNjQsImV4cCI6MTYzNTU4ODM2NH0.4TIciP6yiu8HBzLyEYGV1RmHP6nyHCRnprDWjmdPUfo";
        [Header("SESSION")]
        [SerializeField] string sessionToken;
        [SerializeField] int sessionID;

        [Header("MOOD")]
        [InlineEditor(InlineEditorModes.FullEditor)][OdinSerialize] MoodReport moodReport;

        public void SetParams(int _userID, string _username, string _JWT)
        {
            userID = _userID;
            username = _username;
            jwt = _JWT;
        }

        public void SetName(string _name)
        {
            fullName = _name;
        }

        public void SetMood(Mood _mood)
        {
            moodReport.SetInferedMood(_mood);
        }

        public void SetSessionToken(string _sessionToken)
        {
            sessionToken = _sessionToken;
        }
        public void SetSessionID(int _sessionID)
        {
            sessionID = _sessionID;
        }

        public void Reset()
        {
            userID = -1;
            username = "";
            jwt = "";
            sessionToken = "";
            moodReport.SetInferedMood(Mood.NEUTRAL);
            sessionID = -1;
        }

        public string SessionToken()
        {
            return sessionToken;
        }

        public int UserID()
        {
            if (debugMode)
            {
                return debugUserID;
            }
            else
            {
                return userID;
            }
        }

        public string Name()
        {
            if (debugMode)
            {
                return debugFullName;
            }
            else
            {
                return fullName;
            }
        }

        public string Username()
        {
            if (debugMode)
            {
                return debugUsername;
            }
            else
            {
                return username;
            }
        }

        public string JWT()
        {
            if (debugMode)
            {
                return debugJwt;
            }
            else
            {
                return jwt;
            }
        }

        public int SessionID()
        {
            return sessionID;
        }

        public MoodReport MoodReport()
        {
            return moodReport;
        }
    }


    public enum Mood
    {
        POSITIVE,
        NEGATIVE,
        NEUTRAL
    }
}
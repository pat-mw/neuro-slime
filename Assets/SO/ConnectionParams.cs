using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using ScriptableObjectArchitecture;
using Wenzil.Console;

namespace RetinaNetworking.Server
{
    [CreateAssetMenu(menuName = "CONNECTION PARAMS")]
    public class ConnectionParams : SerializedScriptableObject
    {

        [Header("DEBUG")]
        public bool debugMode = false;

        [Header("MOOD CHANGED")]
        public GameEvent onMoodChanged;
        public GameEvent onCalibrationError;

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
        [SerializeField] int sessionId;

        [Header("INFERENCE")]
        [SerializeField] bool calibrationReceived = false;
        [SerializeField] int inferenceId;

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
            Console.Log($"SETTING MOOD! : {_mood}");
            moodReport.SetInferedMood(_mood);
            onMoodChanged.Raise();
        }

        public void SetSessionToken(string _sessionToken)
        {
            sessionToken = _sessionToken;
        }
        public void SetSessionID(int _sessionID)
        {
            sessionId = _sessionID;
        }

        public void SetInferenceID(int _inferenceId)
        {
            inferenceId = _inferenceId;
        }

        public void SetCalibrationReceived(bool received)
        {
            Wenzil.Console.Console.Log($"CALIBRATION RECEIVED! : {received}");
            calibrationReceived = received;

            if (received == false){
                onCalibrationError.Raise();
            }
        }

        public void Reset()
        {
            userID = -1;
            username = "";
            jwt = "";
            sessionToken = "";
            moodReport.SetInferedMood(Mood.NEUTRAL);
            sessionId = -1;
            inferenceId = -1;
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
            return sessionId;
        }

        public int InferenceID()
        {
            return inferenceId;
        }

        public MoodReport MoodReport()
        {
            return moodReport;
        }

        public Mood FetchMood()
        {
            return moodReport.inferedMood;
        }

        public bool CalibrationReceived()
        {
            return calibrationReceived;
        }
    }


    public enum Mood
    {
        POSITIVE,
        NEGATIVE,
        NEUTRAL
    }
}
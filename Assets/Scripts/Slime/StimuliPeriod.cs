using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using ScriptableObjectArchitecture;

public class StimuliPeriod : MonoBehaviour
{
    public GameEvent onStartStimuli;
    public GameEvent onEndStimuli;
    public GameEvent onShowTrails;
    public GameEvent onDeactivateSimulation;
    public GameEvent onStartRecording;
    public GameEvent onStopRecording;
    public GameEvent onStartGraphing;
    public GameEvent onStopGraphing;
    public GameEvent onTakeScreenshot;
    public GameEvent onStopReceivingBrainData;

    private float stimCycleDuration = GlobalConfig.EPOCH_DURATION;
    public int stimCycleCount = 6;
    int cyclesElapsed = 0;

    private void Awake()
    {
        onStartStimuli.AddListener(StartStim);
        onEndStimuli.AddListener(EndStim);
    }

    void StartStim()
    {
        cyclesElapsed = 0;
        onShowTrails.Raise();
        onStartRecording.Raise();
        onStartGraphing.Raise();
        StimUpdate();
        // then enter cycle of fetching mood every x seconds
    }


    async private void StimUpdate()
    {
        cyclesElapsed += 1;
        if (cyclesElapsed <= stimCycleCount)
        {
            Wenzil.Console.Console.Log($"STIM CYCLE: {cyclesElapsed}");
            await StimDelay(stimCycleDuration);
            StimUpdate();
        }
        else
        {
            onEndStimuli.Raise();
        }
    }

    async UniTask StimDelay(float delayLength)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delayLength), ignoreTimeScale: false);
        return;
    }

    void EndStim()
    {
        Wenzil.Console.Console.Log("Stimuli Period finished - freezing and capturing screenshot");
        onDeactivateSimulation.Raise();
        onStopReceivingBrainData.Raise();
        onStopRecording.Raise();
        onStopGraphing.Raise();
        onTakeScreenshot.Raise();
    }
}

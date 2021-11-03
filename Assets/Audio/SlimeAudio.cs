using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;

[RequireComponent(typeof(AudioSource))]
public class SlimeAudio : MonoBehaviour
{
    public GameEvent onStartCalib;
    public GameEvent onEndCalib;
    public GameEvent onStartStim;
    public GameEvent onEndStim;
    public GameEvent onReset;
    public GameEvent onSoftReset;

    public AudioClip startAudio;
    public AudioClip calibAudio;
    public AudioClip stimAudio;

    private AudioSource audioSource;
    private List<AudioClip> audioQueue = new List<AudioClip>();

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        onStartCalib.AddListener(QueueCalib);
        onEndCalib.AddListener(EndCalib);
        onStartStim.AddListener(QueueStim);
        onEndStim.AddListener(EndStim);
        onReset.AddListener(QueueStart);
        onSoftReset.AddListener(QueueStart);


        QueueStart();
    }


    void QueueCalib()
    {
        audioQueue.Add(calibAudio);
        return;
    }

    void QueueStim()
    {
        audioQueue.Add(stimAudio);
        return;
    }

    void QueueStart()
    {
        audioQueue.Add(startAudio);
        return;
    }

    void EndCalib()
    {
        audioQueue.Remove(calibAudio);
        return;
    }

    void EndStim()
    {
        audioQueue.Remove(stimAudio);
        return;
    }


    // Update is called once per frame
    void Update()
    {
        if (audioSource.isPlaying)
        {
            return;
        }

        audioSource.Stop();

        AudioClip prevClip = audioSource.clip;
        
        
        if (audioQueue.Count > 0)
            audioQueue.RemoveAt(0);

        if (audioQueue.Count == 0)
        {
            // loop clips when we have nothing queued
            audioQueue.Add(prevClip);
        }

        audioSource.clip = audioQueue[0];
        audioSource.Play();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AwesomeCharts;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using ScriptableObjectArchitecture;

namespace RetinaGraph
{
    public enum GraphChannel
    {
        LEFT,
        RIGHT
    }

    [RequireComponent(typeof(LineChart))]
    public class BrainDataGrappher : SerializedMonoBehaviour
    {
        public GraphChannel channel = GraphChannel.LEFT;
        public BrainData brainData;
        public GameEvent onStartGraphing;
        public GameEvent onStopGraphing;

        public Color m_lineColor = Color.red;
        public float m_lineThickness = 4f;
        public bool m_useBezier = true;

        public DelayMode delayMode = DelayMode.sampleRate;
        public int m_frameRate = 24;
        public int m_noOfDataPoints = 50;

        private LineChart graph;

        private int m_sampleRate = GlobalConfig.SAMPLE_RATE;

        private bool isActive = false;


        private LineDataSet data;

        private LineDataSet dataRef;

        private float[] dataBuffer;
        private float[] dataOriginal;
        private int stepSize;
        private int dataOriginalLength;
        void Start()
        {
            Wenzil.Console.Console.Log($"INITIALISING GRAPHER: {channel}");
            graph = GetComponent<LineChart>();
            onStartGraphing.AddListener(StartGraphing);
            onStopGraphing.AddListener(StopGraphing);

            RefreshGraphSettings();

        }

        void StartGraphing()
        {
            isActive = true;
            RefreshGraphSettings();

            GraphLoop();
        }

        public void RefreshGraphSettings()
        {

            dataOriginalLength = brainData.dataBuffer.size;
            stepSize = Mathf.RoundToInt(dataOriginalLength / m_noOfDataPoints);

            graph.GetChartData().Clear();
            data = new LineDataSet();
            data.LineColor = m_lineColor;
            data.LineThickness = m_lineThickness;
            data.UseBezier = m_useBezier;
            for (int i = 0; i < m_noOfDataPoints; i++)
            {
                // var position = i * stepSize / m_sampleRate
                var position = i;
                data.AddEntry(new LineEntry(position, 0));
            }
            graph.GetChartData().DataSets.Add(data);

            // first data set reference once configured
            dataRef = graph.GetChartData().DataSets[0];
        }

        void StopGraphing()
        {
            isActive= false;
        }

        async void GraphLoop()
        {

            while (isActive)
            {
                //Debug.Log($"ENTERING GRAPH LOOP");
                float[] dataReduced = new float[m_noOfDataPoints];

                float[] dataOriginal = new float[dataOriginalLength];


                int stepSize = Mathf.RoundToInt(dataOriginalLength / m_noOfDataPoints);
                if (stepSize <= 0){
                    stepSize = 1;
                }

                //Debug.Log("First loop - reduce data array");
                for (int i = 0; i < dataOriginalLength; i += stepSize)
                {
                    //Debug.Log($"i: {i} - reduced i: {i / stepSize} - data sample: {brainData.dataBuffer.left[i]}");
                    if (i / stepSize > dataReduced.Length - 1)
                        continue;
                    else
                    {
                        switch (channel)
                        {
                            case GraphChannel.LEFT:
                                dataReduced[i / stepSize] = brainData.dataBuffer.left[i];
                                break;
                            case GraphChannel.RIGHT:
                                dataReduced[i / stepSize] = brainData.dataBuffer.right[i];
                                break;

                        }
                    }
                }

                //Debug.Log("Second Loop - graph reduced array");
                for (int i = 0; i < m_noOfDataPoints; i++)
                {
                    graph.GetChartData().DataSets[0].Entries[i].Value = dataReduced[i];
                    //dataRef.Entries[i].Value = dataReduced[i];

                    if (dataReduced[i] == 0)
                    {
                        Debug.LogWarning($"data point value is zero - check streaming");
                    }

                    //Debug.Log($"i: {i} - value: {dataReduced[i]} - copied: {graph.GetChartData().DataSets[0].Entries[i].Value}");
                }

                // Refresh chart
                graph.SetDirty();

                // delay and repeat
                switch (delayMode)
                {
                    case DelayMode.sampleRate:
                        await UniTask.Delay(1000 / m_sampleRate, ignoreTimeScale: false);
                        break;
                    case DelayMode.frameRate:
                        await UniTask.Delay(1000 / m_frameRate, ignoreTimeScale: false);
                        break;
                }
            }
            Wenzil.Console.Console.Log($"Graph Interrupted: {channel}");
        }

        public enum DelayMode
        {
            sampleRate,
            frameRate
        }

        private async void OnEnable()
        {
            Debug.Log($"ENABLED");
            await UniTask.Delay(1000);
            onStartGraphing.Raise();
        }

        private async void OnDisable()
        {
            Debug.Log($"DISABLED");
            await UniTask.Delay(1000);
            onStartGraphing.Raise();
        }

    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public class FpsCounter : MonoBehaviour
    {
        [Tooltip("Display format\r\ncurrent: {0}, average: {1}, maximum: {2}, minimum: {3}")]
        [Multiline]
        public string DisplayFormat = "{0:F0} fps (AVG {1:F0}, MAX {2:F0}, MIN {3:F0})";

        [Tooltip("Sample count for average, maximum and minimum")]
        public int SampleCount = 120;

        [Tooltip("Refresh rate for Text component")]
        [Range(0f, 10f)]
        public float RefreshRate = 1f;

        [NonSerialized]
        private Text displayText;

        [NonSerialized]
        private float deltaTime = 0f;

        [NonSerialized]
        private float refreshTime = 0f;

        [NonSerialized]
        private long frameCount = 0;

        [NonSerialized]
        private List<float> historyFPS = new List<float>();

        [NonSerialized]
        private float totalFPS = 0f;

        [NonSerialized]
        private float currentFPS = 0f;
        public float CurrentFPS { get { return currentFPS; } }

        [NonSerialized]
        private float averageFPS = 0f;
        public float AverageFPS { get { return averageFPS; } }

        [NonSerialized]
        private float maximumFPS = 0f;
        public float MaximumFPS { get { return maximumFPS; } }

        [NonSerialized]
        private float minimumFPS = 0f;
        public float MinimumFPS { get { return minimumFPS; } }

        private void Start()
        {
            displayText = GetComponent<Text>();
        }

        private void Update()
        {
            deltaTime = Time.unscaledDeltaTime;
            refreshTime += deltaTime;
            frameCount++;
            if (deltaTime > 0f)
            {
                currentFPS = 1f / deltaTime;
            }
            else
            {
                currentFPS = 1f;
            }
            if (historyFPS.Count == SampleCount)
            {
                historyFPS.RemoveAt(0);
            }
            historyFPS.Add(currentFPS);
            totalFPS = 0f;
            maximumFPS = 0f;
            minimumFPS = 0f;
            for (int i = 0; i < historyFPS.Count; i++)
            {
                if (maximumFPS < historyFPS[i])
                {
                    maximumFPS = historyFPS[i];
                }
                if (minimumFPS > historyFPS[i] || minimumFPS == 0f)
                {
                    minimumFPS = historyFPS[i];
                }
                totalFPS += historyFPS[i];
            }
            averageFPS = totalFPS / historyFPS.Count;
            if (refreshTime > RefreshRate)
            {
                refreshTime = 0f;
                if (displayText != null)
                {
                    try
                    {
                        displayText.text = string.Format(DisplayFormat, currentFPS, averageFPS, maximumFPS, minimumFPS);
                    }
                    catch (FormatException)
                    {
                    }
                }
            }
        }
    }
}

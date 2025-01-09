using System;
using System.Collections;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class SpriteAnimator : MonoBehaviour
    {
        [SerializeField]
        private int startIndex = -1;

        [NonSerialized]
        private int spriteIndex = -1;
        public int SpriteIndex
        {
            get
            {
                return spriteIndex;
            }
            set
            {
                spriteIndex = value;
                UpdateSprite();
            }
        }

        public Sprite[] Sprites = new Sprite[0];

        public float StartDelay = 0f;
        public float Duration = 1f;
        public int LoopCount = -1;
        public bool ReversePlay = false;
        public bool PlayOnEnable = true;
        public bool IgnoreTimeScale = true;

        [NonSerialized]
        private bool isPlaying = false;

        [NonSerialized]
        private float interval = 0f;

        [NonSerialized]
        private int numLooping = 0;

        [NonSerialized]
        private Image _image;
        private Image image
        {
            get
            {
                if (_image == null)
                {
                    _image = GetComponent<Image>();
                }
                return _image;
            }
        }

        [NonSerialized]
        private Coroutine updatingRoutine;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            UpdateSprite();
            if (PlayOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if (isActiveAndEnabled && spriteIndex >= 0 && spriteIndex < Sprites.Length)
            {
                image.overrideSprite = Sprites[ReversePlay ? Sprites.Length - spriteIndex - 1 : spriteIndex];
            }
            else
            {
                image.overrideSprite = null;
            }
        }

        private IEnumerator Updating()
        {
            if (IgnoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(StartDelay);
            }
            else
            {
                yield return new WaitForSeconds(StartDelay);
            }

            while (isPlaying)
            {
                UpdateSprite();
                if (IgnoreTimeScale)
                {
                    yield return new WaitForSecondsRealtime(interval);
                }
                else
                {
                    yield return new WaitForSeconds(interval);
                }
                spriteIndex++;
                if (spriteIndex < 0 || spriteIndex >= Sprites.Length)
                {
                    spriteIndex = 0;
                }
                if (LoopCount >= 0)
                {
                    if (spriteIndex == startIndex + 1 || (spriteIndex == 0 && startIndex <= 0) || (spriteIndex == 0 && startIndex >= Sprites.Length - 1))
                    {
                        if (numLooping == LoopCount)
                        {
                            Stop();
                            yield break;
                        }
                        numLooping++;
                    }
                }
            }
        }

        public void Play()
        {
            if (!isPlaying)
            {
                spriteIndex = startIndex;
                UpdateSprite();
                interval = Duration / (Sprites.Length + 1);
                isPlaying = true;
                numLooping = 0;
                updatingRoutine = StartCoroutine(Updating());
            }
        }

        public void Stop()
        {
            isPlaying = false;
            numLooping = 0;
            if (updatingRoutine != null)
            {
                StopCoroutine(updatingRoutine);
            }
        }

#if UNITY_EDITOR
        [NonSerialized]
        private float duration = 1f;

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (duration != Duration)
                {
                    interval = Duration / (Sprites.Length + 1);
                    duration = Duration;
                }
            }
        }

        private void Reset()
        {
            numLooping = 0;
            UpdateSprite();
        }
#endif
    }
}

using System;
using UnityEngine;
using UnityEngine.Events;

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

        public UnityEvent onFinish = new UnityEvent();

        [NonSerialized]
        private float delayed = 0f;

        [NonSerialized]
        private float current = 0f;

        [NonSerialized]
        private bool isFirst = true;

        [NonSerialized]
        private bool isPlaying = false;
        public bool IsPlaying
        {
            get { return isPlaying; }
        }

        [NonSerialized]
        private int skips = 0;

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

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (PlayOnEnable && !isPlaying)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            if (PlayOnEnable)
            {
                Finish();
            }
            else
            {
                Stop();
            }
        }

        private void Update()
        {
            if (isPlaying && Sprites.Length > 0)
            {
                if (isFirst)
                {
                    if (StartDelay > 0f && delayed < StartDelay)
                    {
                        delayed += (IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
                    }
                    else
                    {
                        isFirst = false;
                    }
                    return;
                }

                UpdateSprite();

                current += (IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
                interval = Duration / Sprites.Length;
                skips = (int)(current / interval);
                while (skips > 0)
                {
                    skips--;
                    spriteIndex++;
                    current -= interval;
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
                                Finish();
                                break;
                            }
                            numLooping++;
                        }
                    }
                }
            }
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

        public void Play()
        {
            if (!isPlaying)
            {
                spriteIndex = startIndex;
                UpdateSprite();
                delayed = 0f;
                current = 0f;
                numLooping = 0;
                isPlaying = true;
            }
        }

        public void Stop()
        {
            isPlaying = false;
            isFirst = true;
            numLooping = 0;
        }

        public void Finish()
        {
            isPlaying = false;
            isFirst = true;
            numLooping = 0;
            spriteIndex = -1;
            UpdateSprite();
            if (onFinish != null)
            {
                onFinish.Invoke();
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

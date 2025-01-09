using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Slider))]
    public class OnOffSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Color ColorIsOn = Color.green;
        public Color ColorIsOff = Color.red;
        public Color ColorDisabled = Color.white;

        public bool ToggleOnClick = true;

        [NonSerialized]
        private Slider slider;

        [NonSerialized]
        private Image image;

        [NonSerialized]
        private bool beforeDown = false;

        [NonSerialized]
        private float oldValue = 0f;

        [SerializeField]
        private bool _isOn = false;

        public bool isOn
        {
            get
            {
                if (slider != null)
                {
                    _isOn = slider.normalizedValue >= 0.5f;
                }
                return _isOn;
            }
            set
            {
                SetIsOn(value);
            }
        }

        [Serializable]
        public class OnOffSliderEvent : UnityEvent<bool> { }

        public OnOffSliderEvent onChanged = new OnOffSliderEvent();

        private void OnEnable()
        {
            slider = GetComponent<Slider>();
            slider.onValueChanged.AddListener(SliderValueChanged);
            if (slider.handleRect != null)
            {
                image = slider.handleRect.GetComponent<Image>();
            }
            SetIsOnWithoutNotify(isOn);
            oldValue = slider.value;
        }

        private void OnDisable()
        {
            SetIsOnWithoutNotify(isOn);
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(SliderValueChanged);
                slider = null;
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying)
            {
                if (slider != null && oldValue != slider.value)
                {
                    oldValue = slider.value;
                    SetIsOnWithoutNotify(slider.normalizedValue >= 0.5f);
                }
            }
        }

        private void OnValidate()
        {
            if (slider == null)
            {
                slider = GetComponent<Slider>();
            }
            bool wasOn = slider.normalizedValue >= 0.5f;
            if (wasOn != _isOn)
            {
                SetIsOnWithoutNotify(_isOn);
            }
        }
#endif

        private void SliderValueChanged(float value)
        {
            if (slider != null)
            {
                SetIsOn(slider.normalizedValue >= 0.5f);
            }
        }

        public void SetIsOn(bool value)
        {
            if (image != null)
            {
                image.color = enabled ? (value ? ColorIsOn : ColorIsOff) : ColorDisabled;
            }
            if (slider != null)
            {
                bool wasOn = _isOn;
#if UNITY_2019_1_OR_NEWER
                slider.SetValueWithoutNotify(value ? slider.maxValue : slider.minValue);
#else
                Slider.SliderEvent sliderEvent = slider.onValueChanged;
                slider.onValueChanged = new Slider.SliderEvent();
                slider.value = value ? slider.maxValue : slider.minValue;
                slider.onValueChanged = sliderEvent;
#endif
                oldValue = slider.value;
                if (wasOn != value)
                {
                    _isOn = value;
                    onChanged.Invoke(value);
                }
            }
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            if (image != null)
            {
                image.color = enabled ? (value ? ColorIsOn : ColorIsOff) : ColorDisabled;
            }
            if (slider != null)
            {
#if UNITY_2019_1_OR_NEWER
                slider.SetValueWithoutNotify(value ? slider.maxValue : slider.minValue);
#else
                Slider.SliderEvent sliderEvent = slider.onValueChanged;
                slider.onValueChanged = new Slider.SliderEvent();
                slider.value = value ? slider.maxValue : slider.minValue;
                slider.onValueChanged = sliderEvent;
#endif
                oldValue = slider.value;
                _isOn = value;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (ToggleOnClick && eventData.button == PointerEventData.InputButton.Left)
            {
                beforeDown = slider.normalizedValue >= 0.5f;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (ToggleOnClick && eventData.button == PointerEventData.InputButton.Left)
            {
                if (eventData.pointerCurrentRaycast.gameObject == eventData.pointerPressRaycast.gameObject)
                {
                    bool afterUp = slider.normalizedValue >= 0.5f;
                    if (beforeDown == afterUp)
                    {
                        SetIsOn(!beforeDown);
                    }
                }
            }
        }
    }
}

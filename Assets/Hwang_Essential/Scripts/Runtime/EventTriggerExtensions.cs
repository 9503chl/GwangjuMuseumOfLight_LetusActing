using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine
{
    public static class EventTriggerExtensions
    {
        private struct AxisEventInfo
        {
            public EventTrigger eventTrigger;
            public EventTriggerType eventID;
            public UnityAction<BaseEventData> baseCallback;
            public UnityAction<AxisEventData> axisCallback;

            public AxisEventInfo(EventTrigger eventTrigger, EventTriggerType eventID, UnityAction<BaseEventData> baseCallback, UnityAction<AxisEventData> axisCallback)
            {
                this.eventTrigger = eventTrigger;
                this.eventID = eventID;
                this.baseCallback = baseCallback;
                this.axisCallback = axisCallback;
            }
        }

        private struct PointerEventInfo
        {
            public EventTrigger eventTrigger;
            public EventTriggerType eventID;
            public UnityAction<BaseEventData> baseCallback;
            public UnityAction<PointerEventData> pointerCallback;

            public PointerEventInfo(EventTrigger eventTrigger, EventTriggerType eventID, UnityAction<BaseEventData> baseCallback, UnityAction<PointerEventData> pointerCallback)
            {
                this.eventTrigger = eventTrigger;
                this.eventID = eventID;
                this.baseCallback = baseCallback;
                this.pointerCallback = pointerCallback;
            }
        }

        private static List<AxisEventInfo> axisEvents = new List<AxisEventInfo>();
        private static List<PointerEventInfo> pointerEvents = new List<PointerEventInfo>();

        private static EventTrigger.Entry GetTrigger(EventTrigger eventTrigger, EventTriggerType eventID)
        {
            foreach (EventTrigger.Entry trigger in eventTrigger.triggers)
            {
                if (trigger.eventID == eventID)
                {
                    return trigger;
                }
            }
            return null;
        }

        public static void AddListener(this EventTrigger eventTrigger, EventTriggerType eventID, UnityAction<BaseEventData> baseCallback)
        {
            EventTrigger.Entry trigger = GetTrigger(eventTrigger, eventID);
            if (trigger == null)
            {
                trigger = new EventTrigger.Entry();
                trigger.eventID = eventID;
                eventTrigger.triggers.Add(trigger);
            }
            trigger.callback.AddListener(baseCallback);
        }

        public static void AddListener(this EventTrigger eventTrigger, EventTriggerType eventID, UnityAction<AxisEventData> axisCallback)
        {
            UnityAction<BaseEventData> baseCallback = delegate (BaseEventData eventData)
            {
                if (axisCallback != null) axisCallback((AxisEventData)eventData);
            };
            axisEvents.Add(new AxisEventInfo(eventTrigger, eventID, baseCallback, axisCallback));
            AddListener(eventTrigger, eventID, baseCallback);
        }


        public static void AddListener(this EventTrigger eventTrigger, EventTriggerType eventID, UnityAction<PointerEventData> pointerCallback)
        {
            UnityAction<BaseEventData> baseCallback = delegate (BaseEventData eventData)
            {
                if (pointerCallback != null) pointerCallback((PointerEventData)eventData);
            };
            pointerEvents.Add(new PointerEventInfo(eventTrigger, eventID, baseCallback, pointerCallback));
            AddListener(eventTrigger, eventID, baseCallback);
        }

        public static void RemoveListener(this EventTrigger eventTrigger, EventTriggerType eventID, UnityAction<BaseEventData> baseCallback)
        {
            EventTrigger.Entry trigger = GetTrigger(eventTrigger, eventID);
            if (trigger != null && baseCallback != null)
            {
                trigger.callback.RemoveListener(baseCallback);
            }
        }

        public static void RemoveListener(this EventTrigger eventTrigger, EventTriggerType eventID, UnityAction<AxisEventData> axisCallback)
        {
            EventTrigger.Entry trigger = GetTrigger(eventTrigger, eventID);
            if (trigger != null && axisCallback != null)
            {
                for (int i = 0; i < axisEvents.Count; i++)
                {
                    if (axisEvents[i].eventTrigger == eventTrigger && axisEvents[i].eventID == eventID && axisEvents[i].axisCallback == axisCallback)
                    {
                        trigger.callback.RemoveListener(axisEvents[i].baseCallback);
                        axisEvents.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public static void RemoveListener(this EventTrigger eventTrigger, EventTriggerType eventID, UnityAction<PointerEventData> pointerCallback)
        {
            EventTrigger.Entry trigger = GetTrigger(eventTrigger, eventID);
            if (trigger != null && pointerCallback != null)
            {
                for (int i = 0; i < pointerEvents.Count; i++)
                {
                    if (pointerEvents[i].eventTrigger == eventTrigger && pointerEvents[i].eventID == eventID && pointerEvents[i].pointerCallback == pointerCallback)
                    {
                        trigger.callback.RemoveListener(pointerEvents[i].baseCallback);
                        pointerEvents.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public static void RemoveAllListeners(this EventTrigger eventTrigger)
        {
            for (int i = axisEvents.Count - 1; i >= 0; i--)
            {
                if (axisEvents[i].eventTrigger == eventTrigger)
                {
                    axisEvents.RemoveAt(i);
                }
            }
            for (int i = pointerEvents.Count - 1; i >= 0; i--)
            {
                if (pointerEvents[i].eventTrigger == eventTrigger)
                {
                    pointerEvents.RemoveAt(i);
                }
            }
            foreach (EventTrigger.Entry trigger in eventTrigger.triggers)
            {
                trigger.callback.RemoveAllListeners();
            }
            eventTrigger.triggers.Clear();
        }

        public static void RemoveAllListeners(this EventTrigger eventTrigger, EventTriggerType eventID)
        {
            EventTrigger.Entry trigger = GetTrigger(eventTrigger, eventID);
            if (trigger != null)
            {
                for (int i = axisEvents.Count - 1; i >= 0; i--)
                {
                    if (axisEvents[i].eventTrigger == eventTrigger && axisEvents[i].eventID == eventID)
                    {
                        axisEvents.RemoveAt(i);
                    }
                }
                for (int i = pointerEvents.Count - 1; i >= 0; i--)
                {
                    if (pointerEvents[i].eventTrigger == eventTrigger && pointerEvents[i].eventID == eventID)
                    {
                        pointerEvents.RemoveAt(i);
                    }
                }
                trigger.callback.RemoveAllListeners();
                eventTrigger.triggers.Remove(trigger);
            }
        }
    }
}

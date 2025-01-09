using System;
using System.Collections;

namespace UnityEngine
{
    [Serializable]
    [CreateAssetMenu(fileName = "Custom Cursor", menuName = "Preset/Custom Cursor", order = 1001)]
    public class CustomCursor : ScriptableObject
    {
        public Texture2D Texture;
        public Vector2 HotSpot;
        public CursorMode Mode;

        public Texture2D[] AnimationTextures;

        [Range(0f, 10f)]
        public float AnimationSpeed = 1f;

        [NonSerialized]
        private Coroutine animateRoutine;

        private static MonoBehaviour currentOwner;
        private static CustomCursor currentCursor;

        public CustomCursor()
        {
        }

        public CustomCursor(Texture2D texture, Vector2 hotSpot, CursorMode mode = CursorMode.Auto)
        {
            Texture = texture;
            HotSpot = hotSpot;
            Mode = mode;
        }

        private void StartAnimate()
        {
            if (currentOwner != null && currentOwner.isActiveAndEnabled && AnimationTextures.Length > 0)
            {
                if (animateRoutine == null)
                {
                    animateRoutine = currentOwner.StartCoroutine(AnimateCursor());
                }
            }
        }

        private void StopAnimate()
        {
            if (currentOwner != null && currentOwner.isActiveAndEnabled && AnimationTextures.Length > 0)
            {
                if (animateRoutine != null)
                {
                    currentOwner.StopCoroutine(animateRoutine);
                }
            }
            animateRoutine = null;
        }

        private IEnumerator AnimateCursor()
        {
            int animateIndex = 0;
            float animateDelay;
            while (currentOwner != null && currentOwner.isActiveAndEnabled && AnimationTextures.Length > 0)
            {
                Cursor.SetCursor(AnimationTextures[animateIndex++], HotSpot, Mode);
                if (animateIndex >= AnimationTextures.Length)
                {
                    animateIndex = 0;
                }
                animateDelay = 1f / AnimationTextures.Length;
                while (animateDelay > 0f)
                {
                    animateDelay -= Time.deltaTime * AnimationSpeed;
                    yield return null;
                }
            }
            animateRoutine = null;
        }

        public static void Apply(MonoBehaviour behaviour, CustomCursor customCursor)
        {
            if (customCursor != null && currentCursor != customCursor)
            {
                if (currentCursor != null)
                {
                    currentCursor.StopAnimate();
                }
                currentOwner = behaviour;
                currentCursor = customCursor;
                if (currentCursor.Texture != null)
                {
                    Cursor.SetCursor(currentCursor.Texture, currentCursor.HotSpot, currentCursor.Mode);
                }
                currentCursor.StartAnimate();
            }
        }

        public static void Reset(MonoBehaviour behaviour)
        {
            if (currentOwner == null || currentOwner == behaviour)
            {
                if (currentCursor != null)
                {
                    currentCursor.StopAnimate();
                }
                currentOwner = null;
                currentCursor = null;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        public static void ApplyOrReset(MonoBehaviour behaviour, CustomCursor customCursor)
        {
            if (customCursor != null)
            {
                if (currentOwner == null || currentOwner == behaviour)
                {
                    Apply(behaviour, customCursor);
                }
            }
            else
            {
                Reset(behaviour);
            }
        }
    }
}

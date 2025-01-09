using UnityEngine;

namespace UnityEngine.UI
{
    public static class TextExtensions
    {
        public static Vector2 GetTextPreferredSize(this Text textComponent, string value)
        {
            RectTransform rectTransform = textComponent.rectTransform;
            Vector2 rectSize = rectTransform.rect.size;
            TextGenerationSettings settings = textComponent.GetGenerationSettings(rectSize);
            settings.pivot = rectTransform.pivot;
            settings.font = textComponent.font;
            settings.fontSize = textComponent.fontSize;
            settings.fontStyle = textComponent.fontStyle;
            settings.lineSpacing = textComponent.lineSpacing;
            settings.alignByGeometry = textComponent.alignByGeometry;
            settings.richText = textComponent.supportRichText;
            settings.horizontalOverflow = textComponent.horizontalOverflow;
            settings.verticalOverflow = textComponent.verticalOverflow;
            TextGenerator generator = new TextGenerator();
            Vector2 preferredSize = new Vector2(generator.GetPreferredWidth(value, settings), generator.GetPreferredHeight(value, settings));
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                preferredSize /= canvas.scaleFactor;
            }
            return preferredSize;
        }

        public static bool SetTextWithEllipsis(this Text textComponent, string value)
        {
            bool result = false;
            RectTransform rectTransform = textComponent.rectTransform;
            Vector2 rectSize = rectTransform.rect.size;
            TextGenerationSettings settings = textComponent.GetGenerationSettings(rectSize);
            settings.pivot = rectTransform.pivot;
            settings.font = textComponent.font;
            settings.fontSize = textComponent.fontSize;
            settings.fontStyle = textComponent.fontStyle;
            settings.lineSpacing = textComponent.lineSpacing;
            settings.alignByGeometry = textComponent.alignByGeometry;
            settings.richText = textComponent.supportRichText;
            settings.horizontalOverflow = textComponent.horizontalOverflow;
            settings.verticalOverflow = textComponent.verticalOverflow;
            TextGenerator generator = new TextGenerator();
            generator.Populate(value, settings);
            int characterCountVisible = generator.characterCountVisible;
            if (characterCountVisible < value.Length)
            {
                characterCountVisible = 0;
                for (int i = 1; i < value.Length; i++)
                {
                    generator.Populate(value.Substring(0, i) + "...", settings);
                    if (generator.characterCountVisible - 3 > characterCountVisible)
                    {
                        characterCountVisible = i;
                    }
                }
                value = value.Substring(0, characterCountVisible) + "...";
                result = true;
            }
            textComponent.text = value;
            return result;
        }

        public static bool SetTextAndFitWidth(this Text textComponent, string value, int extraWidth = 0, bool expandOnly = false)
        {
            bool result = false;
            RectTransform rectTransform = textComponent.rectTransform;
            Vector2 rectSize = rectTransform.rect.size;
            TextGenerationSettings settings = textComponent.GetGenerationSettings(rectSize);
            settings.pivot = rectTransform.pivot;
            settings.font = textComponent.font;
            settings.fontSize = textComponent.fontSize;
            settings.fontStyle = textComponent.fontStyle;
            settings.lineSpacing = textComponent.lineSpacing;
            settings.alignByGeometry = textComponent.alignByGeometry;
            settings.richText = textComponent.supportRichText;
            settings.horizontalOverflow = textComponent.horizontalOverflow;
            settings.verticalOverflow = textComponent.verticalOverflow;
            TextGenerator generator = new TextGenerator();
            float preferredWidth = generator.GetPreferredWidth(value, settings);
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                preferredWidth /= canvas.scaleFactor;
            }
            if (!expandOnly || rectSize.x < preferredWidth + extraWidth)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth + extraWidth);
                result = true;
            }
            textComponent.text = value;
            return result;
        }

        public static bool SetTextAndFitHeight(this Text textComponent, string value, int extraHeight = 0, bool expandOnly = false)
        {
            bool result = false;
            RectTransform rectTransform = textComponent.rectTransform;
            Vector2 rectSize = rectTransform.rect.size;
            TextGenerationSettings settings = textComponent.GetGenerationSettings(rectSize);
            settings.pivot = rectTransform.pivot;
            settings.font = textComponent.font;
            settings.fontSize = textComponent.fontSize;
            settings.fontStyle = textComponent.fontStyle;
            settings.lineSpacing = textComponent.lineSpacing;
            settings.alignByGeometry = textComponent.alignByGeometry;
            settings.richText = textComponent.supportRichText;
            settings.horizontalOverflow = textComponent.horizontalOverflow;
            settings.verticalOverflow = textComponent.verticalOverflow;
            TextGenerator generator = new TextGenerator();
            float preferredHeight = generator.GetPreferredHeight(value, settings);
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                preferredHeight /= canvas.scaleFactor;
            }
            if (!expandOnly || rectSize.y < preferredHeight + extraHeight)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight + extraHeight);
                result = true;
            }
            textComponent.text = value;
            return result;
        }
    }
}

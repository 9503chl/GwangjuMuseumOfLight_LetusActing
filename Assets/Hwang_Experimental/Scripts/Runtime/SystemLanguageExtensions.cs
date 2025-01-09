using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace UnityEngine
{
    public static class SystemLanguageExtensions
    {
        private static readonly Dictionary<SystemLanguage, string> specificCodes = new Dictionary<SystemLanguage, string>()
        {
            { SystemLanguage.Afrikaans, "af-ZA" }, // Afrikaans (South Africa)
            { SystemLanguage.Arabic, "ar-SA" }, // Arabic (Saudi Arabia)
            { SystemLanguage.Basque, "eu-ES" }, // Basque (Spain)
            { SystemLanguage.Belarusian, "be-BY" }, // Belarusian (Belarus)
            { SystemLanguage.Bulgarian, "bg-BG" }, // Bulgarian (Bulgaria)
            { SystemLanguage.Catalan, "ca-ES" }, // Catalan (Spain)
            { SystemLanguage.Chinese, "zh-CN" }, // Chinese (Simplified)
            { SystemLanguage.Czech, "cs-CZ" }, // Czech (Czech Republic)
            { SystemLanguage.Danish, "da-DK" }, // Danish (Denmark)
            { SystemLanguage.Dutch, "nl-NL" }, // Dutch (Netherlands)
            { SystemLanguage.English, "en-US" }, // English (United States)
            { SystemLanguage.Estonian, "et-EE" }, // Estonian (Estonia)
            { SystemLanguage.Faroese, "fo-FO" }, // Faroese (Faroe Islands)
            { SystemLanguage.Finnish, "fi-FI" }, // Finnish (Finland)
            { SystemLanguage.French, "fr-FR" }, // French (France)
            { SystemLanguage.German, "de-DE" }, // German (Germany)
            { SystemLanguage.Greek, "el-GR" }, // Greek (Greece)
            { SystemLanguage.Hebrew, "he-IL" }, // Hebrew (Israel)
            { SystemLanguage.Hungarian, "hu-HU" }, // Hungarian (Hungary)
            { SystemLanguage.Icelandic, "is-IS" }, // Icelandic (Iceland)
            { SystemLanguage.Indonesian, "id-ID" }, // Indonesian (Indonesia)
            { SystemLanguage.Italian, "it-IT" }, // Italian (Italy)
            { SystemLanguage.Japanese, "ja-JP" }, // Japanese (Japan)
            { SystemLanguage.Korean, "ko-KR" }, // Korean (South Korea)
            { SystemLanguage.Latvian, "lv-LV" }, // Latvian (Latvia)
            { SystemLanguage.Lithuanian, "lt-LT" }, // Lithuanian (Lithuania)
            { SystemLanguage.Norwegian, "nn-NO" }, // Norwegian Nynorsk (Norway)
            { SystemLanguage.Polish, "pl-PL" }, // Polish (Poland)
            { SystemLanguage.Portuguese, "pt-PT" }, // Portuguese (Portugal)
            { SystemLanguage.Romanian, "ro-RO" }, // Romanian (Romania)
            { SystemLanguage.Russian, "ru-RU" }, // Russian (Russia)
            { SystemLanguage.SerboCroatian, "hr-HR" }, // Croatian (Croatia)
            { SystemLanguage.Slovak, "sk-SK" }, // Slovak (Slovakia)
            { SystemLanguage.Slovenian, "sl-SI" }, // Slovenian (Slovenia)
            { SystemLanguage.Spanish, "es-ES" }, // Spanish (Spain)
            { SystemLanguage.Swedish, "sv-SE" }, // Swedish (Sweden)
            { SystemLanguage.Thai, "th-TH" }, // Thai (Thailand)
            { SystemLanguage.Turkish, "tr-TR" }, // Turkish (Turkey)
            { SystemLanguage.Ukrainian, "uk-UA" }, // Ukrainian (Ukraine)
            { SystemLanguage.Vietnamese, "vi-VN" }, // Vietnamese (Vietnam)
            { SystemLanguage.ChineseSimplified, "zh-CN" }, // Chinese (Simplified)
            { SystemLanguage.ChineseTraditional, "zh-TW" }, // Chinese (Traditional)
#if UNITY_2022_2_OR_NEWER
            { SystemLanguage.Hindi, "hi-IN" }, // Hindi (India)
#endif
        };

        public static string GetSpecificCode(this SystemLanguage language)
        {
            if (specificCodes.ContainsKey(language))
            {
                return specificCodes[language];
            }
            return string.Empty;
        }

        public static string GetLanguageCode(this SystemLanguage language)
        {
            if (language != SystemLanguage.Unknown)
            {
                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
                string languageName = string.Format("{0}", language);
                if (language == SystemLanguage.Chinese || language == SystemLanguage.ChineseSimplified)
                {
                    languageName = "Chinese (Simplified)";
                }
                else if (language == SystemLanguage.ChineseTraditional)
                {
                    languageName = "Chinese (Traditional)";
                }
                else
                if (language == SystemLanguage.SerboCroatian)
                {
                    languageName = "Croatian";
                }
                foreach (CultureInfo culture in cultures)
                {
                    if (string.Compare(culture.DisplayName, languageName, true) == 0)
                    {
                        return culture.Name;
                    }
                }
            }
            return string.Empty;
        }

        public static CultureInfo GetCultureInfo(this SystemLanguage language)
        {
            if (language != SystemLanguage.Unknown)
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(GetSpecificCode(language));
                if (culture != null && !string.IsNullOrEmpty(culture.Name))
                {
                    try
                    {
                        if (culture.DateTimeFormat != null)
                        {
                            return culture;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                culture = CultureInfo.GetCultureInfo(GetLanguageCode(language));
                if (culture != null && !string.IsNullOrEmpty(culture.Name))
                {
                    try
                    {
                        if (culture.DateTimeFormat != null)
                        {
                            return culture;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return CultureInfo.CurrentCulture;
        }
    }
}

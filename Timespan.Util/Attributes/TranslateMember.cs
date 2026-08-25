namespace Timespan.Util.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property)]
public class TranslateMember : Attribute {

    public string TranslationKey { get; private set; }
    public string FallbackValue { get; private set; }

    public TranslateMember(string translationKey, string fallbackTranslation) {
        TranslationKey = translationKey;
        FallbackValue = fallbackTranslation;
    }
}

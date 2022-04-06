namespace NetUlid
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class UlidConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            switch (value)
            {
                case string s:
                    return Ulid.Parse(s);
                default:
                    return base.ConvertFrom(context, culture, value);
            }
        }
    }
}

using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Thorium.Core.DataIntegration.Serializers.CsvSerializer.TypeConverters
{
    public class DateTimeTypeConverter : ITypeConverter
    {

        public string DateTimeFormat { get; set; }

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return ((DateTime)value).ToString(DateTimeFormat);
        }

        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            try
            {
                return DateTime.ParseExact(text, DateTimeFormat, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return new DateTime();
            }
        }

        public bool CanConvertFrom(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }
            if (type == typeof(DateTime))
            {
                return true;
            }
            return false;
        }

        public bool CanConvertTo(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }
            if (type == typeof(DateTime))
            {
                return true;
            }
            return false;
        }
    }
}
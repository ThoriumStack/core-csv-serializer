using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Atlas.Modules.DataIntegration.Service.TypeConverters
{
    public class DecimalTypeConverter : ITypeConverter
    {

        public string DecimalFormat { get; set; }


        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            try
            {
                if ((value == null) || (value.GetType() != typeof(Decimal)))
                {
                    return string.Empty;
                }

                var returnFormatted = ((Decimal)value).ToString(DecimalFormat);
                return string.IsNullOrEmpty(returnFormatted) ? value.ToString() : returnFormatted;
            }
            catch (Exception ex)
            {
                return "0";
            }
        }

        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            decimal outValue = 0;
            try
            {
                Decimal.TryParse(text, out outValue);
                return outValue;
            }
            catch (Exception)
            {
                return outValue;
            }
        }

        public bool CanConvertFrom(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }
            if (type == typeof(Decimal))
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
            if (type == typeof(Decimal))
            {
                return true;
            }
            return false;
        }


    }
}
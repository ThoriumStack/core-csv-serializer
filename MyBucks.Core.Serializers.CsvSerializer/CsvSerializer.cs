using Atlas.Modules.DataIntegration.Service.TypeConverters;
using CsvHelper;
using CsvHelper.Configuration;
using MyBucks.Core.DataIntegration.Attributes;
using MyBucks.Core.DataIntegration.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace MyBucks.Core.Serializers.CsvSerializer
{
    public class CsvSerializer : IIntegrationDataSerializer
    {
        public bool HasHeaderRecord { get; set; }
        public string Delimiter { get; set; }
        /// <summary>
        /// This flag tells the reader to ignore white space in the headers when matching the columns to the properties by name.
        /// </summary>
        public bool IgnoreHeaderWhiteSpace { get; set; }
        public bool IgnoreReadingExceptions { get; set; }

        [Obsolete("Please use TrimOptions")]
        public bool TrimFields { get; set; }
        public bool TrimHeaders { get; set; }
        public bool IgnoreQuotes { get; set; }
        public bool QuoteNoFields { get; set; } = false;
        public TrimOptions TrimOptions { get; set; }
        // Summary:
        //     Gets or sets the callback that is called when a reading exception occurs. This
        //     will only happen when CsvHelper.Configuration.CsvConfiguration.IgnoreReadingExceptions
        //     is true, and when calling CsvHelper.ICsvReaderRow.GetRecords``1.
        public Action<CsvHelperException> ReadingExceptionCallback { get; set; }

        //
        // Summary:
        //     Gets or sets a method that gets called when bad data is detected.
        public Action<ReadingContext> BadDataCallback { get; set; }
        public CsvSerializer()
        {

        }

        public IEnumerable<TData> GetData<TData>(MemoryStream rawData) where TData : new()
        {
            rawData.Seek(0, SeekOrigin.Begin);
            using (var sr = new StreamReader(rawData))
            {
                var config = new Configuration();
                CsvConfigure<TData>(config);
                var reader = new CsvReader(sr, config);
                
                IEnumerable<TData> records = reader.GetRecords<TData>().ToList();
                return records;
            }
        }

        public MemoryStream GenerateRawData<TData>(IEnumerable<TData> data)
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            var config = new Configuration();
            CsvConfigure<TData>(config);
            var csv = new CsvWriter(streamWriter, config);
            

            csv.WriteRecords(data);
            streamWriter.Flush();
            return memoryStream;
        }


        private void CsvConfigure<TData>(Configuration configuration)
        {
            configuration.HasHeaderRecord = HasHeaderRecord;
            if (Delimiter != null)
            {
                configuration.Delimiter = Delimiter;
            }
            //configuration.conf = IgnoreHeaderWhiteSpace;

            configuration.TrimOptions = TrimOptions;

            if (TrimHeaders)
            {
                configuration.PrepareHeaderForMatch = (a) => a.Trim();
            }
            configuration.QuoteNoFields = QuoteNoFields;
            configuration.ReadingExceptionOccurred = ReadingExceptionCallback;
            configuration.BadDataFound = BadDataCallback;
            configuration.IgnoreQuotes = IgnoreQuotes;
            RegisterMap<TData>((Configuration)configuration);
        }


        public void ReadSingle<TData, TDiscriminator>(Action<TData> assignAction, Func<TDiscriminator, bool> discriminator, MemoryStream rawData) where TData : new() where TDiscriminator : new()
        {
            rawData.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(rawData);
            var config = new Configuration();
            CsvConfigure<TData>(config);
            var reader = new CsvReader(sr,config);
            
            
            while (reader.Read())
            {
                var discriminatorValue = reader.GetRecord<TDiscriminator>();
                if (discriminator(discriminatorValue))
                {
                    assignAction(reader.GetRecord<TData>());
                    return;
                }
            }
        }

        public void ReadMany<TData, TDiscriminator>(IList<TData> destination, Func<TDiscriminator, bool> discriminator, MemoryStream rawData) where TData : new() where TDiscriminator : new()
        {
            var failedRows = new List<string>();

            rawData.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(rawData);
            var config = new Configuration();
            CsvConfigure<TData>(config);
            var reader = new CsvReader(sr, config);
            

            while (reader.Read())
            {
                var discriminatorValue = reader.GetRecord<TDiscriminator>();
                if (discriminator(discriminatorValue))
                {

                    try
                    {
                        var record = reader.GetRecord<TData>();
                        destination.Add(record);
                    }
                    catch (Exception sException)
                    {

                        failedRows.Add($"\n({reader.Context.RawRow}: {sException.Message})");
                    }
                }
            }

            if (failedRows.Any())
            {
                throw new Exception($"Data Integration Error: Serializer Failure: {string.Join(string.Empty, failedRows)}");
            }
        }


        private void RegisterMap<TData>(Configuration csv)
        {
            
            var map = csv.AutoMap<TData>();
            
            foreach (var prop in map.MemberMaps)
            {
                var dtFormat = prop.Data.Member.GetCustomAttributes(typeof(DateTimeFormatAttribute), true);
                if (dtFormat.Any())
                {
                    var converter = new DateTimeTypeConverter()
                    {
                        DateTimeFormat = ((DateTimeFormatAttribute)dtFormat.First()).DateTimeFormat
                    };
                    prop.Data.TypeConverter = converter;       
                    //prop.TypeConverter(converter);
                }

                var descriptionAttribs = prop.Data.Member.GetCustomAttributes(typeof(ColumnHeaderAttribute), true);
                if (descriptionAttribs.Any())
                {
                    //map.Map(typeof(TData),prop.Data.Member,true).
                    prop.Data.Names.Clear();
                    prop.Data.Names.Add(((ColumnHeaderAttribute)descriptionAttribs.First()).Name);
                    
                }

                var decFormat = prop.Data.Member.GetCustomAttributes(typeof(DecimalFormatAttribute), true);
                if (decFormat.Any())
                {
                    var converter = new DecimalTypeConverter()
                    {
                        DecimalFormat = ((DecimalFormatAttribute)decFormat.First()).DecimalFormat
                    };

                    prop.Data.TypeConverter = converter;
                }
            }
            csv.RegisterClassMap(map);
        }
    }
}

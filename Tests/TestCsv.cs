
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestCsvWrite()
        {

            StreamReader reader = new StreamReader(GenerateData());
            string actual = reader.ReadToEnd();
            var expected = "First Name,BirthDate,NetWorth\r\nGeorge Washington,01-01-01,35\r\nThomas Jefferson,81-01-01,7451255\r\n";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestCsvRead()
        {
            var s = new CsvSerializer();
            s.HasHeaderRecord = true;
            var data = s.GetData<TestCsv>(GenerateData()).ToList();
            Assert.NotNull(data);
            Assert.Equal(2, data.Count);
            Assert.Equal("Thomas Jefferson", data[1].Name);
            Assert.Equal(35m, data[0].NetWorth);
        }

        private MemoryStream GenerateData()
        {
            var s = new CsvSerializer();
            s.HasHeaderRecord = true;
            var testData = GetTestData();

            var stream = s.GenerateRawData(testData);
            stream.Position = 0;
            return stream;
        }

        private List<TestCsv> GetTestData()
        {
            return new List<TestCsv> {
                new TestCsv { BirthDate = DateTime.MinValue, Name = "George Washington", NetWorth =34.8m},
                new TestCsv { BirthDate = DateTime.MinValue.AddYears(80), Name = "Thomas Jefferson", NetWorth =7451254.54m},
            };
        }

        public class TestCsv
        {
            [ColumnHeader("First Name")]
            public string Name { get; set; }
            [DateTimeFormat("yy-MM-dd")]
            public DateTime BirthDate { get; set; }
            [DecimalFormat("0")]
            public decimal NetWorth { get; set; }
        }
    }
}

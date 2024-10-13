//using CsvHelper;
//using System.Globalization;

//namespace BusinessProgressSoft.Models.Services
//{
//    public class CSVService : ICSVService
//    {
//        public IEnumerable<T> ReadCSV<T>(Stream file)
//        {
//            var reader = new StreamReader(file);
//            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

//            var records = csv.GetRecords<T>();
//            return records;
//        }
//    }
//}
using CsvHelper;
using System.Globalization;

namespace BusinessProgressSoft.Models.Services
{
    public class CSVService : ICSVService
    {
        public IEnumerable<T> ReadCSV<T>(Stream file)
        {
            try
            {
                using var reader = new StreamReader(file);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                var records = csv.GetRecords<T>().ToList();
                return records;
            }
            catch (Exception ex)
            {
                // Log the error or throw a custom exception
                throw new InvalidDataException("Error reading the CSV file", ex);
            }
        }
        public void WriteCSV<T>(Stream stream, IEnumerable<T> records)
        {
            using var writer = new StreamWriter(stream, leaveOpen: true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(records); // Write all records to the stream
        }
    }
}


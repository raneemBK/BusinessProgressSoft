namespace BusinessProgressSoft.Models.Services
{
    public interface ICSVService
    {
        public IEnumerable<T> ReadCSV<T>(Stream file);
        void WriteCSV<T>(Stream stream, IEnumerable<T> records);
    }
}

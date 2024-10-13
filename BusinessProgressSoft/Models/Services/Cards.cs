namespace BusinessProgressSoft.Models.Services
{
    public class Cards : ICards
    {
        private readonly BusinessProgressSoftContext _context;
        public Cards(BusinessProgressSoftContext context)
        {
            _context = context;
        }
        public List<Bcard> GetCards()
        {
           return _context.Bcards.ToList();
        }
    }
}

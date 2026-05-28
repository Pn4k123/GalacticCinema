using BookingMovieTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingMovieTicket.Helper
{
    public class xuLyMaVe
    {
        private readonly QuanLyDatVePhimContext db;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public xuLyMaVe(QuanLyDatVePhimContext context)
        {
            db = context;
        }

        public async Task<string> veIdAsync()
        {
            await _lock.WaitAsync();
            try
            {
                var lastId = await db.Ves
                    .Where(v => v.MaVe.StartsWith("V"))
                    .Select(v => v.MaVe.Substring(1))
                    .ToListAsync();

                int maxNum = lastId
                    .Where(s => int.TryParse(s, out _))
                    .Select(s => int.Parse(s))
                    .DefaultIfEmpty(0)
                    .Max();

                return "V" + (maxNum + 1).ToString("D3");
            }
            finally
            {
                _lock.Release();
            }
        }

        public string veId()
        {
            return veIdAsync().GetAwaiter().GetResult();
        }
    }
}
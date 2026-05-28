using BookingMovieTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingMovieTicket.Helper
{
    public class xuLyMaDon
    {
        private readonly QuanLyDatVePhimContext db;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public xuLyMaDon(QuanLyDatVePhimContext context)
        {
            db = context;
        }

        public async Task<string> donIdAsync()
        {
            await _lock.WaitAsync();
            try
            {
                var lastId = await db.DonDatVes
                    .Where(d => d.MaDon.StartsWith("D"))
                    .Select(d => d.MaDon.Substring(1))
                    .ToListAsync();

                int maxNum = lastId
                    .Where(s => int.TryParse(s, out _))
                    .Select(s => int.Parse(s))
                    .DefaultIfEmpty(0)
                    .Max();

                return "D" + (maxNum + 1).ToString("D3");
            }
            finally
            {
                _lock.Release();
            }
        }

        public string donId()
        {
            return donIdAsync().GetAwaiter().GetResult();
        }
    }
}
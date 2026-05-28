using BookingMovieTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingMovieTicket.Helper
{
    public class xuLyMaKH
    {
        private readonly QuanLyDatVePhimContext db;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public xuLyMaKH(QuanLyDatVePhimContext context)
        {
            db = context;
        }

        public async Task<string> khachHangIdAsync()
        {
            await _lock.WaitAsync();
            try
            {
                var lastId = await db.NguoiDungs
                    .Where(kh => kh.MaNd.StartsWith("KH"))
                    .Select(kh => kh.MaNd.Substring(2))
                    .ToListAsync();

                int maxNum = lastId
                    .Where(s => int.TryParse(s, out _))
                    .Select(s => int.Parse(s))
                    .DefaultIfEmpty(0)
                    .Max();

                return "KH" + (maxNum + 1).ToString("D3");
            }
            finally
            {
                _lock.Release();
            }
        }

        public string khachHangId()
        {
            return khachHangIdAsync().GetAwaiter().GetResult();
        }
    }
}
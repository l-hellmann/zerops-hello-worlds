using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DotNetMinimalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EntriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> AddEntry()
        {
            var randomData = Guid.NewGuid().ToString();
            var entry = new Entry { Data = randomData };
            _context.Entries.Add(entry);
            await _context.SaveChangesAsync();

            var count = await _context.Entries.CountAsync();

            return Ok(new { Message = "Entry added successfully with random data.", Data = randomData, Count = count });
        }
    }
}

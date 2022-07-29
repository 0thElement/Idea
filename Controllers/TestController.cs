using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Idea.Data;
using Idea.Models;

namespace Idea.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly DataContext _context;

        public TestController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Test
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestModel>>> GetModels()
        {
          if (_context.Models == null)
          {
              return NotFound();
          }
            return await _context.Models.ToListAsync();
        }

        // GET: api/Test/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestModel>> GetTestModel(int id)
        {
          if (_context.Models == null)
          {
              return NotFound();
          }
            var testModel = await _context.Models.FindAsync(id);

            if (testModel == null)
            {
                return NotFound();
            }

            return testModel;
        }

        // PUT: api/Test/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestModel(int id, TestModel testModel)
        {
            if (id != testModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(testModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TestModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Test
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TestModel>> PostTestModel(TestModel testModel)
        {
          if (_context.Models == null)
          {
              return Problem("Entity set 'DataContext.Models'  is null.");
          }
            _context.Models.Add(testModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTestModel", new { id = testModel.Id }, testModel);
        }

        // DELETE: api/Test/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestModel(int id)
        {
            if (_context.Models == null)
            {
                return NotFound();
            }
            var testModel = await _context.Models.FindAsync(id);
            if (testModel == null)
            {
                return NotFound();
            }

            _context.Models.Remove(testModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TestModelExists(int id)
        {
            return (_context.Models?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalApi.Models;
using Microsoft.AspNetCore.Cors;

namespace AnimalApi.Controllers.V2
{

  [ApiController]
  [Route("api/v{version:apiVersion}/[controller]")]
  [ApiVersion("2.0")]

  public class AnimalsController : ControllerBase
  {
    private readonly AnimalApiContext _db;

    public AnimalsController(AnimalApiContext db)
    {
      _db = db;
    }

    // GET: api/v2/animals
    [EnableCors("Policy")]
    [HttpGet]
    public async Task<IActionResult> Get(string species, string name, int minimumAge, int? page)
    {
      IQueryable<Animal> query = _db.Animals.AsQueryable();

      if (species != null)
      {
        query = query.Where(entry => entry.Species == species);
      }

      if (name != null)
      {
        query = query.Where(entry => entry.Name == name);
      }

      if (minimumAge > 0)
      {
        query = query.Where(entry => entry.Age >= minimumAge);
      }

      int pageCount = query.Count();
      int pageSize = 3;
      int currentPage = page ?? 1;

      var animals = await query
        .Skip((currentPage - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

      var response = new AnimalResponse
      {
        Animals = animals,
        //page number inside the url
        CurrentPage = currentPage,
        //the amount of animals returned from the database
        PageItems  = pageCount,
        //amount of items on the page
        PageSize = pageSize         
      };

      return Ok(response);  
    }

    // GET: api/v2/Animals/5
    [EnableCors("Policy")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Animal>> GetAnimal(int id)
    {
      Animal animal = await _db.Animals.FindAsync(id);

      if (animal == null)
      {
        return NotFound();
      }

      return animal;
    }

    // POST: api/v2/animals
    [HttpPost]
    public async Task<ActionResult<Animal>> Post([FromBody] Animal animal)
    {
      _db.Animals.Add(animal);
      await _db.SaveChangesAsync();

      return CreatedAtAction(nameof(GetAnimal), new { id = animal.AnimalId }, animal);
    }

    // PUT: api/v2/Animals/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Animal animal)
    {
      if (id != animal.AnimalId)
      {
        return BadRequest();
      }

      _db.Animals.Update(animal);

      try
      {
        await _db.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!AnimalExists(id))
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

    private bool AnimalExists(int id)
    {
      return _db.Animals.Any(e => e.AnimalId == id);
    }

    // DELETE: api/v2/Animals/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnimal(int id)
    {
      Animal animal = await _db.Animals.FindAsync(id);
      if (animal == null)
      {
        return NotFound();
      }

      _db.Animals.Remove(animal);
      await _db.SaveChangesAsync();

      return NoContent();
    }

    // GET: api/v2/animals/random
    [HttpGet("random")]
    public async Task<ActionResult<Animal>> GetRandomAnimal()
    {
      List<Animal> animals = await _db.Animals.ToListAsync();
      int random = new Random().Next(animals.Count);
      return animals[random];
    }


  }
}
using MemeGen.ApiService.Persistent;
using MemeGen.Common.Services;
using MemeGen.Contracts.Http.v1.Requests;
using MemeGen.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeGen.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class PersonsController(
    ILogger<PersonsController> logger,
    AppDbContext appDbContext,
    IResponseBuilder responseBuilder) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var persons = await appDbContext.Persons.ToListAsync();
            return Ok(persons);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var person = await appDbContext.Persons.FindAsync(id);

            if (person != null)
                return Ok(person);

            logger.LogWarning("Person with id: {id} not found", id);
            return NotFound();
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreatePersonRequest person)
    {
        try
        {
            logger.LogInformation("Creating new person with name: {Name}", person.Name);

            var existingPerson = await appDbContext.Persons.FirstOrDefaultAsync(p => p.Name == person.Name);
            if (existingPerson != null)
            {
                logger.LogWarning("Person with name: {Name} exists", person.Name);
                return BadRequest("Person with name already exists");
            }

            var newPerson = new Person(person.Name);
            await appDbContext.Persons.AddAsync(newPerson);
            await appDbContext.SaveChangesAsync();
            logger.LogInformation("Person with id: {Id} and {Name} added", newPerson.Id, newPerson.Name);

            return Ok(newPerson);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            logger.LogInformation("Deleting person with id: {Id}", id);
            var person = await appDbContext.Persons.FindAsync(id);
            if (person == null)
            {
                logger.LogWarning("Person with id: {Id} not found", id);
                return NotFound();
            }

            appDbContext.Persons.Remove(person);
            await appDbContext.SaveChangesAsync();
            logger.LogInformation("Person with id: {Id} deleted", id);

            return Ok();
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }
}
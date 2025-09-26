namespace MemeGen.Domain.Entities;

/// <summary>
/// Entity representing a person who can be associated with quotes.
/// </summary>
/// <param name="name">name of the person</param>
public class Person(string name)
{
    public int Id { get; private set; }

    public string Name { get; private set; } = name;
}
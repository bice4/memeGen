namespace MemeGen.Domain.Entities;

public class Person(string name)
{
    public int Id { get; private set; }

    public string Name { get; private set; } = name;
}
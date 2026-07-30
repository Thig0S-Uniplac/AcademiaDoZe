namespace AcademiaDoZe.Domain.Entities.Base;
//thiago kovaslki

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();

}

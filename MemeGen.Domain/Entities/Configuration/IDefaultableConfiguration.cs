namespace MemeGen.Domain.Entities.Configuration;

public interface IDefaultableConfiguration
{
    T CreateDefault<T>() where T : class;
}
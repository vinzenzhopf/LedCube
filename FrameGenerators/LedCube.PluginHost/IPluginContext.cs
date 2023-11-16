namespace LedCube.PluginHost;

public interface IPluginContext
{
    public void UnloadAll();
    public void LoadAssemblies();
    public TType GetService<TType>() where TType : class;
    public IEnumerable<TType> GetServices<TType>() where TType : class;
}
namespace LedCube.PluginHost;

public interface IHostServiceProxy
{
    public TType GetHostService<TType>() where TType : class;
}
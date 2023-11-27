namespace LedCube.PluginHost;

public interface IServiceProxy
{
    public TType GetService<TType>() where TType : class;
    public IEnumerable<TType> GetServices<TType>() where TType : class;
}

public class ServiceProxy : IServiceProxy
{
    private readonly IPluginContext _context;

    public ServiceProxy(IPluginContext context)
    {
        _context = context;
    }

    public TType GetService<TType>() where TType : class
    {
        return _context.GetService<TType>();
    }

    public IEnumerable<TType> GetServices<TType>() where TType : class
    {
        return _context.GetServices<TType>();
    }
}
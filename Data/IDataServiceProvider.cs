public interface IDataServiceProvider
{
    IDataService Current { get; }
    void UseLocal();
    void UseRemote();
    void UpdateRemote(IDataService newRemote);
}

public class DataServiceProvider : IDataServiceProvider
{
    private readonly IDataService _local;
    private IDataService _remote;
    private IDataService _current;

    public DataServiceProvider(IDataService local, IDataService remote)
    {
        _local = local;
        _remote = remote;
        _current = _remote; // or _local, as default
    }

    public IDataService Current => _current;

    public void UseLocal() => _current = _local;
    public void UseRemote() => _current = _remote;

    public void UpdateRemote(IDataService newRemote)
    {
        _remote = newRemote;
        _current = _remote; // Optionally switch to remote immediately
    }
}
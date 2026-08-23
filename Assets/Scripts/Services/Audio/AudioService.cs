using System.Threading.Tasks;

public class AudioService : IService
{
    public AudioService() { }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}

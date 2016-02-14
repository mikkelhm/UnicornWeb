namespace Unicorn.Web.Contracts.Processes
{
    /// <summary>
    /// Defines the interface of a process that can start and stop
    /// </summary>
    public interface IProcessor
    {
        void Start();
        void Stop();
    }
}

namespace SLOY.Shared.Contracts;

public interface ISloyService
{
    Task<SloyServiceResult> InitializeAsync();
    Task<SloyServiceResult> ShutdownAsync();
    SloyServiceResult GetStatus();
}
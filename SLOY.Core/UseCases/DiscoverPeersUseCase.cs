using SLOY.Core.Interfaces;

namespace SLOY.Core.UseCases;

public class DiscoverPeersUseCase
{
    private readonly IMeshRouter _router;
    public DiscoverPeersUseCase(IMeshRouter router) => _router = router;
    public async Task<IReadOnlyList<string>> ExecuteAsync() => (await _router.DiscoverPeersAsync()).AsReadOnly();
}
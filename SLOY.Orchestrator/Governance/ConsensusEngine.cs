using System.Security.Cryptography;
using System.Text;

namespace SLOY.Orchestrator.Governance;

public class ConsensusEngine
{
    private readonly Dictionary<string, ConsensusNode> _nodes = new();
    private readonly int _quorumPercent;
    private readonly Dictionary<string, Proposal> _proposals = new();

    public int NodeCount => _nodes.Count;
    public int QuorumSize => (int)Math.Ceiling(_nodes.Count * _quorumPercent / 100.0);

    public ConsensusEngine(int quorumPercent = 67)
    {
        _quorumPercent = quorumPercent;
    }

    public void RegisterNode(string nodeId, double stake = 1.0)
    {
        _nodes[nodeId] = new ConsensusNode { Id = nodeId, Stake = stake };
    }

    public void UnregisterNode(string nodeId)
    {
        _nodes.Remove(nodeId);
    }

    public string CreateProposal(string proposerId, byte[] data)
    {
        var proposalId = ComputeProposalId(proposerId, data);
        _proposals[proposalId] = new Proposal
        {
            Id = proposalId,
            ProposerId = proposerId,
            Data = data,
            CreatedAt = DateTime.UtcNow
        };
        return proposalId;
    }

    public bool Vote(string nodeId, string proposalId, bool approve)
    {
        if (!_proposals.TryGetValue(proposalId, out var proposal)) return false;
        if (!_nodes.ContainsKey(nodeId)) return false;
        if (proposal.Votes.ContainsKey(nodeId)) return false;

        proposal.Votes[nodeId] = approve;
        return true;
    }

    public ConsensusResult GetResult(string proposalId)
    {
        if (!_proposals.TryGetValue(proposalId, out var proposal))
            return new ConsensusResult { IsDecided = false };

        var totalStake = _nodes.Values.Sum(n => n.Stake);
        var approveStake = proposal.Votes
            .Where(v => v.Value && _nodes.ContainsKey(v.Key))
            .Sum(v => _nodes[v.Key].Stake);

        var rejectStake = proposal.Votes
            .Where(v => !v.Value && _nodes.ContainsKey(v.Key))
            .Sum(v => _nodes[v.Key].Stake);

        var ratio = totalStake > 0 ? approveStake / totalStake : 0;
        var isDecided = ratio >= _quorumPercent / 100.0;

        return new ConsensusResult
        {
            IsDecided = isDecided,
            Approved = isDecided && ratio >= _quorumPercent / 100.0,
            ApprovalRatio = ratio,
            TotalVotes = proposal.Votes.Count,
            ApproveVotes = proposal.Votes.Count(v => v.Value),
            RejectVotes = proposal.Votes.Count(v => !v.Value)
        };
    }

    public void CleanupExpiredProposals(TimeSpan maxAge)
    {
        var expired = _proposals
            .Where(p => DateTime.UtcNow - p.Value.CreatedAt > maxAge)
            .Select(p => p.Key)
            .ToList();

        foreach (var id in expired)
            _proposals.Remove(id);
    }

    private static string ComputeProposalId(string proposerId, byte[] data)
    {
        var input = Encoding.UTF8.GetBytes(proposerId).Concat(data).ToArray();
        return Convert.ToHexString(SHA256.HashData(input))[..16];
    }

    private class ConsensusNode
    {
        public string Id { get; init; } = string.Empty;
        public double Stake { get; set; }
    }

    private class Proposal
    {
        public string Id { get; init; } = string.Empty;
        public string ProposerId { get; init; } = string.Empty;
        public byte[] Data { get; init; } = Array.Empty<byte>();
        public DateTime CreatedAt { get; init; }
        public Dictionary<string, bool> Votes { get; } = new();
    }
}

public class ConsensusResult
{
    public bool IsDecided { get; init; }
    public bool Approved { get; init; }
    public double ApprovalRatio { get; init; }
    public int TotalVotes { get; init; }
    public int ApproveVotes { get; init; }
    public int RejectVotes { get; init; }
}
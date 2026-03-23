using SmartCommunityApi.DTOs;

namespace SmartCommunityApi.Services;

public enum CastVoteResult { Success, AlreadyVoted, TopicNotFound, TopicExpired }

public interface IVoteService
{
    Task<List<VoteTopicDto>> GetActiveTopicsAsync(int userId);
    Task<CastVoteResult> CastVoteAsync(int userId, int topicId, string option);
    Task<VoteTopicWithResultsDto> CreateTopicAsync(CreateVoteTopicRequest request);
    Task<List<VoteTopicWithResultsDto>> GetAllTopicsWithResultsAsync();
}

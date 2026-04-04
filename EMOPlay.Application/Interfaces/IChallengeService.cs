using EMOPlay.Application.DTOs.Challenge;

namespace EMOPlay.Application.Interfaces;

public interface IChallengeService
{
    Task<RecordResponseResponse> RecordResponseAsync(RecordResponseRequest request);
}

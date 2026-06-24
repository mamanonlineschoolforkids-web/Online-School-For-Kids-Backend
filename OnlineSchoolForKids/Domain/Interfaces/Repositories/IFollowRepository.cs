using Domain.Entities.Feed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories;


public interface IFollowRepository
{
    Task<bool> IsFollowingAsync(string followerId, string followeeId, CancellationToken ct = default);
    Task FollowAsync(Follow follow, CancellationToken ct = default);
    Task UnfollowAsync(string followerId, string followeeId, CancellationToken ct = default);

    Task<List<string>> GetFollowingIdsAsync(string userId, CancellationToken ct = default);
    Task<List<string>> GetFollowerIdsAsync(string userId, CancellationToken ct = default);

    Task<int> GetFollowingCountAsync(string userId, CancellationToken ct = default);
    Task<int> GetFollowerCountAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// For each userId in the list, returns whether followerId follows them.
    /// Returns a HashSet of followee IDs that are followed.
    /// </summary>
    Task<HashSet<string>> GetFollowingSetAsync(string followerId, List<string> userIds, CancellationToken ct = default);
}


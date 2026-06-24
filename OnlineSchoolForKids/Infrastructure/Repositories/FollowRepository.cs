using Domain.Entities.Feed;
using Domain.Interfaces.Repositories;
using Infrastructure.Data;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;


public class FollowRepository : IFollowRepository
{
    private readonly IMongoCollection<Follow> _col;

    public FollowRepository(MongoDbContext context)
    {
        _col = context.GetCollection<Follow>("follows");
    }

    public async Task<bool> IsFollowingAsync(
        string followerId, string followeeId, CancellationToken ct = default)
    {
        var count = await _col.CountDocumentsAsync(
            f => f.FollowerId == followerId && f.FolloweeId == followeeId && !f.IsDeleted,
            cancellationToken: ct);
        return count > 0;
    }

    public async Task FollowAsync(Follow follow, CancellationToken ct = default)
    {
        // Upsert: if unfollowed before, restore it
        var filter = Builders<Follow>.Filter.And(
            Builders<Follow>.Filter.Eq(f => f.FollowerId, follow.FollowerId),
            Builders<Follow>.Filter.Eq(f => f.FolloweeId, follow.FolloweeId));

        var existing = await _col.Find(filter).FirstOrDefaultAsync(ct);
        if (existing != null)
        {
            var upd = Builders<Follow>.Update
                .Set(f => f.IsDeleted, false)
                .Set(f => f.UpdatedAt, DateTime.UtcNow);
            await _col.UpdateOneAsync(filter, upd, cancellationToken: ct);
        }
        else
        {
            follow.CreatedAt = DateTime.UtcNow;
            follow.UpdatedAt = DateTime.UtcNow;
            await _col.InsertOneAsync(follow, cancellationToken: ct);
        }
    }

    public async Task UnfollowAsync(
        string followerId, string followeeId, CancellationToken ct = default)
    {
        var upd = Builders<Follow>.Update
            .Set(f => f.IsDeleted, true)
            .Set(f => f.UpdatedAt, DateTime.UtcNow);
        await _col.UpdateOneAsync(
            f => f.FollowerId == followerId && f.FolloweeId == followeeId,
            upd, cancellationToken: ct);
    }

    public async Task<List<string>> GetFollowingIdsAsync(
        string userId, CancellationToken ct = default)
    {
        var docs = await _col
            .Find(f => f.FollowerId == userId && !f.IsDeleted)
            .ToListAsync(ct);
        return docs.Select(f => f.FolloweeId).ToList();
    }

    public async Task<List<string>> GetFollowerIdsAsync(
        string userId, CancellationToken ct = default)
    {
        var docs = await _col
            .Find(f => f.FolloweeId == userId && !f.IsDeleted)
            .ToListAsync(ct);
        return docs.Select(f => f.FollowerId).ToList();
    }

    public async Task<int> GetFollowingCountAsync(
        string userId, CancellationToken ct = default)
    {
        var count = await _col.CountDocumentsAsync(
            f => f.FollowerId == userId && !f.IsDeleted, cancellationToken: ct);
        return (int)count;
    }

    public async Task<int> GetFollowerCountAsync(
        string userId, CancellationToken ct = default)
    {
        var count = await _col.CountDocumentsAsync(
            f => f.FolloweeId == userId && !f.IsDeleted, cancellationToken: ct);
        return (int)count;
    }

    public async Task<HashSet<string>> GetFollowingSetAsync(
        string followerId, List<string> userIds, CancellationToken ct = default)
    {
        var filter = Builders<Follow>.Filter.And(
            Builders<Follow>.Filter.Eq(f => f.FollowerId, followerId),
            Builders<Follow>.Filter.In(f => f.FolloweeId, userIds),
            Builders<Follow>.Filter.Eq(f => f.IsDeleted, false));
        var docs = await _col.Find(filter).ToListAsync(ct);
        return docs.Select(f => f.FolloweeId).ToHashSet();
    }
}

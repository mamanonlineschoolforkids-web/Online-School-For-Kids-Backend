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


public class PostReactionRepository : IPostReactionRepository
{
    private readonly IMongoCollection<PostReaction> _col;

    public PostReactionRepository(MongoDbContext context)
    {
        _col = context.GetCollection<PostReaction>("postReactions");
    }

    public async Task<PostReaction?> GetAsync(
        string postId, string userId, CancellationToken ct = default)
        => await _col
            .Find(r => r.PostId == postId && r.UserId == userId && !r.IsDeleted)
            .FirstOrDefaultAsync(ct);

    public async Task UpsertAsync(PostReaction reaction, CancellationToken ct = default)
    {
        var filter = Builders<PostReaction>.Filter.And(
            Builders<PostReaction>.Filter.Eq(r => r.PostId, reaction.PostId),
            Builders<PostReaction>.Filter.Eq(r => r.UserId, reaction.UserId));

        var update = Builders<PostReaction>.Update
            .Set(r => r.ReactionType, reaction.ReactionType)
            .Set(r => r.IsDeleted, false)
            .Set(r => r.UpdatedAt, DateTime.UtcNow)
            .SetOnInsert(r => r.CreatedAt, DateTime.UtcNow)
            .SetOnInsert(r => r.Id, reaction.Id);

        await _col.UpdateOneAsync(filter, update,
            new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task RemoveAsync(
        string postId, string userId, CancellationToken ct = default)
    {
        var update = Builders<PostReaction>.Update
            .Set(r => r.IsDeleted, true)
            .Set(r => r.UpdatedAt, DateTime.UtcNow);
        await _col.UpdateOneAsync(
            r => r.PostId == postId && r.UserId == userId, update, cancellationToken: ct);
    }

    public async Task<Dictionary<string, string>> GetUserReactionsAsync(
        string userId, List<string> postIds, CancellationToken ct = default)
    {
        var filter = Builders<PostReaction>.Filter.And(
            Builders<PostReaction>.Filter.Eq(r => r.UserId, userId),
            Builders<PostReaction>.Filter.In(r => r.PostId, postIds),
            Builders<PostReaction>.Filter.Eq(r => r.IsDeleted, false));

        var results = await _col.Find(filter).ToListAsync(ct);
        return results.ToDictionary(r => r.PostId, r => r.ReactionType);
    }

    public async Task<Dictionary<string, int>> GetCountsForPostAsync(
        string postId, CancellationToken ct = default)
    {
        var filter = Builders<PostReaction>.Filter.And(
            Builders<PostReaction>.Filter.Eq(r => r.PostId, postId),
            Builders<PostReaction>.Filter.Eq(r => r.IsDeleted, false));

        var reactions = await _col.Find(filter).ToListAsync(ct);
        return reactions
            .GroupBy(r => r.ReactionType)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
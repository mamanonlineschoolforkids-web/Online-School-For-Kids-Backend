using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Feed;

[BsonIgnoreExtraElements]
public class Post : BaseEntity
{
    public string AuthorId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> MediaUrls { get; set; } = new();
    public string MediaType { get; set; } = "text"; // text | image | video

    /// <summary>public = visible to everyone, private = only the author</summary>
    public string Visibility { get; set; } = "public";

    public int CommentsCount { get; set; } = 0;
    public int SharesCount { get; set; } = 0;
    public Dictionary<string, int> ReactionCounts { get; set; } = new();
    public int TotalReactions { get; set; } = 0;
}
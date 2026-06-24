using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Feed;

/// <summary>
/// Stores one reaction per user per post.
/// ReactionType: "like" | "love" | "haha" | "wow" | "sad" | "angry"
/// </summary>
public class PostReaction : BaseEntity
{
    public string PostId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ReactionType { get; set; } = "like";
}
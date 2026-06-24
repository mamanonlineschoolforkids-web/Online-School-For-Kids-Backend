using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Feed;


/// <summary>FollowerId follows FolloweeId</summary>
public class Follow : BaseEntity
{
    public string FollowerId { get; set; } = string.Empty; // the one who follows
    public string FolloweeId { get; set; } = string.Empty; // the one being followed
}

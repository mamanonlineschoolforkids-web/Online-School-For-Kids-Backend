using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Content;
public class VideoProcessingJob : BaseEntity
{
    public string InstructorId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;

    /// <summary>"upload" or "youtube"</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>YouTube URL or uploaded file URL</summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>pending | processing | awaiting_review | completed | failed</summary>
    public string Status { get; set; } = "pending";

    public string? ErrorMessage { get; set; }

    public string? RawTranscript { get; set; }

    public List<VideoChunk> Chunks { get; set; } = new();
}

public class VideoChunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Chunk index from the AI segmentation (0-based)</summary>
    public int Index { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Transcript { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    /// <summary>Set by instructor during review</summary>
    public string? SectionId { get; set; }

    /// <summary>Set by instructor during review</summary>
    public string? LessonTitle { get; set; }

    /// <summary>Whether this chunk has been mapped to a lesson</summary>
    public bool IsSaved { get; set; } = false;

    /// <summary>LessonId after it has been persisted</summary>
    public string? LessonId { get; set; }
}
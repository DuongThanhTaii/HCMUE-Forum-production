namespace UniHub.Forum.Domain.Posts;

public enum PostType
{
    Discussion = 1,  // Regular discussion post
    Question = 2,    // Q&A post with accepted answer support
    Announcement = 3, // Official announcements
    Poll = 4         // Poll post with voting options
}

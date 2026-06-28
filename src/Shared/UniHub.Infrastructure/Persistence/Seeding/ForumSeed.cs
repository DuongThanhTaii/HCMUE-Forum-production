using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Categories.ValueObjects;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.Forum.Domain.Tags;
using UniHub.Forum.Domain.ThreadChannels;

namespace UniHub.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds forum data: categories and tags.
/// </summary>
internal static class ForumSeed
{
    /// <summary>Anchor post for the &quot;general&quot; thread channel: long-form, realistic campus content (not a &quot;demo&quot; stub).</summary>
    private const string ShowcaseGeneralThreadTitle =
        "Sống và học ở HCMUE: kinh nghiệm cân bằng chuyên môn, hoạt động ngoại khóa và sức khỏe tinh thần";

    private static readonly string ShowcaseGeneralThreadBody = """
Xin chào mọi người, mình là sinh viên năm ba, khoa tự nhiên. Sau hơn hai năm gắn bó với HCMUE, mình muốn viết một bài thật “đủ thịt” — không phải checklist sáo rỗng — để các bạn mới vào hoặc đang cảm thấy quá tải có thể tham khảo.

**1. Học bền vững hơn học “nước rút”**

Mình từng lệ thuộc deadline: cày đêm trước kỳ thi, cày qua ngày nộp bài. Kết quả là điểm có khi ổn nhưng kiến thức không ăn sâu, sau nghỉ hè là quên gần hết. Cách mình đổi dần:

• Chia nhỏ mục tiêu theo tuần: mỗi môn ít nhất 2–3 khối ôn tập ngắn (45–60 phút), ghi chú ngay chỗ chưa hiểu để hỏi giảng viên hoặc bạn cùng lớp.
• Ưu tiên buổi sáng ở thư viện hoặc khu tự học yên tĩnh; chiều dành cho bài tập nhóm hoặc thực hành.
• Dùng một cuốn sổ (hoặc file) thống nhất cho từng môn: công thức, ví dụ điển hình, lỗi mình hay mắc — sau này ôn thi chỉ cần lật lại phần đó.

**2. Tận dụng tài nguyên trong trường**

HCMUE có nhiều kênh hỗ trợ mà đôi khi mình chỉ nhận ra khi đã muộn: tài liệu tham khảo ở thư viện, buổi tư vấn học tập, các CLB chuyên môn và cả các workshop kỹ năng mềm. Mình khuyên các bạn:

• Tham ít nhất một hoạt động ngoại khóa “đúng gu” chuyên ngành hoặc đúng sở thích — không cần ôm ba bốn CLB cùng lúc.
• Chủ động xin gặp giảng viên hướng dẫn khi định hướng đề tài, thực tập hoặc học bổng; email ngắn gọn, kèm câu hỏi cụ thể thường được trả lời nhanh hơn tin nhắn chung chung.

**3. Mối quan hệ và ranh giới**

Ở giảng đường, bạn bè là nguồn lực lớn: nhóm học đều đặn giúp mình hiểu bài nhanh hơn tự đọc một mình. Nhưng cũng cần ranh giới: từ chối nhẹ nhàng khi bị kéo vào deadline hộ quá thường xuyên, hoặc khi nhóm làm việc không chia việc rõ ràng. Mình hay dùng bảng Trello/Notion tối giản: ai làm phần nào, hạn nào, họp 15 phút đầu tuần để chỉnh lịch.

**4. Sức khỏe thể chất và tinh thần**

Ngủ đủ, vận động nhẹ và nói chuyện với người tin tưởng không phải là “lãng phí thời gian” — đó là phần giữ cho mình học được lâu dài. Nếu có giai đoạn trầm cảm, lo âu kéo dài, hãy tìm hỗ trợ chuyên môn sớm; điều đó không có nghĩa là bạn yếu đuối.

**Lời kết**

Mỗi người một hoàn cảnh; bài này chỉ là một cách tiếp cận mình đã thử và thấy ổn. Rất mong được nghe thêm kinh nghiệm của các bạn ở các khoa khác — có thể chúng ta cùng bổ sung thành một “cẩm nang” mở cho cộng đồng sinh viên HCMUE.

Chúc mọi người một học kỳ bình an và tiến bộ rõ rệt.
""";

    private static readonly string[] ShowcaseGeneralThreadComments =
    {
        "Đọc xong mình thấy đỡ lo hơn hẳn — đặc biệt phần chia nhỏ mục tiêu theo tuần. Cảm ơn bạn đã viết chi tiết như vậy.",
        "Mình năm nhất, đang cố tìm nhịp học ổn định. Bạn có thể chia sẻ thêm ví dụ cụ thể về khung thời gian một ngày bình thường của bạn không?",
        "Phần ranh giới nhóm học rất thực tế. Mình từng ôm việc hộ cả nhóm đến kiệt sức; giờ sẽ thử bảng chia việc như bạn gợi ý.",
        "Mình đồng ý phần sức khỏe tinh thần — có kỳ mình cố quá dẫn đến burnout, sau đó phải nghỉ giải lao lâu mới lấy lại nhịp.",
        "Bài viết có thể pin làm tài liệu cho tân sinh viên được. Mong diễn đàn có thêm các chia sẻ tương tự từ các khoa khác.",
    };

    private static readonly string[] DefaultThreadReplyComments =
    {
        "Cảm ơn bạn đã chia sẻ, nội dung rất hữu ích.",
        "Mình đã áp dụng một phần và thấy hiệu quả rõ rệt sau vài tuần.",
    };

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding forum data...");

        // 1. Seed / migrate hierarchical categories (khu cha → diễn đàn con)
        await ForumCategoryHierarchySeed.SeedOrMigrateAsync(context, logger);

        // 2. Seed Tags
        if (!await context.Tags.AnyAsync())
        {
            var tagData = new[]
            {
                ("CSharp", "Ngôn ngữ lập trình C#"),
                ("JavaScript", "Ngôn ngữ lập trình JavaScript"),
                ("Python", "Ngôn ngữ lập trình Python"),
                ("DotNet", "Framework .NET"),
                ("React", "Thư viện React.js"),
                ("SQL", "Structured Query Language"),
                ("Git", "Quản lý phiên bản với Git"),
                ("Docker", "Container platform Docker"),
                ("Machine_Learning", "Học máy và AI"),
                ("Web_Development", "Phát triển web"),
                ("Mobile", "Phát triển ứng dụng di động"),
                ("Database", "Cơ sở dữ liệu"),
                ("Algorithm", "Thuật toán và cấu trúc dữ liệu"),
                ("Career", "Định hướng nghề nghiệp"),
                ("Tips", "Mẹo hay và kinh nghiệm"),
            };

            var tags = new List<Tag>();
            var tagId = 1;
            foreach (var (name, desc) in tagData)
            {
                tags.Add(Tag.Create(TagId.Create(tagId++), name, desc).Value);
            }

            context.Tags.AddRange(tags);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} tags.", tags.Count);
        }

        // 2.5 Seed official thread channels (VOZ-style).
        if (!await context.ThreadChannels.AnyAsync())
        {
            var channelData = new[]
            {
                ("general", "General Thread", "Open discussion threads for campus life and casual exchange.", 10, true, true, true, true),
                ("qna", "Q&A Thread", "Question-first channel where accepted answers are encouraged.", 20, true, true, true, true),
                ("tech", "Tech Corner", "Technical debates and engineering deep dives.", 30, true, true, false, true),
                ("buy-sell", "Marketplace", "Buy/sell and exchange. Pin/accepted is disabled to reduce abuse.", 40, true, false, false, true),
            };

            var channels = channelData
                .Select(row => ThreadChannel.Create(
                    row.Item1,
                    row.Item2,
                    row.Item3,
                    row.Item4,
                    row.Item5,
                    row.Item6,
                    row.Item7,
                    row.Item8))
                .ToList();

            context.ThreadChannels.AddRange(channels);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} thread channels.", channels.Count);
        }

        // 3. Seed Posts + Comments for quick local FE testing.
        if (!await context.Posts.AnyAsync())
        {
            // Select strongly-typed ids only; .Id.Value is not translatable with EF value converters.
            var firstAuthorKey = await context.Users
                .AsNoTracking()
                .Select(user => user.Id)
                .FirstOrDefaultAsync();
            var systemAuthorId = firstAuthorKey?.Value ?? Guid.Parse("00000000-0000-0000-0000-000000000001");
            if (systemAuthorId == Guid.Empty)
            {
                systemAuthorId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            }

            var categoryByName = await context.Categories
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Name.Value, c => c.Id.Value, StringComparer.OrdinalIgnoreCase);

            var postSeedData = new[]
            {
                (
                    ShowcaseGeneralThreadTitle,
                    ShowcaseGeneralThreadBody.Trim(),
                    PostType.Discussion,
                    "general",
                    "Đời sống sinh viên"
                ),
                (
                    "EF Core: giảm chi phí Include/ProjectTo khi list bài kèm tag và category",
                    "Mình đang tối ưu API danh sách bài viết: mỗi bài có category, vài tag và đếm comment. Khi Include nhiều tầng thì thời gian phản hồi tăng rõ. Mọi người thường chọn AsSplitQuery, giới họn cột, hay chuyển sang projection/DTO từ đầu? Mình muốn nghe case thực tế hơn là lý thuyết chung chung.",
                    PostType.Question,
                    "qna",
                    "Hỏi đáp"
                ),
                (
                    "Gợi ý kiến trúc frontend: tách slice RTK Query theo feature hay theo domain API?",
                    "Nhóm mình đang chuẩn hóa codebase React sau một kỳ thực tập. Đang phân vân giữa injectEndpoints theo từng feature (forum, chat, career) và gom theo nhóm REST. Bạn nào đã migrate từ bundle lớn sang kiến trúc module, chia sẻ giúp mình vài bài học được không?",
                    PostType.Discussion,
                    "tech",
                    "Lập trình"
                ),
            };

            var threadChannelMap = await context.ThreadChannels
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Code, x => x.Id);

            var posts = new List<Post>();
            for (var i = 0; i < postSeedData.Length; i++)
            {
                var (titleRaw, contentRaw, type, channelCode, categoryName) = postSeedData[i];
                var title = PostTitle.Create(titleRaw).Value;
                var content = PostContent.Create(contentRaw).Value;
                Guid? categoryId = categoryByName.TryGetValue(categoryName, out var cid) ? cid : null;

                threadChannelMap.TryGetValue(channelCode, out var threadChannelId);
                var tagSet = i switch
                {
                    0 => new[] { "thread", "thread:doi-song-hoc-tap", "hcmue", "hoc-tap", "kinh-nghiem", "doi-song-sinh-vien" },
                    1 => new[] { "efcore", "dotnet", "performance", "sql", "Tips" },
                    _ => new[] { "react", "frontend", "architecture", "rtk-query", "hcmue" },
                };

                var post = Post.Create(
                    title,
                    content,
                    type,
                    systemAuthorId,
                    categoryId,
                    tagSet,
                    threadChannelId == Guid.Empty ? null : threadChannelId).Value;
                post.Publish();
                posts.Add(post);
            }

            context.Posts.AddRange(posts);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} forum posts.", posts.Count);

            var comments = new List<Comment>();
            for (var i = 0; i < posts.Count; i++)
            {
                var post = posts[i];
                var templates = i == 0 ? ShowcaseGeneralThreadComments : DefaultThreadReplyComments;
                foreach (var line in templates)
                {
                    var body = CommentContent.Create(line).Value;
                    comments.Add(Comment.Create(post.Id, systemAuthorId, body).Value);
                    post.IncrementCommentCount();
                }
            }

            context.Comments.AddRange(comments);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} forum comments.", comments.Count);
        }

        await EnsureGeneralThreadShowcaseIfEmptyAsync(context, logger);
        await EnsureDiscussionPackPostsAsync(context, logger);
    }

    /// <summary>
    /// Idempotent pack of discussion-heavy threads (many short replies). Safe to run on existing DBs.
    /// </summary>
    private static async Task EnsureDiscussionPackPostsAsync(ApplicationDbContext context, ILogger logger)
    {
        const string markerTitle =
            "Trao đổi trong tuần: làm nhóm online và chia workload thế nào cho đỡ vỡ deadline";

        if (await context.Posts.AsNoTracking().AnyAsync(p => p.Title.Value == markerTitle))
        {
            return;
        }

        var firstAuthorKey = await context.Users
            .AsNoTracking()
            .Select(user => user.Id)
            .FirstOrDefaultAsync();
        var systemAuthorId = firstAuthorKey?.Value ?? Guid.Parse("00000000-0000-0000-0000-000000000001");
        if (systemAuthorId == Guid.Empty)
        {
            systemAuthorId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        var categoryByName = await context.Categories
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Name.Value, c => c.Id.Value, StringComparer.OrdinalIgnoreCase);
        if (categoryByName.Count == 0)
        {
            logger.LogWarning("Skipping discussion pack seed: no categories.");
            return;
        }

        var threadChannelMap = await context.ThreadChannels
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Code, x => x.Id);

        var markerTag = "seed-discussion-pack-v1";

        var pack = new (string Title, string Body, PostType Type, string Channel, string CategoryName, string[] Replies)[]
        {
            (
                markerTitle,
                """
                Tuần này mình muốn mở một chỗ để trao đổi thật “thực chiến” — không checklist chung chung.

                **Context:** nhóm 4–5 người, có người làm ban ngày có người chỉ rảnh tối. Deadline cuối tuần mà vẫn phải họp sync ngắn.

                Mình đang làm theo kiểu:
                • Google Doc một trang duy nhất: mục tiêu tuần, việc đang chạy, blocker (mỗi người ghi 1 dòng).
                • Họp 15 phút đầu tuần + 10 phút cuối tuần — không slide, chỉ nhìn Doc.

                Các bạn chia workload và nhắc nhau trong nhóm kiểu gì để không ai ôm quá tải? Chỗ nào hay vỡ nhất (communication, kỹ thuật, hay expectation)?
                """.Trim(),
                PostType.Discussion,
                "general",
                "Thảo luận chung",
                new[]
                {
                    "Mình hay vỡ ở chỗ expectation: có người nghĩ “xong phần A là xong”, trong khi phần B phụ thuộc A nhưng không ai confirm.",
                    "Thử RACI mini trong Doc: ai Responsible / Accountable cho từng mục — chỉ 4–5 dòng nhưng đỡ tranh cãi.",
                    "Sync tối khó với ai có ca sớm. Mình đề xuất deadline nội bộ sớm hơn deadline giảng viên 24h để có buffer.",
                    "Mình vote họp ngắn + ghi blocker ngay trong chat nhóm, không để sang hôm sau mới nhắc.",
                    "Hay bị kẹt vì một người biến mất 2 ngày — có rule “48h không phản hồi thì escalate lên nhóm” không?",
                    "Với task kỹ thuật, mình chia nhỏ PR: mỗi PR một mục tiêu rõ, review trong ngày để không dồn cuối tuần.",
                    "Mình để ý stress hay đến từ việc không ai chủ động báo delay sớm — nhắn “delay 1 ngày” sớm vẫn hơn im lặng.",
                    "Cảm ơn các bạn — mình sẽ thử RACI + deadline nội bộ sớm hơn tuần sau và báo lại kết quả.",
                }
            ),
            (
                "Offline vs online: làm sao để buổi họp nhóm không thành… đọc slide cho nhau nghe?",
                """
                Một điều mình nhận ra sau vài kỳ: họp dài không đồng nghĩa với tiến độ tốt.

                **Thử nghiệm của nhóm mình:**
                • Agenda tối đa 3 bullet, mỗi bullet có owner và output mong đợi.
                • Không demo slide trừ khi có người yêu cầu trước — ưu tiên screen share repo / prototype.

                Bạn nào có format agenda ngắn mà vẫn giữ được discipline, chia sẻ giúp?
                """.Trim(),
                PostType.Discussion,
                "general",
                "Thảo luận chung",
                new[]
                {
                    "Mình dùng template: Goal → Decision needed → Next step — hết 10 phút là tan họp.",
                    "Hay bị lệch sang “báo cáo tiến độ” thay vì “quyết định”. Giờ mình bắt buộc có ít nhất 1 quyết định ghi ra cuối họp.",
                    "Với nhóm introvert, cho phép comment async trước họp 30p để đỡ áp lực nói live.",
                    "Nếu chỉ có 1 người chuẩn bị kỹ, họp sẽ biến thành mini seminar — rotate người điều phối mỗi tuần.",
                    "Mình đề xuất timebox cứng: 25 phút làm việc + 5 phút note — không extend.",
                    "Off-topic là killer — có “parking lot” list để ghi ý hay nhưng không tranh luận ngay.",
                    "Thử nhé tuần sau mình áp Goal/Decision/Next step.",
                }
            ),
            (
                "Làm sao đặt câu hỏi kỹ thuật để người khác muốn trả lời (thay vì… đọc qua)?",
                """
                Mình hay thấy thread hỏi đáp chết vì thiếu ngữ cảnh: không version, không error message, không minimal repro.

                Gợi ý khung:
                1) Mình đang làm gì / kỳ vọng gì?
                2) Thực tế đang ra gì?
                3) Đã thử những gì (ngắn)?

                Các bạn có thêm rule nào để thread technical đỡ bị “???” không?
                """.Trim(),
                PostType.Question,
                "qna",
                "Hỏi đáp",
                new[]
                {
                    "Rule của mình: luôn dán stack trace (hoặc log rút gọn) — không dán full wall of text.",
                    "“Expected vs actual” hai dòng là đủ để người khác hiểu gap.",
                    "Nếu là frontend, kèm browser + steps reproduce — tiết kiệm 3 round trip hỏi lại.",
                    "Mình hay thêm một câu “mình đoán chỗ nghi ngờ là …” để người trả lời có neo.",
                    "Đồng ý minimal repro — có khi tự tối giản ra được bug luôn.",
                    "Hay gặp thread chỉ ghi “lỗi rồi fix giùm” — không ai muốn nhặt.",
                }
            ),
            (
                "Tech Corner: trade-off giữa “ship nhanh” và “chuẩn hoá kiến trúc” trong đồ án nhóm?",
                """
                Giai đoạn giữa kỳ thường căng: một nhánh muốn refactor sớm, một nhánh muốn vá để kịp demo.

                Mình muốn nghe framework ra quyết định của các nhóm: khi nào chấp nhận nợ kỹ thuật, khi nào phải dừng feature để làm nền?

                Không cần đúng một đáp án — chỉ cần kinh nghiệm thật.
                """.Trim(),
                PostType.Discussion,
                "tech",
                "Lập trình",
                new[]
                {
                    "Demo tuần sau thì mình ưu tiên đường đi vui nhất có thể; refactor sau demo nếu vẫn còn thời gian.",
                    "Nếu bug có thể làm mất điểm khi chấm thì refactor trước — còn style thì sau.",
                    "Mình hay viết ADR 5 dòng: quyết định gì / vì sao / trade-off — để sau không cãi nhau.",
                    "Ship nhanh nhưng có feature flag / kill switch — ít nhất rollback được.",
                    "Hay chia sprint: tuần 1 skeleton + integration; tuần 2 polish — đỡ dồn architecture vào cuối.",
                    "Cảm ơn các góc nhìn — mình sẽ thử ADR ngắn trong nhóm.",
                }
            ),
        };

        var posts = new List<Post>();
        for (var i = 0; i < pack.Length; i++)
        {
            var (titleRaw, body, type, channelCode, categoryName, _) = pack[i];
            var title = PostTitle.Create(titleRaw).Value;
            var content = PostContent.Create(body).Value;
            if (!categoryByName.TryGetValue(categoryName, out var categoryId))
            {
                categoryId = categoryByName.Values.First();
            }
            threadChannelMap.TryGetValue(channelCode, out var threadChannelId);
            var tagSet = new List<string>
            {
                markerTag,
                "thread",
                "thao-luan",
                "hcmue",
                channelCode,
            };

            var post = Post.Create(
                    title,
                    content,
                    type,
                    systemAuthorId,
                    categoryId,
                    tagSet,
                    threadChannelId == Guid.Empty ? null : threadChannelId)
                .Value;
            post.Publish();
            posts.Add(post);
        }

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();

        var comments = new List<Comment>();
        for (var i = 0; i < posts.Count; i++)
        {
            var post = posts[i];
            var (_, _, _, _, _, replyLines) = pack[i]; // CategoryName unused here
            foreach (var line in replyLines)
            {
                var body = CommentContent.Create(line).Value;
                comments.Add(Comment.Create(post.Id, systemAuthorId, body).Value);
                post.IncrementCommentCount();
            }
        }

        context.Comments.AddRange(comments);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded discussion-heavy thread pack ({Count} posts, {CommentCount} comments).", posts.Count, comments.Count);
    }

    /// <summary>
    /// If the database already had posts but nothing in the &quot;general&quot; thread channel, add the showcase thread once (safe to run repeatedly).
    /// </summary>
    private static async Task EnsureGeneralThreadShowcaseIfEmptyAsync(ApplicationDbContext context, ILogger logger)
    {
        var generalChannelId = await context.ThreadChannels
            .AsNoTracking()
            .Where(c => c.Code == "general")
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if (generalChannelId == Guid.Empty)
        {
            return;
        }

        var hasGeneralPost = await context.Posts.AnyAsync(p => p.ThreadChannelId == generalChannelId);
        if (hasGeneralPost)
        {
            return;
        }

        var firstAuthorKey = await context.Users
            .AsNoTracking()
            .Select(user => user.Id)
            .FirstOrDefaultAsync();
        var systemAuthorId = firstAuthorKey?.Value ?? Guid.Parse("00000000-0000-0000-0000-000000000001");
        if (systemAuthorId == Guid.Empty)
        {
            systemAuthorId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        var categoryByName = await context.Categories
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Name.Value, c => c.Id.Value, StringComparer.OrdinalIgnoreCase);
        if (categoryByName.Count == 0)
        {
            logger.LogWarning("Cannot backfill general thread showcase: no categories.");
            return;
        }

        if (!categoryByName.TryGetValue("Đời sống sinh viên", out var categoryId))
        {
            categoryId = categoryByName.Values.First();
        }

        var title = PostTitle.Create(ShowcaseGeneralThreadTitle).Value;
        var body = PostContent.Create(ShowcaseGeneralThreadBody.Trim()).Value;
        var tagSet = new[] { "thread", "thread:doi-song-hoc-tap", "hcmue", "hoc-tap", "kinh-nghiem", "doi-song-sinh-vien" };

        var post = Post.Create(
            title,
            body,
            PostType.Discussion,
            systemAuthorId,
            categoryId,
            tagSet,
            generalChannelId).Value;
        post.Publish();

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var comments = new List<Comment>();
        foreach (var line in ShowcaseGeneralThreadComments)
        {
            var c = CommentContent.Create(line).Value;
            comments.Add(Comment.Create(post.Id, systemAuthorId, c).Value);
            post.IncrementCommentCount();
        }

        context.Comments.AddRange(comments);
        await context.SaveChangesAsync();

        logger.LogInformation("Backfilled showcase post for general thread channel (no prior posts in that channel).");
    }

}

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;

using UniHub.Forum.Domain.Categories;

using UniHub.Forum.Domain.Categories.ValueObjects;
using UniHub.Forum.Domain.Posts.ValueObjects;



namespace UniHub.Infrastructure.Persistence.Seeding;



/// <summary>

/// Cây danh mục kiểu VOZ (khu cha → diễn đàn con). Văn phong ngắn, thực dụng.

/// </summary>

internal static class ForumCategoryHierarchySeed

{

    private const string MarkerParentSlug = "dai-sanh";



    /// <summary>Khu cha cũ (tên i18n) — gán lại con rồi deactivate.</summary>

    private static readonly string[] ObsoleteZoneParentNames =

    [

        "Học tập & Chuyên môn",

        "Cộng đồng & Đời sống",

        "Sự nghiệp",

    ];



    private sealed record CategoryNode(

        string Name,

        string Description,

        int DisplayOrder,

        CategoryNode[]? Children = null);



    private static readonly CategoryNode[] Tree =

    [

        new(

            "Đại sảnh",

            "Thông báo trường, góp ý UniHub và lịch sự kiện.",

            10,

            [

                new("Góp ý", "Báo lỗi, đề xuất, phản hồi về diễn đàn.", 1),

                new("Sự kiện", "Workshop, lễ, hoạt động CLB — Đoàn.", 2),

                new("Thông báo", "Tin chính thức: lịch thi, học phí, quy chế.", 3),

            ]),

        new(

            "Học thuật",

            "Học bài, hỏi đáp, kinh nghiệm môn và đồ án.",

            20,

            [

                new("Học tập", "Outline, tài liệu, cách ôn theo môn.", 1),

                new("Hỏi đáp", "Hỏi nhanh — mong câu trả lời cụ thể.", 2),

                new("Báo cáo & đồ án", "Tiểu luận, nhóm đồ án, format, hướng dẫn.", 3),

            ]),

        new(

            "Công nghệ",

            "Code, máy, mobile — tương tự khu Máy tính bên VOZ.",

            30,

            [

                new("Lập trình", "Ngôn ngữ, framework, debug, review code.", 1),

                new("Phần cứng & PC", "Build PC, linh kiện, màn hình, nhiệt.", 2),

                new("Mobile & tablet", "App, Android/iOS, máy tính bảng.", 3),

            ]),

        new(

            "Cộng đồng",

            "Chém gió, đời sống, giải trí sau giờ học.",

            40,

            [

                new("Thảo luận chung", "Chủ đề tự do, không gò môn.", 1),

                new("Đời sống sinh viên", "KTX, part-time, tâm sự, đời sống.", 2),

                new("Giải trí", "Phim, game, nhạc — chill.", 3),

            ]),

        new(

            "Việc làm",

            "Thực tập, tuyển dụng, CV và phỏng vấn.",

            50,

            [

                new("Tuyển dụng", "Job full-time, fresher, referral.", 1),

                new("Thực tập", "Intern, onboarding, kinh nghiệm thực tế.", 2),

            ]),

    ];



    private static readonly HashSet<string> CanonicalParentNames =

        Tree.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);



    public static async Task SeedOrMigrateAsync(ApplicationDbContext context, ILogger logger)

    {

        if (!await context.Categories.AnyAsync())

        {

            await SeedFreshAsync(context, logger);

            return;

        }



        if (await IsCanonicalTreeAsync(context))

        {

            logger.LogDebug("Forum category tree is already canonical.");

            return;

        }



        await ReconcileToCanonicalTreeAsync(context, logger);

    }



    /// <summary>

    /// Cây chuẩn: có Đại sảnh (dai-sanh), Góp ý là con của Đại sảnh, không còn khu cha i18n cũ.

    /// </summary>

    private static async Task<bool> IsCanonicalTreeAsync(ApplicationDbContext context)

    {

        if (await context.Categories.AnyAsync(c =>

                c.IsActive && ObsoleteZoneParentNames.Contains(c.Name.Value)))

        {

            return false;

        }



        var daiSanh = await context.Categories

            .FirstOrDefaultAsync(c => c.Slug.Value == MarkerParentSlug && c.ParentCategoryId == null && c.IsActive);

        if (daiSanh is null)

        {

            return false;

        }



        var hasGopYChild = await context.Categories.AnyAsync(c =>

            c.IsActive && c.ParentCategoryId == daiSanh.Id && c.Name.Value == "Góp ý");

        if (!hasGopYChild)

        {

            return false;

        }



        var canonicalParentCount = await context.Categories.CountAsync(c =>

            c.IsActive && c.ParentCategoryId == null && CanonicalParentNames.Contains(c.Name.Value));



        return canonicalParentCount == Tree.Length;

    }



    private static async Task SeedFreshAsync(ApplicationDbContext context, ILogger logger)

    {

        var created = 0;

        foreach (var parentNode in Tree)

        {

            created += await CreateNodeAsync(context, parentNode, parentId: null);

        }



        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} forum categories (hierarchical).", created);

    }



    private static async Task ReconcileToCanonicalTreeAsync(ApplicationDbContext context, ILogger logger)

    {

        logger.LogInformation("Reconciling forum categories to canonical VOZ-style tree...");



        var all = await context.Categories.ToListAsync();

        var byName = all

            .GroupBy(c => c.Name.Value, StringComparer.OrdinalIgnoreCase)

            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);



        RenameLegacyTechCategoryIfNeeded(byName);



        var created = 0;

        foreach (var parentNode in Tree)

        {

            var parent = GetOrCreateCanonicalParent(context, byName, parentNode);

            foreach (var childNode in parentNode.Children ?? [])

            {

                if (byName.TryGetValue(childNode.Name, out var existing))

                {

                    var desc = CategoryDescription.Create(childNode.Description).Value;

                    if (existing.Description.Value != childNode.Description)

                    {

                        existing.Update(existing.Name, desc, childNode.DisplayOrder);

                    }



                    existing.AssignHierarchy(parent.Id, childNode.DisplayOrder);

                    if (!existing.IsActive)

                    {

                        existing.Activate();

                    }



                    continue;

                }



                var child = CreateCategory(childNode.Name, childNode.Description, parent.Id, childNode.DisplayOrder);

                context.Categories.Add(child);

                byName[child.Name.Value] = child;

                created++;

            }

        }



        DeactivateObsoleteZoneParents(all);

        DeactivateDuplicateCanonicalRoots(all);



        await context.SaveChangesAsync();

        logger.LogInformation(

            "Forum category reconcile done ({Created} new leaves, {Active} active categories).",

            created,

            await context.Categories.CountAsync(c => c.IsActive));

    }



    private static void RenameLegacyTechCategoryIfNeeded(Dictionary<string, Category> byName)

    {

        if (byName.TryGetValue("Công nghệ", out var legacyTech)

            && legacyTech.ParentCategoryId is null

            && !byName.ContainsKey("Lập trình"))

        {

            var lapTrinhName = CategoryName.Create("Lập trình").Value;

            var lapTrinhDesc = CategoryDescription.Create(

                "Ngôn ngữ, framework, debug, review code.").Value;

            legacyTech.Update(lapTrinhName, lapTrinhDesc, displayOrder: 1);

            byName.Remove("Công nghệ");

            byName["Lập trình"] = legacyTech;

        }

    }



    private static Category GetOrCreateCanonicalParent(

        ApplicationDbContext context,

        Dictionary<string, Category> byName,

        CategoryNode parentNode)

    {

        var expectedSlug = Slug.Create(parentNode.Name).Value.Value;

        var existingBySlug = byName.Values.FirstOrDefault(c =>

            c.ParentCategoryId is null

            && string.Equals(c.Slug.Value, expectedSlug, StringComparison.OrdinalIgnoreCase));



        if (existingBySlug is not null)

        {

            SyncParentMetadata(existingBySlug, parentNode);

            byName[parentNode.Name] = existingBySlug;

            return existingBySlug;

        }



        if (byName.TryGetValue(parentNode.Name, out var existingByName) && existingByName.ParentCategoryId is null)

        {

            SyncParentMetadata(existingByName, parentNode);

            return existingByName;

        }



        var parent = CreateCategory(parentNode.Name, parentNode.Description, parentId: null, parentNode.DisplayOrder);

        context.Categories.Add(parent);

        byName[parent.Name.Value] = parent;

        return parent;

    }



    private static void SyncParentMetadata(Category parent, CategoryNode node)

    {

        var desc = CategoryDescription.Create(node.Description).Value;

        if (parent.Description.Value != node.Description || parent.DisplayOrder != node.DisplayOrder)

        {

            parent.Update(parent.Name, desc, node.DisplayOrder);

        }

        else

        {

            parent.AssignHierarchy(null, node.DisplayOrder);

        }



        if (!parent.IsActive)

        {

            parent.Activate();

        }

    }



    private static void DeactivateObsoleteZoneParents(List<Category> all)

    {

        foreach (var category in all)

        {

            if (!category.IsActive || category.ParentCategoryId is not null)

            {

                continue;

            }



            if (ObsoleteZoneParentNames.Contains(category.Name.Value))

            {

                category.Deactivate();

            }

        }

    }



    /// <summary>Tắt khu cha trùng tên (giữ bản có slug chuẩn).</summary>

    private static void DeactivateDuplicateCanonicalRoots(List<Category> all)

    {

        foreach (var group in all.Where(c => c.ParentCategoryId is null && CanonicalParentNames.Contains(c.Name.Value))

                     .GroupBy(c => c.Name.Value, StringComparer.OrdinalIgnoreCase))

        {

            var ordered = group

                .OrderByDescending(c => c.IsActive)

                .ThenBy(c => c.Slug.Value, StringComparer.OrdinalIgnoreCase)

                .ToList();



            foreach (var duplicate in ordered.Skip(1))

            {

                if (duplicate.IsActive)

                {

                    duplicate.Deactivate();

                }

            }

        }

    }



    private static async Task<int> CreateNodeAsync(

        ApplicationDbContext context,

        CategoryNode node,

        CategoryId? parentId)

    {

        var count = 0;

        var category = CreateCategory(node.Name, node.Description, parentId, node.DisplayOrder);

        context.Categories.Add(category);

        count++;



        if (node.Children is { Length: > 0 })

        {

            await context.SaveChangesAsync();

            foreach (var child in node.Children)

            {

                count += await CreateNodeAsync(context, child, category.Id);

            }

        }



        return count;

    }



    private static Category CreateCategory(

        string name,

        string description,

        CategoryId? parentId,

        int displayOrder)

    {

        var nameVo = CategoryName.Create(name).Value;

        var descVo = CategoryDescription.Create(description).Value;

        return Category.Create(nameVo, descVo, parentId, displayOrder).Value;

    }

}



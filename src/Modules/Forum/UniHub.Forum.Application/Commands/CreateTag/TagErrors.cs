using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.CreateTag;

/// <summary>
/// Common tag-related errors
/// </summary>
public static class TagErrors
{
    public static readonly Error TagNotFound = new(
        "Tag.NotFound",
        "The specified tag does not exist");

    public static readonly Error TagAlreadyExists = new(
        "Tag.AlreadyExists",
        "A tag with this name already exists");

    public static readonly Error TagInUse = new(
        "Tag.InUse",
        "Cannot delete tag that is in use");

    public static readonly Error InvalidPageNumber = new(
        "Tag.InvalidPageNumber",
        "Page number must be greater than 0");

    public static readonly Error InvalidPageSize = new(
        "Tag.InvalidPageSize",
        "Page size must be between 1 and 100");

    public static readonly Error InvalidCount = new(
        "Tag.InvalidCount",
        "Count must be between 1 and 50");
}

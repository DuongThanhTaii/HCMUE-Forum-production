using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Reports;

public sealed record ReportId(int Value) : StronglyTypedId<int>(Value);

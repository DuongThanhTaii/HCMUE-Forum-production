using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Applications;

/// <summary>
/// Contains all domain errors for Application aggregate.
/// </summary>
public static class ApplicationErrors
{
    // Submission errors
    public static readonly Error JobPostingIdEmpty = new(
        "Application.JobPostingIdEmpty",
        "Job posting ID is required.");

    public static readonly Error ApplicantIdEmpty = new(
        "Application.ApplicantIdEmpty",
        "Applicant ID is required.");

    public static readonly Error ResumeRequired = new(
        "Application.ResumeRequired",
        "Resume is required to submit an application.");

    // Status transition errors
    public static readonly Error AlreadyWithdrawn = new(
        "Application.AlreadyWithdrawn",
        "Application has already been withdrawn.");

    public static readonly Error AlreadyRejected = new(
        "Application.AlreadyRejected",
        "Application has already been rejected.");

    public static readonly Error AlreadyAccepted = new(
        "Application.AlreadyAccepted",
        "Application has already been accepted.");

    public static readonly Error CannotWithdrawAfterAccepted = new(
        "Application.CannotWithdrawAfterAccepted",
        "Cannot withdraw an application after accepting the offer.");

    public static readonly Error CannotWithdrawAfterRejected = new(
        "Application.CannotWithdrawAfterRejected",
        "Cannot withdraw an application after it has been rejected.");

    public static readonly Error MustBeOfferedToAccept = new(
        "Application.MustBeOfferedToAccept",
        "Can only accept an application that has been offered.");

    public static readonly Error StatusChangeNotAllowed = new(
        "Application.StatusChangeNotAllowed",
        "This status transition is not allowed.");

    public static readonly Error CannotRejectWithdrawnApplication = new(
        "Application.CannotRejectWithdrawnApplication",
        "Cannot reject an application that has been withdrawn.");

    public static readonly Error CannotOfferWithdrawnApplication = new(
        "Application.CannotOfferWithdrawnApplication",
        "Cannot offer a job to an application that has been withdrawn.");

    // User permission errors
    public static readonly Error NotApplicant = new(
        "Application.NotApplicant",
        "Only the applicant can perform this action.");

    public static readonly Error ReviewerIdEmpty = new(
        "Application.ReviewerIdEmpty",
        "Reviewer ID is required for status changes.");
}

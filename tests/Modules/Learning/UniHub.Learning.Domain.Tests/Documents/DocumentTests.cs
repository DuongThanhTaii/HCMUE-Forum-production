using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Documents.Events;
using UniHub.Learning.Domain.Documents.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Documents;

public class DocumentTests
{
    private readonly Guid _uploaderId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly Guid _reviewerId = Guid.NewGuid();

    private Document CreateValidDocument()
    {
        var title = DocumentTitle.Create("Test Document Title").Value;
        var description = DocumentDescription.Create("Test description").Value;
        var file = DocumentFile.Create("test.pdf", "/uploads/test.pdf", 1024 * 1024, "application/pdf").Value;
        
        return Document.Create(title, description, file, DocumentType.Slide, _uploaderId, _courseId).Value;
    }

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var title = DocumentTitle.Create("Document Title").Value;
        var description = DocumentDescription.Create("Document description").Value;
        var file = DocumentFile.Create("doc.pdf", "/path", 1000, "application/pdf").Value;

        // Act
        var result = Document.Create(title, description, file, DocumentType.Slide, _uploaderId, _courseId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(title);
        result.Value.Description.Should().Be(description);
        result.Value.File.Should().Be(file);
        result.Value.Type.Should().Be(DocumentType.Slide);
        result.Value.Status.Should().Be(DocumentStatus.Draft);
        result.Value.UploaderId.Should().Be(_uploaderId);
        result.Value.CourseId.Should().Be(_courseId);
        result.Value.DownloadCount.Should().Be(0);
        result.Value.ViewCount.Should().Be(0);
        result.Value.AverageRating.Should().Be(0);
        result.Value.RatingCount.Should().Be(0);
    }

    [Fact]
    public void Create_WithoutCourseId_ShouldReturnSuccess()
    {
        // Arrange
        var title = DocumentTitle.Create("Document Title").Value;
        var description = DocumentDescription.Create("Document description").Value;
        var file = DocumentFile.Create("doc.pdf", "/path", 1000, "application/pdf").Value;

        // Act
        var result = Document.Create(title, description, file, DocumentType.Exam, _uploaderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CourseId.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyUploaderId_ShouldReturnFailure()
    {
        // Arrange
        var title = DocumentTitle.Create("Document Title").Value;
        var description = DocumentDescription.Create("Document description").Value;
        var file = DocumentFile.Create("doc.pdf", "/path", 1000, "application/pdf").Value;

        // Act
        var result = Document.Create(title, description, file, DocumentType.Slide, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidUploader");
    }

    [Fact]
    public void Create_ShouldRaiseDocumentCreatedEvent()
    {
        // Arrange
        var title = DocumentTitle.Create("Document Title").Value;
        var description = DocumentDescription.Create("Document description").Value;
        var file = DocumentFile.Create("doc.pdf", "/path", 1000, "application/pdf").Value;

        // Act
        var result = Document.Create(title, description, file, DocumentType.Video, _uploaderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle();
        var domainEvent = result.Value.DomainEvents.First();
        domainEvent.Should().BeOfType<DocumentCreatedEvent>();
        
        var createdEvent = (DocumentCreatedEvent)domainEvent;
        createdEvent.DocumentId.Should().Be(result.Value.Id.Value);
        createdEvent.UploaderId.Should().Be(_uploaderId);
        createdEvent.Type.Should().Be(DocumentType.Video.ToString());
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_OnDraftDocument_ShouldUpdateSuccessfully()
    {
        // Arrange
        var document = CreateValidDocument();
        var newTitle = DocumentTitle.Create("Updated Title").Value;
        var newDescription = DocumentDescription.Create("Updated description").Value;
        var newFile = DocumentFile.Create("updated.pdf", "/uploads/updated.pdf", 2048, "application/pdf").Value;
        var newCourseId = Guid.NewGuid();

        // Act
        var result = document.Update(newTitle, newDescription, newFile, newCourseId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Title.Should().Be(newTitle);
        document.Description.Should().Be(newDescription);
        document.File.Should().Be(newFile);
        document.CourseId.Should().Be(newCourseId);
        document.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_OnDeletedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.Delete(_uploaderId);
        
        var newTitle = DocumentTitle.Create("Updated Title").Value;
        var newDescription = DocumentDescription.Create("Updated description").Value;
        var newFile = DocumentFile.Create("updated.pdf", "/path", 2048, "application/pdf").Value;

        // Act
        var result = document.Update(newTitle, newDescription, newFile, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.Deleted");
    }

    [Fact]
    public void Update_OnApprovedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Approve(_reviewerId);
        
        var newTitle = DocumentTitle.Create("Updated Title").Value;
        var newDescription = DocumentDescription.Create("Updated description").Value;
        var newFile = DocumentFile.Create("updated.pdf", "/path", 2048, "application/pdf").Value;

        // Act
        var result = document.Update(newTitle, newDescription, newFile, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.Approved");
    }

    [Fact]
    public void Update_OnPendingDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        
        var newTitle = DocumentTitle.Create("Updated Title").Value;
        var newDescription = DocumentDescription.Create("Updated description").Value;
        var newFile = DocumentFile.Create("updated.pdf", "/path", 2048, "application/pdf").Value;

        // Act
        var result = document.Update(newTitle, newDescription, newFile, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.PendingApproval");
    }

    [Fact]
    public void Update_ShouldRaiseDocumentUpdatedEvent()
    {
        // Arrange
        var document = CreateValidDocument();
        document.ClearDomainEvents(); // Clear creation event
        
        var newTitle = DocumentTitle.Create("Updated Title").Value;
        var newDescription = DocumentDescription.Create("Updated description").Value;
        var newFile = DocumentFile.Create("updated.pdf", "/path", 2048, "application/pdf").Value;

        // Act
        var result = document.Update(newTitle, newDescription, newFile, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.DomainEvents.Should().ContainSingle();
        var domainEvent = document.DomainEvents.First();
        domainEvent.Should().BeOfType<DocumentUpdatedEvent>();
    }

    #endregion

    #region SubmitForApproval Tests

    [Fact]
    public void SubmitForApproval_OnDraftDocument_ShouldSubmitSuccessfully()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        var result = document.SubmitForApproval();

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(DocumentStatus.PendingApproval);
        document.SubmittedAt.Should().NotBeNull();
        document.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SubmitForApproval_OnRejectedDocument_ShouldSubmitSuccessfully()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Reject(_reviewerId, "Not good enough");

        // Act
        var result = document.SubmitForApproval();

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(DocumentStatus.PendingApproval);
    }

    [Fact]
    public void SubmitForApproval_OnDeletedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.Delete(_uploaderId);

        // Act
        var result = document.SubmitForApproval();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.Deleted");
    }

    [Fact]
    public void SubmitForApproval_OnPendingDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.SubmitForApproval();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.AlreadyPending");
    }

    [Fact]
    public void SubmitForApproval_OnApprovedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Approve(_reviewerId);

        // Act
        var result = document.SubmitForApproval();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.AlreadyApproved");
    }

    [Fact]
    public void SubmitForApproval_ShouldRaiseDocumentSubmittedEvent()
    {
        // Arrange
        var document = CreateValidDocument();
        document.ClearDomainEvents();

        // Act
        var result = document.SubmitForApproval();

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.DomainEvents.Should().ContainSingle();
        var domainEvent = document.DomainEvents.First();
        domainEvent.Should().BeOfType<DocumentSubmittedForApprovalEvent>();
    }

    #endregion

    #region Approve Tests

    [Fact]
    public void Approve_OnPendingDocument_ShouldApproveSuccessfully()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        var comment = "Looks good!";

        // Act
        var result = document.Approve(_reviewerId, comment);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(DocumentStatus.Approved);
        document.ReviewerId.Should().Be(_reviewerId);
        document.ReviewComment.Should().Be(comment);
        document.ReviewedAt.Should().NotBeNull();
        document.UpdatedAt.Should().NotBeNull();
        document.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Approve_WithoutComment_ShouldApproveSuccessfully()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.Approve(_reviewerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.ReviewComment.Should().BeNull();
    }

    [Fact]
    public void Approve_WithEmptyReviewerId_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.Approve(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidReviewer");
    }

    [Fact]
    public void Approve_OnNonPendingDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument(); // Draft status

        // Act
        var result = document.Approve(_reviewerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotPending");
    }

    [Fact]
    public void Approve_OnDeletedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Delete(_uploaderId);

        // Act
        var result = document.Approve(_reviewerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.Deleted");
    }

    [Fact]
    public void Approve_ShouldClearRejectionReason()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Reject(_reviewerId, "Need improvements");
        document.SubmitForApproval();

        // Act
        var result = document.Approve(_reviewerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Approve_ShouldRaiseDocumentApprovedEvent()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.ClearDomainEvents();

        // Act
        var result = document.Approve(_reviewerId, "Good job!");

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.DomainEvents.Should().ContainSingle();
        var domainEvent = document.DomainEvents.First();
        domainEvent.Should().BeOfType<DocumentApprovedEvent>();
        
        var approvedEvent = (DocumentApprovedEvent)domainEvent;
        approvedEvent.ApproverId.Should().Be(_reviewerId);
        approvedEvent.ApprovalComment.Should().Be("Good job!");
    }

    #endregion

    #region Reject Tests

    [Fact]
    public void Reject_OnPendingDocument_ShouldRejectSuccessfully()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        var reason = "Content quality is not sufficient";

        // Act
        var result = document.Reject(_reviewerId, reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(DocumentStatus.Rejected);
        document.ReviewerId.Should().Be(_reviewerId);
        document.RejectionReason.Should().Be(reason);
        document.ReviewedAt.Should().NotBeNull();
        document.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_WithEmptyReviewerId_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.Reject(Guid.Empty, "Some reason");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidReviewer");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_WithNullOrWhitespaceReason_ShouldReturnFailure(string? invalidReason)
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.Reject(_reviewerId, invalidReason!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.MissingRejectionReason");
    }

    [Fact]
    public void Reject_WithTooShortReason_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.Reject(_reviewerId, "Bad");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.RejectionReasonTooShort");
    }

    [Fact]
    public void Reject_OnNonPendingDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument(); // Draft status

        // Act
        var result = document.Reject(_reviewerId, "Some valid reason here");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotPending");
    }

    [Fact]
    public void Reject_OnDeletedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Delete(_uploaderId);

        // Act
        var result = document.Reject(_reviewerId, "Some valid reason");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.Deleted");
    }

    [Fact]
    public void Reject_ShouldTrimReason()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        var reason = "  This needs improvement  ";

        // Act
        var result = document.Reject(_reviewerId, reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.RejectionReason.Should().Be("This needs improvement");
    }

    [Fact]
    public void Reject_ShouldRaiseDocumentRejectedEvent()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.ClearDomainEvents();
        var reason = "Quality not sufficient";

        // Act
        var result = document.Reject(_reviewerId, reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.DomainEvents.Should().ContainSingle();
        var domainEvent = document.DomainEvents.First();
        domainEvent.Should().BeOfType<DocumentRejectedEvent>();
        
        var rejectedEvent = (DocumentRejectedEvent)domainEvent;
        rejectedEvent.RejectorId.Should().Be(_reviewerId);
        rejectedEvent.RejectionReason.Should().Be(reason);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_WithValidDeleterId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var document = CreateValidDocument();
        var deleterId = Guid.NewGuid();

        // Act
        var result = document.Delete(deleterId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(DocumentStatus.Deleted);
        document.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Delete_WithEmptyDeleterId_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        var result = document.Delete(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidDeleter");
    }

    [Fact]
    public void Delete_OnAlreadyDeletedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.Delete(_uploaderId);

        // Act
        var result = document.Delete(_uploaderId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.AlreadyDeleted");
    }

    [Fact]
    public void Delete_ShouldRaiseDocumentDeletedEvent()
    {
        // Arrange
        var document = CreateValidDocument();
        document.ClearDomainEvents();

        // Act
        var result = document.Delete(_uploaderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.DomainEvents.Should().ContainSingle();
        var domainEvent = document.DomainEvents.First();
        domainEvent.Should().BeOfType<DocumentDeletedEvent>();
    }

    #endregion

    #region View and Download Count Tests

    [Fact]
    public void IncrementViewCount_ShouldIncreaseViewCount()
    {
        // Arrange
        var document = CreateValidDocument();
        var initialViewCount = document.ViewCount;

        // Act
        document.IncrementViewCount();

        // Assert
        document.ViewCount.Should().Be(initialViewCount + 1);
        document.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void IncrementViewCount_MultipleTimes_ShouldIncreaseCorrectly()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        document.IncrementViewCount();
        document.IncrementViewCount();
        document.IncrementViewCount();

        // Assert
        document.ViewCount.Should().Be(3);
    }

    [Fact]
    public void IncrementDownloadCount_ShouldIncreaseDownloadCount()
    {
        // Arrange
        var document = CreateValidDocument();
        var initialDownloadCount = document.DownloadCount;

        // Act
        document.IncrementDownloadCount();

        // Assert
        document.DownloadCount.Should().Be(initialDownloadCount + 1);
        document.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void IncrementDownloadCount_MultipleTimes_ShouldIncreaseCorrectly()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        document.IncrementDownloadCount();
        document.IncrementDownloadCount();

        // Assert
        document.DownloadCount.Should().Be(2);
    }

    #endregion

    #region Rating Tests

    [Fact]
    public void AddRating_OnApprovedDocument_ShouldAddSuccessfully()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Approve(_reviewerId);

        // Act
        var result = document.AddRating(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.AverageRating.Should().Be(5);
        document.RatingCount.Should().Be(1);
        document.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void AddRating_WithValidRating_ShouldAddSuccessfully(int rating)
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Approve(_reviewerId);

        // Act
        var result = document.AddRating(rating);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.AverageRating.Should().Be(rating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(10)]
    public void AddRating_WithInvalidRating_ShouldReturnFailure(int invalidRating)
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Approve(_reviewerId);

        // Act
        var result = document.AddRating(invalidRating);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidRating");
    }

    [Fact]
    public void AddRating_OnNonApprovedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument(); // Draft status

        // Act
        var result = document.AddRating(5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotApproved");
    }

    [Fact]
    public void AddRating_MultipleTimes_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Approve(_reviewerId);

        // Act
        document.AddRating(5);
        document.AddRating(4);
        document.AddRating(3);

        // Assert
        document.AverageRating.Should().Be(4.0); // (5+4+3)/3 = 4
        document.RatingCount.Should().Be(3);
    }

    #endregion

    #region Status Check Tests

    [Fact]
    public void IsApproved_OnApprovedDocument_ShouldReturnTrue()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Approve(_reviewerId);

        // Act
        var result = document.IsApproved();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsApproved_OnDraftDocument_ShouldReturnFalse()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        var result = document.IsApproved();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPending_OnPendingDocument_ShouldReturnTrue()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.IsPending();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPending_OnDraftDocument_ShouldReturnFalse()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        var result = document.IsPending();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRejected_OnRejectedDocument_ShouldReturnTrue()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Reject(_reviewerId, "Needs improvement");

        // Act
        var result = document.IsRejected();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRejected_OnDraftDocument_ShouldReturnFalse()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        var result = document.IsRejected();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Event Sourcing Workflow Tests

    [Fact]
    public void ApprovalWorkflow_CompleteCycle_ShouldHaveAllEvents()
    {
        // Arrange
        var document = CreateValidDocument();
        document.ClearDomainEvents();

        // Act - Complete approval workflow
        document.SubmitForApproval();
        document.Approve(_reviewerId);

        // Assert
        document.DomainEvents.Should().HaveCount(2);
        document.DomainEvents.First().Should().BeOfType<DocumentSubmittedForApprovalEvent>();
        document.DomainEvents.Last().Should().BeOfType<DocumentApprovedEvent>();
    }

    [Fact]
    public void RejectionWorkflow_CompleteCycle_ShouldHaveAllEvents()
    {
        // Arrange
        var document = CreateValidDocument();
        document.ClearDomainEvents();

        // Act - Complete rejection workflow
        document.SubmitForApproval();
        document.Reject(_reviewerId, "Not good enough");

        // Assert
        document.DomainEvents.Should().HaveCount(2);
        document.DomainEvents.First().Should().BeOfType<DocumentSubmittedForApprovalEvent>();
        document.DomainEvents.Last().Should().BeOfType<DocumentRejectedEvent>();
    }

    [Fact]
    public void ResubmissionWorkflow_AfterRejection_ShouldHaveAllEvents()
    {
        // Arrange
        var document = CreateValidDocument();
        document.ClearDomainEvents();

        // Act - Reject then resubmit and approve
        document.SubmitForApproval();
        document.Reject(_reviewerId, "Needs improvement");
        document.SubmitForApproval();
        document.Approve(_reviewerId);

        // Assert - Should have 4 events
        document.DomainEvents.Should().HaveCount(4);
        document.DomainEvents.ElementAt(0).Should().BeOfType<DocumentSubmittedForApprovalEvent>();
        document.DomainEvents.ElementAt(1).Should().BeOfType<DocumentRejectedEvent>();
        document.DomainEvents.ElementAt(2).Should().BeOfType<DocumentSubmittedForApprovalEvent>();
        document.DomainEvents.ElementAt(3).Should().BeOfType<DocumentApprovedEvent>();
    }

    #endregion

    #region AI Scan Tests

    [Fact]
    public void RecordAIScan_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.ClearDomainEvents();

        // Act
        var result = document.RecordAIScan("Pass", 0.95);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var domainEvent = document.DomainEvents.OfType<DocumentAIScannedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.DocumentId.Should().Be(document.Id.Value);
        domainEvent.ScanResult.Should().Be("Pass");
        domainEvent.Confidence.Should().Be(0.95);
    }

    [Fact]
    public void RecordAIScan_WithFlaggedReasons_ShouldRecordReasons()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.ClearDomainEvents();
        var flaggedReasons = new List<string> { "Potential plagiarism", "Inappropriate content" };

        // Act
        var result = document.RecordAIScan("Flagged", 0.75, flaggedReasons);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var domainEvent = document.DomainEvents.OfType<DocumentAIScannedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.FlaggedReasons.Should().BeEquivalentTo(flaggedReasons);
    }

    [Fact]
    public void RecordAIScan_WithEmptyScanResult_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.RecordAIScan("", 0.95);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidScanResult");
    }

    [Fact]
    public void RecordAIScan_WithInvalidConfidence_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.RecordAIScan("Pass", 1.5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidConfidence");
    }

    [Fact]
    public void RecordAIScan_OnDraftDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        var result = document.RecordAIScan("Pass", 0.95);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotPending");
    }

    #endregion

    #region Review Started Tests

    [Fact]
    public void StartReview_WithValidReviewer_ShouldReturnSuccess()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.ClearDomainEvents();

        // Act
        var result = document.StartReview(_reviewerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.ReviewerId.Should().Be(_reviewerId);
        var domainEvent = document.DomainEvents.OfType<DocumentReviewStartedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.DocumentId.Should().Be(document.Id.Value);
        domainEvent.ReviewerId.Should().Be(_reviewerId);
    }

    [Fact]
    public void StartReview_WithEmptyReviewerId_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.StartReview(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidReviewer");
    }

    [Fact]
    public void StartReview_OnDraftDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        var result = document.StartReview(_reviewerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotPending");
    }

    [Fact]
    public void StartReview_OnDeletedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Delete(_uploaderId);

        // Act
        var result = document.StartReview(_reviewerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.Deleted");
    }

    #endregion

    #region Request Revision Tests

    [Fact]
    public void RequestRevision_WithValidData_ShouldReturnToDraft()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.ClearDomainEvents();
        var reason = "Please add more citations and references";
        var notes = "Check pages 5-7";

        // Act
        var result = document.RequestRevision(_reviewerId, reason, notes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(DocumentStatus.Draft);
        document.ReviewerId.Should().Be(_reviewerId);
        document.RejectionReason.Should().Be(reason);
        document.ReviewComment.Should().Be(notes);
        
        var domainEvent = document.DomainEvents.OfType<DocumentRevisionRequestedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.DocumentId.Should().Be(document.Id.Value);
        domainEvent.RequestedBy.Should().Be(_reviewerId);
        domainEvent.RevisionReason.Should().Be(reason);
        domainEvent.RevisionNotes.Should().Be(notes);
    }

    [Fact]
    public void RequestRevision_WithEmptyRequesterId_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.RequestRevision(Guid.Empty, "Needs revision");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidRequester");
    }

    [Fact]
    public void RequestRevision_WithEmptyReason_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.RequestRevision(_reviewerId, "");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.MissingRevisionReason");
    }

    [Fact]
    public void RequestRevision_WithShortReason_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();

        // Act
        var result = document.RequestRevision(_reviewerId, "Too short");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.RevisionReasonTooShort");
    }

    [Fact]
    public void RequestRevision_OnDraftDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        var result = document.RequestRevision(_reviewerId, "Please revise");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotPending");
    }

    [Fact]
    public void RequestRevision_OnDeletedDocument_ShouldReturnFailure()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.Delete(_uploaderId);

        // Act
        var result = document.RequestRevision(_reviewerId, "Please revise this document");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.Deleted");
    }

    [Fact]
    public void RequestRevision_AllowsResubmission_AfterRevision()
    {
        // Arrange
        var document = CreateValidDocument();
        document.SubmitForApproval();
        document.RequestRevision(_reviewerId, "Needs more detail");

        // Act - Author can resubmit after revision
        var result = document.SubmitForApproval();

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(DocumentStatus.PendingApproval);
    }

    #endregion

    #region Complete Approval Workflow with AI and Review Tests

    [Fact]
    public void CompleteWorkflow_WithAIScanAndReview_ShouldHaveAllEvents()
    {
        // Arrange
        var document = CreateValidDocument();
        document.ClearDomainEvents();

        // Act - Complete workflow: Submit → AI Scan → Review Start → Approve
        document.SubmitForApproval();
        document.RecordAIScan("Pass", 0.98);
        document.StartReview(_reviewerId);
        document.Approve(_reviewerId, "Looks good!");

        // Assert
        document.DomainEvents.Should().HaveCount(4);
        document.DomainEvents.ElementAt(0).Should().BeOfType<DocumentSubmittedForApprovalEvent>();
        document.DomainEvents.ElementAt(1).Should().BeOfType<DocumentAIScannedEvent>();
        document.DomainEvents.ElementAt(2).Should().BeOfType<DocumentReviewStartedEvent>();
        document.DomainEvents.ElementAt(3).Should().BeOfType<DocumentApprovedEvent>();
    }

    [Fact]
    public void RevisionWorkflow_WithCompleteHistory_ShouldHaveAllEvents()
    {
        // Arrange
        var document = CreateValidDocument();
        document.ClearDomainEvents();

        // Act - Complete revision workflow: Submit → Review → Request Revision → Resubmit → Approve
        document.SubmitForApproval();
        document.StartReview(_reviewerId);
        document.RequestRevision(_reviewerId, "Please add references");
        document.SubmitForApproval();
        document.Approve(_reviewerId);

        // Assert
        document.DomainEvents.Should().HaveCount(5);
        document.DomainEvents.ElementAt(0).Should().BeOfType<DocumentSubmittedForApprovalEvent>();
        document.DomainEvents.ElementAt(1).Should().BeOfType<DocumentReviewStartedEvent>();
        document.DomainEvents.ElementAt(2).Should().BeOfType<DocumentRevisionRequestedEvent>();
        document.DomainEvents.ElementAt(3).Should().BeOfType<DocumentSubmittedForApprovalEvent>();
        document.DomainEvents.ElementAt(4).Should().BeOfType<DocumentApprovedEvent>();
    }

    #endregion
}

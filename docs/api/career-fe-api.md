# Career API for Frontend (`ApiResponse` Envelope)

## Base URL
- `/api/v1/jobs`
- `/api/v1/companies`
- `/api/v1/applications`
- `/api/v1/recruiters`

## Envelope
```json
{
  "success": true,
  "data": {},
  "message": "optional success message",
  "error": null
}
```

Error sample:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "error": "Human readable message"
}
```

## Jobs

### `GET /api/v1/jobs`
- **Auth**: optional
- **Query**: `companyId`, `jobType`, `experienceLevel`, `status`, `city`, `isRemote`, `searchTerm`, `page`, `pageSize`
- **200**: `ApiResponse<JobPostingListResponse>`

### `GET /api/v1/jobs/search`
- **Auth**: optional
- **Query**: `keywords`, `companyId`, `jobType`, `experienceLevel`, `city`, `isRemote`, `minSalary`, `maxSalary`, `currency`, `skills`, `tags`, `postedAfter`, `postedBefore`, `sortBy`, `page`, `pageSize`
- **200**: `ApiResponse<JobPostingSearchResponse>`

### `GET /api/v1/jobs/{id}`
- **Auth**: optional
- **200**: `ApiResponse<JobPostingResponse>`
- **404**: failure envelope

### `POST /api/v1/jobs`
- **Auth**: required
- **Body**: `CreateJobPostingCommand`
- **201**: `ApiResponse<JobPostingResponse>`

### `PUT /api/v1/jobs/{id}`
- **Auth**: required
- **Body**: `UpdateJobPostingCommand` (must include matching `jobPostingId`)
- **200**: `ApiResponse<JobPostingResponse>`
- **400**: failure envelope (`ID mismatch` or validation)

### `POST /api/v1/jobs/{id}/publish`
- **Auth**: required
- **200**: `ApiResponse<null>` with message `Job posting published successfully`

### `POST /api/v1/jobs/{id}/close`
- **Auth**: required
- **Body**: `CloseJobPostingCommand` (must include matching `jobPostingId`)
- **200**: `ApiResponse<null>` with message `Job posting closed successfully`

### `POST /api/v1/jobs/{id}/save`
- **Auth**: required
- **200**: `ApiResponse<null>` with message `Job saved successfully`

### `DELETE /api/v1/jobs/{id}/save`
- **Auth**: required
- **200**: `ApiResponse<null>` with message `Job unsaved successfully`

### `GET /api/v1/jobs/saved`
- **Auth**: required
- **Query**: `userId` (required), `page`, `pageSize`
- **200**: `ApiResponse<SavedJobsResponse>`

### `GET /api/v1/jobs/{id}/saved`
- **Auth**: required
- **Query**: `userId` (required)
- **200**: `ApiResponse<{ isSaved: boolean }>`

## Companies

### `POST /api/v1/companies`
- **Auth**: required
- **Body**: `RegisterCompanyCommand`
- **201**: `ApiResponse<CompanyResponse>`

### `POST /api/v1/companies/logo/upload`
- **Auth**: required
- **Content-Type**: `multipart/form-data`
- **Body**: field `file` — ảnh (đuôi `.png`, `.jpg`, `.jpeg`, `.gif`, `.webp`, `.bmp`)
- **200**: `ApiResponse` — `data`: `{ url: string }`, message `Company logo uploaded successfully`

### `GET /api/v1/companies/mine`
- **Auth**: required
- **200**: `ApiResponse` — danh sách công ty user đã đăng ký hoặc được link recruiter: `{ id, name, status, logoUrl }[]`

### `POST /api/v1/companies/{id}/approve`
- **Auth**: required
- **Permission**: `admin.system.manage`
- **200**: phê duyệt đăng ký company đang pending (chi tiết message theo handler)

### `GET /api/v1/companies/{id}`
- **Auth**: optional
- **200**: `ApiResponse<CompanyDetailResponse>`
- **404**: failure envelope

### `GET /api/v1/companies/{id}/statistics`
- **Auth**: required
- **200**: `ApiResponse<CompanyStatisticsResponse>`
- **404**: failure envelope

### `GET /api/v1/companies/{id}/jobs`
- **Auth**: optional
- **Query**: `page`, `pageSize`
- **200**: `ApiResponse<JobPostingListResponse>`

### `GET /api/v1/companies/{id}/applications`
- **Auth**: required
- **Query**: `page`, `pageSize`
- **200**: `ApiResponse<RecentApplicationsResponse>`
- **404**: failure envelope

## Applications

### `POST /api/v1/applications`
- **Auth**: required
- **Body**: `SubmitApplicationCommand`
- **201**: `ApiResponse<ApplicationResponse>`

### `GET /api/v1/applications`
- **Auth**: required
- **Query**: `page`, `pageSize`
- **Note**: uses authenticated user as applicant
- **200**: `ApiResponse<ApplicationsByApplicantResponse>`

### `GET /api/v1/applications/{id}`
- **Auth**: required
- **200**: `ApiResponse<ApplicationResponse>`
- **404**: failure envelope

### `PUT /api/v1/applications/{id}/status`
- **Auth**: required
- **Body**: `UpdateApplicationStatusCommand` (must include matching `applicationId`)
- **200**: `ApiResponse<null>` with message `Application status updated successfully`

### `POST /api/v1/applications/{id}/withdraw`
- **Auth**: required
- **Body**: `WithdrawApplicationCommand` (must include matching `applicationId`)
- **200**: `ApiResponse<null>` with message `Application withdrawn successfully`

### `POST /api/v1/applications/{id}/accept`
- **Auth**: required
- **Body**: `AcceptApplicationCommand` (must include matching `applicationId`)
- **200**: `ApiResponse<null>` with message `Job offer accepted successfully`

### `POST /api/v1/applications/{id}/reject`
- **Auth**: required
- **Body**: `RejectApplicationCommand` (must include matching `applicationId`)
- **200**: `ApiResponse<null>` with message `Application rejected successfully`

### `GET /api/v1/applications/jobs/{jobId}`
- **Auth**: required
- **Query**: `page`, `pageSize`
- **200**: `ApiResponse<ApplicationListResponse>`

## Recruiters

### `POST /api/v1/recruiters`
- **Auth**: required
- **Body**: `AddRecruiterCommand`
- **201**: `ApiResponse<RecruiterResponse>`

### `GET /api/v1/recruiters/companies/{companyId}`
- **Auth**: required
- **Query**: `activeOnly` (optional)
- **200**: `ApiResponse<RecruitersResponse>`

### `GET /api/v1/recruiters/check?userId=&companyId=`
- **Auth**: required
- **200**: `ApiResponse<IsRecruiterResponse>`

### `PUT /api/v1/recruiters/{id}/permissions`
- **Auth**: required
- **Body**: `UpdateRecruiterPermissionsCommand` (must include matching `recruiterId`)
- **200**: `ApiResponse<null>` with message `Recruiter permissions updated successfully`

### `POST /api/v1/recruiters/{id}/deactivate`
- **Auth**: required
- **Body**: `DeactivateRecruiterCommand` (must include matching `recruiterId`)
- **200**: `ApiResponse<null>` with message `Recruiter deactivated successfully`

### `POST /api/v1/recruiters/{id}/reactivate`
- **Auth**: required
- **Body**: `ReactivateRecruiterCommand` (must include matching `recruiterId`)
- **200**: `ApiResponse<null>` with message `Recruiter reactivated successfully`

## Schemas

### `CreateJobPostingCommand`
- `title` (string)
- `description` (string)
- `companyId` (guid)
- `postedBy` (guid)
- `jobType` (string enum)
- `experienceLevel` (string enum)
- `city` (string)
- `district` (string | null, optional)
- `address` (string | null, optional)
- `isRemote` (boolean, optional; default `false`)
- `minSalary` (number | null, optional)
- `maxSalary` (number | null, optional)
- `salaryCurrency` (string | null, optional)
- `salaryPeriod` (string | null, optional)
- `deadline` (datetime | null, optional)

### `UpdateJobPostingCommand`
- `jobPostingId` (guid)
- `title` (string)
- `description` (string)
- `jobType` (string enum)
- `experienceLevel` (string enum)
- `city` (string)
- `district` (string | null, optional)
- `address` (string | null, optional)
- `isRemote` (boolean, optional; default `false`)
- `minSalary` (number | null, optional)
- `maxSalary` (number | null, optional)
- `salaryCurrency` (string | null, optional)
- `salaryPeriod` (string | null, optional)
- `deadline` (datetime | null, optional)

### `CloseJobPostingCommand`
- `jobPostingId` (guid)
- `reason` (string)

### `JobPostingResponse`
- `jobPostingId` (guid)
- `title` (string)
- `description` (string)
- `companyId` (guid)
- `postedBy` (guid)
- `jobType` (string)
- `experienceLevel` (string)
- `status` (string)
- `salary` (object | null)
  - `minAmount` (number)
  - `maxAmount` (number)
  - `currency` (string)
  - `period` (string)
- `location` (object)
  - `city` (string)
  - `district` (string | null)
  - `address` (string | null)
  - `isRemote` (boolean)
- `deadline` (datetime | null)
- `createdAt` (datetime)
- `updatedAt` (datetime | null)
- `publishedAt` (datetime | null)
- `viewCount` (int)
- `applicationCount` (int)
- `tags` (string[])

### `JobPostingListResponse`
- `items` (JobPostingSummary[])
- `totalCount` (int)
- `page` (int)
- `pageSize` (int)
- `totalPages` (int)

### `JobPostingSummary`
- `jobPostingId` (guid)
- `title` (string)
- `companyId` (guid)
- `jobType` (string)
- `experienceLevel` (string)
- `status` (string)
- `city` (string)
- `isRemote` (boolean)
- `salary` (object | null)
  - `minAmount` (number)
  - `maxAmount` (number)
  - `currency` (string)
  - `period` (string)
- `deadline` (datetime | null)
- `createdAt` (datetime)
- `publishedAt` (datetime | null)
- `viewCount` (int)
- `applicationCount` (int)

### `JobPostingSearchResponse`
- `items` (JobPostingSearchResult[])
- `totalCount` (int)
- `page` (int)
- `pageSize` (int)
- `totalPages` (int)
- `metadata` (SearchMetadata)

### `JobPostingSearchResult`
- `jobPostingId` (guid)
- `title` (string)
- `description` (string)
- `companyId` (guid)
- `jobType` (string)
- `experienceLevel` (string)
- `status` (string)
- `salary` (object | null)
  - `minAmount` (number)
  - `maxAmount` (number)
  - `currency` (string)
  - `period` (string)
- `location` (object)
  - `city` (string)
  - `district` (string | null)
  - `address` (string | null)
  - `isRemote` (boolean)
- `requirements` (string[])
- `tags` (string[])
- `createdAt` (datetime)
- `publishedAt` (datetime | null)
- `viewCount` (int)
- `applicationCount` (int)
- `relevanceScore` (number)

### `SearchMetadata`
- `searchKeywords` (string | null)
- `filtersApplied` (int)
- `averageRelevanceScore` (number)
- `searchDuration` (string, TimeSpan)

### `SavedJobsResponse`
- `items` (SavedJobDto[])
- `totalCount` (int)
- `page` (int)
- `pageSize` (int)
- `totalPages` (int)

### `SavedJobDto`
- `jobPostingId` (guid)
- `title` (string)
- `companyId` (guid)
- `companyName` (string)
- `jobType` (string)
- `experienceLevel` (string)
- `status` (string)
- `location` (object)
  - `city` (string)
  - `district` (string | null)
  - `isRemote` (boolean)
- `salary` (object | null)
  - `minAmount` (number)
  - `maxAmount` (number)
  - `currency` (string)
  - `period` (string)
- `publishedAt` (datetime | null)
- `deadline` (datetime | null)
- `viewCount` (int)
- `applicationCount` (int)
- `savedAt` (datetime)

### `RegisterCompanyCommand`
- `name` (string)
- `description` (string)
- `industry` (string enum)
- `size` (string enum)
- `email` (string)
- `phone` (string | null)
- `address` (string | null)
- `registeredBy` (guid)
- `website` (string | null, optional)
- `logoUrl` (string | null, optional)
- `foundedYear` (int | null, optional)
- `linkedIn` (string | null, optional)
- `facebook` (string | null, optional)
- `twitter` (string | null, optional)
- `instagram` (string | null, optional)
- `youTube` (string | null, optional)

### `CompanyResponse`
- `companyId` (guid)
- `name` (string)
- `description` (string)
- `industry` (string)
- `size` (string)
- `status` (string)
- `contactInfo` (object)
  - `email` (string)
  - `phone` (string | null)
  - `address` (string | null)
- `website` (string | null)
- `logoUrl` (string | null)
- `foundedYear` (int | null)
- `registeredAt` (datetime)

### `CompanyDetailResponse`
- `companyId` (guid)
- `name` (string)
- `description` (string)
- `industry` (string)
- `size` (string)
- `website` (string | null)
- `logoUrl` (string | null)
- `location` (string | null)
- `status` (string)
- `registeredAt` (datetime)
- `totalJobPostings` (int)
- `activeJobPostings` (int)

### `CompanyStatisticsResponse`
- `companyId` (guid)
- `companyName` (string)
- `overview` (CompanyOverviewStats)
- `jobPostings` (JobPostingStats)
- `applications` (ApplicationStats)
- `topPerformingJobs` (TopJobPosting[])

### `CompanyOverviewStats`
- `totalJobPostings` (int)
- `activeJobPostings` (int)
- `totalApplications` (int)
- `totalViews` (int)
- `lastJobPostedAt` (datetime | null)

### `JobPostingStats`
- `draft` (int)
- `published` (int)
- `paused` (int)
- `closed` (int)
- `expired` (int)

### `ApplicationStats`
- `pending` (int)
- `reviewing` (int)
- `shortlisted` (int)
- `interviewed` (int)
- `offered` (int)
- `accepted` (int)
- `rejected` (int)
- `withdrawn` (int)
- `acceptanceRate` (number)
- `rejectionRate` (number)

### `TopJobPosting`
- `jobPostingId` (guid)
- `title` (string)
- `applicationCount` (int)
- `viewCount` (int)
- `publishedAt` (datetime)

### `SubmitApplicationCommand`
- `jobPostingId` (guid)
- `applicantId` (guid)
- `resumeFileName` (string)
- `resumeFileUrl` (string)
- `resumeFileSizeBytes` (long)
- `resumeContentType` (string)
- `coverLetterContent` (string | null, optional)

### `ApplicationResponse`
- `id` (guid)
- `jobPostingId` (guid)
- `applicantId` (guid)
- `status` (string)
- `resume` (ResumeDto)
- `coverLetter` (string | null)
- `submittedAt` (datetime)
- `lastStatusChangedAt` (datetime | null)
- `lastStatusChangedBy` (guid | null)
- `reviewNotes` (string | null)

### `ResumeDto`
- `fileName` (string)
- `fileUrl` (string)
- `fileSizeBytes` (long)
- `contentType` (string)

### `ApplicationsByApplicantResponse`
- `items` (ApplicantApplicationSummary[])
- `totalCount` (int)
- `page` (int)
- `pageSize` (int)
- `totalPages` (int)

### `ApplicantApplicationSummary`
- `id` (guid)
- `jobPostingId` (guid)
- `jobPostingTitle` (string)
- `companyName` (string)
- `status` (string)
- `submittedAt` (datetime)
- `lastStatusChangedAt` (datetime | null)
- `hasCoverLetter` (boolean)

### `ApplicationListResponse`
- `items` (ApplicationSummary[])
- `totalCount` (int)
- `page` (int)
- `pageSize` (int)
- `totalPages` (int)

### `ApplicationSummary`
- `id` (guid)
- `jobPostingId` (guid)
- `applicantId` (guid)
- `status` (string)
- `submittedAt` (datetime)
- `lastStatusChangedAt` (datetime | null)
- `hasCoverLetter` (boolean)

### `RecentApplicationsResponse`
- `items` (ApplicationSummaryDto[])
- `totalCount` (int)
- `page` (int)
- `pageSize` (int)
- `totalPages` (int)

### `ApplicationSummaryDto`
- `applicationId` (guid)
- `jobPostingId` (guid)
- `jobTitle` (string)
- `applicantId` (guid)
- `applicantName` (string)
- `status` (string)
- `submittedAt` (datetime)
- `lastStatusChangedAt` (datetime | null)
- `hasCoverLetter` (boolean)

### `UpdateApplicationStatusCommand`
- `applicationId` (guid)
- `reviewerId` (guid)
- `targetStatus` (string)
- `notes` (string | null, optional)

### `WithdrawApplicationCommand`
- `applicationId` (guid)
- `applicantId` (guid)
- `reason` (string | null, optional)

### `AcceptApplicationCommand`
- `applicationId` (guid)
- `applicantId` (guid)

### `RejectApplicationCommand`
- `applicationId` (guid)
- `reviewerId` (guid)
- `reason` (string | null, optional)

### `AddRecruiterCommand`
- `userId` (guid)
- `companyId` (guid)
- `canManageJobPostings` (boolean, optional; default `true`)
- `canReviewApplications` (boolean, optional; default `true`)
- `canUpdateApplicationStatus` (boolean, optional; default `true`)
- `canInviteRecruiters` (boolean, optional; default `false`)
- `addedBy` (guid)

### `RecruiterResponse`
- `recruiterId` (guid)
- `userId` (guid)
- `companyId` (guid)
- `permissions` (RecruiterPermissionsDto)
- `status` (string)
- `addedBy` (guid)
- `addedAt` (datetime)

### `RecruitersResponse`
- `recruiters` (RecruiterDto[])
- `totalCount` (int)

### `RecruiterDto`
- `recruiterId` (guid)
- `userId` (guid)
- `status` (string)
- `permissions` (RecruiterPermissionsDto)
- `addedAt` (datetime)

### `IsRecruiterResponse`
- `isRecruiter` (boolean)
- `isActive` (boolean)
- `permissions` (RecruiterPermissionsDto | null)

### `RecruiterPermissionsDto`
- `canManageJobPostings` (boolean)
- `canReviewApplications` (boolean)
- `canUpdateApplicationStatus` (boolean)
- `canInviteRecruiters` (boolean)

### `UpdateRecruiterPermissionsCommand`
- `recruiterId` (guid)
- `canManageJobPostings` (boolean)
- `canReviewApplications` (boolean)
- `canUpdateApplicationStatus` (boolean)
- `canInviteRecruiters` (boolean)
- `updatedBy` (guid)

### `DeactivateRecruiterCommand`
- `recruiterId` (guid)
- `deactivatedBy` (guid)

### `ReactivateRecruiterCommand`
- `recruiterId` (guid)
- `reactivatedBy` (guid)

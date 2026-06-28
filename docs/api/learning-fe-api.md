# Learning API for Frontend (`ApiResponse` Envelope)

## Base URL
- `/api/v1/courses`
- `/api/v1/faculties`
- `/api/v1/documents`

## Response Envelope (applies to all endpoints)
```json
{
  "success": true,
  "data": {},
  "message": "optional success message",
  "error": null
}
```

Error example:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "error": "Human readable message"
}
```

---

## Faculties

### `GET /api/v1/faculties`
- **Auth**: anonymous
- **200**: `ApiResponse<List<FacultyListItemResponse>>`

### `GET /api/v1/faculties/{id}`
- **Auth**: anonymous
- **200**: `ApiResponse<FacultyDetailResponse>`
- **404**: `ApiResponse<null>` (failure)

### `POST /api/v1/faculties`
- **Auth**: required
- **Body**: `CreateFacultyRequest`
- **201**: `ApiResponse<CreateFacultyResponse>`
- **400**: `ApiResponse<null>` (failure)

---

## Courses

### `GET /api/v1/courses?facultyId=&semester=`
- **Auth**: anonymous
- **200**: `ApiResponse<List<CourseListItemResponse>>`

### `GET /api/v1/courses/{id}`
- **Auth**: anonymous
- **200**: `ApiResponse<CourseDetailResponse>`
- **404**: `ApiResponse<null>` (failure)

### `POST /api/v1/courses`
- **Auth**: required
- **Body**: `CreateCourseRequest`
- **201**: `ApiResponse<CreateCourseResponse>`

### `PUT /api/v1/courses/{id}`
- **Auth**: required
- **Body**: `UpdateCourseRequest`
- **200**: `ApiResponse<null>` with success message `Course updated successfully`

### `DELETE /api/v1/courses/{id}`
- **Auth**: required
- **Body**: `DeleteCourseRequest` (requires `deletedBy`)
- **200**: `ApiResponse<null>` with success message `Course deleted successfully`

### `POST /api/v1/courses/{id}/moderators`
- **Auth**: required
- **Body**: `AssignModeratorRequest` (requires `moderatorId`, `assignedBy`)
- **200**: `ApiResponse<null>` with success message `Moderator assigned successfully`

### `DELETE /api/v1/courses/{id}/moderators/{moderatorId}`
- **Auth**: required
- **Body**: `RemoveModeratorRequest` (requires `removedBy`)
- **200**: `ApiResponse<null>` with success message `Moderator removed successfully`

---

## Documents

### `GET /api/v1/documents`
- **Auth**: anonymous
- **Query**: `searchTerm`, `courseId`, `facultyId`, `documentType`, `status`, `sortBy`, `sortDescending`, `pageNumber`, `pageSize`
- **200**: `ApiResponse<SearchDocumentsResult>`

### `GET /api/v1/documents/{id}`
- **Auth**: anonymous
- **200**: `ApiResponse<DocumentDetailResponse>`
- **404**: `ApiResponse<null>` (failure)

### `POST /api/v1/documents/upload`
- **Auth**: required
- **Content-Type**: `multipart/form-data`
- **Body**: `UploadDocumentRequest` (file + metadata)
- **201**: `ApiResponse<UploadDocumentResponse>`

### `POST /api/v1/documents/{id}/rate`
- **Auth**: required
- **Body**: `RateDocumentRequest`
- **200**: `ApiResponse<null>` with success message `Document rated successfully`

### `POST /api/v1/documents/{id}/download`
- **Auth**: required
- **Body**: `DownloadDocumentRequest`
- **200**: `ApiResponse<null>` with success message `Document download tracked successfully`

### `POST /api/v1/documents/{id}/approve`
- **Auth**: required
- **Body**: `ApproveDocumentRequest`
- **200**: `ApiResponse<null>` with success message `Document approved successfully`

### `POST /api/v1/documents/{id}/reject`
- **Auth**: required
- **Body**: `RejectDocumentRequest`
- **200**: `ApiResponse<null>` with success message `Document rejected successfully`

### `POST /api/v1/documents/{id}/request-revision`
- **Auth**: required
- **Body**: `RequestRevisionRequest`
- **200**: `ApiResponse<null>` with success message `Revision requested successfully`

## Schemas

### `FacultyListItemResponse`
- `facultyId` (guid)
- `code` (string)
- `name` (string)
- `description` (string)
- `status` (string)
- `courseCount` (int)
- `createdAt` (datetime)

### `FacultyDetailResponse`
- `facultyId` (guid)
- `code` (string)
- `name` (string)
- `description` (string)
- `status` (string)
- `managerId` (guid | null)
- `courseCount` (int)
- `createdBy` (guid)
- `createdAt` (datetime)
- `updatedAt` (datetime | null)

### `CreateFacultyRequest`
- `code` (string)
- `name` (string)
- `description` (string | null)
- `managerId` (guid | null)

### `CreateFacultyResponse`
- `facultyId` (guid)
- `code` (string)
- `name` (string)

### `CourseListItemResponse`
- `courseId` (guid)
- `code` (string)
- `name` (string)
- `description` (string)
- `semester` (string)
- `credits` (int)
- `facultyId` (guid | null)
- `createdAt` (datetime)
- `documentCount` (int)

### `CourseDetailResponse`
- `courseId` (guid)
- `code` (string)
- `name` (string)
- `description` (string)
- `semester` (string)
- `credits` (int)
- `facultyId` (guid | null)
- `createdBy` (guid)
- `createdAt` (datetime)
- `updatedAt` (datetime | null)
- `moderatorIds` (guid[])
- `documentCount` (int)
- `isDeleted` (boolean)

### `CreateCourseRequest`
- `code` (string)
- `name` (string)
- `description` (string | null)
- `semester` (string)
- `credits` (int)
- `createdBy` (guid)
- `facultyId` (guid | null)

### `CreateCourseResponse`
- `courseId` (guid)
- `code` (string)
- `name` (string)

### `UpdateCourseRequest`
- `name` (string)
- `description` (string | null)
- `semester` (string)
- `credits` (int)

### `DeleteCourseRequest`
- `deletedBy` (guid)

### `AssignModeratorRequest`
- `moderatorId` (guid)
- `assignedBy` (guid)

### `RemoveModeratorRequest`
- `removedBy` (guid)

### `SearchDocumentsResult`
- `documents` (DocumentSearchDto[])
- `totalCount` (int)
- `pageNumber` (int)
- `pageSize` (int)
- `totalPages` (int)

### `DocumentSearchDto`
- `id` (guid)
- `title` (string)
- `description` (string)
- `documentType` (string)
- `status` (string)
- `fileName` (string)
- `fileSize` (long)
- `contentType` (string)
- `averageRating` (number)
- `ratingCount` (int)
- `viewCount` (int)
- `downloadCount` (int)
- `uploaderId` (guid)
- `courseId` (guid | null)
- `createdAt` (datetime)
- `updatedAt` (datetime)

### `DocumentDetailResponse`
- `id` (guid)
- `title` (string)
- `description` (string)
- `documentType` (string)
- `status` (string)
- `fileName` (string)
- `fileSize` (long)
- `contentType` (string)
- `averageRating` (number)
- `ratingCount` (int)
- `viewCount` (int)
- `downloadCount` (int)
- `uploaderId` (guid)
- `courseId` (guid | null)
- `reviewerId` (guid | null)
- `reviewComment` (string | null)
- `rejectionReason` (string | null)
- `createdAt` (datetime)
- `updatedAt` (datetime | null)
- `submittedAt` (datetime | null)
- `reviewedAt` (datetime | null)

### `UploadDocumentRequest`
- `title` (string)
- `description` (string | null)
- `fileName` (string)
- `fileContent` (byte[])
- `contentType` (string)
- `fileSize` (long)
- `documentType` (int)
- `uploaderId` (guid)
- `courseId` (guid | null)

### `UploadDocumentResponse`
- `documentId` (guid)
- `title` (string)

### `RateDocumentRequest`
- `userId` (guid)
- `rating` (int)

### `DownloadDocumentRequest`
- `userId` (guid)

### `ApproveDocumentRequest`
- `reviewerId` (guid)
- `comment` (string | null)

### `RejectDocumentRequest`
- `reviewerId` (guid)
- `reason` (string)

### `RequestRevisionRequest`
- `reviewerId` (guid)
- `reason` (string)

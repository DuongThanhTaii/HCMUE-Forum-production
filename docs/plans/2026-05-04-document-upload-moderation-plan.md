# Kế hoạch Triển khai: Upload Tài liệu & Kiểm duyệt (Tích hợp Cloudinary)

## 1. Tổng quan
Kế hoạch này hướng dẫn chi tiết các bước xây dựng tính năng Upload tài liệu và hệ thống Kiểm duyệt tài liệu (Duyệt/Từ chối/Yêu cầu sửa đổi) cho module `Learning`. Đồng thời, hệ thống sẽ được cấu hình để upload thẳng file lên **Cloudinary** thay vì lưu cục bộ.

## 2. Phần Backend (Cập nhật API & Tích hợp Cloudinary)

### 2.1. Cấu hình Cloudinary
- **Cài đặt package**: Thêm thư viện `CloudinaryDotNet` vào project `UniHub.Infrastructure` hoặc `UniHub.Learning.Infrastructure`.
- **Cấu hình AppSettings**: Thêm các thông số `CloudinarySettings` (CloudName, ApiKey, ApiSecret) vào `appsettings.json`.
- **Tạo Service**: Tạo `IFileStorageService` và class implementation `CloudinaryStorageService` để xử lý việc upload file (hỗ trợ định dạng PDF, Word, PowerPoint, v.v.).

### 2.2. Cập nhật DTOs & Controller (`DocumentsController.cs`)
- Đổi kiểu dữ liệu của file đầu vào từ `byte[] FileContent` sang `IFormFile File` trong `UploadDocumentRequest` để hỗ trợ stream file trực tiếp từ Form Data.
- Xử lý logic Upload: Controller sẽ chuyển `IFormFile` xuống Application layer. Application layer sẽ gọi `CloudinaryStorageService` để upload file và lấy về `SecureUrl`.
- Lưu URL trả về từ Cloudinary vào cột `file_path` (hoặc `file_url` nếu có) của Entity `Document`.

## 3. Phần Frontend (Giao diện người dùng)

### 3.1. Tính năng Upload Tài liệu (Dành cho Sinh viên/Giảng viên)
- **Tạo Component `DocumentUploadModal` / `DocumentUploadPage`**:
  - Giao diện kéo thả file (Drag & Drop) hoặc chọn file từ thiết bị (có thể dùng thư viện như `react-dropzone`).
  - Form nhập thông tin: `Title` (Tên tài liệu), `Description` (Mô tả), `DocumentType` (Dropdown: Bài giảng, Bài tập, Tham khảo...), và chọn `Course` (Môn học).
  - Validation: Ràng buộc dung lượng (ví dụ: < 10MB) và định dạng file cho phép (.pdf, .docx, .pptx).
- **API Integration**: Sử dụng `axios` với `Content-Type: multipart/form-data` để gọi API `POST /api/v1/documents/upload`.

### 3.2. Tính năng Kiểm duyệt Tài liệu (Dành cho Moderator/Lecturer)
- **Tạo Component `ModerationDashboard`**:
  - Bảng danh sách các tài liệu đang chờ duyệt (Pending / Status = 0).
  - Nút "Xem trước" (Preview) hoặc "Tải về" để kiểm tra nội dung.
- **Tạo các hành động Kiểm duyệt**:
  - **Approve (Duyệt)**: Giao diện xác nhận (Confirm Modal) kèm tuỳ chọn nhập comment. Gọi API `POST /api/v1/documents/{id}/approve`.
  - **Reject (Từ chối)**: Bắt buộc nhập lý do từ chối (Reason). Gọi API `POST /api/v1/documents/{id}/reject`.
  - **Request Revision (Yêu cầu sửa)**: Bắt buộc nhập yêu cầu sửa chữa. Gọi API `POST /api/v1/documents/{id}/request-revision`.

## 4. Các bước thực hiện & Phân công (Handoff)
- [ ] **Bước 1 (BE)**: Cung cấp Cloudinary Keys -> Cài đặt `CloudinaryDotNet` -> Viết `CloudinaryStorageService`.
- [ ] **Bước 2 (BE)**: Sửa lại `DocumentsController.cs` và các Command để nhận `IFormFile` thay vì `byte[]`.
- [ ] **Bước 3 (FE)**: Tạo API hooks (Sử dụng React Query) cho tính năng Upload và lấy danh sách tài liệu chờ duyệt.
- [ ] **Bước 4 (FE)**: Xây dựng UI Upload Form với `multipart/form-data`.
- [ ] **Bước 5 (FE)**: Xây dựng UI Bảng điều khiển kiểm duyệt (Moderation Dashboard).

---
*Lưu ý: Plan này đã sẵn sàng để chuyển cho người khác thực hiện.*

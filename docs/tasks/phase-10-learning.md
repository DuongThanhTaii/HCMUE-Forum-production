# TASK-107: Learning Module

> **Documents, courses, faculties, upload, approvals**

---

## 📋 TASK INFO

| Property         | Value                             |
| ---------------- | --------------------------------- |
| **Task ID**      | TASK-107                          |
| **Module**       | Learning                          |
| **Status**       | ⬜ NOT_STARTED                    |
| **Priority**     | 🔴 Critical                       |
| **Estimate**     | 10 hours                          |
| **Branch**       | `feature/TASK-107-learning-module` |
| **Dependencies** | TASK-104, TASK-105                 |

---

## 🎯 OBJECTIVES

- Build documents list with search/filters
- Create document detail with viewer
- Implement upload document form (with Cloudinary)
- Add rating system for documents
- Build courses and faculties pages
- Create approval queue for moderators
- Track downloads and views

---

## 📡 BACKEND API ENDPOINTS

```http
GET /api/v1/documents?courseId={id}&facultyId={id}&type={type}&page=1
GET /api/v1/documents/{id}
POST /api/v1/documents/upload (multipart/form-data)
POST /api/v1/documents/{id}/rate
POST /api/v1/documents/{id}/download
POST /api/v1/documents/{id}/approve (Moderator)
POST /api/v1/documents/{id}/reject (Moderator)

GET /api/v1/courses?facultyId={id}&semester={semester}
GET /api/v1/courses/{id}
POST /api/v1/courses (Admin)

GET /api/v1/faculties
GET /api/v1/faculties/{id}
POST /api/v1/faculties (Admin)
```

---

## 📁 KEY FILES

### 1. Documents List with Cloudinary Upload

**File**: `src/app/[locale]/(main)/learning/documents/page.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useDocuments } from '@/hooks/api/learning/useDocuments';
import { DocumentCard } from '@/components/features/learning/DocumentCard';
import { DocumentFilters } from '@/components/features/learning/DocumentFilters';
import { Button } from '@/components/ui/button';
import { Upload } from 'lucide-react';
import { Link } from '@/lib/i18n/routing';
import { Skeleton } from '@/components/ui/skeleton';

export default function DocumentsPage() {
  const [filters, setFilters] = useState({
    courseId: '',
    facultyId: '',
    documentType: '',
    page: 1,
  });

  const { data, isLoading } = useDocuments(filters);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Tài liệu học tập</h1>
          <p className="text-muted-foreground">Tìm kiếm và tải tài liệu</p>
        </div>
        <Button asChild>
          <Link href="/learning/documents/upload">
            <Upload className="mr-2 h-4 w-4" />
            Tải lên
          </Link>
        </Button>
      </div>

      <DocumentFilters filters={filters} onChange={setFilters} />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {isLoading
          ? Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-48" />
            ))
          : data?.items.map((doc) => <DocumentCard key={doc.id} document={doc} />)}
      </div>
    </div>
  );
}
```

### 2. Upload Document with Cloudinary

**File**: `src/app/[locale]/(main)/learning/documents/upload/page.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useRouter } from '@/lib/i18n/routing';
import { useUploadDocument } from '@/hooks/api/learning/useUploadDocument';
import { uploadToCloudinary } from '@/lib/cloudinary/upload';
import { documentSchema, type DocumentInput } from '@/lib/validations/document.schema';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from '@/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { FileUploader } from '@/components/shared/FileUploader';
import { toast } from 'sonner';

export default function UploadDocumentPage() {
  const router = useRouter();
  const [uploading, setUploading] = useState(false);
  const { mutate: uploadDocument, isPending } = useUploadDocument();

  const form = useForm<DocumentInput>({
    resolver: zodResolver(documentSchema),
  });

  const onSubmit = async (data: DocumentInput) => {
    const file = form.watch('file');
    if (!file) {
      toast.error('Vui lòng chọn file');
      return;
    }

    try {
      setUploading(true);
      
      // Upload to Cloudinary
      const cloudinaryResult = await uploadToCloudinary(file, {
        folder: 'unihub/documents',
        resource_type: 'raw',
      });

      // Submit to backend
      uploadDocument({
        ...data,
        fileUrl: cloudinaryResult.secure_url,
        fileName: file.name,
        fileSize: file.size,
        contentType: file.type,
      }, {
        onSuccess: () => {
          toast.success('Tải lên thành công! Đang chờ duyệt.');
          router.push('/learning/documents');
        },
      });
    } catch (error) {
      toast.error('Lỗi khi tải file lên');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Tải lên tài liệu</CardTitle>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <FormField
                control={form.control}
                name="title"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Tiêu đề</FormLabel>
                    <FormControl>
                      <Input placeholder="Ví dụ: Bài giảng Cấu trúc dữ liệu..." {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Mô tả</FormLabel>
                    <FormControl>
                      <Textarea placeholder="Mô tả ngắn về tài liệu..." {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="courseId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Môn học</FormLabel>
                    <Select onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Chọn môn học" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="course1">Cấu trúc dữ liệu</SelectItem>
                        <SelectItem value="course2">Lập trình Web</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="documentType"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Loại tài liệu</FormLabel>
                    <Select onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Chọn loại" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="Lecture">Bài giảng</SelectItem>
                        <SelectItem value="Assignment">Bài tập</SelectItem>
                        <SelectItem value="Exam">Đề thi</SelectItem>
                        <SelectItem value="Reference">Tài liệu tham khảo</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="file"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Tệp tài liệu</FormLabel>
                    <FormControl>
                      <FileUploader
                        accept=".pdf,.doc,.docx,.ppt,.pptx"
                        maxSize={10 * 1024 * 1024} // 10MB
                        onChange={field.onChange}
                      />
                    </FormControl>
                    <FormDescription>
                      Hỗ trợ: PDF, DOC, DOCX, PPT, PPTX. Tối đa 10MB
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="flex justify-end space-x-2">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => router.back()}
                  disabled={uploading || isPending}
                >
                  Hủy
                </Button>
                <Button type="submit" disabled={uploading || isPending}>
                  {uploading ? 'Đang tải lên...' : isPending ? 'Đang lưu...' : 'Tải lên'}
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
}
```

### 3. Cloudinary Upload Helper

**File**: `src/lib/cloudinary/upload.ts`

```typescript
interface CloudinaryUploadOptions {
  folder?: string;
  resource_type?: 'image' | 'video' | 'raw' | 'auto';
  public_id?: string;
}

interface CloudinaryUploadResult {
  secure_url: string;
  public_id: string;
  format: string;
  resource_type: string;
  bytes: number;
}

export async function uploadToCloudinary(
  file: File,
  options: CloudinaryUploadOptions = {}
): Promise<CloudinaryUploadResult> {
  const cloudName = process.env.NEXT_PUBLIC_CLOUDINARY_CLOUD_NAME;
  const uploadPreset = process.env.NEXT_PUBLIC_CLOUDINARY_UPLOAD_PRESET;

  if (!cloudName || !uploadPreset) {
    throw new Error('Cloudinary configuration missing');
  }

  const formData = new FormData();
  formData.append('file', file);
  formData.append('upload_preset', uploadPreset);
  
  if (options.folder) {
    formData.append('folder', options.folder);
  }
  if (options.public_id) {
    formData.append('public_id', options.public_id);
  }

  const resourceType = options.resource_type || 'auto';
  const url = `https://api.cloudinary.com/v1_1/${cloudName}/${resourceType}/upload`;

  const response = await fetch(url, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Upload failed');
  }

  return await response.json();
}
```

### 4. Document Card Component

**File**: `src/components/features/learning/DocumentCard.tsx`

```tsx
import { Link } from '@/lib/i18n/routing';
import { Card, CardContent, CardFooter } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Star, Download, Eye, FileText } from 'lucide-react';

interface Document {
  id: string;
  title: string;
  description: string;
  documentType: string;
  fileSize: number;
  downloadCount: number;
  viewCount: number;
  averageRating: number;
  ratingCount: number;
  status: string;
  uploaderName: string;
  courseName: string;
}

interface DocumentCardProps {
  document: Document;
}

export function DocumentCard({ document }: DocumentCardProps) {
  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  return (
    <Card className="flex flex-col">
      <CardContent className="flex-1 pt-6">
        <div className="mb-2 flex items-start justify-between">
          <FileText className="h-8 w-8 text-primary" />
          <Badge variant={document.status === 'Approved' ? 'default' : 'secondary'}>
            {document.status}
          </Badge>
        </div>
        
        <Link href={`/learning/documents/${document.id}`}>
          <h3 className="mb-2 text-lg font-semibold hover:underline">
            {document.title}
          </h3>
        </Link>
        
        <p className="mb-3 text-sm text-muted-foreground line-clamp-2">
          {document.description}
        </p>

        <div className="space-y-2 text-xs text-muted-foreground">
          <div className="flex items-center justify-between">
            <span>{document.courseName}</span>
            <span>{formatFileSize(document.fileSize)}</span>
          </div>
          
          <div className="flex items-center space-x-3">
            <div className="flex items-center space-x-1">
              <Star className="h-3 w-3 fill-yellow-400 text-yellow-400" />
              <span>{document.averageRating.toFixed(1)}</span>
              <span>({document.ratingCount})</span>
            </div>
            <div className="flex items-center space-x-1">
              <Download className="h-3 w-3" />
              <span>{document.downloadCount}</span>
            </div>
            <div className="flex items-center space-x-1">
              <Eye className="h-3 w-3" />
              <span>{document.viewCount}</span>
            </div>
          </div>
        </div>
      </CardContent>

      <CardFooter>
        <Button variant="outline" className="w-full" asChild>
          <Link href={`/learning/documents/${document.id}`}>
            Xem chi tiết
          </Link>
        </Button>
      </CardFooter>
    </Card>
  );
}
```

### 5. Approval Queue (Moderator)

**File**: `src/app/[locale]/(main)/learning/approvals/page.tsx`

```tsx
'use client';

import { useApprovalQueue } from '@/hooks/api/learning/useApprovalQueue';
import { DocumentCard } from '@/components/features/learning/DocumentCard';
import { Button } from '@/components/ui/button';
import { Check, X } from 'lucide-react';
import { useApproveDocument } from '@/hooks/api/learning/useApproveDocument';
import { useRejectDocument } from '@/hooks/api/learning/useRejectDocument';
import { toast } from 'sonner';

export default function ApprovalsPage() {
  const { data: documents, refetch } = useApprovalQueue();
  const { mutate: approve } = useApproveDocument();
  const { mutate: reject } = useRejectDocument();

  const handleApprove = (documentId: string) => {
    approve(documentId, {
      onSuccess: () => {
        toast.success('Đã duyệt tài liệu');
        refetch();
      },
    });
  };

  const handleReject = (documentId: string) => {
    reject(documentId, {
      onSuccess: () => {
        toast.success('Đã từ chối tài liệu');
        refetch();
      },
    });
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Duyệt tài liệu</h1>
        <p className="text-muted-foreground">
          Phê duyệt hoặc từ chối tài liệu chờ duyệt
        </p>
      </div>

      {documents?.length === 0 ? (
        <div className="rounded-lg border p-8 text-center text-muted-foreground">
          Không có tài liệu nào chờ duyệt
        </div>
      ) : (
        <div className="grid gap-4">
          {documents?.map((doc) => (
            <div key={doc.id} className="rounded-lg border p-4">
              <DocumentCard document={doc} />
              <div className="mt-4 flex justify-end space-x-2">
                <Button
                  variant="outline"
                  onClick={() => handleReject(doc.id)}
                >
                  <X className="mr-2 h-4 w-4" />
                  Từ chối
                </Button>
                <Button onClick={() => handleApprove(doc.id)}>
                  <Check className="mr-2 h-4 w-4" />
                  Phê duyệt
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Documents list with filters
- [ ] Upload document with Cloudinary
- [ ] Document detail with download button
- [ ] Rating system (1-5 stars)
- [ ] Download tracking
- [ ] View count increments
- [ ] Courses list by faculty
- [ ] Course detail with documents
- [ ] Faculties list
- [ ] Approval queue (moderator only)
- [ ] Approve/reject documents
- [ ] Search documents
- [ ] File type validation
- [ ] File size limit (10MB)

---

_Last Updated: 2026-02-10_

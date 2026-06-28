# TASK-109: Career Module

> **Jobs, companies, applications, saved jobs**

---

## 📋 TASK INFO

| Property         | Value                           |
| ---------------- | ------------------------------- |
| **Task ID**      | TASK-109                        |
| **Module**       | Career Hub                      |
| **Status**       | ⬜ NOT_STARTED                  |
| **Priority**     | 🟡 Medium                       |
| **Estimate**     | 10 hours                        |
| **Branch**       | `feature/TASK-109-career-module` |
| **Dependencies** | TASK-104, TASK-105               |

---

## 🎯 OBJECTIVES

- Build jobs list with advanced filters
- Create job detail page
- Implement application submission
- Add saved jobs functionality
- Build company profiles
- Create recruiter job posting form
- Show application status tracking

---

## 📡 BACKEND API ENDPOINTS

```http
GET /api/v1/jobs?location={loc}&jobType={type}&experience={level}&page=1
GET /api/v1/jobs/search?q={query}&filters...
GET /api/v1/jobs/{id}
POST /api/v1/jobs (Recruiter)
PUT /api/v1/jobs/{id}
POST /api/v1/jobs/{id}/publish
POST /api/v1/jobs/{id}/close
POST /api/v1/jobs/{id}/save
DELETE /api/v1/jobs/{id}/save
GET /api/v1/jobs/saved

POST /api/v1/applications
GET /api/v1/applications (my applications)
GET /api/v1/applications/{id}
PUT /api/v1/applications/{id}/status
POST /api/v1/applications/{id}/withdraw

GET /api/v1/companies/{id}
GET /api/v1/companies/{id}/jobs
POST /api/v1/companies (Register company)
```

---

## 📁 KEY FILES

### 1. Jobs List with Filters

**File**: `src/app/[locale]/(main)/career/jobs/page.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useJobs } from '@/hooks/api/career/useJobs';
import { JobCard } from '@/components/features/career/JobCard';
import { JobFilters } from '@/components/features/career/JobFilters';
import { Button } from '@/components/ui/button';
import { Briefcase } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';
import { Link } from '@/lib/i18n/routing';
import { useAuth } from '@/hooks/auth/useAuth';

export default function JobsPage() {
  const { user } = useAuth();
  const [filters, setFilters] = useState({
    location: '',
    jobType: '',
    experienceLevel: '',
    salaryMin: '',
    salaryMax: '',
    page: 1,
  });

  const { data, isLoading } = useJobs(filters);
  const isRecruiter = user?.roles.includes('Recruiter');

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Tìm việc làm</h1>
          <p className="text-muted-foreground">
            {data?.totalCount || 0} công việc phù hợp
          </p>
        </div>
        {isRecruiter && (
          <Button asChild>
            <Link href="/career/jobs/create">
              <Briefcase className="mr-2 h-4 w-4" />
              Đăng tin
            </Link>
          </Button>
        )}
      </div>

      <JobFilters filters={filters} onChange={setFilters} />

      <div className="grid gap-4 lg:grid-cols-2">
        {isLoading
          ? Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-48" />
            ))
          : data?.items.map((job) => <JobCard key={job.id} job={job} />)}
      </div>
    </div>
  );
}
```

### 2. Job Card Component

**File**: `src/components/features/career/JobCard.tsx`

```tsx
import { Link } from '@/lib/i18n/routing';
import { Card, CardContent, CardFooter } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Building2, MapPin, DollarSign, Clock, Bookmark } from 'lucide-react';
import { useSaveJob } from '@/hooks/api/career/useSaveJob';
import { useState } from 'react';

interface Job {
  id: string;
  title: string;
  company: {
    id: string;
    name: string;
    logo?: string;
  };
  location: string;
  salaryMin?: number;
  salaryMax?: number;
  jobType: string;
  experienceLevel: string;
  deadline: string;
  isSaved: boolean;
}

interface JobCardProps {
  job: Job;
}

export function JobCard({ job }: JobCardProps) {
  const [isSaved, setIsSaved] = useState(job.isSaved);
  const { mutate: toggleSave } = useSaveJob();

  const handleSave = (e: React.MouseEvent) => {
    e.preventDefault();
    toggleSave({ jobId: job.id, save: !isSaved });
    setIsSaved(!isSaved);
  };

  const formatSalary = () => {
    if (!job.salaryMin && !job.salaryMax) return 'Thỏa thuận';
    if (!job.salaryMax) return `Từ ${job.salaryMin?.toLocaleString()} VNĐ`;
    return `${job.salaryMin?.toLocaleString()} - ${job.salaryMax?.toLocaleString()} VNĐ`;
  };

  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardContent className="pt-6">
        <div className="flex items-start justify-between">
          <div className="flex space-x-4">
            {job.company.logo && (
              <img
                src={job.company.logo}
                alt={job.company.name}
                className="h-12 w-12 rounded object-cover"
              />
            )}
            <div className="flex-1">
              <Link href={`/career/jobs/${job.id}`}>
                <h3 className="text-lg font-semibold hover:underline">
                  {job.title}
                </h3>
              </Link>
              <div className="mt-1 flex items-center text-sm text-muted-foreground">
                <Building2 className="mr-1 h-4 w-4" />
                <Link
                  href={`/career/companies/${job.company.id}`}
                  className="hover:underline"
                >
                  {job.company.name}
                </Link>
              </div>
            </div>
          </div>

          <Button
            variant="ghost"
            size="icon"
            onClick={handleSave}
            className={isSaved ? 'text-primary' : ''}
          >
            <Bookmark className={isSaved ? 'fill-current' : ''} />
          </Button>
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          <Badge variant="secondary">
            <MapPin className="mr-1 h-3 w-3" />
            {job.location}
          </Badge>
          <Badge variant="secondary">
            <DollarSign className="mr-1 h-3 w-3" />
            {formatSalary()}
          </Badge>
          <Badge variant="outline">{job.jobType}</Badge>
          <Badge variant="outline">{job.experienceLevel}</Badge>
        </div>

        <div className="mt-4 flex items-center text-xs text-muted-foreground">
          <Clock className="mr-1 h-3 w-3" />
          Hạn nộp: {new Date(job.deadline).toLocaleDateString('vi-VN')}
        </div>
      </CardContent>

      <CardFooter>
        <Button className="w-full" asChild>
          <Link href={`/career/jobs/${job.id}`}>Xem chi tiết</Link>
        </Button>
      </CardFooter>
    </Card>
  );
}
```

### 3. Job Detail & Application Form

**File**: `src/app/[locale]/(main)/career/jobs/[id]/page.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useJob } from '@/hooks/api/career/useJob';
import { useSubmitApplication } from '@/hooks/api/career/useSubmitApplication';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { FileUploader } from '@/components/shared/FileUploader';
import { Building2, MapPin, DollarSign, Clock, Send } from 'lucide-react';
import { toast } from 'sonner';

export default function JobDetailPage({ params }: { params: { id: string } }) {
  const { data: job, isLoading } = useJob(params.id);
  const { mutate: submitApplication, isPending } = useSubmitApplication();
  const [coverLetter, setCoverLetter] = useState('');
  const [cv, setCV] = useState<File | null>(null);

  const handleApply = () => {
    if (!coverLetter || !cv) {
      toast.error('Vui lòng điền đầy đủ thông tin');
      return;
    }

    submitApplication({
      jobPostingId: params.id,
      coverLetter,
      cv,
    }, {
      onSuccess: () => {
        toast.success('Nộp đơn thành công!');
      },
    });
  };

  if (isLoading) return <div>Loading...</div>;
  if (!job) return <div>Không tìm thấy công việc</div>;

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <Card>
        <CardHeader>
          <div className="flex items-start justify-between">
            <div>
              <CardTitle className="text-2xl">{job.title}</CardTitle>
              <div className="mt-2 flex items-center text-muted-foreground">
                <Building2 className="mr-2 h-4 w-4" />
                <span>{job.company.name}</span>
              </div>
            </div>
            {job.company.logo && (
              <img
                src={job.company.logo}
                alt={job.company.name}
                className="h-16 w-16 rounded object-cover"
              />
            )}
          </div>
        </CardHeader>

        <CardContent className="space-y-6">
          <div className="flex flex-wrap gap-2">
            <Badge>
              <MapPin className="mr-1 h-3 w-3" />
              {job.location}
            </Badge>
            <Badge>
              <DollarSign className="mr-1 h-3 w-3" />
              {job.salaryMin?.toLocaleString()} - {job.salaryMax?.toLocaleString()} VNĐ
            </Badge>
            <Badge variant="outline">{job.jobType}</Badge>
            <Badge variant="outline">{job.experienceLevel}</Badge>
            <Badge variant="secondary">
              <Clock className="mr-1 h-3 w-3" />
              Hạn: {new Date(job.deadline).toLocaleDateString('vi-VN')}
            </Badge>
          </div>

          <Separator />

          <div>
            <h3 className="mb-2 text-lg font-semibold">Mô tả công việc</h3>
            <div
              className="prose prose-sm max-w-none"
              dangerouslySetInnerHTML={{ __html: job.description }}
            />
          </div>

          <div>
            <h3 className="mb-2 text-lg font-semibold">Yêu cầu</h3>
            <div
              className="prose prose-sm max-w-none"
              dangerouslySetInnerHTML={{ __html: job.requirements }}
            />
          </div>

          {job.benefits && (
            <div>
              <h3 className="mb-2 text-lg font-semibold">Quyền lợi</h3>
              <div
                className="prose prose-sm max-w-none"
                dangerouslySetInnerHTML={{ __html: job.benefits }}
              />
            </div>
          )}

          <Separator />

          <Dialog>
            <DialogTrigger asChild>
              <Button size="lg" className="w-full">
                <Send className="mr-2 h-4 w-4" />
                Nộp đơn ứng tuyển
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-2xl">
              <DialogHeader>
                <DialogTitle>Nộp đơn ứng tuyển: {job.title}</DialogTitle>
              </DialogHeader>
              <div className="space-y-4">
                <div>
                  <Label htmlFor="coverLetter">Thư xin việc</Label>
                  <Textarea
                    id="coverLetter"
                    placeholder="Viết vài dòng giới thiệu bản thân..."
                    className="min-h-[150px]"
                    value={coverLetter}
                    onChange={(e) => setCoverLetter(e.target.value)}
                  />
                </div>

                <div>
                  <Label htmlFor="cv">CV (PDF)</Label>
                  <FileUploader
                    accept=".pdf"
                    maxSize={5 * 1024 * 1024}
                    onChange={setCV}
                  />
                </div>

                <Button
                  className="w-full"
                  onClick={handleApply}
                  disabled={isPending}
                >
                  {isPending ? 'Đang nộp...' : 'Nộp đơn'}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </CardContent>
      </Card>
    </div>
  );
}
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Jobs list with pagination
- [ ] Advanced filters (location, type, salary, experience)
- [ ] Job search functionality
- [ ] Job detail page with full description
- [ ] Application submission form
- [ ] CV upload to Cloudinary
- [ ] Save/unsave jobs
- [ ] Saved jobs page
- [ ] My applications page with status
- [ ] Company profile page
- [ ] Company jobs list
- [ ] Post job form (recruiter only)
- [ ] Publish/close job (recruiter)
- [ ] Application status tracking

---

_Last Updated: 2026-02-10_

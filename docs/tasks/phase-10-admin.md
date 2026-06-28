# TASK-112: Admin Dashboard Module

> **User management, reports, analytics, moderation**

---

## 📋 TASK INFO

| Property         | Value                           |
| ---------------- | ------------------------------- |
| **Task ID**      | TASK-112                        |
| **Module**       | Admin Dashboard                 |
| **Status**       | ⬜ NOT_STARTED                  |
| **Priority**     | 🟡 Medium                       |
| **Estimate**     | 8 hours                         |
| **Branch**       | `feature/TASK-112-admin-module` |
| **Dependencies** | TASK-104, TASK-105               |

---

## 🎯 OBJECTIVES

- Build admin dashboard with statistics
- Create user management table
- Implement role assignment
- Add reports management
- Build analytics charts
- Create moderation tools
- Add audit logs viewer

---

## 📡 BACKEND API ENDPOINTS

```http
GET /api/v1/admin/statistics
GET /api/v1/admin/analytics/registrations?days=30
GET /api/v1/admin/analytics/activity

GET /api/v1/admin/users?page=1&role={role}&search={q}
PUT /api/v1/admin/users/{id}/roles
POST /api/v1/admin/users/{id}/ban
POST /api/v1/admin/users/{id}/unban
DELETE /api/v1/admin/users/{id}

GET /api/v1/admin/reports?status={status}&type={type}
PUT /api/v1/admin/reports/{id}/resolve
PUT /api/v1/admin/reports/{id}/reject

GET /api/v1/admin/audit-logs?page=1

POST /api/v1/faculties (Admin)
PUT /api/v1/faculties/{id}
DELETE /api/v1/faculties/{id}

POST /api/v1/courses (Admin)
PUT /api/v1/courses/{id}
DELETE /api/v1/courses/{id}
```

---

## 📁 KEY FILES

### 1. Admin Dashboard

**File**: `src/app/[locale]/(admin)/admin/page.tsx`

```tsx
'use client';

import { useAdminStatistics } from '@/hooks/api/admin/useAdminStatistics';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Users, FileText, Briefcase, MessageSquare, TrendingUp } from 'lucide-react';
import { RegistrationChart } from '@/components/features/admin/RegistrationChart';
import { ActivityChart } from '@/components/features/admin/ActivityChart';

export default function AdminDashboard() {
  const { data: stats } = useAdminStatistics();

  const statCards = [
    {
      title: 'Tổng người dùng',
      value: stats?.totalUsers || 0,
      icon: Users,
      trend: '+12%',
    },
    {
      title: 'Bài viết',
      value: stats?.totalPosts || 0,
      icon: MessageSquare,
      trend: '+8%',
    },
    {
      title: 'Tài liệu',
      value: stats?.totalDocuments || 0,
      icon: FileText,
      trend: '+15%',
    },
    {
      title: 'Việc làm',
      value: stats?.totalJobs || 0,
      icon: Briefcase,
      trend: '+5%',
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Trang quản trị</h1>
        <p className="text-muted-foreground">Tổng quan hệ thống</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {statCards.map((card) => (
          <Card key={card.title}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium">{card.title}</CardTitle>
              <card.icon className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{card.value.toLocaleString()}</div>
              <p className="text-xs text-muted-foreground">
                <span className="text-green-600">{card.trend}</span> so với tháng trước
              </p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Đăng ký mới (30 ngày)</CardTitle>
          </CardHeader>
          <CardContent>
            <RegistrationChart />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Hoạt động hệ thống</CardTitle>
          </CardHeader>
          <CardContent>
            <ActivityChart />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
```

### 2. User Management Table

**File**: `src/app/[locale]/(admin)/admin/users/page.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useAdminUsers } from '@/hooks/api/admin/useAdminUsers';
import { useUpdateUserRoles } from '@/hooks/api/admin/useUpdateUserRoles';
import { useBanUser } from '@/hooks/api/admin/useBanUser';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Checkbox } from '@/components/ui/checkbox';
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';
import { Search, Shield, Ban } from 'lucide-react';
import { toast } from 'sonner';

const AVAILABLE_ROLES = ['User', 'Student', 'Moderator', 'Recruiter', 'Admin'];

export default function UsersManagementPage() {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const { data } = useAdminUsers({ page, search });
  const { mutate: updateRoles } = useUpdateUserRoles();
  const { mutate: banUser } = useBanUser();
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [editingUserId, setEditingUserId] = useState<string | null>(null);

  const handleUpdateRoles = (userId: string) => {
    updateRoles({ userId, roles: selectedRoles }, {
      onSuccess: () => {
        toast.success('Cập nhật vai trò thành công');
        setEditingUserId(null);
      },
    });
  };

  const handleBanUser = (userId: string, isBanned: boolean) => {
    const action = isBanned ? 'unban' : 'ban';
    banUser({ userId, action }, {
      onSuccess: () => {
        toast.success(isBanned ? 'Đã mở khóa người dùng' : 'Đã khóa người dùng');
      },
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Quản lý người dùng</h1>
          <p className="text-muted-foreground">
            Tổng: {data?.totalCount || 0} người dùng
          </p>
        </div>
      </div>

      <div className="flex items-center space-x-2">
        <Search className="h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Tìm kiếm theo tên hoặc email..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="max-w-sm"
        />
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Người dùng</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Vai trò</TableHead>
              <TableHead>Trạng thái</TableHead>
              <TableHead>Ngày tham gia</TableHead>
              <TableHead className="text-right">Hành động</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data?.items.map((user) => (
              <TableRow key={user.id}>
                <TableCell>
                  <div className="flex items-center space-x-3">
                    <Avatar className="h-8 w-8">
                      <AvatarImage src={user.avatarUrl} />
                      <AvatarFallback>{user.fullName[0]}</AvatarFallback>
                    </Avatar>
                    <span className="font-medium">{user.fullName}</span>
                  </div>
                </TableCell>
                <TableCell>{user.email}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-1">
                    {user.roles.map((role) => (
                      <Badge key={role} variant="secondary">
                        {role}
                      </Badge>
                    ))}
                  </div>
                </TableCell>
                <TableCell>
                  {user.isBanned ? (
                    <Badge variant="destructive">Đã khóa</Badge>
                  ) : (
                    <Badge variant="default">Hoạt động</Badge>
                  )}
                </TableCell>
                <TableCell>
                  {new Date(user.createdAt).toLocaleDateString('vi-VN')}
                </TableCell>
                <TableCell className="text-right">
                  <div className="flex justify-end space-x-2">
                    <Dialog>
                      <DialogTrigger asChild>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setEditingUserId(user.id);
                            setSelectedRoles(user.roles);
                          }}
                        >
                          <Shield className="mr-1 h-3 w-3" />
                          Vai trò
                        </Button>
                      </DialogTrigger>
                      <DialogContent>
                        <DialogHeader>
                          <DialogTitle>Chỉnh sửa vai trò</DialogTitle>
                        </DialogHeader>
                        <div className="space-y-3">
                          {AVAILABLE_ROLES.map((role) => (
                            <div key={role} className="flex items-center space-x-2">
                              <Checkbox
                                id={role}
                                checked={selectedRoles.includes(role)}
                                onCheckedChange={(checked) => {
                                  if (checked) {
                                    setSelectedRoles([...selectedRoles, role]);
                                  } else {
                                    setSelectedRoles(selectedRoles.filter((r) => r !== role));
                                  }
                                }}
                              />
                              <label htmlFor={role} className="cursor-pointer">
                                {role}
                              </label>
                            </div>
                          ))}
                        </div>
                        <Button onClick={() => handleUpdateRoles(user.id)}>
                          Lưu thay đổi
                        </Button>
                      </DialogContent>
                    </Dialog>

                    <Button
                      variant={user.isBanned ? 'default' : 'destructive'}
                      size="sm"
                      onClick={() => handleBanUser(user.id, user.isBanned)}
                    >
                      <Ban className="mr-1 h-3 w-3" />
                      {user.isBanned ? 'Mở khóa' : 'Khóa'}
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
```

### 3. Reports Management

**File**: `src/app/[locale]/(admin)/admin/reports/page.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useReports } from '@/hooks/api/admin/useReports';
import { useResolveReport } from '@/hooks/api/admin/useResolveReport';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { AlertCircle, Check, X } from 'lucide-react';
import { toast } from 'sonner';

export default function ReportsPage() {
  const [status, setStatus] = useState<'Pending' | 'Resolved' | 'Rejected'>('Pending');
  const { data: reports } = useReports({ status });
  const { mutate: resolveReport } = useResolveReport();

  const handleResolve = (reportId: string, action: 'resolve' | 'reject') => {
    resolveReport({ reportId, action }, {
      onSuccess: () => {
        toast.success(action === 'resolve' ? 'Đã xử lý báo cáo' : 'Đã từ chối báo cáo');
      },
    });
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Quản lý báo cáo</h1>
        <p className="text-muted-foreground">Xem và xử lý báo cáo vi phạm</p>
      </div>

      <Tabs value={status} onValueChange={(v) => setStatus(v as any)}>
        <TabsList>
          <TabsTrigger value="Pending">
            Chờ xử lý
            {reports?.totalCount ? ` (${reports.totalCount})` : ''}
          </TabsTrigger>
          <TabsTrigger value="Resolved">Đã xử lý</TabsTrigger>
          <TabsTrigger value="Rejected">Đã từ chối</TabsTrigger>
        </TabsList>

        <TabsContent value={status} className="space-y-4">
          {reports?.items.length === 0 ? (
            <Card>
              <CardContent className="py-12 text-center text-muted-foreground">
                Không có báo cáo nào
              </CardContent>
            </Card>
          ) : (
            reports?.items.map((report) => (
              <Card key={report.id}>
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="flex items-center space-x-2">
                        <AlertCircle className="h-5 w-5 text-destructive" />
                        <span>{report.title}</span>
                      </CardTitle>
                      <div className="mt-2 flex gap-2">
                        <Badge>{report.type}</Badge>
                        <Badge variant="outline">{report.status}</Badge>
                      </div>
                    </div>
                    {status === 'Pending' && (
                      <div className="flex space-x-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleResolve(report.id, 'reject')}
                        >
                          <X className="mr-1 h-4 w-4" />
                          Từ chối
                        </Button>
                        <Button
                          size="sm"
                          onClick={() => handleResolve(report.id, 'resolve')}
                        >
                          <Check className="mr-1 h-4 w-4" />
                          Xử lý
                        </Button>
                      </div>
                    )}
                  </div>
                </CardHeader>
                <CardContent>
                  <p className="text-sm">{report.description}</p>
                  <div className="mt-4 text-xs text-muted-foreground">
                    Báo cáo bởi: {report.reporterName} •{' '}
                    {new Date(report.createdAt).toLocaleDateString('vi-VN')}
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
```

### 4. Admin Layout with Protected Route

**File**: `src/app/[locale]/(admin)/layout.tsx`

```tsx
import { redirect } from '@/lib/i18n/routing';
import { Sidebar } from '@/components/shared/layouts/AdminSidebar';

export default async function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  // This will be handled by middleware, but add client-side check too
  return (
    <div className="flex min-h-screen">
      <Sidebar />
      <main className="flex-1 p-8">{children}</main>
    </div>
  );
}
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Admin dashboard with statistics
- [ ] User management table
- [ ] Search users
- [ ] Update user roles
- [ ] Ban/unban users
- [ ] Delete users
- [ ] Reports management
- [ ] Resolve/reject reports
- [ ] Analytics charts (registrations, activity)
- [ ] Audit logs viewer
- [ ] Faculty management (CRUD)
- [ ] Course management (CRUD)
- [ ] Role-based access control
- [ ] Admin route protection

---

_Last Updated: 2026-02-10_

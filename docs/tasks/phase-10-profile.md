# TASK-110: Profile & Settings Module

> **User profile, settings, activity history, change password**

---

## 📋 TASK INFO

| Property         | Value                             |
| ---------------- | --------------------------------- |
| **Task ID**      | TASK-110                          |
| **Module**       | Profile & Settings                |
| **Status**       | ⬜ NOT_STARTED                    |
| **Priority**     | 🟡 Medium                         |
| **Estimate**     | 6 hours                           |
| **Branch**       | `feature/TASK-110-profile-module` |
| **Dependencies** | TASK-104, TASK-105                 |

---

## 🎯 OBJECTIVES

- Build user profile view
- Create edit profile form
- Implement change password
- Add notification preferences
- Show activity history
- Display user statistics
- Profile picture upload

---

## 📡 BACKEND API ENDPOINTS

```http
GET /api/v1/me
PUT /api/v1/me
PUT /api/v1/me/password
POST /api/v1/me/avatar (multipart/form-data)
GET /api/v1/me/notifications/preferences
PUT /api/v1/me/notifications/preferences
GET /api/v1/me/activity
GET /api/v1/me/statistics

GET /api/v1/users/{id}
GET /api/v1/users/{id}/posts
GET /api/v1/users/{id}/documents
```

---

## 📁 KEY FILES

### 1. Profile Page

**File**: `src/app/[locale]/(main)/profile/page.tsx`

```tsx
'use client';

import { useAuth } from '@/hooks/auth/useAuth';
import { useProfile } from '@/hooks/api/profile/useProfile';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Link } from '@/lib/i18n/routing';
import { Settings, Mail, Calendar, MapPin } from 'lucide-react';
import { ActivityTimeline } from '@/components/features/profile/ActivityTimeline';
import { UserStatistics } from '@/components/features/profile/UserStatistics';

export default function ProfilePage() {
  const { user } = useAuth();
  const { data: profile } = useProfile();

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col items-center gap-4 sm:flex-row sm:items-start">
            <Avatar className="h-24 w-24">
              <AvatarImage src={profile?.avatarUrl} alt={profile?.fullName} />
              <AvatarFallback>{profile?.fullName?.[0]}</AvatarFallback>
            </Avatar>

            <div className="flex-1 text-center sm:text-left">
              <div className="flex flex-col items-center gap-2 sm:flex-row sm:items-start">
                <h1 className="text-2xl font-bold">{profile?.fullName}</h1>
                <div className="flex flex-wrap gap-1">
                  {profile?.roles.map((role) => (
                    <Badge key={role} variant="secondary">
                      {role}
                    </Badge>
                  ))}
                </div>
              </div>

              <div className="mt-2 space-y-1 text-sm text-muted-foreground">
                <div className="flex items-center justify-center gap-2 sm:justify-start">
                  <Mail className="h-4 w-4" />
                  <span>{profile?.email}</span>
                </div>
                {profile?.faculty && (
                  <div className="flex items-center justify-center gap-2 sm:justify-start">
                    <MapPin className="h-4 w-4" />
                    <span>{profile.faculty.name}</span>
                  </div>
                )}
                <div className="flex items-center justify-center gap-2 sm:justify-start">
                  <Calendar className="h-4 w-4" />
                  <span>
                    Tham gia {new Date(profile?.createdAt || '').toLocaleDateString('vi-VN')}
                  </span>
                </div>
              </div>
            </div>

            <Button asChild variant="outline">
              <Link href="/settings">
                <Settings className="mr-2 h-4 w-4" />
                Cài đặt
              </Link>
            </Button>
          </div>

          {profile?.bio && (
            <p className="mt-4 text-sm text-muted-foreground">{profile.bio}</p>
          )}
        </CardContent>
      </Card>

      <UserStatistics />

      <Tabs defaultValue="activity" className="w-full">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="activity">Hoạt động</TabsTrigger>
          <TabsTrigger value="posts">Bài viết</TabsTrigger>
          <TabsTrigger value="documents">Tài liệu</TabsTrigger>
        </TabsList>
        
        <TabsContent value="activity">
          <ActivityTimeline />
        </TabsContent>
        
        <TabsContent value="posts">
          <Card>
            <CardHeader>
              <CardTitle>Bài viết của tôi</CardTitle>
            </CardHeader>
            <CardContent>
              {/* Posts list */}
            </CardContent>
          </Card>
        </TabsContent>
        
        <TabsContent value="documents">
          <Card>
            <CardHeader>
              <CardTitle>Tài liệu của tôi</CardTitle>
            </CardHeader>
            <CardContent>
              {/* Documents list */}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
```

### 2. Settings Page

**File**: `src/app/[locale]/(main)/settings/page.tsx`

```tsx
'use client';

import { useState } from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { EditProfileForm } from '@/components/features/profile/EditProfileForm';
import { ChangePasswordForm } from '@/components/features/profile/ChangePasswordForm';
import { NotificationPreferences } from '@/components/features/profile/NotificationPreferences';
import { User, Lock, Bell } from 'lucide-react';

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState('profile');

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Cài đặt</h1>
        <p className="text-muted-foreground">Quản lý thông tin cá nhân và tùy chọn</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="profile">
            <User className="mr-2 h-4 w-4" />
            Hồ sơ
          </TabsTrigger>
          <TabsTrigger value="security">
            <Lock className="mr-2 h-4 w-4" />
            Bảo mật
          </TabsTrigger>
          <TabsTrigger value="notifications">
            <Bell className="mr-2 h-4 w-4" />
            Thông báo
          </TabsTrigger>
        </TabsList>

        <TabsContent value="profile">
          <Card>
            <CardHeader>
              <CardTitle>Chỉnh sửa hồ sơ</CardTitle>
              <CardDescription>
                Cập nhật thông tin cá nhân của bạn
              </CardDescription>
            </CardHeader>
            <CardContent>
              <EditProfileForm />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="security">
          <Card>
            <CardHeader>
              <CardTitle>Đổi mật khẩu</CardTitle>
              <CardDescription>
                Thay đổi mật khẩu của bạn
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ChangePasswordForm />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="notifications">
          <Card>
            <CardHeader>
              <CardTitle>Tùy chọn thông báo</CardTitle>
              <CardDescription>
                Quản lý các loại thông báo bạn muốn nhận
              </CardDescription>
            </CardHeader>
            <CardContent>
              <NotificationPreferences />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
```

### 3. Edit Profile Form

**File**: `src/components/features/profile/EditProfileForm.tsx`

```tsx
'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useProfile } from '@/hooks/api/profile/useProfile';
import { useUpdateProfile } from '@/hooks/api/profile/useUpdateProfile';
import { useUploadAvatar } from '@/hooks/api/profile/useUploadAvatar';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Upload } from 'lucide-react';
import { toast } from 'sonner';
import { useState } from 'react';

const profileSchema = z.object({
  fullName: z.string().min(2, 'Tên phải có ít nhất 2 ký tự'),
  bio: z.string().max(500, 'Giới thiệu tối đa 500 ký tự').optional(),
  phoneNumber: z.string().optional(),
});

type ProfileInput = z.infer<typeof profileSchema>;

export function EditProfileForm() {
  const { data: profile } = useProfile();
  const { mutate: updateProfile, isPending } = useUpdateProfile();
  const { mutate: uploadAvatar, isPending: isUploading } = useUploadAvatar();
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);

  const form = useForm<ProfileInput>({
    resolver: zodResolver(profileSchema),
    values: {
      fullName: profile?.fullName || '',
      bio: profile?.bio || '',
      phoneNumber: profile?.phoneNumber || '',
    },
  });

  const onSubmit = (data: ProfileInput) => {
    updateProfile(data, {
      onSuccess: () => {
        toast.success('Cập nhật hồ sơ thành công');
      },
    });
  };

  const handleAvatarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setAvatarPreview(reader.result as string);
    };
    reader.readAsDataURL(file);

    // Upload
    uploadAvatar(file, {
      onSuccess: () => {
        toast.success('Cập nhật ảnh đại diện thành công');
      },
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Avatar className="h-24 w-24">
          <AvatarImage src={avatarPreview || profile?.avatarUrl} />
          <AvatarFallback>{profile?.fullName?.[0]}</AvatarFallback>
        </Avatar>
        <div>
          <Button variant="outline" asChild disabled={isUploading}>
            <label className="cursor-pointer">
              <Upload className="mr-2 h-4 w-4" />
              {isUploading ? 'Đang tải...' : 'Đổi ảnh'}
              <input
                type="file"
                accept="image/*"
                className="hidden"
                onChange={handleAvatarChange}
              />
            </label>
          </Button>
          <p className="mt-2 text-xs text-muted-foreground">
            JPG, PNG hoặc GIF. Tối đa 2MB.
          </p>
        </div>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <FormField
            control={form.control}
            name="fullName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Họ và tên</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="phoneNumber"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Số điện thoại</FormLabel>
                <FormControl>
                  <Input {...field} type="tel" />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="bio"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Giới thiệu</FormLabel>
                <FormControl>
                  <Textarea
                    {...field}
                    placeholder="Viết vài dòng về bạn..."
                    className="min-h-[100px]"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <div className="flex justify-end">
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Đang lưu...' : 'Lưu thay đổi'}
            </Button>
          </div>
        </form>
      </Form>
    </div>
  );
}
```

### 4. Change Password Form

**File**: `src/components/features/profile/ChangePasswordForm.tsx`

```tsx
'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useChangePassword } from '@/hooks/api/profile/useChangePassword';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { toast } from 'sonner';

const passwordSchema = z.object({
  currentPassword: z.string().min(1, 'Vui lòng nhập mật khẩu hiện tại'),
  newPassword: z
    .string()
    .min(8, 'Mật khẩu phải có ít nhất 8 ký tự')
    .regex(/[A-Z]/, 'Phải có ít nhất 1 chữ hoa')
    .regex(/[a-z]/, 'Phải có ít nhất 1 chữ thường')
    .regex(/[0-9]/, 'Phải có ít nhất 1 số'),
  confirmPassword: z.string(),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'Mật khẩu xác nhận không khớp',
  path: ['confirmPassword'],
});

type PasswordInput = z.infer<typeof passwordSchema>;

export function ChangePasswordForm() {
  const { mutate: changePassword, isPending } = useChangePassword();

  const form = useForm<PasswordInput>({
    resolver: zodResolver(passwordSchema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  });

  const onSubmit = (data: PasswordInput) => {
    changePassword({
      currentPassword: data.currentPassword,
      newPassword: data.newPassword,
    }, {
      onSuccess: () => {
        toast.success('Đổi mật khẩu thành công');
        form.reset();
      },
      onError: () => {
        toast.error('Mật khẩu hiện tại không đúng');
      },
    });
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="currentPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Mật khẩu hiện tại</FormLabel>
              <FormControl>
                <Input type="password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="newPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Mật khẩu mới</FormLabel>
              <FormControl>
                <Input type="password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="confirmPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Xác nhận mật khẩu mới</FormLabel>
              <FormControl>
                <Input type="password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end">
          <Button type="submit" disabled={isPending}>
            {isPending ? 'Đang đổi...' : 'Đổi mật khẩu'}
          </Button>
        </div>
      </form>
    </Form>
  );
}
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] View user profile
- [ ] Edit profile form
- [ ] Upload avatar to Cloudinary
- [ ] Change password
- [ ] Notification preferences
- [ ] Activity timeline
- [ ] User statistics (posts, documents, reputation)
- [ ] View other users' profiles
- [ ] Profile validation
- [ ] Image optimization
- [ ] Password strength validation

---

_Last Updated: 2026-02-10_

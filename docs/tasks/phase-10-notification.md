# TASK-111: Notification Center Module

> **Real-time notifications, SignalR, notification dropdown, preferences**

---

## 📋 TASK INFO

| Property         | Value                                   |
| ---------------- | --------------------------------------- |
| **Task ID**      | TASK-111                                |
| **Module**       | Notifications                           |
| **Status**       | ⬜ NOT_STARTED                          |
| **Priority**     | 🟡 Medium                               |
| **Estimate**     | 5 hours                                 |
| **Branch**       | `feature/TASK-111-notification-module` |
| **Dependencies** | TASK-104, TASK-105, TASK-108            |

---

## 🎯 OBJECTIVES

- Build notification dropdown component
- Connect to SignalR NotificationHub
- Real-time notification updates
- Mark as read functionality
- Notification preferences page
- Group notifications by type
- Show unread count badge

---

## 📡 BACKEND API ENDPOINTS

```http
GET /api/v1/notifications?page=1&unreadOnly=false
GET /api/v1/notifications/{id}
POST /api/v1/notifications/{id}/read
POST /api/v1/notifications/read-all
DELETE /api/v1/notifications/{id}
GET /api/v1/notifications/unread-count

# SignalR Hub
/hubs/notifications
- ReceiveNotification(notification)
```

---

## 📁 KEY FILES

### 1. Notification Hub Client

**File**: `src/lib/signalr/notificationHub.ts`

```typescript
import { SignalRConnection } from './connection';
import { Notification } from '@/types/api/notification';

export class NotificationHubClient {
  private connection: SignalRConnection;

  constructor(hubUrl: string, accessToken: string) {
    this.connection = new SignalRConnection(hubUrl, accessToken);
  }

  async start() {
    await this.connection.start();
  }

  async stop() {
    await this.connection.stop();
  }

  onReceiveNotification(callback: (notification: Notification) => void) {
    this.connection.on('ReceiveNotification', callback);
  }

  offReceiveNotification(callback: (notification: Notification) => void) {
    this.connection.off('ReceiveNotification', callback);
  }
}
```

### 2. Notification Store

**File**: `src/stores/notification.store.ts`

```typescript
import { create } from 'zustand';
import { Notification } from '@/types/api/notification';

interface NotificationState {
  notifications: Notification[];
  unreadCount: number;
  addNotification: (notification: Notification) => void;
  markAsRead: (notificationId: string) => void;
  markAllAsRead: () => void;
  removeNotification: (notificationId: string) => void;
  setUnreadCount: (count: number) => void;
}

export const useNotificationStore = create<NotificationState>((set) => ({
  notifications: [],
  unreadCount: 0,

  addNotification: (notification) =>
    set((state) => ({
      notifications: [notification, ...state.notifications],
      unreadCount: state.unreadCount + 1,
    })),

  markAsRead: (notificationId) =>
    set((state) => ({
      notifications: state.notifications.map((n) =>
        n.id === notificationId ? { ...n, isRead: true } : n
      ),
      unreadCount: Math.max(0, state.unreadCount - 1),
    })),

  markAllAsRead: () =>
    set((state) => ({
      notifications: state.notifications.map((n) => ({ ...n, isRead: true })),
      unreadCount: 0,
    })),

  removeNotification: (notificationId) =>
    set((state) => ({
      notifications: state.notifications.filter((n) => n.id !== notificationId),
    })),

  setUnreadCount: (count) =>
    set({ unreadCount: count }),
}));
```

### 3. Notification Dropdown Component

**File**: `src/components/features/notifications/NotificationDropdown.tsx`

```tsx
'use client';

import { useEffect, useState } from 'react';
import { useNotifications } from '@/hooks/api/notifications/useNotifications';
import { useMarkAsRead } from '@/hooks/api/notifications/useMarkAsRead';
import { useMarkAllAsRead } from '@/hooks/api/notifications/useMarkAllAsRead';
import { useNotificationStore } from '@/stores/notification.store';
import { useNotificationHub } from '@/hooks/realtime/useNotificationHub';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Bell, Check, Trash } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { vi } from 'date-fns/locale';
import { Link } from '@/lib/i18n/routing';

export function NotificationDropdown() {
  const [isOpen, setIsOpen] = useState(false);
  const { data: notifications } = useNotifications({ page: 1, pageSize: 10 });
  const { mutate: markAsRead } = useMarkAsRead();
  const { mutate: markAllAsRead } = useMarkAllAsRead();
  const { unreadCount } = useNotificationStore();
  
  // Connect to SignalR hub
  useNotificationHub();

  const handleMarkAsRead = (notificationId: string) => {
    markAsRead(notificationId);
  };

  const handleMarkAllAsRead = () => {
    markAllAsRead();
  };

  const getNotificationIcon = (type: string) => {
    switch (type) {
      case 'PostLike':
        return '❤️';
      case 'PostComment':
        return '💬';
      case 'JobApplication':
        return '📄';
      case 'DocumentApproved':
        return '✅';
      case 'DocumentRejected':
        return '❌';
      default:
        return '🔔';
    }
  };

  return (
    <DropdownMenu open={isOpen} onOpenChange={setIsOpen}>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <Bell className="h-5 w-5" />
          {unreadCount > 0 && (
            <Badge
              variant="destructive"
              className="absolute -right-1 -top-1 h-5 w-5 rounded-full p-0 text-xs"
            >
              {unreadCount > 9 ? '9+' : unreadCount}
            </Badge>
          )}
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" className="w-80">
        <DropdownMenuLabel className="flex items-center justify-between">
          <span>Thông báo</span>
          {unreadCount > 0 && (
            <Button
              variant="ghost"
              size="sm"
              onClick={handleMarkAllAsRead}
              className="h-auto p-0 text-xs text-primary"
            >
              Đánh dấu đã đọc tất cả
            </Button>
          )}
        </DropdownMenuLabel>
        <DropdownMenuSeparator />

        <ScrollArea className="h-[400px]">
          {notifications?.items.length === 0 ? (
            <div className="py-8 text-center text-sm text-muted-foreground">
              Không có thông báo mới
            </div>
          ) : (
            notifications?.items.map((notification) => (
              <DropdownMenuItem
                key={notification.id}
                className={`flex cursor-pointer items-start space-x-3 p-3 ${
                  !notification.isRead ? 'bg-muted/50' : ''
                }`}
                onClick={() => {
                  handleMarkAsRead(notification.id);
                  setIsOpen(false);
                }}
              >
                <span className="text-2xl">{getNotificationIcon(notification.type)}</span>
                <div className="flex-1 space-y-1">
                  <p className="text-sm font-medium leading-tight">
                    {notification.title}
                  </p>
                  <p className="text-xs text-muted-foreground line-clamp-2">
                    {notification.message}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {formatDistanceToNow(new Date(notification.createdAt), {
                      locale: vi,
                      addSuffix: true,
                    })}
                  </p>
                </div>
                {!notification.isRead && (
                  <div className="h-2 w-2 rounded-full bg-primary" />
                )}
              </DropdownMenuItem>
            ))
          )}
        </ScrollArea>

        <DropdownMenuSeparator />
        <DropdownMenuItem asChild>
          <Link
            href="/notifications"
            className="w-full text-center text-sm text-primary"
            onClick={() => setIsOpen(false)}
          >
            Xem tất cả thông báo
          </Link>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
```

### 4. Notification Hub Hook

**File**: `src/hooks/realtime/useNotificationHub.ts`

```typescript
import { useEffect, useRef } from 'react';
import { useAuth } from '@/hooks/auth/useAuth';
import { useNotificationStore } from '@/stores/notification.store';
import { NotificationHubClient } from '@/lib/signalr/notificationHub';
import { Notification } from '@/types/api/notification';
import { toast } from 'sonner';

export function useNotificationHub() {
  const { token } = useAuth();
  const { addNotification } = useNotificationStore();
  const hubRef = useRef<NotificationHubClient | null>(null);

  useEffect(() => {
    if (!token) return;

    const hubUrl = `${process.env.NEXT_PUBLIC_API_URL}/hubs/notifications`;
    const hub = new NotificationHubClient(hubUrl, token);

    const handleReceiveNotification = (notification: Notification) => {
      addNotification(notification);
      
      // Show toast notification
      toast.info(notification.title, {
        description: notification.message,
        duration: 5000,
      });
    };

    hub.onReceiveNotification(handleReceiveNotification);
    hub.start().catch((err) => console.error('NotificationHub error:', err));

    hubRef.current = hub;

    return () => {
      hub.offReceiveNotification(handleReceiveNotification);
      hub.stop();
    };
  }, [token, addNotification]);

  return hubRef.current;
}
```

### 5. Notifications Page

**File**: `src/app/[locale]/(main)/notifications/page.tsx`

```tsx
'use client';

import { useState } from 'react';
import { useNotifications } from '@/hooks/api/notifications/useNotifications';
import { useMarkAsRead } from '@/hooks/api/notifications/useMarkAsRead';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Check } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { vi } from 'date-fns/locale';

export default function NotificationsPage() {
  const [filter, setFilter] = useState<'all' | 'unread'>('all');
  const { data: notifications } = useNotifications({
    page: 1,
    unreadOnly: filter === 'unread',
  });
  const { mutate: markAsRead } = useMarkAsRead();

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Thông báo</h1>
        <p className="text-muted-foreground">Quản lý tất cả thông báo của bạn</p>
      </div>

      <Tabs value={filter} onValueChange={(v) => setFilter(v as 'all' | 'unread')}>
        <TabsList>
          <TabsTrigger value="all">Tất cả</TabsTrigger>
          <TabsTrigger value="unread">Chưa đọc</TabsTrigger>
        </TabsList>

        <TabsContent value={filter} className="space-y-3">
          {notifications?.items.length === 0 ? (
            <Card>
              <CardContent className="py-12 text-center text-muted-foreground">
                Không có thông báo
              </CardContent>
            </Card>
          ) : (
            notifications?.items.map((notification) => (
              <Card
                key={notification.id}
                className={!notification.isRead ? 'border-l-4 border-l-primary' : ''}
              >
                <CardContent className="flex items-start justify-between p-4">
                  <div className="flex-1">
                    <h3 className="font-semibold">{notification.title}</h3>
                    <p className="mt-1 text-sm text-muted-foreground">
                      {notification.message}
                    </p>
                    <p className="mt-2 text-xs text-muted-foreground">
                      {formatDistanceToNow(new Date(notification.createdAt), {
                        locale: vi,
                        addSuffix: true,
                      })}
                    </p>
                  </div>
                  {!notification.isRead && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => markAsRead(notification.id)}
                    >
                      <Check className="h-4 w-4" />
                    </Button>
                  )}
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

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Notification dropdown in navbar
- [ ] Unread count badge
- [ ] Real-time notifications via SignalR
- [ ] Mark as read functionality
- [ ] Mark all as read
- [ ] Notifications page with filters
- [ ] Toast notifications for new items
- [ ] Notification preferences
- [ ] Group by type
- [ ] Delete notifications
- [ ] Sound/visual alerts (optional)

---

_Last Updated: 2026-02-10_

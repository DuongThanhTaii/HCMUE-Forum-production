# TASK-106: Forum Module

> **Posts, comments, voting, tags, search functionality**

---

## 📋 TASK INFO

| Property         | Value                           |
| ---------------- | ------------------------------- |
| **Task ID**      | TASK-106                        |
| **Module**       | Forum                           |
| **Status**       | ✅ COMPLETED                    |
| **Priority**     | 🔴 Critical                     |
| **Estimate**     | 12 hours                        |
| **Actual**       | 8 hours                         |
| **Completed**    | 2026-02-10                      |
| **Branch**       | `feature/TASK-106-forum-module` |
| **Dependencies** | TASK-104, TASK-105              |

---

## 🎯 OBJECTIVES

- Build posts list with filters (category, type, status)
- Create post detail page with comments
- Implement create/edit post forms
- Add voting system (upvote/downvote)
- Build comment thread with replies
- Add bookmark functionality
- Implement tag system
- Create search page with full-text search

---

## 📡 BACKEND API ENDPOINTS

```http
GET /api/v1/posts?category={id}&type={type}&status={status}&page=1&pageSize=20
GET /api/v1/posts/{id}
POST /api/v1/posts
PUT /api/v1/posts/{id}
DELETE /api/v1/posts/{id}
POST /api/v1/posts/{id}/publish
POST /api/v1/posts/{id}/pin
POST /api/v1/posts/{id}/vote
GET /api/v1/posts/{id}/comments
POST /api/v1/posts/{id}/bookmark
DELETE /api/v1/posts/{id}/bookmark
POST /api/v1/posts/{id}/report

POST /api/v1/comments/posts/{postId}
PUT /api/v1/comments/{id}
DELETE /api/v1/comments/{id}
POST /api/v1/comments/{id}/vote
POST /api/v1/comments/{id}/accept

GET /api/v1/tags
GET /api/v1/tags/popular
GET /api/v1/search?q={query}&type=posts
```

---

## 📁 KEY FILES

### 1. Posts List Page

**File**: `src/app/[locale]/(main)/forum/page.tsx`

```tsx
"use client";

import { useState } from "react";
import { usePosts } from "@/hooks/api/forum/usePosts";
import { PostCard } from "@/components/features/forum/PostCard";
import { PostFilters } from "@/components/features/forum/PostFilters";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { Link } from "@/lib/i18n/routing";
import { Breadcrumbs } from "@/components/shared/layouts/Breadcrumbs";

export default function ForumPage() {
  const [filters, setFilters] = useState({
    category: "",
    type: "",
    status: "",
    page: 1,
  });

  const { data, isLoading, error } = usePosts(filters);

  return (
    <div className="space-y-6">
      <Breadcrumbs />

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Diễn đàn</h1>
          <p className="text-muted-foreground">
            Đặt câu hỏi, chia sẻ kiến thức
          </p>
        </div>
        <Button asChild>
          <Link href="/forum/create">
            <Plus className="mr-2 h-4 w-4" />
            Tạo bài viết
          </Link>
        </Button>
      </div>

      <PostFilters filters={filters} onChange={setFilters} />

      <div className="grid gap-4">
        {isLoading ? (
          Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-32 w-full" />
          ))
        ) : error ? (
          <div className="rounded-lg border border-destructive p-4 text-center text-destructive">
            Không thể tải bài viết
          </div>
        ) : data?.items.length === 0 ? (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Chưa có bài viết nào
          </div>
        ) : (
          data?.items.map((post) => <PostCard key={post.id} post={post} />)
        )}
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex justify-center">
          <Button
            variant="outline"
            onClick={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
            disabled={filters.page >= data.totalPages}
          >
            Tải thêm
          </Button>
        </div>
      )}
    </div>
  );
}
```

### 2. Post Card Component

**File**: `src/components/features/forum/PostCard.tsx`

```tsx
import { Link } from "@/lib/i18n/routing";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { VoteButtons } from "./VoteButtons";
import { Eye, MessageCircle, Bookmark } from "lucide-react";
import { formatDistanceToNow } from "date-fns";
import { vi } from "date-fns/locale";

interface Post {
  id: string;
  title: string;
  content: string;
  authorId: string;
  authorName: string;
  authorAvatar?: string;
  category: { id: string; name: string };
  tags: Array<{ id: string; name: string }>;
  voteScore: number;
  commentCount: number;
  viewCount: number;
  isPinned: boolean;
  isBookmarked: boolean;
  createdAt: string;
}

interface PostCardProps {
  post: Post;
}

export function PostCard({ post }: PostCardProps) {
  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardHeader className="flex-row items-start space-x-4">
        <VoteButtons postId={post.id} score={post.voteScore} />
        <div className="flex-1 space-y-2">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <Link href={`/forum/${post.id}`} className="hover:underline">
                <h3 className="text-lg font-semibold">{post.title}</h3>
              </Link>
              <p className="mt-1 text-sm text-muted-foreground line-clamp-2">
                {post.content}
              </p>
            </div>
            {post.isPinned && (
              <Badge variant="secondary" className="ml-2">
                Đã ghim
              </Badge>
            )}
          </div>

          <div className="flex flex-wrap gap-2">
            <Badge variant="outline">{post.category.name}</Badge>
            {post.tags.map((tag) => (
              <Link key={tag.id} href={`/forum/tags/${tag.name}`}>
                <Badge variant="secondary" className="hover:bg-secondary/80">
                  #{tag.name}
                </Badge>
              </Link>
            ))}
          </div>

          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <div className="flex items-center space-x-2">
              <Avatar className="h-6 w-6">
                <AvatarImage src={post.authorAvatar} />
                <AvatarFallback>{post.authorName[0]}</AvatarFallback>
              </Avatar>
              <span>{post.authorName}</span>
              <span>•</span>
              <span>
                {formatDistanceToNow(new Date(post.createdAt), {
                  addSuffix: true,
                  locale: vi,
                })}
              </span>
            </div>

            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-1">
                <Eye className="h-4 w-4" />
                <span>{post.viewCount}</span>
              </div>
              <div className="flex items-center space-x-1">
                <MessageCircle className="h-4 w-4" />
                <span>{post.commentCount}</span>
              </div>
              {post.isBookmarked && (
                <Bookmark className="h-4 w-4 fill-current" />
              )}
            </div>
          </div>
        </div>
      </CardHeader>
    </Card>
  );
}
```

### 3. Vote Buttons Component

**File**: `src/components/features/forum/VoteButtons.tsx`

```tsx
"use client";

import { useState } from "react";
import { useVote } from "@/hooks/api/forum/useVote";
import { Button } from "@/components/ui/button";
import { ChevronUp, ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils/cn";

interface VoteButtonsProps {
  postId: string;
  score: number;
  userVote?: "Upvote" | "Downvote" | null;
}

export function VoteButtons({
  postId,
  score: initialScore,
  userVote,
}: VoteButtonsProps) {
  const [score, setScore] = useState(initialScore);
  const [vote, setVote] = useState(userVote);
  const { mutate: submitVote } = useVote();

  const handleVote = (voteType: "Upvote" | "Downvote") => {
    const newVote = vote === voteType ? null : voteType;
    const scoreDiff =
      newVote === "Upvote" ? 1 : newVote === "Downvote" ? -1 : 0;
    const prevScore = vote === "Upvote" ? -1 : vote === "Downvote" ? 1 : 0;

    setScore(score + prevScore + scoreDiff);
    setVote(newVote);

    submitVote({ postId, voteType: newVote || voteType });
  };

  return (
    <div className="flex flex-col items-center space-y-1">
      <Button
        variant="ghost"
        size="sm"
        onClick={() => handleVote("Upvote")}
        className={cn("h-8 w-8 p-0", vote === "Upvote" && "text-orange-500")}
      >
        <ChevronUp className="h-5 w-5" />
      </Button>
      <span
        className={cn("text-sm font-medium", {
          "text-orange-500": score > 0,
          "text-blue-500": score < 0,
        })}
      >
        {score}
      </span>
      <Button
        variant="ghost"
        size="sm"
        onClick={() => handleVote("Downvote")}
        className={cn("h-8 w-8 p-0", vote === "Downvote" && "text-blue-500")}
      >
        <ChevronDown className="h-5 w-5" />
      </Button>
    </div>
  );
}
```

### 4. Create Post Form

**File**: `src/app/[locale]/(main)/forum/create/page.tsx`

```tsx
"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "@/lib/i18n/routing";
import { useCreatePost } from "@/hooks/api/forum/useCreatePost";
import { postSchema, type PostInput } from "@/lib/validations/post.schema";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function CreatePostPage() {
  const router = useRouter();
  const { mutate: createPost, isPending } = useCreatePost();

  const form = useForm<PostInput>({
    resolver: zodResolver(postSchema),
    defaultValues: {
      title: "",
      content: "",
      categoryId: "",
      postType: "Discussion",
      tags: [],
    },
  });

  const onSubmit = (data: PostInput) => {
    createPost(data, {
      onSuccess: (post) => {
        router.push(`/forum/${post.id}`);
      },
    });
  };

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Tạo bài viết mới</CardTitle>
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
                      <Input
                        placeholder="Nhập tiêu đề bài viết..."
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="categoryId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Danh mục</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Chọn danh mục" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="general">Tổng quát</SelectItem>
                        <SelectItem value="academic">Học tập</SelectItem>
                        <SelectItem value="tech">Công nghệ</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="postType"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Loại bài viết</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="Discussion">Thảo luận</SelectItem>
                        <SelectItem value="Question">Câu hỏi</SelectItem>
                        <SelectItem value="Announcement">Thông báo</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="content"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Nội dung</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Viết nội dung bài viết..."
                        className="min-h-[200px]"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="flex justify-end space-x-2">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => router.back()}
                >
                  Hủy
                </Button>
                <Button type="submit" disabled={isPending}>
                  {isPending ? "Đang tạo..." : "Tạo bài viết"}
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

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Posts list with pagination
- [ ] Filter by category, type, status
- [ ] Upvote/downvote functional
- [ ] Create post form with validation
- [ ] Edit post (author only)
- [ ] Delete post (author + moderator)
- [ ] Post detail page
- [ ] Comment section with replies
- [ ] Accept answer (for Questions)
- [ ] Bookmark posts
- [ ] Tag system working
- [ ] Search posts by title/content
- [ ] Pin posts (moderator only)
- [ ] View count increments
- [ ] Responsive design

---

_Last Updated: 2026-02-10_

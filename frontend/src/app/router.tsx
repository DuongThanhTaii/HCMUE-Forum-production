import { createBrowserRouter, Navigate } from 'react-router-dom';
import { RequireAuth, RequireRole } from './guards';
import { AdminGuard } from './guards/AdminGuard';
import { AuthLayout } from '@shared/components/layouts/AuthLayout';
import { MainLayout } from '@shared/components/layouts/MainLayout';
import { ModLayout } from '@shared/components/layouts/ModLayout';
import { AdminLayout } from '@shared/components/layouts/AdminLayout';
import { ForumDetailPage } from '@features/forum/components/ForumDetailPage';
import { ForumSavedPostsPage } from '@features/forum/components/ForumSavedPostsPage';
import { HomePage } from '@features/forum/components/HomePage';
import { ExplorePage } from '@features/forum/components/ExplorePage';
import { CategoryPage } from '@features/forum/components/CategoryPage';
import { ModReportsPage } from '@features/forum/components/ModReportsPage';
import { ModPostsPage } from '@features/forum/components/ModPostsPage';
import { ModLearningApprovalsPage } from '@features/learning/components/ModLearningApprovalsPage';
import { ModDashboardPage } from '@features/forum/components/ModDashboardPage';
import { ForumCreatePostPage } from '@features/forum/components/ForumCreatePostPage';
import { LearningDocumentsPage } from '@features/learning/components/LearningDocumentsPage';
import { LearningDocumentDetailPage } from '@features/learning/components/LearningDocumentDetailPage';
import { LearningFacultiesPage } from '@features/learning/components/LearningFacultiesPage';
import { LearningCoursesPage } from '@features/learning/components/LearningCoursesPage';
import { CareerJobsPage } from '@features/career/components/CareerJobsPage';
import { CareerJobDetailPage } from '@features/career/components/CareerJobDetailPage';
import { CareerCompanyRegisterPage } from '@features/career/components/CareerCompanyRegisterPage';
import { CareerCompanyDashboardPage } from '@features/career/components/CareerCompanyDashboardPage';
import { LoginPage } from '@features/auth/components/LoginPage';
import { RegisterPage } from '@features/auth/components/RegisterPage';
import { AzurePopupCompletePage } from '@features/auth/components/AzurePopupCompletePage';
import { AdminRolesPage } from '@features/admin/roles/components/AdminRolesPage';
import { AdminUsersPage } from '@features/admin/users/components/AdminUsersPage';
import { AdminOverridesPage } from '@features/admin/overrides/components/AdminOverridesPage';
import { AdminTogglesPage } from '@features/admin/observability/components/AdminTogglesPage';
import { AdminThreadChannelsPage } from '@features/admin/observability/components/AdminThreadChannelsPage';
import { AdminActionLogsPage } from '@features/admin/observability/components/AdminActionLogsPage';
import { AdminAuditLogsPage } from '@features/admin/observability/components/AdminAuditLogsPage';
import { AdminCategoriesPage } from '@features/admin/observability/components/AdminCategoriesPage';
import { AdminDashboardPage } from '@features/admin/components/AdminDashboardPage';
import { ChatPage } from '@features/chat/components/ChatPage';
import { AssistantPage } from '@features/assistant/components/AssistantPage';
import { MaintenancePage } from '@shared/components/system/MaintenancePage';

/**
 * Single root `path: '/'` tree so `/home`, `/login`, and `navigate('/home')` resolve reliably.
 * Multiple sibling `{ path: '/' }` route objects confused matching for nested paths in RR 6/7.
 */
export const appRoutes = [
  {
    path: '/',
    children: [
      { index: true, element: <Navigate to="/home" replace /> },
      {
        path: 'login',
        element: <AuthLayout />,
        children: [{ index: true, element: <LoginPage /> }],
      },
      {
        path: 'register',
        element: <AuthLayout />,
        children: [{ index: true, element: <RegisterPage /> }],
      },
      {
        path: 'auth/azure/popup-complete',
        element: <AzurePopupCompletePage />,
      },
      {
        path: 'maintenance',
        element: <MaintenancePage />,
      },
      {
        element: <MainLayout />,
        children: [
          {
            path: 'learning/documents/:id',
            element: <LearningDocumentDetailPage />,
          },
          { path: 'learning/documents', element: <LearningDocumentsPage /> },
          { path: 'learning/faculties', element: <LearningFacultiesPage /> },
          { path: 'learning/courses', element: <LearningCoursesPage /> },
          { path: 'career/company-register', element: <CareerCompanyRegisterPage /> },
          { path: 'home', element: <HomePage /> },
          {
            element: <RequireAuth />,
            children: [
              // Deprecated routes - Redirecting to Explore
              { path: 'forum', element: <Navigate to="/explore" replace /> },
              { path: 'forum/posts', element: <Navigate to="/explore" replace /> },
              { path: 'forum/threads', element: <Navigate to="/explore" replace /> },
              
              // New routing
              { path: 'explore', element: <ExplorePage /> },
              { path: 'discussions/:categorySlug', element: <CategoryPage /> },
              { path: 'threads/:threadId', element: <ForumDetailPage /> },
              
              // Backward compatible or unchanged routes
              { path: 'forum/saved', element: <ForumSavedPostsPage /> },
              { path: 'forum/new', element: <ForumCreatePostPage /> },
              { path: 'forum/:id', element: <ForumDetailPage /> },
              { path: 'career/jobs', element: <CareerJobsPage /> },
              { path: 'career/jobs/:id', element: <CareerJobDetailPage /> },
              { path: 'career/company-dashboard', element: <CareerCompanyDashboardPage /> },
              { path: 'chat', element: <ChatPage /> },
              { path: 'chat/ai', element: <AssistantPage /> },
              { path: 'assistant', element: <AssistantPage /> },
            ],
          },
        ],
      },
    ],
  },
  {
    path: '/mod',
    element: <RequireRole roles={['Moderator']} />,
    children: [
      {
        path: '/mod',
        element: <ModLayout />,
        children: [
          { index: true, element: <Navigate to="/mod/dashboard" replace /> },
          { path: 'dashboard', element: <ModDashboardPage /> },
          { path: 'reports', element: <ModReportsPage /> },
          { path: 'posts', element: <ModPostsPage /> },
          { path: 'categories', element: <AdminCategoriesPage /> },
          { path: 'thread-channels', element: <AdminThreadChannelsPage /> },
          { path: 'learning', element: <ModLearningApprovalsPage /> },
        ],
      },
    ],
  },
  {
    path: '/admin',
    element: <AdminGuard />,
    children: [
      {
        path: '/admin',
        element: <AdminLayout />,
        children: [
          { index: true, element: <Navigate to="/admin/dashboard" replace /> },
          { path: 'dashboard', element: <AdminDashboardPage /> },
          { path: 'users', element: <AdminUsersPage /> },
          { path: 'roles', element: <AdminRolesPage /> },
          { path: 'permissions', element: <Navigate to="/admin/roles" replace /> },
          { path: 'overrides/users', element: <AdminOverridesPage /> },
          { path: 'overrides/groups', element: <AdminOverridesPage /> },
          { path: 'forum/categories', element: <AdminCategoriesPage /> },
          { path: 'forum/thread-channels', element: <AdminThreadChannelsPage /> },
          { path: 'toggles', element: <AdminTogglesPage /> },
          { path: 'logs/actions', element: <AdminActionLogsPage /> },
          { path: 'logs/audit', element: <AdminAuditLogsPage /> },
        ],
      },
    ],
  },
];

export const appRouter = createBrowserRouter(appRoutes);

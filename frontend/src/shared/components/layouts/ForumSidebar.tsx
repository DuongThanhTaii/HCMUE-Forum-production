import { Link, useLocation } from 'react-router-dom';
import {
  BookMarked,
  BookOpen,
  Bot,
  Building2,
  Hash,
  Home,
  Layers3,
  LayoutGrid,
  MessageSquare,
  Tag,
} from 'lucide-react';
import type { ComponentType } from 'react';
import { useTranslation } from 'react-i18next';
import { useGetForumCategoriesQuery } from '@features/forum/api/forum.list.api';
import { buildCategoryGroups } from '@features/forum/lib/forumCategoryTree';

type SidebarItem = {
  to: string;
  labelKey: string;
  label?: string;
  icon: ComponentType<{ className?: string }>;
};

const MAIN_ITEMS: SidebarItem[] = [
  { to: '/home', labelKey: 'nav.home', icon: Home },
  { to: '/forum', labelKey: 'forum.sidebar.main.forumIndex', icon: LayoutGrid },
  { to: '/forum/posts', labelKey: 'forum.sidebar.main.allPosts', icon: MessageSquare },
  { to: '/forum/saved', labelKey: 'forum.sidebar.main.saved', icon: BookMarked },
  { to: '/assistant', labelKey: 'forum.sidebar.main.assistant', icon: Bot },
];

const TOPIC_ITEMS: SidebarItem[] = [
  { to: '/forum/threads', labelKey: 'forum.sidebar.topics.threads', icon: Hash },
];

const LEARNING_ITEMS: SidebarItem[] = [
  { to: '/learning/documents', labelKey: 'forum.sidebar.categories.learningDocs', icon: Layers3 },
  { to: '/learning/faculties', labelKey: 'forum.sidebar.categories.learningFaculties', icon: Building2 },
  { to: '/learning/courses', labelKey: 'forum.sidebar.categories.learningCourses', icon: BookOpen },
  { to: '/career/jobs', labelKey: 'forum.sidebar.categories.career', icon: Layers3 },
];

const TAG_ITEMS: SidebarItem[] = [
  { to: '/forum/posts?tab=hot', labelKey: 'forum.sidebar.tags.hot', icon: Tag },
];

function isItemActive(pathname: string, search: string, to: string) {
  const currentParams = new URLSearchParams(search);
  const targetUrl = new URL(to, 'https://hcmue.local');
  const targetPath = targetUrl.pathname;
  const targetParams = targetUrl.searchParams;

  if (pathname === '/forum' && targetPath === '/forum' && pathname === targetPath) {
    return search === '' && targetParams.size === 0;
  }

  if (!(pathname === targetPath || (targetPath !== '/forum' && pathname.startsWith(`${targetPath}/`)))) {
    return false;
  }

  let matchesQuery = true;
  targetParams.forEach((value, key) => {
    if (currentParams.get(key) !== value) {
      matchesQuery = false;
    }
  });

  return matchesQuery;
}

function SidebarSection({
  title,
  items,
  pathname,
  search,
}: {
  title: string;
  items: SidebarItem[];
  pathname: string;
  search: string;
}) {
  const { t } = useTranslation();

  return (
    <section className="space-y-1">
      <h2 className="px-2 text-[11px] font-semibold uppercase tracking-wide text-slate-500">{title}</h2>
      <div className="space-y-0.5">
        {items.map((item) => {
          const active = isItemActive(pathname, search, item.to);
          const Icon = item.icon;
          const label = item.label ?? t(item.labelKey);
          return (
            <Link
              key={item.to}
              to={item.to}
              className={`flex items-center gap-2 rounded-md px-2 py-1.5 text-[13px] transition-colors ${
                active ? 'bg-primary/10 font-medium text-primary' : 'text-slate-700 hover:bg-slate-100'
              }`}
            >
              <Icon className="h-3.5 w-3.5 shrink-0" />
              <span className="truncate">{label}</span>
            </Link>
          );
        })}
      </div>
    </section>
  );
}

export function ForumSidebar() {
  const { pathname, search } = useLocation();
  const { t } = useTranslation();
  const { data: categories = [] } = useGetForumCategoriesQuery();
  const groups = buildCategoryGroups(categories);

  return (
    <aside className="fixed left-0 top-14 z-30 hidden h-[calc(100dvh-3.5rem)] w-64 border-r border-slate-200 bg-white lg:block">
      <div className="h-full space-y-4 overflow-y-auto p-3">
        <SidebarSection title={t('forum.sidebar.sections.main')} items={MAIN_ITEMS} pathname={pathname} search={search} />
        <SidebarSection title={t('forum.sidebar.sections.topics')} items={TOPIC_ITEMS} pathname={pathname} search={search} />

        {groups.map(({ parent, children }) => {
          const items: SidebarItem[] = children.map((cat) => ({
            to: `/forum/posts?category=${cat.id}`,
            labelKey: '',
            label: cat.name,
            icon: MessageSquare,
          }));
          return (
            <SidebarSection
              key={parent.id}
              title={parent.name}
              items={items}
              pathname={pathname}
              search={search}
            />
          );
        })}

        <SidebarSection
          title={t('forum.sidebar.sections.learningCareer')}
          items={LEARNING_ITEMS}
          pathname={pathname}
          search={search}
        />
        <SidebarSection title={t('forum.sidebar.sections.tags')} items={TAG_ITEMS} pathname={pathname} search={search} />

        <div className="rounded-md border border-jasper/20 bg-jasper/5 px-2 py-1.5 text-[11px] text-jasper">
          {t('forum.sidebar.notice')}
        </div>
      </div>
    </aside>
  );
}

import type { ComponentType } from 'react'
import {
  Briefcase,
  Cpu,
  GraduationCap,
  Megaphone,
  MessageSquare,
  Users,
} from 'lucide-react'
import type { ForumCategoryOption } from '../api/forum.list.api'

export type CategoryAccent = 'amber' | 'indigo' | 'emerald' | 'sky' | 'slate' | 'violet'

export type ForumCategoryGroup = {
  parent: ForumCategoryOption
  children: ForumCategoryOption[]
  accent: CategoryAccent
  iconKey: string
}

const ACCENT_BY_SLUG: Record<string, CategoryAccent> = {
  'dai-sanh': 'amber',
  'hoc-thuat': 'indigo',
  'cong-nghe': 'violet',
  'cong-dong': 'emerald',
  'viec-lam': 'sky',
}

const ICON_BY_SLUG: Record<string, ComponentType<{ className?: string }>> = {
  'dai-sanh': Megaphone,
  'hoc-thuat': GraduationCap,
  'cong-nghe': Cpu,
  'cong-dong': Users,
  'viec-lam': Briefcase,
}

const ACCENT_STYLES: Record<
  CategoryAccent,
  { header: string; icon: string; badge: string }
> = {
  amber: {
    header: 'border-amber-200/80 bg-gradient-to-r from-amber-50 to-amber-50/40',
    icon: 'text-amber-700 bg-amber-100',
    badge: 'bg-amber-100 text-amber-800',
  },
  indigo: {
    header: 'border-indigo-200/80 bg-gradient-to-r from-indigo-50 to-indigo-50/40',
    icon: 'text-indigo-700 bg-indigo-100',
    badge: 'bg-indigo-100 text-indigo-800',
  },
  violet: {
    header: 'border-violet-200/80 bg-gradient-to-r from-violet-50 to-violet-50/40',
    icon: 'text-violet-700 bg-violet-100',
    badge: 'bg-violet-100 text-violet-800',
  },
  emerald: {
    header: 'border-emerald-200/80 bg-gradient-to-r from-emerald-50 to-emerald-50/40',
    icon: 'text-emerald-700 bg-emerald-100',
    badge: 'bg-emerald-100 text-emerald-800',
  },
  sky: {
    header: 'border-sky-200/80 bg-gradient-to-r from-sky-50 to-sky-50/40',
    icon: 'text-sky-700 bg-sky-100',
    badge: 'bg-sky-100 text-sky-800',
  },
  slate: {
    header: 'border-slate-200 bg-slate-50',
    icon: 'text-slate-700 bg-slate-100',
    badge: 'bg-slate-100 text-slate-700',
  },
}

const BORDER_BY_ACCENT: Record<CategoryAccent, string> = {
  amber: 'border-l-amber-500',
  indigo: 'border-l-indigo-500',
  violet: 'border-l-violet-500',
  emerald: 'border-l-emerald-500',
  sky: 'border-l-sky-500',
  slate: 'border-l-slate-400',
}

export function getCategoryAccentStyles(accent: CategoryAccent) {
  return ACCENT_STYLES[accent]
}

export function getCategoryBorderAccent(accent: CategoryAccent) {
  return BORDER_BY_ACCENT[accent]
}

export function getCategoryGroupIcon(iconKey: string) {
  return ICON_BY_SLUG[iconKey] ?? MessageSquare
}

function resolveAccent(slug: string): CategoryAccent {
  return ACCENT_BY_SLUG[slug] ?? 'slate'
}

/** Diễn đàn con — dùng khi tạo bài, không chọn khu cha. */
export function getLeafCategories(categories: ForumCategoryOption[]): ForumCategoryOption[] {
  const parentIdsWithChildren = new Set(
    categories.filter((c) => categories.some((ch) => ch.parentCategoryId === c.id)).map((c) => c.id),
  )
  return categories
    .filter((c) => !parentIdsWithChildren.has(c.id))
    .sort((a, b) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name, 'vi'))
}

export function buildCategoryGroups(categories: ForumCategoryOption[]): ForumCategoryGroup[] {
  const sorted = [...categories].sort(
    (a, b) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name, 'vi'),
  )

  const hasHierarchy = sorted.some((c) => c.parentCategoryId)
  if (!hasHierarchy) {
    return sorted.map((c) => ({
      parent: c,
      children: [c],
      accent: 'slate' as const,
      iconKey: 'legacy',
    }))
  }

  const parents = sorted.filter((c) => !c.parentCategoryId)
  const groups: ForumCategoryGroup[] = []
  const parentIdsWithChildren = new Set(
    sorted.filter((c) => sorted.some((ch) => ch.parentCategoryId === c.id)).map((c) => c.id),
  )

  for (const parent of parents) {
    if (!parentIdsWithChildren.has(parent.id)) {
      continue
    }

    const children = sorted
      .filter((c) => c.parentCategoryId === parent.id)
      .sort((a, b) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name, 'vi'))

    groups.push({
      parent,
      children,
      accent: resolveAccent(parent.slug),
      iconKey: parent.slug,
    })
  }

  // Danh mục phẳng còn sót (chưa gán khu) — gom vào Đại sảnh nếu có, không tạo block riêng từng mục.
  const strayLeaves = sorted.filter(
    (c) => !c.parentCategoryId && !parentIdsWithChildren.has(c.id),
  )
  if (strayLeaves.length > 0) {
    const daiSanh = groups.find((g) => g.iconKey === 'dai-sanh')
    if (daiSanh) {
      const existing = new Set(daiSanh.children.map((c) => c.id))
      for (const leaf of strayLeaves) {
        if (!existing.has(leaf.id)) {
          daiSanh.children.push(leaf)
        }
      }
      daiSanh.children.sort(
        (a, b) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name, 'vi'),
      )
    } else {
      groups.push({
        parent: {
          id: '__stray__',
          name: 'Đại sảnh',
          description: 'Các mục chưa gán khu — sẽ được đồng bộ khi chạy lại API.',
          slug: 'dai-sanh',
          parentCategoryId: null,
          postCount: strayLeaves.reduce((s, c) => s + c.postCount, 0),
          displayOrder: 10,
        },
        children: strayLeaves,
        accent: 'amber',
        iconKey: 'dai-sanh',
      })
    }
  }

  const assignedChildIds = new Set(groups.flatMap((g) => g.children.map((c) => c.id)))
  const orphans = sorted.filter((c) => c.parentCategoryId && !assignedChildIds.has(c.id))
  if (orphans.length > 0) {
    groups.push({
      parent: {
        id: '__orphan__',
        name: 'Khác',
        description: 'Danh mục chưa gán khu.',
        slug: 'khac',
        parentCategoryId: null,
        postCount: orphans.reduce((s, c) => s + c.postCount, 0),
        displayOrder: 999,
      },
      children: orphans,
      accent: 'slate',
      iconKey: 'khac',
    })
  }

  return groups
}

export function formatForumCount(n: number): string {
  if (n >= 1_000_000) return `${(n / 1_000_000).toFixed(1).replace(/\.0$/, '')}M`
  if (n >= 1_000) return `${(n / 1_000).toFixed(1).replace(/\.0$/, '')}K`
  return String(n)
}

import { NavLink, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

type AdminNavItem = {
  to: string;
  labelKey: string;
  fallbackLabel: string;
};

type AdminNavGroup = {
  titleKey: string;
  fallbackTitle: string;
  items: AdminNavItem[];
};

const adminNavGroups: AdminNavGroup[] = [
  {
    titleKey: 'admin.sidebar.groups.access',
    fallbackTitle: 'Access control',
    items: [
      { to: '/admin/dashboard', labelKey: 'admin.dashboard', fallbackLabel: 'Dashboard' },
      { to: '/admin/users', labelKey: 'admin.users', fallbackLabel: 'Users' },
      { to: '/admin/roles', labelKey: 'admin.roles', fallbackLabel: 'Roles' },
    ],
  },
  {
    titleKey: 'admin.sidebar.groups.overrides',
    fallbackTitle: 'Overrides',
    items: [
      { to: '/admin/overrides/users', labelKey: 'admin.overrides.users', fallbackLabel: 'User overrides' },
      { to: '/admin/overrides/groups', labelKey: 'admin.overrides.groups', fallbackLabel: 'Group overrides' },
    ],
  },
  {
    titleKey: 'admin.sidebar.groups.forum',
    fallbackTitle: 'Forum',
    items: [
      {
        to: '/admin/forum/thread-channels',
        labelKey: 'admin.threadChannels',
        fallbackLabel: 'Thread channels',
      },
    ],
  },
  {
    titleKey: 'admin.sidebar.groups.observability',
    fallbackTitle: 'Observability',
    items: [
      { to: '/admin/toggles', labelKey: 'admin.toggles', fallbackLabel: 'Feature toggles' },
      { to: '/admin/logs/actions', labelKey: 'admin.logs.actions', fallbackLabel: 'Action logs' },
      { to: '/admin/logs/audit', labelKey: 'admin.logs.audit', fallbackLabel: 'Audit logs' },
    ],
  },
];

const isActiveMatch = (itemPath: string, currentPath: string) =>
  currentPath === itemPath || currentPath.startsWith(`${itemPath}/`);

export function AdminSidebar() {
  const { t } = useTranslation();
  const location = useLocation();

  return (
    <aside className="w-72 shrink-0 rounded-xl border border-rose-200 bg-white p-4">
      <NavLink
        to="/home"
        className="mb-5 inline-flex items-center rounded-md border border-rose-200 px-3 py-2 text-sm font-medium text-rose-700 hover:bg-rose-50"
      >
        {t('common.back')}
      </NavLink>

      <nav aria-label={t('admin.layout.title')} className="space-y-5">
        {adminNavGroups.map((group) => (
          <div key={group.titleKey}>
            <p className="mb-2 px-1 text-xs font-semibold uppercase tracking-wide text-slate-500">
              {t(group.titleKey)}
            </p>
            <div className="space-y-1">
              {group.items.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={false}
                  className={({ isActive }) => {
                    const active = isActive || isActiveMatch(item.to, location.pathname);
                    return `block rounded-md px-3 py-2 text-sm ${
                      active
                        ? 'bg-rose-100 font-semibold text-rose-700'
                        : 'text-slate-700 hover:bg-slate-100'
                    }`;
                  }}
                >
                  {t(item.labelKey)}
                </NavLink>
              ))}
            </div>
          </div>
        ))}
      </nav>
    </aside>
  );
}

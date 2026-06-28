type PlaceholderProps = {
  titleKey: string;
};

import { useTranslation } from 'react-i18next';

export function Placeholder({ titleKey }: PlaceholderProps) {
  const { t } = useTranslation();

  return <div className="rounded-xl bg-white border border-slate-200 p-4">{t(titleKey)}</div>;
}

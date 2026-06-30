import { motion } from 'framer-motion';
import type { ReactNode } from 'react';

interface SuggestionChipProps {
  title: string;
  icon?: ReactNode;
  onClick: (label: string) => void;
  index: number;
}

export function SuggestionChip({ title, icon, onClick, index }: SuggestionChipProps) {
  return (
    <motion.button
      initial={{ opacity: 0, y: 15 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: index * 0.05 + 0.1, duration: 0.3, ease: 'easeOut' }}
      onClick={() => onClick(title)}
      className="flex items-center gap-2 px-4 py-2 bg-transparent border border-[#E5E7EB] rounded-full hover:bg-black/5 hover:border-[#D1D5DB] transition-all duration-200 active:scale-95 whitespace-nowrap text-[13px] text-[#4B5563]"
    >
      {icon && <span className="flex items-center justify-center">{icon}</span>}
      <span className="font-medium">{title}</span>
    </motion.button>
  );
}

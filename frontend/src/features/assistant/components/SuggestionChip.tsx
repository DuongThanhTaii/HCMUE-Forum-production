import { motion } from 'framer-motion';
import type { ReactNode } from 'react';

interface SuggestionChipProps {
  title: string;
  description: string;
  icon?: ReactNode;
  iconBg?: string;
  iconColor?: string;
  onClick: (label: string) => void;
  index: number;
}

export function SuggestionChip({ title, description, icon, iconBg = '#F3F4F6', iconColor = '#1E5EFF', onClick, index }: SuggestionChipProps) {
  return (
    <motion.button
      initial={{ opacity: 0, y: 15 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: index * 0.05 + 0.1, duration: 0.3, ease: 'easeOut' }}
      onClick={() => onClick(title)}
      className="flex items-start text-left gap-3.5 p-4 bg-white border border-[#E5E7EB] rounded-2xl hover:bg-[#F9FAFB] hover:border-[#D1D5DB] hover:shadow-sm transition-all duration-200 active:scale-95 shadow-[0_1px_2px_rgba(0,0,0,0.02)] w-full sm:w-[calc(50%-8px)] lg:w-[calc(33.333%-11px)] group"
    >
      <div 
        className="flex items-center justify-center w-11 h-11 rounded-xl text-xl flex-shrink-0 transition-transform group-hover:scale-105"
        style={{ backgroundColor: iconBg, color: iconColor }}
      >
        {icon}
      </div>
      <div>
        <h3 className="text-sm font-semibold text-[#111827] group-hover:text-[#1E5EFF] transition-colors">{title}</h3>
        <p className="text-xs text-[#6B7280] mt-1 leading-relaxed">{description}</p>
      </div>
    </motion.button>
  );
}

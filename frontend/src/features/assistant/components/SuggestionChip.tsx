import { motion } from 'framer-motion';

interface SuggestionChipProps {
  label: string;
  icon?: string;
  onClick: (label: string) => void;
  index: number;
}

export function SuggestionChip({ label, icon, onClick, index }: SuggestionChipProps) {
  return (
    <motion.button
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: index * 0.05 + 0.2, duration: 0.3, ease: 'easeOut' }}
      onClick={() => onClick(label)}
      className="flex items-center gap-2 px-4 py-2.5 bg-white border border-[#E5E7EB] rounded-full text-sm text-[#111827] font-medium hover:bg-[#EDF4FF] hover:border-[#1E5EFF] hover:text-[#1E5EFF] transition-all duration-200 active:scale-95 shadow-sm"
    >
      {icon && <span>{icon}</span>}
      {label}
    </motion.button>
  );
}

import { motion } from 'framer-motion';
import { SuggestionChip } from './SuggestionChip';
import { Bot } from 'lucide-react';

interface EmptyStateProps {
  onSuggestClick: (query: string) => void;
}

export function EmptyState({ onSuggestClick }: EmptyStateProps) {
  const suggestions = [
    { label: 'Admission', icon: '📝' },
    { label: 'Faculty Office', icon: '🏢' },
    { label: 'Academic Calendar', icon: '📅' },
    { label: 'Tuition', icon: '💳' },
    { label: 'Curriculum', icon: '📚' },
    { label: 'Graduation', icon: '🎓' },
  ];

  return (
    <div className="flex flex-col items-center justify-center h-full px-6 text-center max-w-2xl mx-auto my-auto mt-[10vh]">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.5, ease: 'easeOut' }}
        className="w-20 h-20 bg-gradient-to-tr from-[#1E5EFF] to-[#EDF4FF] text-white rounded-3xl flex items-center justify-center shadow-lg mb-6 rotate-3"
      >
        <Bot size={40} strokeWidth={2} />
      </motion.div>
      
      <motion.h1
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1, duration: 0.4 }}
        className="text-2xl sm:text-3xl font-bold text-[#111827] mb-3 tracking-tight"
      >
        Welcome to HCMUE AI Assistant
      </motion.h1>
      
      <motion.p
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.2, duration: 0.4 }}
        className="text-base text-[#6B7280] mb-10 max-w-lg"
      >
        I can help you answer questions about your university regulations, study paths, and more. Try asking one of the suggestions below.
      </motion.p>
      
      <div className="flex flex-wrap justify-center gap-3">
        {suggestions.map((s, index) => (
          <SuggestionChip
            key={s.label}
            label={s.label}
            icon={s.icon}
            index={index}
            onClick={onSuggestClick}
          />
        ))}
      </div>
    </div>
  );
}

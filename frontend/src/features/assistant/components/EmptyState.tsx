import { motion } from 'framer-motion';
import { SuggestionChip } from './SuggestionChip';
import { ChatInput } from './ChatInput';
import { useTranslation } from 'react-i18next';
import React from 'react';

interface EmptyStateProps {
  input: string;
  setInput: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  isLoading: boolean;
  onSuggestClick: (query: string) => void;
}

export function EmptyState({ input, setInput, onSubmit, isLoading, onSuggestClick }: EmptyStateProps) {
  const { t } = useTranslation('assistant');

  const questions = [
    t('questions.q1', 'Cách tính điểm GPA học kỳ này?'),
    t('questions.q2', 'Điều kiện xét học bổng khuyến khích'),
    t('questions.q3', 'Lịch học lại môn Giải tích 1'),
    t('questions.q4', 'Quy định bảo lưu kết quả học tập')
  ];

  const categories = [
    t('categories.c1', 'Học vụ & Đào tạo'),
    t('categories.c2', 'Tính điểm & Công cụ'),
    t('categories.c3', 'Học bổng & Học phí')
  ];

  return (
    <div className="relative flex flex-col items-center justify-center h-full px-4 text-center w-full mx-auto bg-transparent z-10">
      <div className="z-10 w-full max-w-3xl flex flex-col items-center">
        <motion.h1
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.4 }}
          className="text-2xl sm:text-[32px] font-semibold text-[#111827] mb-2 tracking-tight"
        >
          {t('emptyState.welcomeTitle', 'Chào mừng đến với Trợ lý ảo HCMUE')}
        </motion.h1>
        
        <motion.p
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1, duration: 0.4 }}
          className="text-[15px] sm:text-base text-[#6B7280] mb-8"
        >
          {t('emptyState.welcomeSubtitle', 'Giải đáp quy chế đào tạo, lộ trình học tập, học bổng và nhiều hơn nữa.')}
        </motion.p>
        
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2, duration: 0.4 }}
          className="w-full mb-6"
        >
          <ChatInput 
            input={input} 
            setInput={setInput} 
            onSubmit={onSubmit} 
            isLoading={isLoading} 
            isCentered={true}
          />
        </motion.div>

        <motion.div 
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.3, duration: 0.5 }}
          className="flex flex-col items-center gap-4 w-full"
        >
          <div className="flex flex-wrap justify-center gap-2 max-w-2xl">
            {questions.map((q, index) => (
              <SuggestionChip
                key={index}
                title={q}
                index={index}
                onClick={onSuggestClick}
              />
            ))}
          </div>

          <div className="flex flex-wrap justify-center gap-2 max-w-2xl mt-2">
            {categories.map((c, index) => (
              <SuggestionChip
                key={index + questions.length}
                title={c}
                icon={<div className="w-1.5 h-1.5 rounded-full bg-[#CF373D]" />}
                index={index + questions.length}
                onClick={onSuggestClick}
              />
            ))}
          </div>
        </motion.div>
      </div>
    </div>
  );
}

import { motion } from 'framer-motion';
import { SuggestionChip } from './SuggestionChip';
import { Bot, GraduationCap, Calculator, Wallet, Award, Building2, ClipboardList } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface EmptyStateProps {
  onSuggestClick: (query: string) => void;
}

export function EmptyState({ onSuggestClick }: EmptyStateProps) {
  const { t } = useTranslation('assistant');

  const suggestions = [
    { 
      title: t('suggestions.s1.title', 'Học vụ & Đào tạo'), 
      description: t('suggestions.s1.description', 'Điểm số, qua môn, học lại, bảo lưu...'),
      icon: <GraduationCap size={22} strokeWidth={2} />,
      iconBg: '#EDF4FF',
      iconColor: '#1E5EFF'
    },
    { 
      title: t('suggestions.s2.title', 'Tính điểm & Công cụ'), 
      description: t('suggestions.s2.description', 'Tính GPA, rèn luyện, học bổng...'),
      icon: <Calculator size={22} strokeWidth={2} />,
      iconBg: '#FDF4FF',
      iconColor: '#C026D3'
    },
    { 
      title: t('suggestions.s3.title', 'Học bổng & Học phí'), 
      description: t('suggestions.s3.description', 'Điều kiện xét, mức thưởng, miễn giảm...'),
      icon: <Wallet size={22} strokeWidth={2} />,
      iconBg: '#F5F3FF',
      iconColor: '#7C3AED'
    },
    { 
      title: t('suggestions.s4.title', 'Rèn luyện & Khen thưởng'), 
      description: t('suggestions.s4.description', 'Điểm rèn luyện, kỷ luật, danh hiệu...'),
      icon: <Award size={22} strokeWidth={2} />,
      iconBg: '#FEF2F2',
      iconColor: '#DC2626'
    },
    { 
      title: t('suggestions.s5.title', 'Phòng ban & Liên hệ'), 
      description: t('suggestions.s5.description', 'SĐT, email, địa chỉ, KTX...'),
      icon: <Building2 size={22} strokeWidth={2} />,
      iconBg: '#FFFBEB',
      iconColor: '#D97706'
    },
    { 
      title: t('suggestions.s6.title', 'Quy trình Hành chính'), 
      description: t('suggestions.s6.description', 'Thủ tục, nộp đơn, mẫu biểu...'),
      icon: <ClipboardList size={22} strokeWidth={2} />,
      iconBg: '#ECFDF5',
      iconColor: '#059669'
    },
  ];

  return (
    <div className="flex flex-col items-center justify-center h-full px-4 text-center max-w-4xl mx-auto my-auto mt-[10vh]">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.5, ease: 'easeOut' }}
        className="w-16 h-16 sm:w-20 sm:h-20 bg-gradient-to-tr from-[#1E5EFF] to-[#EDF4FF] text-white rounded-[1.25rem] sm:rounded-3xl flex items-center justify-center shadow-[0_8px_20px_rgba(30,94,255,0.2)] mb-6 rotate-3"
      >
        <Bot size={36} strokeWidth={2.5} className="w-8 h-8 sm:w-10 sm:h-10" />
      </motion.div>
      
      <motion.h1
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1, duration: 0.4 }}
        className="text-2xl sm:text-3xl font-bold text-[#111827] mb-3 tracking-tight"
      >
        {t('emptyState.welcomeTitle', 'Chào mừng đến với Trợ lý Ảo HCMUE')}
      </motion.h1>
      
      <motion.p
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.2, duration: 0.4 }}
        className="text-[15px] sm:text-base text-[#6B7280] mb-10 max-w-lg leading-relaxed"
      >
        {t('emptyState.welcomeSubtitle', 'Tôi có thể giúp bạn giải đáp các câu hỏi về quy chế đại học, lộ trình học tập, v.v. Hãy thử chọn một trong các gợi ý dưới đây.')}
      </motion.p>
      
      <div className="flex flex-wrap justify-center gap-3 w-full">
        {suggestions.map((s, index) => (
          <SuggestionChip
            key={s.title}
            title={s.title}
            description={s.description}
            icon={s.icon}
            iconBg={s.iconBg}
            iconColor={s.iconColor}
            index={index}
            onClick={onSuggestClick}
          />
        ))}
      </div>
    </div>
  );
}

import { X } from 'lucide-react';
import { ForumSidebarContent } from './ForumSidebar';

interface MobileMenuDrawerProps {
  isOpen: boolean;
  onClose: () => void;
}

export function MobileMenuDrawer({ isOpen, onClose }: MobileMenuDrawerProps) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bottom-14 z-40 flex flex-col bg-white lg:hidden animate-in slide-in-from-bottom-2 duration-200">
      <div className="flex h-14 shrink-0 items-center justify-between border-b border-slate-200 bg-white px-4 shadow-sm">
        <h2 className="text-lg font-bold text-slate-800">Menu</h2>
        <button
          type="button"
          onClick={onClose}
          className="rounded-full p-2 text-slate-500 hover:bg-slate-100"
        >
          <X className="h-6 w-6" />
        </button>
      </div>
      <div className="flex-1 overflow-hidden bg-slate-50/50">
        {/* Reuse ForumSidebarContent which has its own scrolling container */}
        <ForumSidebarContent onClickItem={onClose} />
      </div>
    </div>
  );
}

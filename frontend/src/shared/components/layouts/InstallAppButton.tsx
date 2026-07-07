import { useState } from 'react';
import { Download, Share, PlusSquare, X } from 'lucide-react';
import { usePWAInstall } from '../../hooks/usePWAInstall';
import { useTranslation } from 'react-i18next';

export function InstallAppButton() {
  const { t } = useTranslation();
  const { isInstallable, isIOS, isStandalone, promptInstall } = usePWAInstall();
  const [showIOSModal, setShowIOSModal] = useState(false);

  // If already installed or neither installable nor iOS, don't show the button
  if (isStandalone || (!isInstallable && !isIOS)) {
    return null;
  }

  const handleInstallClick = () => {
    if (isInstallable) {
      promptInstall();
    } else if (isIOS) {
      setShowIOSModal(true);
    }
  };

  return (
    <>
      <button
        onClick={handleInstallClick}
        className="hidden sm:flex h-8 items-center gap-1.5 rounded-md border border-primary/20 bg-primary/5 px-3 text-xs font-medium text-primary hover:bg-primary/10 transition-colors"
        title="Install App"
      >
        <Download className="h-3.5 w-3.5" />
        <span>Install App</span>
      </button>

      {/* Mobile Icon only */}
      <button
        onClick={handleInstallClick}
        className="flex sm:hidden h-8 w-8 items-center justify-center rounded-md text-primary hover:bg-slate-100 transition-colors"
        title="Install App"
      >
        <Download className="h-4 w-4" />
      </button>

      {/* iOS Install Guide Modal */}
      {showIOSModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
          <div className="relative w-full max-w-sm rounded-xl bg-white p-6 shadow-2xl animate-in fade-in zoom-in-95 duration-200">
            <button
              onClick={() => setShowIOSModal(false)}
              className="absolute right-4 top-4 text-slate-400 hover:text-slate-600"
            >
              <X className="h-5 w-5" />
            </button>
            
            <div className="flex flex-col items-center text-center">
              <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                <Download className="h-8 w-8" />
              </div>
              
              <h3 className="mb-2 text-lg font-bold text-slate-900">
                Install UniHub App
              </h3>
              <p className="mb-6 text-sm text-slate-600">
                Install this application on your home screen for quick and easy access when you're on the go.
              </p>
              
              <div className="w-full rounded-lg bg-slate-50 p-4 text-left text-sm text-slate-700">
                <div className="mb-3 flex items-center gap-3">
                  <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/20 text-xs font-bold text-primary">1</span>
                  <p className="flex items-center gap-1.5">
                    Tap the <Share className="h-4 w-4 text-blue-500" /> <b>Share</b> icon below.
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/20 text-xs font-bold text-primary">2</span>
                  <p className="flex items-center gap-1.5">
                    Select <PlusSquare className="h-4 w-4 text-slate-600" /> <b>Add to Home Screen</b>.
                  </p>
                </div>
              </div>
              
              <button
                onClick={() => setShowIOSModal(false)}
                className="mt-6 w-full rounded-lg bg-primary py-2.5 text-sm font-semibold text-white hover:bg-primary/90 transition-colors"
              >
                Got it
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

import { useState, useEffect } from 'react';

interface BeforeInstallPromptEvent extends Event {
  readonly platforms: string[];
  readonly userChoice: Promise<{
    outcome: 'accepted' | 'dismissed';
    platform: string;
  }>;
  prompt(): Promise<void>;
}

export function usePWAInstall() {
  const [deferredPrompt, setDeferredPrompt] = useState<BeforeInstallPromptEvent | null>(null);
  const [isInstallable, setIsInstallable] = useState(false);
  const [isIOS, setIsIOS] = useState(false);
  const [isStandalone, setIsStandalone] = useState(false);

  useEffect(() => {
    // Check if device is iOS
    const userAgent = window.navigator.userAgent.toLowerCase();
    const isIosDevice = /iphone|ipad|ipod/.test(userAgent);
    setIsIOS(isIosDevice);

    // Check if app is already installed (standalone mode)
    const isStandaloneMode = 
      window.matchMedia('(display-mode: standalone)').matches || 
      // @ts-ignore
      ('standalone' in window.navigator && window.navigator.standalone === true);
    setIsStandalone(isStandaloneMode);

    // Handle Android/Chrome install prompt
    const handleBeforeInstallPrompt = (e: Event) => {
      e.preventDefault();
      setDeferredPrompt(e as BeforeInstallPromptEvent);
      setIsInstallable(true);
    };

    window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt);

    // Listen for successful installation
    window.addEventListener('appinstalled', () => {
      setDeferredPrompt(null);
      setIsInstallable(false);
      setIsStandalone(true);
    });

    return () => {
      window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt);
    };
  }, []);

  const promptInstall = async () => {
    if (deferredPrompt) {
      try {
        await deferredPrompt.prompt();
      } catch (error) {
        console.error("Install prompt error:", error);
      }
      // Note: We don't manually clear the state here.
      // 1. If accepted, the 'appinstalled' event will fire and hide the button.
      // 2. If dismissed, the button remains (though clicking again might require a reload, it prevents the UI from abruptly changing).
    }
  };

  return {
    isInstallable,
    isIOS,
    isStandalone,
    promptInstall
  };
}

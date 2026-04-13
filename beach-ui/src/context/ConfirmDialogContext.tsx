import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react';

type ConfirmTone = 'default' | 'danger';

type ConfirmDialogOptions = {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  tone?: ConfirmTone;
};

type ConfirmDialogContextValue = {
  confirm: (options: ConfirmDialogOptions) => Promise<boolean>;
};

type PendingDialog = ConfirmDialogOptions & {
  id: number;
};

const ConfirmDialogContext = createContext<ConfirmDialogContextValue | null>(null);

type ConfirmDialogProviderProps = {
  children: ReactNode;
};

const defaultOptions = {
  title: 'Devam etmek istediğinize emin misiniz?',
  confirmText: 'Onayla',
  cancelText: 'Vazgeç',
  tone: 'default' as ConfirmTone,
};

export const ConfirmDialogProvider = ({ children }: ConfirmDialogProviderProps) => {
  const [dialog, setDialog] = useState<PendingDialog | null>(null);
  const resolverRef = useRef<((value: boolean) => void) | null>(null);
  const nextIdRef = useRef(0);

  const closeDialog = useCallback((result: boolean) => {
    resolverRef.current?.(result);
    resolverRef.current = null;
    setDialog(null);
  }, []);

  const confirm = useCallback((options: ConfirmDialogOptions) => {
    const id = nextIdRef.current + 1;
    nextIdRef.current = id;

    setDialog({
      ...defaultOptions,
      ...options,
      id,
    });

    return new Promise<boolean>((resolve) => {
      resolverRef.current = resolve;
    });
  }, []);

  const value = useMemo<ConfirmDialogContextValue>(
    () => ({
      confirm,
    }),
    [confirm]
  );

  const confirmButtonClass =
    dialog?.tone === 'danger'
      ? 'bg-rose-600 hover:bg-rose-700 focus:ring-rose-200'
      : 'bg-slate-900 hover:bg-slate-700 focus:ring-slate-200';

  return (
    <ConfirmDialogContext.Provider value={value}>
      {children}

      {dialog && (
        <div className="fixed inset-0 z-[120] flex items-center justify-center p-4">
          <button
            type="button"
            aria-label="Kapat"
            onClick={() => closeDialog(false)}
            className="absolute inset-0 bg-slate-950/55 backdrop-blur-sm"
          />

          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby={`confirm-dialog-title-${dialog.id}`}
            className="relative w-full max-w-md rounded-[2rem] border border-slate-200 bg-white p-6 shadow-2xl"
          >
            <div className="mb-6">
              <p
                id={`confirm-dialog-title-${dialog.id}`}
                className="text-lg font-black tracking-tight text-slate-900"
              >
                {dialog.title}
              </p>
              <p className="mt-2 text-sm font-medium leading-relaxed text-slate-500">{dialog.message}</p>
            </div>

            <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
              <button
                type="button"
                onClick={() => closeDialog(false)}
                className="rounded-2xl border border-slate-200 px-5 py-3 text-sm font-black uppercase tracking-widest text-slate-600 transition hover:bg-slate-50"
              >
                {dialog.cancelText}
              </button>
              <button
                type="button"
                onClick={() => closeDialog(true)}
                className={`rounded-2xl px-5 py-3 text-sm font-black uppercase tracking-widest text-white transition focus:outline-none focus:ring-4 ${confirmButtonClass}`}
              >
                {dialog.confirmText}
              </button>
            </div>
          </div>
        </div>
      )}
    </ConfirmDialogContext.Provider>
  );
};

export const useConfirmDialog = (): ConfirmDialogContextValue => {
  const context = useContext(ConfirmDialogContext);
  if (context == null) {
    throw new Error('useConfirmDialog must be used within a ConfirmDialogProvider');
  }

  return context;
};

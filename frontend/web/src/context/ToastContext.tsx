import { createContext, useContext } from "react";
import { useToast } from "../lib/useToast";
import { ToastContainer } from "../components/Toast";
import type { ToastType } from "../lib/useToast";

type ToastContextValue = {
  push: (msg: string, type?: ToastType) => void;
  error: (msg: string) => void;
  success: (msg: string) => void;
};

const ToastContext = createContext<ToastContextValue | undefined>(undefined);

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const { toasts, push, dismiss } = useToast();

  const value: ToastContextValue = {
    push,
    error:   (msg) => push(msg, "error"),
    success: (msg) => push(msg, "success"),
  };

  return (
    <ToastContext.Provider value={value}>
      {children}
      {/* Renders for the entire app — no need to add ToastContainer in every page */}
      <ToastContainer toasts={toasts} dismiss={dismiss} />
    </ToastContext.Provider>
  );
}

export function useGlobalToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useGlobalToast must be used within ToastProvider");
  return ctx;
}

import { useEffect, useState } from "react";

export type Countdown = {
  days: number;
  hours: number;
  minutes: number;
  seconds: number;
  isExpired: boolean;
  isUpcoming: boolean;
};

export function useCountdown(endTime: string, startTime?: string): Countdown {
  const calc = (): Countdown => {
    const now = Date.now();
    const end = new Date(endTime).getTime();
    const start = startTime ? new Date(startTime).getTime() : now;

    if (now < start) {
      return { days: 0, hours: 0, minutes: 0, seconds: 0, isExpired: false, isUpcoming: true };
    }

    const diff = end - now;
    if (diff <= 0) {
      return { days: 0, hours: 0, minutes: 0, seconds: 0, isExpired: true, isUpcoming: false };
    }

    const days    = Math.floor(diff / 86_400_000);
    const hours   = Math.floor((diff % 86_400_000) / 3_600_000);
    const minutes = Math.floor((diff % 3_600_000)  / 60_000);
    const seconds = Math.floor((diff % 60_000)     / 1_000);
    return { days, hours, minutes, seconds, isExpired: false, isUpcoming: false };
  };

  const [cd, setCd] = useState<Countdown>(calc);

  useEffect(() => {
    const id = setInterval(() => setCd(calc()), 1000);
    return () => clearInterval(id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [endTime, startTime]);

  return cd;
}

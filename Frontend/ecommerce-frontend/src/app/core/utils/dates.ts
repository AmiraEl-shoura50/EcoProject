const pad = (n: number): string => String(n).padStart(2, '0');

// تاريخ اليوم بتوقيت المستخدم بصيغة yyyy-MM-dd (تنفع مع input type="date")
export function todayLocal(): string {
  const d = new Date();
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

export function monthStartLocal(): string {
  const d = new Date();
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-01`;
}
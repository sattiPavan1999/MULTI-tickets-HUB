type ClassValue = string | number | null | undefined | false | Record<string, boolean>;

export function cn(...inputs: ClassValue[]): string {
  const out: string[] = [];
  for (const v of inputs) {
    if (!v) continue;
    if (typeof v === 'string' || typeof v === 'number') {
      out.push(String(v));
    } else if (typeof v === 'object') {
      for (const [k, on] of Object.entries(v)) {
        if (on) out.push(k);
      }
    }
  }
  return out.join(' ');
}

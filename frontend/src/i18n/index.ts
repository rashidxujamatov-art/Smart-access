import { uz } from './uz';

type Dict = typeof uz;

type Leaves<T, P extends string = ''> = {
  [K in keyof T & string]: T[K] extends string ? `${P}${K}` : Leaves<T[K], `${P}${K}.`>;
}[keyof T & string];

export type TKey = Leaves<Dict>;

/** Matnni kalit bo'yicha oladi va {param} larni almashtiradi. */
export function t(key: TKey, params?: Record<string, string | number>): string {
  let value: unknown = uz;
  for (const part of key.split('.')) {
    value = (value as Record<string, unknown>)[part];
  }
  let text = typeof value === 'string' ? value : key;
  if (params) {
    for (const [k, v] of Object.entries(params)) {
      text = text.split(`{${k}}`).join(String(v));
    }
  }
  return text;
}

/** Backend xato kodini matnga aylantiradi (backend matn qaytarmaydi — faqat kod). */
export function errorText(code: string | undefined, params?: Record<string, string | number>): string {
  const errors = uz.errors as Record<string, string>;
  return code && code in errors ? t(`errors.${code}` as TKey, params) : t('errors.unknown');
}

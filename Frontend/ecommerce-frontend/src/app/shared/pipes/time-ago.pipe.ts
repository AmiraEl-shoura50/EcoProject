import { Pipe, PipeTransform } from '@angular/core';

// أرقام لاتينية (1 2 3) عشان تتماشى مع باقي الموقع
const relativeFormat = new Intl.RelativeTimeFormat('ar-u-nu-latn', { numeric: 'auto' });

@Pipe({ name: 'timeAgo', standalone: true })
export class TimeAgoPipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) return '';

    const date = typeof value === 'string' ? new Date(toUtc(value)) : value;
    const seconds = Math.round((date.getTime() - Date.now()) / 1000);
    const abs = Math.abs(seconds);

    if (abs < 45) return 'الآن';
    if (abs < 3600) return relativeFormat.format(Math.round(seconds / 60), 'minute');
    if (abs < 86400) return relativeFormat.format(Math.round(seconds / 3600), 'hour');
    if (abs < 7 * 86400) return relativeFormat.format(Math.round(seconds / 86400), 'day');

    return date.toLocaleDateString('en-GB'); // dd/MM/yyyy
  }
}

// أي تاريخ من غير علامة توقيت بنعامله كـ UTC (شبكة أمان زيادة جنب إصلاح الباك)
function toUtc(value: string): string {
  return /(Z|[+-]\d{2}:?\d{2})$/i.test(value) ? value : value + 'Z';
}

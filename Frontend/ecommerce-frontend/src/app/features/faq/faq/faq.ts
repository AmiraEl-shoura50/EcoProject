import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, ChevronDown } from 'lucide-angular';
import { SITE } from '../../../core/config/site-config';

interface FaqItem {
  question: string;
  answer: string;
}

@Component({
  selector: 'app-faq',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './faq.html',
  styleUrl: './faq.css'
})
export class Faq {
  readonly ChevronDownIcon = ChevronDown;

  openIndex = signal<number | null>(null);

  faqs: FaqItem[] = [
    {
      question: 'إزاي أقدر أتابع حالة طلبي؟',
      answer: 'من صفحة "طلباتي" في حسابك هتلاقي كل طلباتك وحالة كل واحد منهم، وبيوصلك كمان إشعار على الجرس مع كل تحديث.'
    },
    {
      question: 'إيه طرق الدفع المتاحة؟',
      answer: 'الطرق المتاحة بتظهر لك في صفحة إتمام الطلب. بنوفر التحويل (زي فودافون كاش وإنستاباي) برفع إثبات التحويل، أو الدفع الإلكتروني المباشر بالبطاقة.'
    },
    {
      question: 'إزاي أرفع إثبات التحويل؟',
      answer: 'بعد تأكيد الطلب بتفتح لك صفحة فيها رابط التحويل ومكان رفع صورة الإيصال. البائع بيراجع الإثبات ويؤكد الدفع، وبيوصلك إشعار بالنتيجة.'
    },
    {
      question: 'هل ممكن ألغي طلبي؟',
      answer: 'أيوه، من صفحة "طلباتي" تقدر تلغي الطلب طالما لسه ما اتشحنش.'
    },
    {
      question: 'وإيه سياسة الإرجاع؟',
      answer: `لو وصلك منتج بعيب أو مختلف عن الوصف، تواصل معنا خلال ${SITE.policies.returnWindowDays} يومًا من الاستلام. التفاصيل الكاملة في صفحة سياسة الإرجاع والإلغاء.`
    },
    {
      question: 'كام مدة التوصيل المتوقعة؟',
      answer: `من ${SITE.policies.deliveryMinDays} إلى ${SITE.policies.deliveryMaxDays} أيام عمل من تأكيد الدفع، وبتختلف حسب منطقتك والبائع.`
    },
    {
      question: 'إزاي أقدر أبيع منتجاتي على المنصة؟',
      answer: 'سجّل حساب جديد واختر نوع الحساب "بائع"، وبعدها تقدر تضيف منتجاتك وتدير متجرك وطلباتك من لوحة التحكم.'
    }
  ];

  toggle(index: number): void {
    this.openIndex.set(this.openIndex() === index ? null : index);
  }
}
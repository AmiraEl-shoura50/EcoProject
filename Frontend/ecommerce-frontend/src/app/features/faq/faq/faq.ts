import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, ChevronDown } from 'lucide-angular';

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
      answer: 'من صفحة "طلباتي" في حسابك، هتلاقي كل طلباتك وحالة كل واحد منهم (قيد الانتظار، مؤكد، تم الشحن، تم التوصيل).'
    },
    {
      question: 'إيه طرق الدفع المتاحة؟',
      answer: 'نوفر الدفع عن طريق فودافون كاش وإنستاباي (برفع إثبات التحويل)، بالإضافة إلى الدفع الإلكتروني المباشر عبر بطاقات الفيزا والماستركارد وأبل باي.'
    },
    {
      question: 'هل ممكن أرجع منتج بعد الشراء؟',
      answer: 'نعم، يمكنك طلب إرجاع أي منتج خلال 14 يوم من تاريخ الاستلام، بشرط أن يكون المنتج في حالته الأصلية.'
    },
    
    {
      question: 'كام مدة التوصيل المتوقعة؟',
      answer: 'مدة التوصيل تتراوح من 2 إلى 5 أيام عمل حسب منطقتك والبائع المسؤول عن الطلب.'
    }
  ];

  toggle(index: number): void {
    this.openIndex.set(this.openIndex() === index ? null : index);
  }
}
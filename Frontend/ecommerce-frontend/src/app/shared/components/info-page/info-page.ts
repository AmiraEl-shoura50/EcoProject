import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, ChevronLeft } from 'lucide-angular';

@Component({
  selector: 'app-info-page',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './info-page.html',
  styleUrl: './info-page.css'
})
export class InfoPage {
  readonly ChevronIcon = ChevronLeft;

  title = input.required<string>();
  subtitle = input<string>('');
  wide = input<boolean>(false);            // عرض أكبر (صفحة التواصل)
  showContactCta = input<boolean>(true);   // بطاقة "تحتاج مساعدة؟" في الآخر
}
import { Component, HostListener, computed, effect, input, signal, untracked } from '@angular/core';
import { LucideAngularModule, ChevronRight, ChevronLeft, X } from 'lucide-angular';

@Component({
  selector: 'app-product-gallery',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './product-gallery.html',
  styleUrl: './product-gallery.css'
})
export class ProductGallery {
  readonly PrevIcon = ChevronRight; // الموقع RTL: السابق عند اليمين
  readonly NextIcon = ChevronLeft;
  readonly CloseIcon = X;

  images = input<string[]>([]);
  alt = input<string>('');

  selectedIndex = signal<number>(0);
  isOpen = signal<boolean>(false);

  current = computed(() => this.images()[this.selectedIndex()] ?? null);

  constructor() {
    // لما المنتج يتغيّر (التنقل بين منتجات من غير ما الصفحة تتهدم) نرجع لأول صورة
    effect(() => {
      this.images();
      untracked(() => {
        this.selectedIndex.set(0);
        this.isOpen.set(false);
      });
    });
  }

  select(index: number): void {
    this.selectedIndex.set(index);
  }

  next(): void {
    const count = this.images().length;
    if (count > 1) this.selectedIndex.update(i => (i + 1) % count);
  }

  prev(): void {
    const count = this.images().length;
    if (count > 1) this.selectedIndex.update(i => (i - 1 + count) % count);
  }

  open(): void {
    if (this.current()) this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
  }

  @HostListener('document:keydown', ['$event'])
  onKey(event: KeyboardEvent): void {
    if (!this.isOpen()) return;

    // RTL: السهم الشمال = التالي، واليمين = السابق
    if (event.key === 'Escape') this.close();
    else if (event.key === 'ArrowLeft') this.next();
    else if (event.key === 'ArrowRight') this.prev();
  }
}
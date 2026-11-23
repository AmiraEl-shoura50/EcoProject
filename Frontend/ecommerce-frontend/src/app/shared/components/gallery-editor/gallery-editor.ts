import { Component, OnDestroy, model } from '@angular/core';
import { LucideAngularModule, ImagePlus, Trash2, ChevronRight, ChevronLeft, Star } from 'lucide-angular';
import { ToastService } from '../../../core/services/toast';

export interface GalleryItem {
  uid: string;              // مفتاح ثابت لتتبع العنصر في الواجهة
  url: string;              // رابط العرض (رابط Cloudinary أو معاينة محلية)
  file: File | null;        // الملف لو صورة جديدة لسه متترفعتش
  imageId: number | null;   // رقم الصورة لو موجودة على السيرفر
}

@Component({
  selector: 'app-gallery-editor',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './gallery-editor.html',
  styleUrl: './gallery-editor.css'
})
export class GalleryEditor implements OnDestroy {
  readonly AddIcon = ImagePlus;
  readonly TrashIcon = Trash2;
  readonly EarlierIcon = ChevronRight; // الموقع RTL: الأول عند اليمين
  readonly LaterIcon = ChevronLeft;
  readonly StarIcon = Star;

  readonly maxImages = 6;
  private readonly maxImageSize = 5 * 1024 * 1024; // 5MB
  private counter = 0;

  // ربط ثنائي الاتجاه مع الصفحة اللي بتستخدمه: [(items)]="gallery"
  items = model<GalleryItem[]>([]);

  constructor(private toastService: ToastService) {}

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = ''; // عشان نقدر نختار نفس الملف تاني

    if (files.length === 0) return;

    const room = this.maxImages - this.items().length;
    if (files.length > room) {
      this.toastService.show(`الحد الأقصى ${this.maxImages} صور للمنتج`, 'error');
    }

    const added: GalleryItem[] = [];

    for (const file of files.slice(0, Math.max(room, 0))) {
      if (!file.type.startsWith('image/')) {
        this.toastService.show(`"${file.name}" ليس ملف صورة`, 'error');
        continue;
      }

      if (file.size > this.maxImageSize) {
        this.toastService.show(`"${file.name}" أكبر من 5 ميجابايت`, 'error');
        continue;
      }

      added.push({
        uid: `new-${this.counter++}`,
        url: URL.createObjectURL(file),
        file,
        imageId: null
      });
    }

    if (added.length > 0) this.items.update(list => [...list, ...added]);
  }

  remove(item: GalleryItem): void {
    if (item.file) URL.revokeObjectURL(item.url);
    this.items.update(list => list.filter(i => i.uid !== item.uid));
  }

  // نقل الصورة خطوة للأول (-1) أو للآخر (+1)
  move(index: number, step: number): void {
    const target = index + step;
    const list = [...this.items()];
    if (target < 0 || target >= list.length) return;

    [list[index], list[target]] = [list[target], list[index]];
    this.items.set(list);
  }

  makeMain(index: number): void {
    if (index === 0) return;

    const list = [...this.items()];
    const [item] = list.splice(index, 1);
    this.items.set([item, ...list]);
  }

  ngOnDestroy(): void {
    this.items().forEach(item => {
      if (item.file) URL.revokeObjectURL(item.url);
    });
  }
}
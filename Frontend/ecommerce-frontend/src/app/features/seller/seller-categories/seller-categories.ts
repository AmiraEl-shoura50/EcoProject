import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule, Plus, Pencil, Trash2, X, ImagePlus ,Eye ,EyeOff} from 'lucide-angular';
import { CategoryService } from '../../../core/services/category';
import { ToastService } from '../../../core/services/toast';
import { SellerCategory } from '../../../core/models/category.model';

@Component({
  selector: 'app-seller-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './seller-categories.html',
  styleUrl: './seller-categories.css'
})
export class SellerCategories implements OnInit {
  readonly PlusIcon = Plus;
  readonly EditIcon = Pencil;
  readonly TrashIcon = Trash2;
  readonly CloseIcon = X;
  readonly ImageIcon = ImagePlus;
  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;
  togglingId = signal<number | null>(null);
  categories = signal<SellerCategory[]>([]);
  isLoading = signal<boolean>(true);
  deletingId = signal<number | null>(null);

  // ---- النافذة (إضافة / تعديل) ----
  isModalOpen = signal<boolean>(false);
  editingCategory = signal<SellerCategory | null>(null);
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  isSaving = signal<boolean>(false);

  categoryForm: FormGroup;

  private readonly maxImageSize = 5 * 1024 * 1024; // 5MB

  // تجميع الأقسام الفرعية تحت القسم الرئيسي التابعة له
  groups = computed(() => {
    const map = new Map<string, SellerCategory[]>();

    for (const category of this.categories()) {
      const list = map.get(category.parentCategoryName) ?? [];
      list.push(category);
      map.set(category.parentCategoryName, list);
    }

    return Array.from(map, ([parentName, items]) => ({ parentName, items }))
      .sort((a, b) => a.parentName.localeCompare(b.parentName, 'ar'));
  });

  constructor(
    private fb: FormBuilder,
    public categoryService: CategoryService,
    private toastService: ToastService
  ) {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.maxLength(500)]],
      parentCategoryId: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    this.categoryService.loadTree(); // الأقسام الرئيسية المتاحة للاختيار
    this.loadCategories();
  }

  private loadCategories(): void {
    this.isLoading.set(true);

    this.categoryService.getMine().subscribe({
      next: (response) => {
        this.categories.set(response.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ---- النافذة ----
  openCreate(): void {
    this.editingCategory.set(null);
    this.categoryForm.get('parentCategoryId')?.enable();
    this.categoryForm.reset({ name: '', description: '', parentCategoryId: null });
    this.selectedFile.set(null);
    this.previewUrl.set(null);
    this.isModalOpen.set(true);
  }

  openEdit(category: SellerCategory): void {
    this.editingCategory.set(category);
    this.categoryForm.reset({
      name: category.name,
      description: category.description,
      parentCategoryId: category.parentCategoryId
    });
    // القسم الرئيسي مينفعش يتغيّر بعد الإنشاء
    this.categoryForm.get('parentCategoryId')?.disable();
    this.selectedFile.set(null);
    this.previewUrl.set(category.imageUrl);
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.toastService.show('من فضلك اختر ملف صورة فقط', 'error');
      input.value = '';
      return;
    }

    if (file.size > this.maxImageSize) {
      this.toastService.show('حجم الصورة كبير، الحد الأقصى 5 ميجابايت', 'error');
      input.value = '';
      return;
    }

    this.selectedFile.set(file);
    this.previewUrl.set(URL.createObjectURL(file));
  }

  save(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const value = this.categoryForm.getRawValue();
    const editing = this.editingCategory();

    // الأسماء بحرف كبير عشان تطابق CreateCategoryDto / UpdateCategoryDto
    const formData = new FormData();
    formData.append('Name', value.name.trim());
    formData.append('Description', value.description.trim());
    if (!editing) formData.append('ParentCategoryId', String(value.parentCategoryId));

    const file = this.selectedFile();
    if (file) formData.append('Image', file);

    this.isSaving.set(true);

    const request = editing
      ? this.categoryService.update(editing.id, formData)
      : this.categoryService.create(formData);

    request.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isModalOpen.set(false);
        this.toastService.show(editing ? 'تم تعديل القسم بنجاح' : 'تمت إضافة القسم بنجاح', 'success');

        this.loadCategories();
        this.categoryService.loadTree(); // تحديث قائمة الأقسام في الـ Navbar
      },
      error: () => this.isSaving.set(false)
    });
  }

  delete(category: SellerCategory): void {
    if (!confirm(`هل تريد حذف القسم "${category.name}"؟`)) return;

    this.deletingId.set(category.id);

    this.categoryService.delete(category.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toastService.show('تم حذف القسم بنجاح', 'success');

        this.loadCategories();
        this.categoryService.loadTree();
      },
      error: () => this.deletingId.set(null)
    });
  }

  toggleVisibility(category: SellerCategory): void {
    if (category.isHiddenByAdmin) return;

    const hide = !category.isHiddenBySeller;
    if (hide && !confirm(`هل تريد إخفاء القسم "${category.name}" ومنتجاته عن العملاء؟ تقدر تظهره في أي وقت.`)) return;

    this.togglingId.set(category.id);

    this.categoryService.setVisibility(category.id, hide).subscribe({
      next: () => {
        this.togglingId.set(null);
        this.toastService.show(hide ? 'تم إخفاء القسم عن العملاء' : 'تم إظهار القسم للعملاء', 'success');
        this.categories.update(list =>
          list.map(c => (c.id === category.id ? { ...c, isHiddenBySeller: hide } : c))
        );
      },
      error: () => this.togglingId.set(null)
    });
  }
}
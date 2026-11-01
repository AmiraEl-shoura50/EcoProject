import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule, Plus, Pencil, Trash2, X, ImagePlus } from 'lucide-angular';
import { CategoryService } from '../../../core/services/category';
import { ToastService } from '../../../core/services/toast';
import { AdminCategory, AdminSubCategory } from '../../../core/models/category.model';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './admin-categories.html',
  styleUrl: './admin-categories.css'
})
export class AdminCategories implements OnInit {
  readonly PlusIcon = Plus;
  readonly EditIcon = Pencil;
  readonly TrashIcon = Trash2;
  readonly CloseIcon = X;
  readonly ImageIcon = ImagePlus;

  categories = signal<AdminCategory[]>([]);
  isLoading = signal<boolean>(true);
  deletingId = signal<number | null>(null);

  // ---- النافذة ----
  isModalOpen = signal<boolean>(false);
  editingCategory = signal<AdminCategory | null>(null);
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  isSaving = signal<boolean>(false);

  categoryForm: FormGroup;

  private readonly maxImageSize = 5 * 1024 * 1024; // 5MB

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    private toastService: ToastService
  ) {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.maxLength(500)]]
    });
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.isLoading.set(true);

    this.categoryService.getAdminTree().subscribe({
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
    this.categoryForm.reset({ name: '', description: '' });
    this.selectedFile.set(null);
    this.previewUrl.set(null);
    this.isModalOpen.set(true);
  }

  openEdit(category: AdminCategory): void {
    this.editingCategory.set(category);
    this.categoryForm.reset({ name: category.name, description: category.description });
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

    // مفيش ParentCategoryId هنا، وده اللي بيخلي الباك يعتبره قسم رئيسي
    const formData = new FormData();
    formData.append('Name', value.name.trim());
    formData.append('Description', value.description.trim());

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
        this.refresh();
      },
      error: () => this.isSaving.set(false)
    });
  }

  // ---- الحذف ----
  deleteMain(category: AdminCategory): void {
    if (!confirm(`هل تريد حذف القسم "${category.name}"؟`)) return;
    this.runDelete(category.id);
  }

  deleteSub(sub: AdminSubCategory): void {
    const store = sub.storeName ? ` (للمتجر: ${sub.storeName})` : '';
    if (!confirm(`هل تريد حذف القسم الفرعي "${sub.name}"${store}؟`)) return;
    this.runDelete(sub.id);
  }

  private runDelete(id: number): void {
    this.deletingId.set(id);

    this.categoryService.delete(id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toastService.show('تم حذف القسم بنجاح', 'success');
        this.refresh();
      },
      error: () => this.deletingId.set(null)
    });
  }

  // نحدّث القائمة هنا وقائمة الأقسام في الـ Navbar
  private refresh(): void {
    this.loadCategories();
    this.categoryService.loadTree();
  }
}
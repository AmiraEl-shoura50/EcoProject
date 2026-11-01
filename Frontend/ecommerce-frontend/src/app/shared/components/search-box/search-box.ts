import { Component, DestroyRef, ElementRef, HostListener, OnInit, inject, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, debounceTime, distinctUntilChanged, map, of, switchMap } from 'rxjs';
import { LucideAngularModule, Search } from 'lucide-angular';
import { ProductService } from '../../../core/services/product';
import { Product } from '../../../core/models/product.model';

@Component({
  selector: 'app-search-box',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule],
  templateUrl: './search-box.html',
  styleUrl: './search-box.css'
})
export class SearchBox implements OnInit {
  readonly SearchIcon = Search;

  // بيتبعت لما المستخدم يروح لصفحة (عشان الـ Navbar يقفل قائمة الموبايل)
  searched = output<void>();

  term = signal<string>('');
  suggestions = signal<Product[]>([]);
  isSearching = signal<boolean>(false);
  isOpen = signal<boolean>(false);

  private input$ = new Subject<string>();
  private destroyRef = inject(DestroyRef);

  constructor(
    private productService: ProductService,
    private router: Router,
    private host: ElementRef<HTMLElement>
  ) {}

  ngOnInit(): void {
    this.input$
      .pipe(
        map(value => value.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(term => {
          if (term.length < 2) return of(null);

          return this.productService
            .search({ searchTerm: term, pageNumber: 1, pageSize: 5 })
            .pipe(catchError(() => of(null)));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        this.isSearching.set(false);
        this.suggestions.set(response?.data.items ?? []);
      });
  }

  onInput(value: string): void {
    this.term.set(value);

    const hasEnoughChars = value.trim().length >= 2;
    this.isOpen.set(hasEnoughChars);
    this.isSearching.set(hasEnoughChars);
    if (!hasEnoughChars) this.suggestions.set([]);

    this.input$.next(value);
  }

  onFocus(): void {
    if (this.term().trim().length >= 2) this.isOpen.set(true);
  }

  submit(): void {
    const term = this.term().trim();
    if (!term) return;

    this.isOpen.set(false);
    this.router.navigate(['/search'], { queryParams: { q: term } });
    this.searched.emit();
  }

  closeAll(): void {
    this.isOpen.set(false);
    this.term.set('');
    this.suggestions.set([]);
    this.searched.emit();
  }

  // قفل القائمة لما المستخدم يضغط برا المكوّن
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.isOpen.set(false);
    }
  }
}
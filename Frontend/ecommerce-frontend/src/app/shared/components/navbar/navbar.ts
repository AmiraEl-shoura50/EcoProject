import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, ShoppingCart, Heart, User, LogOut, Search, Menu, X, ChevronDown } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth';
import { CategoryService } from '../../../core/services/category';
import { Category } from '../../../core/models/category.model';
import { CartService } from '../../../core/services/cart';
import { WishlistService } from '../../../core/services/wishlist';
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar implements OnInit {
  // ---- State ----
  isMenuOpen = false;
  isMobileMenuOpen = false;
  mobileExpandedCategoryId: number | null = null;
  hoveredCategoryId: number | null = null;
  isMoreCategoriesOpen = false;
  categories = signal<Category[]>([]);

  private readonly visibleCategoriesCount = 4;

  visibleCategories = computed(() =>
    this.categories().slice(0, this.visibleCategoriesCount)
  );

  remainingCategories = computed(() =>
    this.categories().slice(this.visibleCategoriesCount)
  );

  // ---- Icons ----
  readonly CartIcon = ShoppingCart;
  readonly HeartIcon = Heart;
  readonly UserIcon = User;
  readonly LogOutIcon = LogOut;
  readonly SearchIcon = Search;
  readonly MenuIcon = Menu;
  readonly CloseIcon = X;
  readonly ChevronDownIcon = ChevronDown;

  constructor(
    public authService: AuthService,
    private categoryService: CategoryService,
     public cartService: CartService,
  public wishlistService: WishlistService
  ) {}

  ngOnInit(): void {
    this.categoryService.getTree().subscribe({
      next: (response) => {
        this.categories.set(response.data);
      }
    });
  }

  // ---- Desktop user dropdown ----
  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu(): void {
    this.isMenuOpen = false;
  }

  // ---- Desktop mega menu ----
  showSubMenu(categoryId: number): void {
    this.hoveredCategoryId = categoryId;
  }

  hideSubMenu(): void {
    this.hoveredCategoryId = null;
  }

  // ---- More categories dropdown ----
  toggleMoreCategories(): void {
    this.isMoreCategoriesOpen = !this.isMoreCategoriesOpen;
  }

  closeMoreCategories(): void {
    this.isMoreCategoriesOpen = false;
  }

  // ---- Mobile menu ----
  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
    if (!this.isMobileMenuOpen) {
      this.mobileExpandedCategoryId = null;
    }
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
    this.mobileExpandedCategoryId = null;
  }

  toggleMobileCategory(categoryId: number): void {
    this.mobileExpandedCategoryId =
      this.mobileExpandedCategoryId === categoryId ? null : categoryId;
  }

  logout(): void {
    this.authService.logout();
    this.closeMenu();
  }
}
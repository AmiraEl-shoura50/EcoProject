import { Component, OnInit,  computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink,Router } from '@angular/router';
import { LucideAngularModule, ShoppingCart, Heart, User, LogOut,  Menu, X, ChevronDown ,Percent , Bell, CheckCheck, Trash2  } from 'lucide-angular';
import { SearchBox } from '../search-box/search-box';
import { AuthService } from '../../../core/services/auth';
import { CategoryService } from '../../../core/services/category';

import { CartService } from '../../../core/services/cart';
import { WishlistService } from '../../../core/services/wishlist';
import { ProductService } from '../../../core/services/product';
import { NotificationService } from '../../../core/services/notification';
import { AppNotification } from '../../../core/models/notification.model';
import { TimeAgoPipe } from '../../pipes/time-ago.pipe';
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule, SearchBox, TimeAgoPipe],
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
  isNotificationsOpen = false;

  get categories() {
    return this.categoryService.tree;
  }

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
  readonly MenuIcon = Menu;
  readonly CloseIcon = X;
  readonly ChevronDownIcon = ChevronDown;
  readonly PercentIcon = Percent;
  readonly BellIcon = Bell;
  readonly CheckAllIcon = CheckCheck;
  readonly TrashIcon = Trash2;
  constructor(
    public authService: AuthService,
    private categoryService: CategoryService,
    public cartService: CartService,
    public wishlistService: WishlistService,
    public productService: ProductService,
    public notificationService: NotificationService ,
    private router: Router

  ) {}

  ngOnInit(): void {
    this.categoryService.loadTree();
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
  toggleNotifications(): void {
  this.isNotificationsOpen = !this.isNotificationsOpen;
  if (this.isNotificationsOpen) this.isMenuOpen = false;
}

closeNotifications(): void {
  this.isNotificationsOpen = false;
}

openNotification(notification: AppNotification): void {
  this.closeNotifications();
  this.notificationService.markAsRead(notification).subscribe();
  this.router.navigateByUrl(this.notificationService.targetFor(notification, this.authService.userRole()));
}

markAllRead(): void {
  this.notificationService.markAllAsRead().subscribe();
}
deleteNotification(notification: AppNotification): void {
  this.notificationService.delete(notification).subscribe();
}
}
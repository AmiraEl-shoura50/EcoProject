import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product';
import { Product } from '../../../core/models/product.model';
import { ProductCard } from '../../../shared/components/product-card/product-card';
// (عدّل عدد الـ ../ حسب مكان كل ملف بالظبط)

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, ProductCard],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit {
  products = signal<Product[]>([]);
  isLoading = signal<boolean>(true);
  skeletonItems = [1, 2, 3, 4, 5, 6, 7, 8];

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  private loadProducts(): void {
    this.isLoading.set(true);

    this.productService.search({ pageNumber: 1, pageSize: 12 }).subscribe({
      next: (response) => {
        this.products.set(response.data.items);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}
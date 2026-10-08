import { Injectable, signal ,computed} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Wishlist } from '../models/wishlist.model';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private apiUrl = `${environment.apiUrl}/wishlist`;

  wishlist = signal<Wishlist | null>(null);
  itemsCount = computed(() => this.wishlist()?.items.length ?? 0); 
  constructor(private http: HttpClient) {}

  loadWishlist(): void {
    this.http.get<ApiResponse<Wishlist>>(this.apiUrl).subscribe({
      next: (response) => this.wishlist.set(response.data)
    });
  }

  addItem(productId: number) {
    return this.http.post<ApiResponse<null>>(`${this.apiUrl}/items`, { productId }).pipe(
      tap(() => this.loadWishlist())
    );
  }

  removeItem(productId: number) {
    return this.http.delete<ApiResponse<null>>(`${this.apiUrl}/items/${productId}`).pipe(
      tap(() => this.loadWishlist())
    );
  }

  isInWishlist(productId: number): boolean {
    return this.wishlist()?.items.some(i => i.productId === productId) ?? false;
  }
}
import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AdminCategory, Category, SellerCategory } from '../models/category.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private apiUrl = `${environment.apiUrl}/categories`;

  // شجرة الأقسام مخزنة هنا ومشتركة (الـ Navbar وصفحات البائع بيقروا منها)
  tree = signal<Category[]>([]);
  treeLoaded = signal<boolean>(false); // ✅ جديد: عشان نفرّق بين "لسه بيتحمّل" و"القسم مش موجود"


  constructor(private http: HttpClient) {}

loadTree(): void {
    this.getTree().subscribe({
      next: (response) => {
        this.tree.set(response.data);
        this.treeLoaded.set(true);
      }
    });
  }

  getTree(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(`${this.apiUrl}/tree`);
  }

  getById(id: number): Observable<ApiResponse<Category>> {
    return this.http.get<ApiResponse<Category>>(`${this.apiUrl}/${id}`);
  }

  // ---- خاص بالبائع ----
  getMine(): Observable<ApiResponse<SellerCategory[]>> {
    return this.http.get<ApiResponse<SellerCategory[]>>(`${this.apiUrl}/mine`);
  }
  // ---- خاص بالأدمن ----
  getAdminTree(): Observable<ApiResponse<AdminCategory[]>> {
    return this.http.get<ApiResponse<AdminCategory[]>>(`${this.apiUrl}/admin`);
  }

  create(data: FormData): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.apiUrl, data);
  }

  update(id: number, data: FormData): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
    setVisibility(id: number, isHidden: boolean): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/${id}/visibility`, { isHidden });
  }
}
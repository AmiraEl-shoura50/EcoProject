import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PaymentGroup, PaymentProofInfo, PendingPayment } from '../models/payment.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentGroupService {
  private apiUrl = `${environment.apiUrl}/payment-groups`;

  constructor(private http: HttpClient) {}

  getById(id: number): Observable<ApiResponse<PaymentGroup>> {
    return this.http.get<ApiResponse<PaymentGroup>>(`${this.apiUrl}/${id}`);
  }

  getPending(): Observable<ApiResponse<PendingPayment[]>> {
    return this.http.get<ApiResponse<PendingPayment[]>>(`${this.apiUrl}/pending`);
  }

  submitProof(groupId: number, image: File, transferReference?: string): Observable<ApiResponse<PaymentProofInfo>> {
    // الأسماء بحرف كبير عشان تطابق SubmitPaymentProofDto
    const formData = new FormData();
    formData.append('Image', image);
    if (transferReference) formData.append('TransferReference', transferReference);

    return this.http.post<ApiResponse<PaymentProofInfo>>(`${this.apiUrl}/${groupId}/proof`, formData);
  }
}
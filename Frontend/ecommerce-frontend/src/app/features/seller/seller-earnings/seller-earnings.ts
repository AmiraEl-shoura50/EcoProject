import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Download } from 'lucide-angular';
import { SettlementService } from '../../../core/services/settlement';
import { PAYOUT_LABELS, SellerEarnings, Settlement } from '../../../core/models/settlement.model';
import { downloadBlob } from '../../../core/utils/download';

@Component({
  selector: 'app-seller-earnings',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule],
  templateUrl: './seller-earnings.html',
  styleUrl: './seller-earnings.css'
})
export class SellerEarningsPage implements OnInit {
  readonly DownloadIcon = Download;

  earnings = signal<SellerEarnings | null>(null);
  isLoading = signal<boolean>(true);

  history = signal<Settlement[]>([]);
  isLoadingHistory = signal<boolean>(true);
  historyTotal = signal<number>(0);
  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  exportingId = signal<number | null>(null);

  private readonly pageSize = 10;

  constructor(private settlementService: SettlementService) {}

  ngOnInit(): void {
    this.settlementService.getEarnings().subscribe({
      next: (response) => {
        this.earnings.set(response.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });

    this.loadHistory();
  }

  payoutLabel(method: string | null): string {
    return method ? (PAYOUT_LABELS[method] ?? method) : 'غير محدد';
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;

    this.currentPage.set(page);
    this.loadHistory();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  exportSettlement(settlement: Settlement): void {
    this.exportingId.set(settlement.id);

    this.settlementService.exportMySettlement(settlement.id).subscribe({
      next: (blob) => {
        this.exportingId.set(null);
        downloadBlob(blob, `تسوية-${settlement.id}.xlsx`);
      },
      error: () => this.exportingId.set(null)
    });
  }

  private loadHistory(): void {
    this.isLoadingHistory.set(true);

    this.settlementService.getMySettlements(this.currentPage(), this.pageSize).subscribe({
      next: (response) => {
        const result = response.data;
        this.history.set(result.items);
        this.historyTotal.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.isLoadingHistory.set(false);
      },
      error: () => this.isLoadingHistory.set(false)
    });
  }
}
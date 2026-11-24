import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Copy, Download, Pencil, X } from 'lucide-angular';
import { SettlementService } from '../../../core/services/settlement';
import { ToastService } from '../../../core/services/toast';
import { PAYOUT_LABELS, SellerBalance, Settlement } from '../../../core/models/settlement.model';
import { monthStartLocal, todayLocal } from '../../../core/utils/dates';
import { downloadBlob } from '../../../core/utils/download';

type Tab = 'balances' | 'history';

@Component({
  selector: 'app-admin-settlements',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LucideAngularModule],
  templateUrl: './admin-settlements.html',
  styleUrl: './admin-settlements.css'
})
export class AdminSettlements implements OnInit {
  readonly CopyIcon = Copy;
  readonly DownloadIcon = Download;
  readonly EditIcon = Pencil;
  readonly CloseIcon = X;

  tab = signal<Tab>('balances');

  // ---- الأرصدة ----
  cutoff = signal<string>(todayLocal());
  balances = signal<SellerBalance[]>([]);
  isLoading = signal<boolean>(true);
  showAll = signal<boolean>(false);

  // البائعين اللي ليهم مستحقات أو لسه عمولتهم متحددتش (الباقي بيظهر بـ "عرض الكل")
  visibleBalances = computed(() =>
    this.showAll()
      ? this.balances()
      : this.balances().filter(b => b.ordersCount > 0 || b.commissionPercent === null)
  );

  totalNet = computed(() =>
    Math.round(this.visibleBalances().reduce((sum, b) => sum + b.netAmount, 0) * 100) / 100
  );

  // ---- تعديل العمولة ----
  editingCommissionId = signal<number | null>(null);
  commissionValue = signal<number | null>(null);
  isSavingCommission = signal<boolean>(false);

  // ---- نافذة التحويل ----
  settleTarget = signal<SellerBalance | null>(null);
  adjustment = signal<number>(0);
  adjustmentNote = '';
  reference = '';
  isSettling = signal<boolean>(false);

  finalNet = computed(() => {
    const target = this.settleTarget();
    return target ? Math.round((target.netAmount + this.adjustment()) * 100) / 100 : 0;
  });

  // ---- تقرير الفترة ----
  reportFrom = signal<string>(monthStartLocal());
  reportTo = signal<string>(todayLocal());
  isExportingReport = signal<boolean>(false);

  // ---- السجل ----
  history = signal<Settlement[]>([]);
  isLoadingHistory = signal<boolean>(false);
  historyTotal = signal<number>(0);
  historyPage = signal<number>(1);
  historyTotalPages = signal<number>(1);
  exportingId = signal<number | null>(null);

  private readonly historyPageSize = 10;

  constructor(
    private settlementService: SettlementService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadBalances();
  }

  payoutLabel(method: string | null): string {
    return method ? (PAYOUT_LABELS[method] ?? method) : 'غير محدد';
  }

  setTab(tab: Tab): void {
    this.tab.set(tab);

    if (tab === 'history' && this.history().length === 0) this.loadHistory();
  }

  // ================= الأرصدة =================

  onCutoffChange(value: string): void {
    if (!value) return;

    this.cutoff.set(value);
    this.loadBalances();
  }

  loadBalances(silent = false): void {
    if (!silent) this.isLoading.set(true);

    this.settlementService.getBalances(this.cutoff()).subscribe({
      next: (response) => {
        this.balances.set(response.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  copy(text: string): void {
    navigator.clipboard.writeText(text).then(
      () => this.toastService.show('تم النسخ', 'success'),
      () => this.toastService.show('تعذر النسخ', 'error')
    );
  }

  // ---- العمولة ----
  startEditCommission(balance: SellerBalance): void {
    this.editingCommissionId.set(balance.sellerId);
    this.commissionValue.set(balance.commissionPercent);
  }

  cancelEditCommission(): void {
    this.editingCommissionId.set(null);
  }

  onCommissionInput(raw: string): void {
    const text = raw.trim();
    this.commissionValue.set(text === '' ? null : Number(text));
  }

  canSaveCommission(): boolean {
    const value = this.commissionValue();
    return value !== null && Number.isFinite(value) && value >= 0 && value <= 100;
  }

  saveCommission(balance: SellerBalance): void {
    const value = this.commissionValue();
    if (!this.canSaveCommission() || value === null) return;

    this.isSavingCommission.set(true);

    this.settlementService.setCommission(balance.sellerId, Math.round(value * 100) / 100).subscribe({
      next: () => {
        this.isSavingCommission.set(false);
        this.editingCommissionId.set(null);
        this.toastService.show('تم حفظ نسبة العمولة', 'success');
        this.loadBalances(true); // الأرقام بتتغيّر بتغيّر النسبة
      },
      error: () => this.isSavingCommission.set(false)
    });
  }

  // ---- التحويل ----
  openSettle(balance: SellerBalance): void {
    this.settleTarget.set(balance);
    this.adjustment.set(0);
    this.adjustmentNote = '';
    this.reference = '';
  }

  closeSettle(): void {
    if (!this.isSettling()) this.settleTarget.set(null);
  }

  onAdjustmentInput(raw: string): void {
    const value = Number(raw);
    this.adjustment.set(Number.isFinite(value) ? Math.round(value * 100) / 100 : 0);
  }

  canConfirmSettle(): boolean {
    if (this.finalNet() <= 0) return false;
    return this.adjustment() === 0 || this.adjustmentNote.trim().length > 0;
  }

  confirmSettle(): void {
    const target = this.settleTarget();
    if (!target || !this.canConfirmSettle()) return;

    const question =
      `هل حوّلت فعلًا ${this.finalNet()} جنيه إلى "${target.storeName}"` +
      ` (${this.payoutLabel(target.payoutMethod)}: ${target.payoutAccount})؟\n` +
      `هيتم قفل ${target.ordersCount} طلب ومش هيتحسبوا تاني.`;

    if (!confirm(question)) return;

    this.isSettling.set(true);

    this.settlementService.settle(target.sellerId, {
      cutoff: this.cutoff(),
      expectedOrdersCount: target.ordersCount,
      adjustmentAmount: this.adjustment(),
      adjustmentNote: this.adjustment() === 0 ? null : this.adjustmentNote.trim(),
      reference: this.reference.trim() || null
    }).subscribe({
      next: () => {
        this.isSettling.set(false);
        this.settleTarget.set(null);
        this.toastService.show('تم تسجيل التحويل وإشعار البائع', 'success');
        this.loadBalances(true);
        this.history.set([]); // السجل هيتحمّل من جديد لما تفتحيه
      },
      error: () => {
        this.isSettling.set(false);
        this.loadBalances(true); // لو الأرقام اتغيّرت نعرض الجديد
      }
    });
  }

  // ================= تقرير الفترة =================

  exportReport(): void {
    const from = this.reportFrom();
    const to = this.reportTo();

    if (!from || !to || to < from) {
      this.toastService.show('اختر فترة صحيحة (تاريخ النهاية بعد البداية)', 'error');
      return;
    }

    this.isExportingReport.set(true);

    this.settlementService.exportPeriod(from, to).subscribe({
      next: (blob) => {
        this.isExportingReport.set(false);
        downloadBlob(blob, `تقرير-المبيعات-${from}_${to}.xlsx`);
      },
      error: () => this.isExportingReport.set(false)
    });
  }

  // ================= السجل =================

  loadHistory(): void {
    this.isLoadingHistory.set(true);

    this.settlementService.getHistory(this.historyPage(), this.historyPageSize).subscribe({
      next: (response) => {
        const result = response.data;
        this.history.set(result.items);
        this.historyTotal.set(result.totalCount);
        this.historyTotalPages.set(result.totalPages);
        this.isLoadingHistory.set(false);
      },
      error: () => this.isLoadingHistory.set(false)
    });
  }

  goToHistoryPage(page: number): void {
    if (page < 1 || page > this.historyTotalPages()) return;

    this.historyPage.set(page);
    this.loadHistory();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  exportSettlement(settlement: Settlement): void {
    this.exportingId.set(settlement.id);

    this.settlementService.exportSettlement(settlement.id).subscribe({
      next: (blob) => {
        this.exportingId.set(null);
        downloadBlob(blob, `تسوية-${settlement.id}-${settlement.storeName}.xlsx`);
      },
      error: () => this.exportingId.set(null)
    });
  }
}
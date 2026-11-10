import { Component, computed, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DailySales } from '../../../core/models/dashboard.model';

interface Bar {
  date: string;
  amount: number;
  height: number;
  label: string;
  tooltip: string;
}

@Component({
  selector: 'app-sales-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sales-chart.html',
  styleUrl: './sales-chart.css'
})
export class SalesChart {
  data = input<DailySales[]>([]);
  title = input<string>('المبيعات');

  readonly ranges = [7, 14, 30];
  range = signal<number>(14);

  private readonly dateFormat = new Intl.DateTimeFormat('ar-u-nu-latn', {
    weekday: 'long', day: 'numeric', month: 'long'
  });

  visible = computed(() => this.data().slice(-this.range()));

  total = computed(() =>
    Math.round(this.visible().reduce((sum, d) => sum + d.amount, 0) * 100) / 100
  );

  hasSales = computed(() => this.total() > 0);

  bars = computed<Bar[]>(() => {
    const items = this.visible();
    const max = Math.max(0, ...items.map(d => d.amount));
    const step = this.range() === 30 ? 5 : this.range() === 14 ? 2 : 1;

    return items.map((item, index) => {
      const date = new Date(`${item.date}T00:00:00`);
      const fromEnd = items.length - 1 - index;
      const showLabel = fromEnd % step === 0; // آخر يوم (النهارده) دايمًا ظاهر
      const day = date.getDate();

      return {
        date: item.date,
        amount: item.amount,
        height: max > 0 ? (item.amount / max) * 100 : 0,
        label: showLabel ? (day === 1 ? `${day}/${date.getMonth() + 1}` : `${day}`) : '',
        tooltip: `${this.dateFormat.format(date)}: ${item.amount} جنيه`
      };
    });
  });
}
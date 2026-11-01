import { Component, computed, input } from '@angular/core';
import { LucideAngularModule, Star } from 'lucide-angular';

@Component({
  selector: 'app-stars',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './stars.html',
  styleUrl: './stars.css'
})
export class Stars {
  readonly StarIcon = Star;
  readonly positions = [1, 2, 3, 4, 5];

  value = input<number>(0);
  size = input<number>(16);

  percent = computed(() => Math.max(0, Math.min(100, (this.value() / 5) * 100)));
}
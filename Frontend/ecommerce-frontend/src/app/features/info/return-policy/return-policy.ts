import { Component } from '@angular/core';
import { InfoPage } from '../../../shared/components/info-page/info-page';
import { SITE } from '../../../core/config/site-config';

@Component({
  selector: 'app-return-policy',
  standalone: true,
  imports: [InfoPage],
  templateUrl: './return-policy.html',
  styleUrl: './return-policy.css'
})
export class ReturnPolicy {
  readonly site = SITE;
}
import { Component } from '@angular/core';
import { InfoPage } from '../../../shared/components/info-page/info-page';
import { SITE } from '../../../core/config/site-config';

@Component({
  selector: 'app-shipping-policy',
  standalone: true,
  imports: [InfoPage],
  templateUrl: './shipping-policy.html',
  styleUrl: './shipping-policy.css'
})
export class ShippingPolicy {
  readonly site = SITE;
}
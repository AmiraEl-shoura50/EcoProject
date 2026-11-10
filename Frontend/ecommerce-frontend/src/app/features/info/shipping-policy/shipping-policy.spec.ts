import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingPolicy } from './shipping-policy';

describe('ShippingPolicy', () => {
  let component: ShippingPolicy;
  let fixture: ComponentFixture<ShippingPolicy>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShippingPolicy],
    }).compileComponents();

    fixture = TestBed.createComponent(ShippingPolicy);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

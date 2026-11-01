import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellerDiscounts } from './seller-discounts';

describe('SellerDiscounts', () => {
  let component: SellerDiscounts;
  let fixture: ComponentFixture<SellerDiscounts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SellerDiscounts],
    }).compileComponents();

    fixture = TestBed.createComponent(SellerDiscounts);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

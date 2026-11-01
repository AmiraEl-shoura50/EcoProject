import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellerProducts } from './seller-products';

describe('SellerProducts', () => {
  let component: SellerProducts;
  let fixture: ComponentFixture<SellerProducts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SellerProducts],
    }).compileComponents();

    fixture = TestBed.createComponent(SellerProducts);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

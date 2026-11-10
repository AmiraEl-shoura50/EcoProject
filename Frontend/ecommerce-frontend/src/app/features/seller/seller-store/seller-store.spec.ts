import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellerStore } from './seller-store';

describe('SellerStore', () => {
  let component: SellerStore;
  let fixture: ComponentFixture<SellerStore>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SellerStore],
    }).compileComponents();

    fixture = TestBed.createComponent(SellerStore);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

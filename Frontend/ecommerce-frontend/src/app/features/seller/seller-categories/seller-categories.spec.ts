import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellerCategories } from './seller-categories';

describe('SellerCategories', () => {
  let component: SellerCategories;
  let fixture: ComponentFixture<SellerCategories>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SellerCategories],
    }).compileComponents();

    fixture = TestBed.createComponent(SellerCategories);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

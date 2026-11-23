import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductGallery } from './product-gallery';

describe('ProductGallery', () => {
  let component: ProductGallery;
  let fixture: ComponentFixture<ProductGallery>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductGallery],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductGallery);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

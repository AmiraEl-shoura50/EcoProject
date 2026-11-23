import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GalleryEditor } from './gallery-editor';

describe('GalleryEditor', () => {
  let component: GalleryEditor;
  let fixture: ComponentFixture<GalleryEditor>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GalleryEditor],
    }).compileComponents();

    fixture = TestBed.createComponent(GalleryEditor);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

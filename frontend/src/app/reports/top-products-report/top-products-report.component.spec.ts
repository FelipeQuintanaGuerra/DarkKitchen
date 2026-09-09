import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TopProductsReportComponent } from './top-products-report.component';

describe('TopProductsReportComponent', () => {
  let component: TopProductsReportComponent;
  let fixture: ComponentFixture<TopProductsReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TopProductsReportComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TopProductsReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

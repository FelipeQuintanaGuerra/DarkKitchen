import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilteredOrdersComponent } from './filtered-orders.component';

describe('FilteredOrdersComponent', () => {
  let component: FilteredOrdersComponent;
  let fixture: ComponentFixture<FilteredOrdersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FilteredOrdersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FilteredOrdersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

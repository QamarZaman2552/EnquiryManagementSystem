import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Enquiries } from './enquiries';

describe('Enquiries', () => {
  let component: Enquiries;
  let fixture: ComponentFixture<Enquiries>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Enquiries],
    }).compileComponents();

    fixture = TestBed.createComponent(Enquiries);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

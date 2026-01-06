import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService, PremiumVsPayoutDto } from '../../core/services/report.service';

@Component({
  selector: 'app-premium-payout-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="report-card">
      <h3>Premium vs Payout Overview</h3>
      <div *ngIf="loading">
        Loading...
      </div>
      <div *ngIf="!loading && data" class="stats-grid">
        <div class="stat-item">
          <div class="stat-label">Total Premiums</div>
          <div class="stat-value premium">{{ data.totalPremiumsCollected | currency:'INR':'symbol' }}</div>
        </div>
        <div class="stat-item">
          <div class="stat-label">Total Payouts</div>
          <div class="stat-value payout">{{ data.totalClaimsPaid | currency:'INR':'symbol' }}</div>
        </div>
        <div class="stat-item highlight">
          <div class="stat-label">Remaining Coverage</div>
          <div class="stat-value" [class.profit]="data.remainingCoverage >= 0" [class.loss]="data.remainingCoverage < 0">
            {{ data.remainingCoverage | currency:'INR':'symbol' }}
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .report-card { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); margin-bottom: 20px; }
    .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 20px; }
    .stat-item { text-align: center; padding: 10px; border-radius: 8px; background: #f9fafb; }
    .stat-label { font-size: 0.9em; color: #6b7280; margin-bottom: 5px; }
    .stat-value { font-size: 1.5em; font-weight: bold; }
    .premium { color: #059669; }
    .payout { color: #d97706; }
    .highlight { background: #eff6ff; }
    .profit { color: #16a34a; }
    .loss { color: #dc2626; }
  `]
})
export class PremiumPayoutChartComponent implements OnInit {
  data: PremiumVsPayoutDto | null = null;
  loading = true;

  constructor(private reportService: ReportService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading = true;

    this.reportService.getPremiumVsPayout().subscribe({
      next: (data) => {
        this.data = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}

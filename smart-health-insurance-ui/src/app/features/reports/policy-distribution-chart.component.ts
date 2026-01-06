import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService, PolicyDistributionDto } from '../../core/services/report.service';

@Component({
  selector: 'app-policy-distribution-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="report-card">
      <h3>Policies by Plan & Status</h3>
      <div *ngIf="loading">
        Loading...
      </div>
      <div *ngIf="!loading && data.length === 0">No data available</div>
      
      <div *ngIf="!loading && data.length > 0" class="chart-container">
        <div *ngFor="let item of data" class="bar-container">
          <div class="label-row">
            <span class="label">{{ item.policyType }} ({{ item.status }})</span>
            <span class="count">{{ item.count }}</span>
          </div>
          <div class="bar-bg">
            <div class="bar" 
                 [style.width.%]="(item.count / maxCount) * 100"
                 [ngClass]="item.status.toLowerCase()">
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .report-card { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); margin-bottom: 20px; }
    .chart-container { display: flex; flex-direction: column; gap: 15px; margin-top: 10px; }
    .bar-container { width: 100%; }
    .label-row { display: flex; justify-content: space-between; margin-bottom: 5px; font-size: 0.9em; font-weight: 500; color: #374151; }
    .bar-bg { width: 100%; height: 12px; background-color: #f3f4f6; border-radius: 6px; overflow: hidden; }
    .bar { height: 100%; border-radius: 6px; transition: width 0.8s ease-out; }
    .active { background-color: #10b981; } /* Green */
    .expired { background-color: #ef4444; } /* Red */
    .suspended { background-color: #f59e0b; } /* Orange */
  `]
})
export class PolicyDistributionChartComponent implements OnInit {
  data: PolicyDistributionDto[] = [];
  maxCount = 1;
  loading = true;

  constructor(private reportService: ReportService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading = true;

    this.reportService.getPolicyDistribution().subscribe({
      next: (data) => {
        this.data = data;
        this.maxCount = Math.max(...data.map(i => i.count), 1);
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

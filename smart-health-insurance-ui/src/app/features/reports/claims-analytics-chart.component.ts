import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService, ClaimAnalyticsDto } from '../../core/services/report.service';

@Component({
  selector: 'app-claims-analytics-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="report-card">
      <h3>Claims Analytics</h3>
      <div *ngIf="loading">Loading...</div>
      <div *ngIf="!loading && data.length === 0">No claims data available</div>

      <div *ngIf="!loading && data.length > 0" class="dashboard-grid">
        <!-- Pie Chart Section -->
        <div class="chart-section">
          <h4>Status Distribution</h4>
          <div class="pie-chart-wrapper">
            <div class="pie-chart" [style.background]="pieChartGradient"></div>
            <div class="legend">
              <div *ngFor="let item of pieLegend" class="legend-item">
                <span class="dot" [style.background-color]="item.color"></span>
                <span>{{ item.label }} ({{ item.percentage }}%)</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Table Section -->
        <div class="table-section">
          <h4>Details by Hospital</h4>
          <table class="analytics-table">
            <thead>
              <tr>
                <th>Hospital</th>
                <th>Status</th>
                <th>Count</th>
                <th class="amount-col">Amount</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let item of data">
                <td class="hospital-name">{{ item.hospitalName }}</td>
                <td>
                  <span class="status-badge" [ngClass]="item.status.toLowerCase()">{{ item.status }}</span>
                </td>
                <td>{{ item.count }}</td>
                <td class="amount-cell">
                  <div class="amount-wrapper">
                    <div class="amount-row">
                      <span class="label">Claimed:</span>
                      <span class="amount-text muted">{{ item.totalAmount | currency:'INR':'symbol':'1.0-0' }}</span>
                    </div>
                    <div class="amount-row">
                      <span class="label">Approved:</span>
                      <span class="amount-text bold" 
                            [class.text-green]="item.totalApprovedAmount > 0"
                            [class.text-red]="item.totalApprovedAmount === 0 && item.status === 'Rejected'">
                        {{ item.totalApprovedAmount | currency:'INR':'symbol':'1.0-0' }}
                      </span>
                    </div>
                    
                    <div class="progress-bg">
                      <div class="progress-bar" 
                           [style.width.%]="(item.totalAmount / maxAmount) * 100"
                           [class.bar-approved]="item.status === 'Approved' || item.status === 'Paid'"
                           [class.bar-rejected]="item.status === 'Rejected'"
                           [class.bar-pending]="item.status === 'Submitted' || item.status === 'Pending'">
                      </div>
                    </div>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .report-card { background: white; padding: 24px; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); margin-bottom: 24px; }
    h3 { margin-top: 0; margin-bottom: 20px; font-size: 1.1em; color: #111827; }
    h4 { margin: 0 0 15px 0; font-size: 0.95em; color: #4b5563; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; }
    
    .dashboard-grid { display: flex; gap: 30px; flex-wrap: wrap; }
    .chart-section { flex: 0 0 250px; display: flex; flex-direction: column; align-items: center; }
    .table-section { flex: 1; min-width: 300px; overflow-x: auto; }

    /* Pie Chart Styles */
    .pie-chart-wrapper { display: flex; flex-direction: column; align-items: center; gap: 15px; }
    .pie-chart { width: 160px; height: 160px; border-radius: 50%; box-shadow: 0 2px 4px rgba(0,0,0,0.1); transition: transform 0.3s; }
    .pie-chart:hover { transform: scale(1.05); }
    .legend { display: flex; flex-direction: column; gap: 8px; width: 100%; }
    .legend-item { display: flex; align-items: center; gap: 8px; font-size: 0.85em; color: #374151; }
    .dot { width: 10px; height: 10px; border-radius: 50%; }

    /* Table Styles */
    .analytics-table { width: 100%; border-collapse: separate; border-spacing: 0; }
    .analytics-table th { text-align: left; padding: 10px; border-bottom: 2px solid #f3f4f6; color: #6b7280; font-size: 0.75em; text-transform: uppercase; font-weight: 600; }
    .analytics-table td { text-align: left; padding: 12px 10px; border-bottom: 1px solid #f3f4f6; vertical-align: middle; }
    .hospital-name { font-weight: 500; color: #1f2937; font-size: 0.9em; }
    .status-badge { padding: 4px 8px; border-radius: 4px; font-size: 0.7em; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
    
    .approved, .paid { background: #dcfce7; color: #15803d; }
    .rejected { background: #fee2e2; color: #b91c1c; }
    .submitted, .pending { background: #dbeafe; color: #1d4ed8; }
    
    .amount-col { width: 35%; }
    .amount-cell { min-width: 140px; }
    .amount-wrapper { display: flex; flex-direction: column; gap: 4px; }
    .amount-row { display: flex; justify-content: space-between; align-items: center; font-size: 0.8em; }
    .label { color: #6b7280; font-weight: 500; }
    .amount-text { font-family: 'Roboto Mono', monospace; }
    .muted { color: #9ca3af; text-decoration: line-through; }
    .bold { font-weight: 700; font-size: 1.1em; }
    .text-green { color: #059669; }
    .text-red { color: #dc2626; }
    .progress-bg { width: 100%; height: 4px; background-color: #f3f4f6; border-radius: 2px; overflow: hidden; }
    .progress-bar { height: 100%; border-radius: 2px; }
    .bar-approved { background-color: #10b981; }
    .bar-rejected { background-color: #ef4444; }
    .bar-pending { background-color: #3b82f6; }
  `]
})
export class ClaimsAnalyticsChartComponent implements OnInit {
  data: ClaimAnalyticsDto[] = [];
  maxAmount = 1;
  loading = true;

  // Pie Chart Props
  pieChartGradient = '';
  pieLegend: { label: string, color: string, percentage: number }[] = [];

  constructor(private reportService: ReportService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading = true;

    this.reportService.getClaimsAnalytics().subscribe({
      next: (data) => {
        this.data = data;
        this.maxAmount = Math.max(...data.map(i => i.totalAmount), 1);
        this.calculatePieChart();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  calculatePieChart() {
    // Group by status
    const statusMap = new Map<string, number>();
    let totalCount = 0;

    this.data.forEach(item => {
      const current = statusMap.get(item.status) || 0;
      statusMap.set(item.status, current + item.count);
      totalCount += item.count;
    });

    const colors: { [key: string]: string } = {
      'Paid': '#10b981', // Green
      'Approved': '#10b981',
      'Rejected': '#ef4444', // Red
      'Submitted': '#3b82f6', // Blue
      'Pending': '#3b82f6',
      'default': '#9ca3af'
    };

    // Calculate Segments
    let currentDeg = 0;
    const gradientParts: string[] = [];
    this.pieLegend = [];

    statusMap.forEach((count, status) => {
      const percentage = (count / totalCount) * 100;
      const deg = (count / totalCount) * 360;
      const color = colors[status] || colors['default'];

      gradientParts.push(`${color} ${currentDeg}deg ${currentDeg + deg}deg`);

      this.pieLegend.push({
        label: status,
        color: color,
        percentage: Math.round(percentage)
      });

      currentDeg += deg;
    });

    this.pieChartGradient = `conic-gradient(${gradientParts.join(', ')})`;
  }
}

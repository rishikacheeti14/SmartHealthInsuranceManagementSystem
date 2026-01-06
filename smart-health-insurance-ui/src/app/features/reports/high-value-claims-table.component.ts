import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService, HighValueClaimDto } from '../../core/services/report.service';

@Component({
  selector: 'app-high-value-claims-table',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="report-card">
      <div class="card-header">
        <h3>High Value Claims</h3>
        <span class="subtitle">> ₹100,000</span>
      </div>
      
      <div *ngIf="loading">Loading...</div>
      <div *ngIf="!loading && data.length === 0">No high value claims found</div>

      <div *ngIf="!loading && data.length > 0" class="table-container">
        <table class="claims-table">
          <thead>
            <tr>
              <th>Claim #</th>
              <th>Amount</th>
              <th>Hospital</th>
              <th>Diagnosis</th>
              <th>Date</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of data">
              <td class="claim-id">{{ item.claimNumber }}</td>
              <td class="amount">{{ item.claimAmount | currency:'INR':'symbol' }}</td>
              <td class="hospital">{{ item.hospitalName }}</td>
              <td class="diagnosis">{{ item.diagnosis }}</td>
              <td class="date">{{ item.submittedAt | date:'mediumDate' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .report-card { background: white; padding: 24px; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -1px rgba(0,0,0,0.06); margin-bottom: 24px; }
    .card-header { display: flex; align-items: baseline; gap: 10px; margin-bottom: 20px; }
    h3 { margin: 0; color: #111827; font-size: 1.125rem; font-weight: 600; }
    .subtitle { color: #6b7280; font-size: 0.875rem; font-weight: 500; }
    
    .table-container { overflow-x: auto; }
    .claims-table { width: 100%; border-collapse: separate; border-spacing: 0; }
    .claims-table th { text-align: left; padding: 12px 16px; border-bottom: 1px solid #e5e7eb; color: #6b7280; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; background: #f9fafb; }
    .claims-table td { padding: 16px; border-bottom: 1px solid #f3f4f6; color: #374151; font-size: 0.875rem; }
    .claims-table tr:last-child td { border-bottom: none; }
    .claims-table tr:hover td { background-color: #f9fafb; }
    
    .claim-id { font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace; color: #4b5563; }
    .amount { font-weight: 600; color: #dc2626; }
    .hospital { font-weight: 500; color: #111827; }
    .diagnosis { color: #4b5563; max-width: 250px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .date { color: #6b7280; white-space: nowrap; }
  `]
})
export class HighValueClaimsTableComponent implements OnInit {
  data: HighValueClaimDto[] = [];
  loading = true;

  constructor(private reportService: ReportService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading = true;

    this.reportService.getHighValueClaims().subscribe({
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

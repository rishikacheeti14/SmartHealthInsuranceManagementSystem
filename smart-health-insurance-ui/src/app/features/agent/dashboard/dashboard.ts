import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { ReportService } from '../../../core/services/report.service';

@Component({
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    BaseChartDirective
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {
  showReports = false;

  public donutChartData: ChartData<'doughnut'> = { labels: [], datasets: [{ data: [], backgroundColor: ['#0F172A', '#D97706', '#10B981'] }] };
  public donutChartType: ChartType = 'doughnut';
  public donutChartOptions: ChartConfiguration<'doughnut'>['options'] = { responsive: true, maintainAspectRatio: false };

  public policyStatusChartData: ChartData<'pie'> = { labels: [], datasets: [{ data: [], backgroundColor: ['#10B981', '#EF4444', '#6366F1', '#F59E0B'] }] };
  public policyStatusChartType: ChartType = 'pie';
  public policyStatusChartOptions: ChartConfiguration<'pie'>['options'] = { responsive: true, maintainAspectRatio: false };

  public claimsChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [{ data: [], label: 'Claims by Status', maxBarThickness: 40 }]
  };
  public claimsChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: { y: { beginAtZero: true } },
    plugins: { legend: { position: 'bottom' } }
  };

  public hospitalChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [{ data: [], label: 'Claims by Hospital', backgroundColor: '#6366F1', maxBarThickness: 40 }]
  };
  public hospitalChartOptions: ChartConfiguration<'bar'>['options'] = {
    indexAxis: 'y',
    responsive: true,
    maintainAspectRatio: false
  };

  public premiumChartData: ChartConfiguration<'bar'>['data'] = {
    labels: ['Premiums', 'Payouts'],
    datasets: []
  };

  public premiumChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: { stacked: true },
      y: { beginAtZero: true, stacked: true }
    },
    plugins: {
      legend: { display: true, position: 'top' },
      tooltip: {
        mode: 'index',
        intersect: false,
        filter: (item) => item.raw as number > 0 
      }
    }
  };

  private planColors: string[] = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6'];

  public highValueClaims: any[] = [];

  public totalPolicies = 0;
  public totalClaims = 0;
  public totalClaimsValue = 0;

  constructor(private router: Router, private reportService: ReportService) { }

  ngOnInit() {
    this.loadReports();
  }

  navigateTo(path: string) {
    this.router.navigate(['/agent', path]);
  }

  toggleReports() {
    this.showReports = !this.showReports;
  }

  loadReports() {
    this.reportService.getPolicyDistribution().subscribe(data => {

      const typeGroups = new Map<string, number>();

      const statusGroups = new Map<string, number>();

      this.totalPolicies = 0;

      data.forEach(d => {
        this.totalPolicies += d.count;

        const currentType = typeGroups.get(d.policyType) || 0;
        typeGroups.set(d.policyType, currentType + d.count);

        const statusLabel = this.getStatusLabel(d.status);
        const currentStatus = statusGroups.get(statusLabel) || 0;
        statusGroups.set(statusLabel, currentStatus + d.count);
      });

      this.donutChartData = {
        labels: Array.from(typeGroups.keys()),
        datasets: [{ data: Array.from(typeGroups.values()), backgroundColor: ['#0F172A', '#D97706', '#10B981', '#6366F1'] }]
      };

      this.policyStatusChartData = {
        labels: Array.from(statusGroups.keys()),
        datasets: [{ data: Array.from(statusGroups.values()), backgroundColor: ['#10B981', '#F59E0B', '#EF4444', '#cbd5e1'] }]
      };
    });


    this.reportService.getClaimsAnalytics().subscribe(data => {

      const statusGroups = new Map<string, number>();

      const hospitalGroups = new Map<string, number>();

      this.totalClaims = 0;
      this.totalClaimsValue = 0;

      data.forEach(d => {
        this.totalClaims += d.count;
        this.totalClaimsValue += d.totalAmount;

        const statusLabel = this.getClaimStatusLabel(d.status);
        const currentStatus = statusGroups.get(statusLabel) || 0;
        statusGroups.set(statusLabel, currentStatus + d.count);

        const hospital = d.hospitalName || 'Unknown';
        const currentHosp = hospitalGroups.get(hospital) || 0;
        hospitalGroups.set(hospital, currentHosp + d.count);
      });

      this.claimsChartData = {
        labels: Array.from(statusGroups.keys()),
        datasets: [{ data: Array.from(statusGroups.values()), label: 'Claims Count', backgroundColor: '#0F172A' }]
      };

      this.hospitalChartData = {
        labels: Array.from(hospitalGroups.keys()),
        datasets: [{ data: Array.from(hospitalGroups.values()), label: 'Claims by Hospital', backgroundColor: '#6366F1' }]
      };
    });

    this.reportService.getPremiumVsPayout().subscribe(data => {
      const datasets: any[] = [];
      const plans = data.planBreakdown || [];

      if (plans.length === 0 && (data.totalPremiumsCollected > 0 || data.totalClaimsPaid > 0)) {
        datasets.push({
          label: 'Total',
          data: [data.totalPremiumsCollected, data.totalClaimsPaid],
          backgroundColor: '#3B82F6'
        });
      } else {
        plans.forEach((plan: any, index: number) => {
          const color = this.planColors[index % this.planColors.length];
          datasets.push({
            label: plan.planName,
            data: [plan.totalPremium, plan.totalPayout],
            backgroundColor: color,
            hoverBackgroundColor: color,
            maxBarThickness: 40
          });
        });
      }

      this.premiumChartData = {
        labels: ['Premiums Paid', 'Claims Payout'],
        datasets: datasets
      };
    });

    this.reportService.getHighValueClaims().subscribe(data => {
      this.highValueClaims = data;
    });
  }

  getStatusLabel(status: any): string {
    const statuses = ['Active', 'Inactive', 'Cancelled', 'Expired'];
    return typeof status === 'number' ? (statuses[status] || 'Unknown') : status;
  }

  getClaimStatusLabel(status: any): string {
    const statuses = ['Submitted', 'Approved', 'Rejected', 'Paid', 'AdditionalInfoRequired'];
    return typeof status === 'number' ? (statuses[status] || 'Unknown') : status;
  }
}

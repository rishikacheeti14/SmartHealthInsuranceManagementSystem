import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ReportService } from '../../../core/services/report.service';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';

import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-customer-dashboard',
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
  firstName = 'User';
  showReport = false;

  public premiumChartData: ChartConfiguration<'bar'>['data'] = {
    labels: ['Premiums', 'Payouts'],
    datasets: []
  };

  public premiumChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: { stacked: true },
      y: {
        type: 'logarithmic',
        stacked: true,
        min: 1000 
      }
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

  private planColors: string[] = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6']; // Blue, Green, Yellow, Red, Purple

  constructor(private router: Router, private auth: AuthService, private reportService: ReportService) { }

  ngOnInit() {
    const user = this.auth.currentUser;
    if (user) {
      this.firstName = user.unique_name || 'Customer';
    }
    this.loadReports();
  }

  navigateTo(path: string) {
    this.router.navigate(['/customer', path]);
  }

  toggleReport() {
    this.showReport = !this.showReport;
  }

  loadReports() {
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
            maxBarThickness: 60
          });
        });
      }

      this.premiumChartData = {
        labels: ['Premiums Paid', 'Claims Payout'],
        datasets: datasets
      };
    });
  }
}

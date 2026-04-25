import { Component, OnInit } from '@angular/core';
import { ResultsService } from '../services/results.service';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class ReportComponent implements OnInit {
results: any[] = [];

pageSize = 10;
currentPage = 1;

  constructor(private service: ResultsService) {}

  ngOnInit() {
    this.service.getResults().subscribe(data => {
      this.results = data.sort((a, b) => a.runTime - b.runTime);
    });
  }

  get paginatedResults() {
  const start = (this.currentPage - 1) * this.pageSize;
  return this.results.slice(start, start + this.pageSize);
}

nextPage() {
  if (this.currentPage * this.pageSize < this.results.length) {
    this.currentPage++;
  }
}

prevPage() {
  if (this.currentPage > 1) {
    this.currentPage--;
  }
}
}
import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ReportComponent } from './report/report.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ReportComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('report-app');
}

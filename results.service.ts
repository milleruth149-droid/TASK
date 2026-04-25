import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../models/result.model';

@Injectable({
  providedIn: 'root'
})
export class ResultsService {

  private apiUrl = 'http://localhost:5000/results';// קישור לפרויקט פייטון 

  constructor(private http: HttpClient) {}

 getResults() {
  return this.http.get<any[]>(this.apiUrl);
}
}
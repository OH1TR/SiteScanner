import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-scheduled-work-items',
  templateUrl: './scheduled-work-items.component.html'
})
export class ScheduledWorkItems {
  public items: ScheduledWorkItem[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<ScheduledWorkItem[]>(baseUrl + 'data/ScheduledWorkItems').subscribe(result => {
      this.items = result;
    }, error => console.error(error));
  }
}

interface ScheduledWorkItem {
  id: string;
  created: Date;
  lastScheduledTime: Date;
  interval: number;
}

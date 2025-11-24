import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss',
    standalone: false
})
export class HomeComponent implements OnInit {
  ngOnInit(): void {
    document.title = "Deed - Home page"
  }
}

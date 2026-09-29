import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastContainer } from './shared/components/toast-container/toast-container';
import { Navbar } from './shared/components/navbar/navbar';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastContainer, Navbar],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Smart Shop');
}

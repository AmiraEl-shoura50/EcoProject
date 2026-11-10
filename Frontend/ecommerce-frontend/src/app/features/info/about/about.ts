import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InfoPage } from '../../../shared/components/info-page/info-page';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [RouterLink, InfoPage],
  templateUrl: './about.html',
  styleUrl: './about.css'
})
export class About {}
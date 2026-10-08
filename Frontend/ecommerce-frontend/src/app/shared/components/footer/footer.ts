import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Facebook, Instagram, Twitter, Mail, Phone, MapPin } from 'lucide-angular';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule],
  templateUrl: './footer.html',
  styleUrl: './footer.css'
})
export class Footer {
  currentYear = new Date().getFullYear();

  readonly FacebookIcon = Facebook;
  readonly InstagramIcon = Instagram;
  readonly TwitterIcon = Twitter;
  readonly MailIcon = Mail;
  readonly PhoneIcon = Phone;
  readonly MapPinIcon = MapPin;
}
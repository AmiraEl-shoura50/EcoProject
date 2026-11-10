import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Facebook, Instagram, Twitter, Mail, Phone, MapPin } from 'lucide-angular';
import { SITE } from '../../../core/config/site-config';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './footer.html',
  styleUrl: './footer.css'
})
export class Footer {
  readonly site = SITE;
  readonly phoneHref = 'tel:' + SITE.contact.phone.replace(/[^\d+]/g, '');
  readonly currentYear = new Date().getFullYear();

  readonly FacebookIcon = Facebook;
  readonly InstagramIcon = Instagram;
  readonly TwitterIcon = Twitter;
  readonly MailIcon = Mail;
  readonly PhoneIcon = Phone;
  readonly MapPinIcon = MapPin;
}
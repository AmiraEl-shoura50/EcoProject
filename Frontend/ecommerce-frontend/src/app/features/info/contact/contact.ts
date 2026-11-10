import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule, Phone, Mail, MapPin, Clock, Facebook, Instagram, Twitter, CheckCircle } from 'lucide-angular';
import { InfoPage } from '../../../shared/components/info-page/info-page';
import { ContactService } from '../../../core/services/contact';
import { CustomerService } from '../../../core/services/customer';
import { AuthService } from '../../../core/services/auth';
import { SITE } from '../../../core/config/site-config';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [ReactiveFormsModule, LucideAngularModule, InfoPage],
  templateUrl: './contact.html',
  styleUrl: './contact.css'
})
export class Contact implements OnInit {
  readonly site = SITE;
  readonly phoneHref = 'tel:' + SITE.contact.phone.replace(/[^\d+]/g, '');
  readonly maxMessage = 2000;

  readonly PhoneIcon = Phone;
  readonly MailIcon = Mail;
  readonly MapPinIcon = MapPin;
  readonly ClockIcon = Clock;
  readonly FacebookIcon = Facebook;
  readonly InstagramIcon = Instagram;
  readonly TwitterIcon = Twitter;
  readonly CheckIcon = CheckCircle;

  form: FormGroup;
  isSending = signal<boolean>(false);
  sent = signal<boolean>(false);

  constructor(
    private fb: FormBuilder,
    private contactService: ContactService,
    private customerService: CustomerService,
    private authService: AuthService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
      subject: ['', [Validators.required, Validators.maxLength(150)]],
      message: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(this.maxMessage)]]
    });
  }

  ngOnInit(): void {
    // العميل المسجّل: بنملا اسمه وبريده تلقائيًا
    if (this.authService.userRole() !== 'Customer') return;

    this.customerService.getMe().subscribe({
      next: (response) => {
        const customer = response.data;

        this.form.patchValue({
          name: `${customer.firstName} ${customer.lastName}`.trim(),
          email: customer.email
        });
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSending.set(true);

    this.contactService.send(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSending.set(false);
        this.sent.set(true);
      },
      error: () => this.isSending.set(false) // رسالة الخطأ بتظهر من الـ Interceptor
    });
  }

  sendAnother(): void {
    this.form.patchValue({ subject: '', message: '' });
    this.form.markAsUntouched();
    this.sent.set(false);
  }
}
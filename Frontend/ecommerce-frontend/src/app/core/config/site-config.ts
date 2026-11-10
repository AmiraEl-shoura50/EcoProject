export interface SiteConfig {
  name: string;
  contact: { phone: string; email: string; address: string; workingHours: string };
  social: { facebook: string; instagram: string; twitter: string };
  policies: {
    deliveryMinDays: number;
    deliveryMaxDays: number;
    returnWindowDays: number;
    shippingNote: string;
    refundNote: string;
  };
}

// ⚠️ كل القيم هنا افتراضية: عدّلها حسب بيانات وسياسة موقعك الفعلية
export const SITE: SiteConfig = {
  name: 'Smart Shop',

  contact: {
    phone: '+20 100 000 0000',
    email: 'support@smartshop.com',
    address: 'القاهرة، مصر',
    workingHours: 'يوميًا من 10 صباحًا حتى 8 مساءً'
  },

  // اترك الرابط فاضي ('') لو مفيش حساب، والأيقونة هتختفي من الموقع
  social: {
    facebook: '',
    instagram: '',
    twitter: ''
  },

  policies: {
    deliveryMinDays: 2,
    deliveryMaxDays: 5,
    returnWindowDays: 14,
    shippingNote: 'لا يضيف الموقع رسوم شحن على إجمالي طلبك. وأي رسوم يحددها البائع يتم الاتفاق عليها معه مباشرة.',
    refundNote: 'عند الموافقة على الإرجاع يتم استرداد المبلغ بنفس طريقة الدفع الأصلية.'
  }
};
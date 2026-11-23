import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { roleGuard } from './core/guards/role-guard';



export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home/home').then(m => m.Home)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password').then(m => m.ForgotPassword)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password').then(m => m.ResetPassword)
  },
  {
  path: 'category/:id',
  loadComponent: () => import('./features/category/category-products/category-products').then(m => m.CategoryProducts)
},
{
  path: 'products/:id',
  loadComponent: () => import('./features/products/product-details/product-details').then(m => m.ProductDetails)
},
{
  path: 'faq',
  title: 'الأسئلة الشائعة | Smart Shop',
  loadComponent: () => import('./features/faq/faq/faq').then(m => m.Faq)
},{
  path: 'about',
  title: 'من نحن | Smart Shop',
  loadComponent: () => import('./features/info/about/about').then(m => m.About)
},
{
  path: 'contact',
  title: 'تواصل معنا | Smart Shop',
  loadComponent: () => import('./features/info/contact/contact').then(m => m.Contact)
},
{
  path: 'shipping-policy',
  title: 'الشحن والتوصيل | Smart Shop',
  loadComponent: () => import('./features/info/shipping-policy/shipping-policy').then(m => m.ShippingPolicy)
},
{
  path: 'return-policy',
  title: 'سياسة الإرجاع والإلغاء | Smart Shop',
  loadComponent: () => import('./features/info/return-policy/return-policy').then(m => m.ReturnPolicy)
}
,
{
  path: 'orders',
  loadComponent: () => import('./features/orders/orders-page/orders-page').then(m => m.OrdersPage),
  canActivate: [authGuard]
},
{
  path: 'wishlist',
  loadComponent: () => import('./features/wishlist/wishlist-page/wishlist-page').then(m => m.WishlistPage),
  canActivate: [authGuard]
},
{
  path: 'cart',
  loadComponent: () => import('./features/cart/cart-page/cart-page').then(m => m.CartPage),
  canActivate: [authGuard]
},
{
  path: 'checkout',
  loadComponent: () => import('./features/checkout/checkout/checkout').then(m => m.Checkout),
  canActivate: [authGuard]
},
{
  path: 'orders/:id/payment-proof',
  loadComponent: () => import('./features/orders/payment-proof/payment-proof').then(m => m.PaymentProof),
  canActivate: [authGuard]
},
{
  path: 'profile',
  loadComponent: () => import('./features/profile/profile-page/profile-page').then(m => m.ProfilePage),
  canActivate: [authGuard]
},
{
  path: 'seller',
  loadComponent: () => import('./features/seller/seller-layout/seller-layout').then(m => m.SellerLayout),
  canActivate: [roleGuard('Seller')],
  children: [
    {
    path: '',
    pathMatch: 'full',
    title: 'نظرة عامة | Smart Shop',
    loadComponent: () => import('./features/seller/seller-dashboard/seller-dashboard').then(m => m.SellerDashboardPage)
    },
    {
      path: 'orders',
      loadComponent: () => import('./features/seller/seller-orders/seller-orders').then(m => m.SellerOrders)
    },
    {
      path: 'products',
      loadComponent: () => import('./features/seller/seller-products/seller-products').then(m => m.SellerProducts)
    },
    {
      path: 'categories',
      loadComponent: () => import('./features/seller/seller-categories/seller-categories').then(m => m.SellerCategories)
    },
    {
      path: 'discounts',
      loadComponent: () => import('./features/seller/seller-discounts/seller-discounts').then(m => m.SellerDiscounts)
    },
    {
      path: 'store',
      title: 'متجري | Smart Shop',
      loadComponent: () => import('./features/seller/seller-store/seller-store').then(m => m.SellerStore)
    }
  ]
},
{
  path: 'search',
  loadComponent: () => import('./features/search/search-results/search-results').then(m => m.SearchResults)
},
{
  path: 'offers',
  loadComponent: () => import('./features/offers/offers-page/offers-page').then(m => m.OffersPage)
},
{
  path: 'admin',
  loadComponent: () => import('./features/admin/admin-layout/admin-layout').then(m => m.AdminLayout),
  canActivate: [roleGuard('Admin')],
  children: [
    {
      path: '',
      pathMatch: 'full',
      title: 'نظرة عامة | Smart Shop',
      loadComponent: () => import('./features/admin/admin-dashboard/admin-dashboard').then(m => m.AdminDashboardPage)
    },
    {
      path: 'users',
      loadComponent: () => import('./features/admin/admin-users/admin-users').then(m => m.AdminUsers)
    },
    {
      path: 'categories',
      loadComponent: () => import('./features/admin/admin-categories/admin-categories').then(m => m.AdminCategories)
    },
    {
      path: 'payment-methods',
      loadComponent: () => import('./features/admin/admin-payment-methods/admin-payment-methods').then(m => m.AdminPaymentMethods)
    },
    {
      path: 'messages',
      title: 'الرسائل | Smart Shop',
      loadComponent: () => import('./features/admin/admin-messages/admin-messages').then(m => m.AdminMessages)
    },
    {
      path: 'products',
      title: 'المنتجات | Smart Shop',
      loadComponent: () => import('./features/admin/admin-products/admin-products').then(m => m.AdminProducts)
    },
    {
      path: 'payments',
      title: 'الدفعات | Smart Shop',
      loadComponent: () => import('./features/admin/admin-payments/admin-payments').then(m => m.AdminPayments)
    }
  ]
},
{
  path: 'share-receipt',
  loadComponent: () => import('./features/orders/share-receipt/share-receipt').then(m => m.ShareReceipt),
  canActivate: [roleGuard('Customer')]
},
{
  path: 'notifications',
  loadComponent: () => import('./features/notifications/notifications-page/notifications-page').then(m => m.NotificationsPage),
  canActivate: [authGuard]
},
{
  path: 'store/:id',
  title: 'المتجر | Smart Shop',
  loadComponent: () => import('./features/store/store-page/store-page').then(m => m.StorePage)
},
{
  path: 'payments/:id',
  title: 'الدفع | Smart Shop',
  loadComponent: () => import('./features/payments/payment-page/payment-page').then(m => m.PaymentPage),
  canActivate: [roleGuard('Customer')]
},
{
  path: '**',
  title: 'الصفحة غير موجودة | Smart Shop',
  loadComponent: () => import('./features/not-found/not-found/not-found').then(m => m.NotFound)
}
];
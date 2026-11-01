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
  loadComponent: () => import('./features/faq/faq/faq').then(m => m.Faq)
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
    { path: '', redirectTo: 'orders', pathMatch: 'full' },
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
    { path: '', redirectTo: 'users', pathMatch: 'full' },
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
}
];
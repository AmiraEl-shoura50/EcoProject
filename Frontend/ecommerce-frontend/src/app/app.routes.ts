import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';



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
}
];
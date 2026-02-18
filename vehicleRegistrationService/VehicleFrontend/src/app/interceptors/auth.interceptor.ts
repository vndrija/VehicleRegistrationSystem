import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  try {
    const token = localStorage.getItem('token');
    if (token) {
      req = req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) });
    }
  } catch {
    // ignore when localStorage isn't available
  }

  return next(req);
};

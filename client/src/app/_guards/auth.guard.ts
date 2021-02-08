import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, observeOn } from 'rxjs/operators';
import { AccountService } from '../_services/account.service';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private accountService:AccountService, private toastr:ToastrService){}

  canActivate(): Observable<boolean>  {
     return this.accountService.currentUser$.pipe(
  map((response) =>{        
   if(response){
    return true;
   }
   else{
     this.toastr.error('You shall not pass!')
    return false;
    }
   }
 ));
  }
  
}

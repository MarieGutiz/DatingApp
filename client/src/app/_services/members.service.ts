import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeader } from './paginationHelper';




@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = []; 
  memberCache= new Map();
  user!: User;
  userParams!: UserParams;

  constructor(private http: HttpClient, private accountService:AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user=>{
      this.user=user;
      this.userParams= new UserParams(user);
    })
   }

   getUserParams(){
     return this.userParams;
   }

   setUserParams(params: UserParams){
     this.userParams= params;
   }

   resetUserParams(){
     this.userParams = new UserParams(this.user);
     return this.userParams;
   }

  getMembers(userParams: UserParams){  
    // if(this.members.length >0) return of(this.members) //caching
    var response = this.memberCache.get(Object.values(userParams).join('-'))
    if(response)
    {
      return of(response)
    }

    let params = getPaginationHeader(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString()) ;
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
    
    return getPaginatedResult<Member[]>(this.baseUrl +'users',params, this.http)
    .pipe(map(response =>{
      this.memberCache.set(Object.values(userParams).join('-'),response)
      return response
    }))
    
  }
  

  getMember(username:string){
    // const member = this.members.find(x=> x.username === username);
    const member = [...this.memberCache.values()]
    .reduce((arr,elem)=> arr.concat(elem.result),[])//flat array no array of array
    .find((member:Member)=> member.username === username)//cacheing the results

    if(member) {return of(member)}

    return this.http.get<Member>(this.baseUrl +'users/'+username);
  }

  updateMember(member:Member){
    return this.http.put(this.baseUrl+'users',member).pipe(
      map(()=>{
        const index = this.members.indexOf(member);
        this.members[index]=member;
      })
    );
  }

  setMainPhoto(photoId:Number){
     return this.http.put(this.baseUrl+'users/set-main-photo/'+photoId,{})
  }

  deletePhoto(photoId:Number){
    return this.http.delete(this.baseUrl+'users/delete-photo/'+photoId, {});
  }

  addLike(username:string){
    return this.http.post(this.baseUrl +'likes/'+username,{});
  }

  getLikes(predicate:string, pageNumber: number, PageSize: number){
    let params= getPaginationHeader(pageNumber,PageSize);
    params = params.append('predicate', predicate);

    // return this.http.get<Member[]>(this.baseUrl+'likes?predicate='+predicate);
    return getPaginatedResult<Member[]>(this.baseUrl+'likes', params, this.http);
  }
}

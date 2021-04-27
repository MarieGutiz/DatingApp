import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', { static: false})  memberTabs!: TabsetComponent;


  member!: Member;
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];
  activeTab!: TabDirective;
  messages: Message[] = [];
  value!: string;
  user!: User;
  showctrl=false;

  constructor(public presence:PresenceService, private route:ActivatedRoute, private messageService:MessageService,
    private accountService:AccountService, private router:Router) {
      this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user =user);
      this.router.routeReuseStrategy.shouldReuseRoute = ()=> false;
     }


  ngOnInit(): void {
  
    this.route.data.subscribe(data => {
      this.member = data.member;
    })
    //this.loadMember();
    // this.route.queryParams.subscribe(params=>{ 
    //   params.tab ? this.selectTab(params.tab):this.selectTab(0);
    // })
  
    this.galleryOptions=[
      {
        width:'500px',
        height:'500px',
        imagePercent:100,
        thumbnailsColumns:4,
        imageAnimation:NgxGalleryAnimation.Slide,
        preview:false,
      }
    ]
     
    this.galleryImages =this.getImages();
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this.route.queryParams.subscribe(params=>{ 
       params.tab ? this.selectTab(params.tab):this.selectTab(0);
     })
    });
    
  }

  getImages():NgxGalleryImage[]{

    const imageUrl =[];
    for (const photos of this.member.photos) {
      imageUrl.push({
        small:photos?.url,
        medium:photos?.url,
        big:photos?.url
      })
     
    }
    return imageUrl;
  }

 onTabActivated(data:TabDirective){//para que no carge lo demas  
  this.activeTab = data;
  if(this.activeTab.heading === 'Messages' && this.messages.length ===0){
   // this.loadMessage()
   this.messageService.createHubConnection(this.user,this.member.username);
   setTimeout(()=>{  
    this.showctrl=true;
    // console.log("memberdatail show "+this.showctrl) //problems with tabs     
    }, 1000)
   
  }
  else{
    this.messageService.stopHubConnection();
   this.showctrl =false
  }
 }

 loadMessage(){
  this.messageService.getMessageThread(this.member.username).subscribe(messages =>{
    this.messages = messages;
  })
}
selectTab(tabId:number){
  this.memberTabs.tabs[tabId].active = true;
}

ngOnDestroy(): void {
  this.messageService.stopHubConnection();
}

}

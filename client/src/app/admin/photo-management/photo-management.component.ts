import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[] = [];
  index: number = 0;
  user: User;

  constructor(private adminService: AdminService, private messageService: MessageService) { }

  ngOnInit(): void {
    this.getPhotosToModerate();
  }

  getPhotosToModerate() {
    this.adminService.getPhotosToModerate().subscribe(photos => {
      console.table(photos)
      this.photos = photos;
    })
  }

  decrement() {
    if (this.index <= 0) {
      this.index = this.photos.length - 1;
      return;
    }
    this.index--;
  }

  increment() {
    if (this.index >= this.photos.length - 1) {
      this.index = 0;
      return;
    }
    this.index++;
  }

  approve() {
    var username = this.photos[this.index].username;
    // this.adminService.approvePhoto(this.photos[this.index].id).subscribe(() => {
      this.messageService.sendMessage(username, 'Your photo was approved!');
    //   this.photos.splice(this.index, 1);
    // })
  }

  reject() {
    var username = this.photos[this.index].username;
    // this.adminService.deletePhoto(this.photos[this.index].id).subscribe(() => {
      this.messageService.sendMessage(username, 'Your photo was unable to be approved! It was in violation of our code of conduct!');
    //   this.photos.splice(this.index, 1);
    // })
  }
}

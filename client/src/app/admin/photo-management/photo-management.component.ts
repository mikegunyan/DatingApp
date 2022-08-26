import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[] = [];
  index: number = 0;

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.getPhotosToModerate();
  }

  getPhotosToModerate() {
    this.adminService.getPhotosToModerate().subscribe(photos => {
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
    console.log('Approved')
    this.photos.splice(this.index, 1);
  }

  reject() {
    console.log('Rejected')
    this.photos.splice(this.index, 1);
  }
}
